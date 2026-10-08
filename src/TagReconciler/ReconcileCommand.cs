using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace TagReconciler
{
    /// <summary>
    /// Compares instrument tags and services in the model with the instrument index and reports:
    /// MissingInModel, MissingInIndex, ServiceMismatch, DuplicateInModel. Writes
    /// reconcile_report_yyyyMMdd-HHmmss.csv and exports the Instrument Schedule to revit_schedule.txt.
    /// </summary>
    [Transaction(TransactionMode.ReadOnly)]
    public class ReconcileCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData data, ref string message, ElementSet elements)
        {
            Document doc = data.Application.ActiveUIDocument?.Document;
            if (doc == null) return Result.Cancelled;
            IcConfig cfg;
            try { cfg = IcConfig.Load(); IcConfig.RequireFiles(cfg.IndexCsv); }
            catch (Exception ex) { TaskDialog.Show("Reconcile Instrument Tags", ex.Message); return Result.Failed; }

            var index = U.IndexServices(cfg.IndexCsv);
            var model = U.Instruments(doc);
            var issues = new List<string[]>();   // Issue, Tag, IndexService, ModelService, ElementId

            foreach (var g in model.GroupBy(m => m.tag, StringComparer.OrdinalIgnoreCase))
            {
                bool inIndex = index.TryGetValue(g.Key, out string idxSvc);
                if (!inIndex)
                    foreach (var m in g)
                        issues.Add(new[] { "MissingInIndex", m.tag, "", Svc(m.el), m.el.Id.Value.ToString() });
                if (g.Count() > 1)
                    issues.Add(new[] { "DuplicateInModel", g.Key, inIndex ? idxSvc : "", "",
                                       string.Join(";", g.Select(m => m.el.Id.Value.ToString())) });
                if (inIndex)
                    foreach (var m in g)
                        if (!string.Equals(Svc(m.el), idxSvc?.Trim() ?? "", StringComparison.Ordinal))
                            issues.Add(new[] { "ServiceMismatch", m.tag, idxSvc, Svc(m.el), m.el.Id.Value.ToString() });
            }
            var modelTags = new HashSet<string>(model.Select(m => m.tag), StringComparer.OrdinalIgnoreCase);
            foreach (var kv in index.Where(kv => !modelTags.Contains(kv.Key)).OrderBy(kv => kv.Key))
                issues.Add(new[] { "MissingInModel", kv.Key, kv.Value, "", "" });

            string dir = ReportDir(cfg, doc);
            string report = System.IO.Path.Combine(dir, $"reconcile_report_{DateTime.Now:yyyyMMdd-HHmmss}.csv");
            var sb = new StringBuilder("Issue,Tag,IndexService,ModelService,ElementId\n");
            foreach (var i in issues.OrderBy(i => i[0]).ThenBy(i => i[1]))
                sb.AppendLine(string.Join(",", i.Select(Csv.Escape)));
            System.IO.File.WriteAllText(report, sb.ToString(), Encoding.UTF8);

            string sched = "Schedule export: 'Instrument Schedule' not found.";
            ViewSchedule vs = new FilteredElementCollector(doc).OfClass(typeof(ViewSchedule)).Cast<ViewSchedule>()
                .FirstOrDefault(v => v.Name == "Instrument Schedule");
            if (vs != null)
            {
                try { vs.Export(dir, "revit_schedule.txt", new ViewScheduleExportOptions()); sched = "Schedule exported: " + System.IO.Path.Combine(dir, "revit_schedule.txt"); }
                catch (Exception ex) { sched = "Schedule export failed: " + ex.Message; }
            }

            string[] kinds = { "MissingInModel", "MissingInIndex", "ServiceMismatch", "DuplicateInModel" };
            string counts = string.Join("\n", kinds.Select(k => $"{k}: {issues.Count(i => i[0] == k)}"));
            TaskDialog.Show("Reconcile Instrument Tags",
                $"Instruments in model: {model.Count}   In index: {index.Count}\n\n{counts}\n\n" +
                (issues.Count == 0 ? "Model and index agree.\n\n" : "") +
                $"Report: {report}\n{sched}");
            return Result.Succeeded;
        }

        private static string Svc(Element e) => e.LookupParameter(U.ServiceParam)?.AsString()?.Trim() ?? "";

        private static string ReportDir(IcConfig cfg, Document doc)
        {
            if (!string.IsNullOrEmpty(cfg.ReportDir) && System.IO.Directory.Exists(cfg.ReportDir)) return cfg.ReportDir;
            string d = System.IO.Path.GetDirectoryName(doc.PathName ?? "");
            return !string.IsNullOrEmpty(d) && System.IO.Directory.Exists(d) ? d : System.IO.Path.GetTempPath();
        }
    }
}
