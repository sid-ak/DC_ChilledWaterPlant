# System Basis

How the chilled water plant works and how it is controlled.

## The plant

Two chillers make cold water. In cold weather an economizer (a heat exchanger) uses the cooling towers instead, so the chillers can switch off. Pumps send the cold water to two air handlers (CRAHs) that cool the server room.

The plant has two loops. The primary loop runs through the chillers or the economizer. The secondary loop feeds the air handlers. A short bypass pipe (the decoupler) joins them so each loop can run its own flow.

| Tag | What it is |
|---|---|
| CH-1, CH-2 | Chillers |
| PCHWP-1, PCHWP-2 | Primary pumps, fixed speed, one per running chiller |
| SCHWP-1, SCHWP-2 | Secondary pumps, variable speed, one lead and one standby |
| WSE-1 | Economizer heat exchanger (free cooling) |
| CRAH-1, CRAH-2 | Air handlers in the server room |
| ET-1 | Expansion tank |
| CP-1 | Control panel: PLC connected to the building management system |

Targets: 8°C supply water and 18°C supply air.

## Two modes

| Mode | Cooling comes from | Valves |
|---|---|---|
| Chillers | The chillers | Chiller valves open, economizer valve closed |
| Free cooling | The cooling towers, through the economizer | Economizer valve open, chiller valves closed |

The plant never runs both modes at once.

## Switching modes

- Switch to free cooling when it has been cold and dry enough outside for 30 minutes. The controller works out the wet-bulb temperature from the outdoor temperature and humidity sensors; the switch point is about 2.5°C wet-bulb.
- Switch back to chillers when the wet-bulb rises above 4°C, or when the supply water stays more than 1°C too warm for 10 minutes.
- Stay in each mode for at least 60 minutes before switching again.
- Open the new path and confirm it is open before closing the old one.

## Chiller staging

- Start the second chiller when the running one is near full load (above 90% for 10 minutes), when the supply water runs warm (above 9°C for 10 minutes), or when water flows backwards through the decoupler.
- Stop a chiller when the load drops below 40% for 15 minutes.
- To start a chiller: open its valve, confirm it is open, start its pump, confirm flow, then start the chiller.
- To stop a chiller: stop it, run its pump for 5 more minutes, stop the pump, then close the valve.
- Swap the lead chiller every week.

## Pumps

- Secondary pumps speed up or slow down to hold a set pressure at the far air handler (CRAH-1). The standby pump starts if the lead pump runs above 90% speed for 10 minutes. The lead swaps every week.
- Primary pumps run one per running chiller. In free cooling, one pump runs through the economizer.

## Air handlers

Each air handler valve opens or closes to keep supply air at 18°C. The valve reports its actual position, and an alarm comes up if it doesn't match the command.

## Interlocks

| Rule | What happens | Why |
|---|---|---|
| A chiller starts only after its valve is proven open and its flow switch sees flow within 30 seconds | Otherwise the start is stopped and alarmed | A chiller with no water flow can freeze and damage itself |
| Flow is lost for 5 seconds while a chiller runs | The chiller stops and alarms | Same reason |
| A pump starts only if a valve on its path is open | The start is blocked | Stops the pump pushing against closed valves |
| No command may close every path valve at once | The command is blocked and alarmed | Same reason |
| Expansion tank level is low | High-priority alarm | Points to a leak or loss of water |
| A leak is detected under an air handler | Top-priority alarm, no automatic shutdown | Cutting cooling to the servers could do more harm than the leak |

The chillers' own flow protection stays in place. These interlocks are a second layer.

## Main alarms

| Priority | Alarms |
|---|---|
| 1 (highest) | Leak, chiller flow lost, expansion tank low, supply water more than 2°C too warm for 5 minutes |
| 2 | Equipment fault (chiller, pump drive, air handler), valve not reaching position within 60 seconds, low tank level, low loop pressure with both pumps at full speed, broken sensor wire |
| 3 | Air handler valve position mismatch, low pump discharge pressure |
