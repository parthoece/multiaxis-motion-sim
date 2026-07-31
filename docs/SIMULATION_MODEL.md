# Simulation Model

<!-- DOC-NAV:START -->
[Home](../README.md) · [Docs](README.md) · [Start](GETTING_STARTED.md) · [Implement](IMPLEMENTATION_GUIDE.md) · [Architecture](ARCHITECTURE.md) · [Test](TEST_STRATEGY.md) · [Interview](INTERVIEW_PREP.md)
<!-- DOC-NAV:END -->


## Purpose

The simulator models software-observable machine behavior, not mechanical physics.

## XYZ profile

| Axis | Minimum | Maximum | Home |
|---|---:|---:|---:|
| X | 0 mm | 500 mm | 0 mm |
| Y | 0 mm | 400 mm | 0 mm |
| Z | 0 mm | 150 mm | 150 mm |

Homing order is Z, X, Y.

## Time model

Motion duration is calculated from Euclidean distance and requested velocity, then multiplied by a configurable time scale.

Each move is divided into deterministic intermediate steps. This allows:

- continuous status observation;
- visible position progress in the HMI;
- cancellation during travel;
- reliable stop tests;
- fast execution without changing ordering.

The model does not claim servo-loop timing or physical trajectory fidelity.

## Stop and cancellation model

The active application workflow owns a linked cancellation token. Operator Stop cancels that token and calls the motion adapter's stop method.

The simulator's pending delays and movement loop observe the token, so cancelled motion does not continue to its target. The stop service then waits for workflow completion.

## Surface model

The virtual workpiece surface is nominally 10 mm with a deterministic coordinate-based variation. The same seed and inspection point produce the same measurement.

The out-of-tolerance scenario adds a fixed offset so failure is reproducible.

## Simplifications

The simulator does not model:

- acceleration and jerk;
- servo-loop dynamics;
- following error;
- backlash;
- structural compliance;
- sensor noise over time;
- collision geometry;
- thermal effects.

Add a model only when a software requirement depends on it.

---

<!-- DOC-FOOTER:START -->
[← Previous: Implementation Guide](IMPLEMENTATION_GUIDE.md) · [Documentation index](README.md) · [Next: Plc Io Map →](PLC_IO_MAP.md) · [Back to top](#simulation-model)
<!-- DOC-FOOTER:END -->
