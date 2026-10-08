using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Autodesk.Revit.DB;

namespace TagReconciler
{
    /// <summary>Paths to the project data, read from TagReconciler.config.json next to the DLL.</summary>
    public class IcConfig
    {
        public string LayoutJson { get; set; }
        public string PlacementCsv { get; set; }
        public string IndexCsv { get; set; }
        public string SharedParams { get; set; }
        public string ReportDir { get; set; }

        public static IcConfig Load()
        {
            string dir = System.IO.Path.GetDirectoryName(typeof(IcConfig).Assembly.Location);
            string path = System.IO.Path.Combine(dir, "TagReconciler.config.json");
            if (!System.IO.File.Exists(path)) throw new FileNotFoundException("Config not found next to the add-in DLL", path);
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<IcConfig>(System.IO.File.ReadAllText(path), opts);
        }

        public static void RequireFiles(params string[] paths)
        {
            var missing = paths.Where(p => string.IsNullOrEmpty(p) || !System.IO.File.Exists(p)).ToList();
            if (missing.Count > 0)
                throw new FileNotFoundException(
                    "Input not reachable:\n" + string.Join("\n", missing) +
                    "\n\nCheck that the VM session has the DataCenter folder redirected (\\\\tsclient\\DataCenter).");
        }
    }

    /// <summary>Small CSV reader (RFC 4180 quoting) returning rows keyed by header name.</summary>
    public static class Csv
    {
        public static List<Dictionary<string, string>> Read(string path)
        {
            var table = Parse(System.IO.File.ReadAllText(path, System.Text.Encoding.UTF8));
            var rows = new List<Dictionary<string, string>>();
            if (table.Count == 0) return rows;
            string[] header = table[0].Select(h => h.Trim().TrimStart('\uFEFF')).ToArray();
            foreach (string[] f in table.Skip(1))
            {
                if (f.Length == 1 && string.IsNullOrWhiteSpace(f[0])) continue;   // blank line
                var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < header.Length; i++) row[header[i]] = i < f.Length ? f[i].Trim() : "";
                rows.Add(row);
            }
            return rows;
        }

