# Clash Fix List

Navisworks run 1 (`clash_before.xml`) found 4 clashes, all on the cable tray.

| # | Test | Location (x, y, z mm) | Elements (Revit ID) | Fix |
|---|---|---|---|---|
| 1 | Tray vs Piping | 8770, 8023, 2975 | Tray 928554, Pipe 928023 | Move tray segment T-D |
| 2 | Tray vs Piping | 8728, 6977, 2978 | Tray 928554, Pipe 928017 | Move tray segment T-D |
| 3 | Tray vs Structure | 6800, 5925, 3012 | Tray 928550, Column 927992 | Move tray segment T-B |
| 4 | Tray vs Structure | 13600, 5925, 2990 | Tray 928550, Column 927993 | Move tray segment T-B |

## Tray moves

Each segment moved sideways until it cleared columns, pipes and ducts by at least 300 mm. Connected segments were stretched to follow.

| Segment | Before | After |
|---|---|---|
| T-B | Runs along y = 6000 | Runs along y = 5400 (moved 600 mm) |
| T-D | Runs along x = 8750 | Runs along x = 8250 (moved 500 mm) |

T-A now ends at y = 5400, and T-C, T-D and T-F now start there.

## Conduit drops

Run 1 showed no conduit clashes, but Navisworks read the conduits as lines, so that test could not see them. A geometry check found conduit legs crossing pipes or ducts at PT-221, PT-321, LSL-513, TCV-411 and TT-422. Those drops were rerouted to clear everything by at least 15 mm.

Navisworks run 2 checks all of these fixes.
