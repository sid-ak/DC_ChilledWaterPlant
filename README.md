# Data Center Chilled Water Plant, I&C Package

## Objective

- Work through the rough I&C scope for a realistic data center system.
- Make engineering decisions that define instrumentation and controls
- Design deliverables that document and coordinate them

## Basis of Design

The model is an open source reference model by Modelica maintained by Lawrence Berkeley National Laboratory (LBNL) of a primary-secondary chiller plant with two chillers and a non-integrated waterside economizer.

Reference: [NonIntegratedPrimarySecondaryEconomizer](https://build.openmodelica.org/Documentation/Buildings.Applications.DataCenters.ChillerCooled.Examples.NonIntegratedPrimarySecondaryEconomizer.html)

## Scope

- Instrumentation selection and ranges
    - P&ID with ANSI/ISA-5.1 tagging
- Control philosophy: staging, mode switch, interlocks
    - Instrument index and I/O list
- Fail safe positions, at the system level
    - Control panel layout and BOM
- Revit model: instrument placement and cable tray
- Gap analysis: what the schematic lacks for a real P&ID
- Navisworks clash detection and resolution
- Automated model-to-index consistency check
