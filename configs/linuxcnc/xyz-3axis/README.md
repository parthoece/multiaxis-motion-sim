# LinuxCNC XYZ Simulation Profile

<!-- DOC-NAV:START -->
[Home](../../../README.md) · [Docs](../../../docs/README.md) · [Start](../../../docs/GETTING_STARTED.md) · [Implement](../../../docs/IMPLEMENTATION_GUIDE.md) · [Architecture](../../../docs/ARCHITECTURE.md) · [Test](../../../docs/TEST_STRATEGY.md) · [Interview](../../../docs/INTERVIEW_PREP.md)
<!-- DOC-NAV:END -->


This profile is an independent LinuxCNC simulation for the initial XYZ machine.

## Mapping

| Joint | Coordinate | Travel |
|---|---|---:|
| 0 | X | 0–500 mm |
| 1 | Y | 0–400 mm |
| 2 | Z | 0–150 mm |

Homing order is Z, X, Y. The starter profile uses immediate homing so it can run without virtual home-switch logic.

## Run

```bash
linuxcnc machine.ini
```

Then:

1. clear E-stop;
2. enable the machine;
3. home all;
4. load `../../../gcode/xyz-3axis/rectangle.ngc`;
5. run the path;
6. complete the manual test matrix.

## Probing

The probing G-code requires `motion.probe-input`. The starter HAL keeps it false. Automatic position-based probe triggering is a later LinuxCNC milestone.

## Status

This profile must be verified on the contributor's LinuxCNC installation. Do not claim it as tested until the manual matrix contains versioned evidence.

---

<!-- DOC-FOOTER:START -->
[Documentation index](../../../docs/README.md) · [Back to top](#linuxcnc-xyz-simulation-profile)
<!-- DOC-FOOTER:END -->
