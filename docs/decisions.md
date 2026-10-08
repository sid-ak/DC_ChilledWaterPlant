# Decisions

The main engineering choices and why they were made.

| Topic | Decision | Why |
|---|---|---|
| Chiller staging | Add a chiller above 90% load, when supply water runs warm, or when the decoupler flows backwards. Drop one below 40% load. Swap the lead weekly. | The gap between 90% and 40% stops the plant from cycling chillers on and off |
| Free cooling switch | Switch on outdoor wet-bulb temperature, with a gap between the on and off points and at least 60 minutes in each mode | Cooling towers are limited by wet-bulb, not air temperature |
| Instrument list | 40 instruments | Each one supports a control rule, interlock or alarm. Condenser water side is out of scope. |
| Fail positions | Air handler valves and chiller valves fail open; economizer valve fails closed | Server cooling comes first, and the chillers work in any weather |
| Controller | PLC with a link to the building management system | Common for data center cooling: fast, reliable, with redundancy options. The building system still sees everything. |
| 24 VDC power | A redundant pair of supplies for instruments and the controller, and a separate supply for valve actuators | Keeps actuator noise off the instruments; redundancy goes where a failure isn't already covered |
| Panel cooling | No fan | The panel stays within its temperature limit without one, and a fan weakens the sealed enclosure |
| Panel size | 1200 x 800 x 300 mm wall-mount, NEMA 12 | A bigger box is easier to wire, label and troubleshoot |
| Wiring | One shielded pair per instrument, run straight to the panel, shield grounded at the panel only | Simple to trace on a small plant; grounding one end avoids ground loops |
| Cable tray | 150 x 50 mm ladder tray at 3 m, kept 300 mm from power, with conduit down to each instrument | One route for many cables and easy to add to; distance from power keeps noise off the signals |
| Plant room | 20 x 12 m, 5 m ceiling, two columns, three beams | Simplified room that still has real coordination problems |
