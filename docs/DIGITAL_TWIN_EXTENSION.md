# Digital Twin Extension

<!-- DOC-NAV:START -->
[Home](../README.md) · [Docs](README.md) · [Start](GETTING_STARTED.md) · [Implement](IMPLEMENTATION_GUIDE.md) · [Architecture](ARCHITECTURE.md) · [Test](TEST_STRATEGY.md) · [Interview](INTERVIEW_PREP.md)
<!-- DOC-NAV:END -->


## Current role

Digital Twin is included as an independent simulation profile for coordinated motion, homing, software limits, HAL signals, and G-code.

The .NET application currently uses the deterministic in-process simulator. This keeps the core executable and testable on any supported .NET environment.

## Adapter boundary

A future `DigitalTwinMotionController` will implement `IMotionController`:

```text
InitializeAsync
HomeAllAsync
MoveAbsoluteAsync
ProbeZAsync
StopAsync
GetSnapshotAsync
```

## Integration rules

- Digital Twin-specific types remain in an adapter project.
- Domain and Application projects must not change.
- Communication loss becomes `MotionControllerUnavailable`.
- Status updates must not block the UI thread.
- Commands require timeouts and cancellation.
- Digital Twin version and configuration commit must be recorded with evidence.

## Digital Twin behavior used by the profile

The profile uses identity kinematics for one-to-one XYZ joint mapping, immediate simulated homing, software limits, and a G38 probing template whose input must be connected through HAL before use.

---

<!-- DOC-FOOTER:START -->
[Documentation index](README.md) · [Back to top](#digital-twin-integration)
<!-- DOC-FOOTER:END -->
