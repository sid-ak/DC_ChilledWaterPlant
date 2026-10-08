# Instrument Selection

The plant has 40 instruments. The full list with tags, ranges and signals is in `data/instrument_index.xlsx`.

## What is measured and why

| Area | Tags | Why |
|---|---|---|
| Main supply and return (6) | TT-501, TT-502, FT-503, PDT-504, FT-505, PT-506 | Supply temperature, load for staging, pump speed control, backwards flow in the decoupler |
| Expansion tank (3) | LT-511, PT-512, LSL-513 | Catch water loss early |
| Chillers (6 each) | FS-1x1, TT-1x2, XV-1x3, ZSO-1x3, ZSC-1x3, TT-1x4 | Prove flow and valve position before a chiller starts |
| Pumps (4) | PT-211, PT-221, PT-311, PT-321 | Confirm each pump is moving water |
| Air handlers (5 each) | TCV-4x1, ZT-4x1, TT-4x2, TT-4x3, XS-4x4 | Control supply air, catch a stuck valve, catch leaks |
| Free cooling (5) | XV-601, ZSO-601, ZSC-601, TT-602, MT-603 | Decide when to switch modes and prove the valves moved |

Every instrument supports a control rule, interlock or alarm in the system basis.

## Instrument types

- **Temperature:** RTD in a thermowell. More accurate than a thermocouple at chilled water temperatures, and it can be replaced without draining the pipe.
- **Flow:** magnetic flow meter. Accurate, no moving parts, and reads flow in both directions, which the decoupler needs.
- **Loop pressure:** differential pressure across the far air handler, so the pumps only work as hard as needed.
- **Isolation valves:** on/off butterfly valves with open and closed switches, so the controls know the valve actually moved.
- **Air handler valves:** control valves with position feedback, to catch a stuck valve.
- **Leak:** sensing cable on the floor around each air handler.
- **Outdoor:** temperature and humidity sensor in a sun shield.

## Fail positions

What each valve does if it loses power or signal.

| Valve | Fails | Why |
|---|---|---|
| Air handler valves (TCV-411, TCV-421) | Open | Keeping the servers cool comes first |
| Chiller valves (XV-113, XV-123) | Open | Chillers work in any weather |
| Economizer valve (XV-601) | Closed | Free cooling only works when it is cold |

If everything fails at once, the chiller paths are open, so the pumps always have somewhere to push water.

## Where instruments go

- Temperature sensors sit far enough after a tee for the water to mix.
- Flow meters sit in straight pipe that stays full of water.
- Loop pressure is measured at the far air handler, CRAH-1.
- Pump pressure is measured on each pump discharge.
- The outdoor sensor sits on the shaded north wall, away from exhausts and the cooling tower plume.
- Leak cable runs on the floor around each air handler.
- Every instrument can be reached for maintenance, with its display facing the aisle and clear of valve handles.