        public static List<string[]> Parse(string text)
        {
            var rows = new List<string[]>();
            var row = new List<string>();
            var sb = new System.Text.StringBuilder();
            bool quoted = false;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (quoted)
                {
                    if (c == '"')
                    {
                        if (i + 1 < text.Length && text[i + 1] == '"') { sb.Append('"'); i++; }
                        else quoted = false;
                    }
                    else sb.Append(c);
                }
                else if (c == '"') quoted = true;
                else if (c == ',') { row.Add(sb.ToString()); sb.Clear(); }
                else if (c == '\n') { row.Add(sb.ToString()); sb.Clear(); rows.Add(row.ToArray()); row.Clear(); }
                else if (c != '\r') sb.Append(c);
            }
            if (sb.Length > 0 || row.Count > 0) { row.Add(sb.ToString()); rows.Add(row.ToArray()); }
            return rows;
        }

        public static string Escape(string s)
        {
            s = s ?? "";
            return (s.Contains(",") || s.Contains("\"")) ? "\"" + s.Replace("\"", "\"\"") + "\"" : s;
        }
    }

    public static class U
    {
        public const string BuildMark = "IC-BUILD";
        public const string DropMark = "IC-DROP";
        public const string TagParam = "InstrumentTag";
        public const string ServiceParam = "InstrumentService";

        /// <summary>Elevation of the base level (internal units); all layout Z values are measured from it.</summary>
        public static double Z0 = 0;
        public static double Mm(double v) => UnitUtils.ConvertToInternalUnits(v, UnitTypeId.Millimeters);
        public static double ToMm(double ft) => UnitUtils.ConvertFromInternalUnits(ft, UnitTypeId.Millimeters);
        public static XYZ P(double x, double y, double z) => new XYZ(Mm(x), Mm(y), Z0 + Mm(z));
        public static double D(string s) => double.Parse(s, System.Globalization.CultureInfo.InvariantCulture);

        public static readonly BuiltInCategory[] InstrumentCategories =
        {
            BuiltInCategory.OST_GenericModel, BuiltInCategory.OST_PipeAccessory,
            BuiltInCategory.OST_DuctAccessory, BuiltInCategory.OST_MechanicalEquipment
        };

        public static string Comments(Element e) =>
            e.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS)?.AsString() ?? "";

        public static void SetComments(Element e, string s) =>
            e.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS)?.Set(s);

        public static Level BaseLevel(Document doc)
        {
            Level l = new FilteredElementCollector(doc).OfClass(typeof(Level)).Cast<Level>()
                .OrderBy(x => Math.Abs(x.ProjectElevation)).First();
            Z0 = l.ProjectElevation;
            return l;
        }

        /// <summary>Find the end connector of an MEP curve closest to a point.</summary>
        public static Connector EndAt(MEPCurve c, XYZ p)
        {
            Connector best = null; double d = double.MaxValue;
            foreach (Connector k in c.ConnectorManager.Connectors)
            {
                if (k.ConnectorType != ConnectorType.End) continue;
                double dd = k.Origin.DistanceTo(p);
                if (dd < d) { d = dd; best = k; }
            }
            return best;
        }

        /// <summary>Join free end connectors that meet at the same point: 2 = elbow (or union if straight),
        /// 3 = tee (straight-through pair + branch). Returns (joined, failed).</summary>
        public static (int ok, int fail) ConnectEnds(Document doc, IEnumerable<MEPCurve> curves, string mark = null)
        {
            var free = new List<Connector>();
            foreach (MEPCurve c in curves)
                foreach (Connector k in c.ConnectorManager.Connectors)
                    if (k.ConnectorType == ConnectorType.End && !k.IsConnected) free.Add(k);
            var groups = new List<List<Connector>>();
            double tol = Mm(5);
            foreach (Connector k in free)
            {
                var g = groups.FirstOrDefault(gr => gr[0].Origin.DistanceTo(k.Origin) < tol);
                if (g == null) groups.Add(new List<Connector> { k }); else g.Add(k);
            }
            int ok = 0, fail = 0;
            foreach (var g in groups)
            {
                if (g.Count < 2) continue;
                try
                {
                    FamilyInstance fit;
                    if (g.Count == 2)
                    {
                        double dot = g[0].CoordinateSystem.BasisZ.DotProduct(g[1].CoordinateSystem.BasisZ);
                        fit = dot < -0.99 ? doc.Create.NewUnionFitting(g[0], g[1]) : doc.Create.NewElbowFitting(g[0], g[1]);
                    }
                    else if (g.Count == 3)
                    {
                        Connector a = null, b = null;
                        for (int i = 0; i < 3 && a == null; i++)
                            for (int j = i + 1; j < 3; j++)
                                if (g[i].CoordinateSystem.BasisZ.DotProduct(g[j].CoordinateSystem.BasisZ) < -0.99)
                                { a = g[i]; b = g[j]; break; }
                        if (a == null) throw new InvalidOperationException("no straight-through pair");
                        Connector br = g.First(x => x != a && x != b);
                        fit = doc.Create.NewTeeFitting(a, b, br);
                    }
                    else throw new InvalidOperationException(g.Count + "-way junction");
                    if (mark != null && fit != null) SetComments(fit, mark);
                    ok++;
                }
                catch { fail++; }
            }
            return (ok, fail);
        }

        /// <summary>Instrument elements: any element in the instrument categories with a non-empty InstrumentTag.</summary>
        public static List<(Element el, string tag)> Instruments(Document doc)
        {
            var list = new List<(Element, string)>();
            var filter = new ElementMulticategoryFilter(InstrumentCategories.ToList());
            foreach (Element e in new FilteredElementCollector(doc).WherePasses(filter).WhereElementIsNotElementType())
            {
                string tag = e.LookupParameter(TagParam)?.AsString()?.Trim();
                if (!string.IsNullOrEmpty(tag)) list.Add((e, tag));
            }
            return list;
        }

        public static Dictionary<string, string> IndexServices(string indexCsv)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var r in Csv.Read(indexCsv))
                if (r.TryGetValue("Tag", out var t) && !string.IsNullOrWhiteSpace(t))
                    map[t.Trim()] = r.TryGetValue("Service", out var s) ? s.Trim() : "";
            return map;
        }
    }

    /// <summary>Deletes warnings so a long build doesn't stop on "elements overlap" style messages.</summary>
    public class SwallowWarnings : IFailuresPreprocessor
    {
        public FailureProcessingResult PreprocessFailures(FailuresAccessor a)
        {
            bool resolved = false;
            foreach (FailureMessageAccessor f in a.GetFailureMessages())
            {
                if (f.GetSeverity() == FailureSeverity.Warning) { a.DeleteWarning(f); continue; }
                // Errors: apply Revit's default resolution (usually deleting the offending element, e.g. a
                // fitting that can't be placed) so one bad element doesn't roll back the whole step.
                if (f.HasResolutions()) { a.ResolveFailure(f); resolved = true; }
            }
            return resolved ? FailureProcessingResult.ProceedWithCommit : FailureProcessingResult.Continue;
        }

        public static Transaction Start(Document doc, string name)
        {
            var t = new Transaction(doc, name);
            var o = t.GetFailureHandlingOptions();
            o.SetFailuresPreprocessor(new SwallowWarnings());
            t.SetFailureHandlingOptions(o);
            t.Start();
            return t;
        }
    }
}
