# Configuration Reference

<!-- DOC-NAV:START -->
[Home](../README.md) · [Docs](README.md) · [Start](GETTING_STARTED.md) · [Implement](IMPLEMENTATION_GUIDE.md) · [Architecture](ARCHITECTURE.md) · [Test](TEST_STRATEGY.md) · [Interview](INTERVIEW_PREP.md)
<!-- DOC-NAV:END -->


## .NET SDK

`global.json` selects the .NET 10 SDK family.

```json
{
  "sdk": {
    "version": "10.0.302",
    "rollForward": "latestFeature",
    "allowPrerelease": false
  }
}
```

## Shared build properties

`Directory.Build.props` applies:

- `net10.0`;
- nullable reference types;
- implicit usings;
- warnings as errors;
- latest analysis level;
- deterministic builds;
- NuGet audit.

The WPF project overrides the target framework with `net10.0-windows`.

## Central package versions

`Directory.Packages.props` controls versions for:

- `Microsoft.Data.Sqlite`;
- `Microsoft.NET.Test.Sdk`;
- `xunit.v3`;
- `xunit.runner.visualstudio`.

Do not place package versions directly in individual project files unless there is a documented exception.

## Simulation configuration

`SimulationScenario` controls:

| Property | Purpose |
|---|---|
| `ActiveFault` | Deterministic injected fault |
| `Seed` | Reproducible surface variation |
| `TimeScale` | Simulation execution speed |

## Operation timing

| Setting | Current value | Purpose |
|---|---:|---|
| HMI status interval | 100 ms | Live status updates without blocking UI |
| Stop confirmation timeout | 5 s | Maximum wait for active workflow termination |
| HMI command timeout | 30 s | Bounds an operator command invocation |
| Simulation time scale | Scenario-defined | Speeds deterministic virtual motion |

These are software design targets, not real-time or machinery-safety guarantees.

## XYZ travel

| Axis | Minimum | Maximum | Home |
|---|---:|---:|---:|
| X | 0 mm | 500 mm | 0 mm |
| Y | 0 mm | 400 mm | 0 mm |
| Z | 0 mm | 150 mm | 150 mm |

The same travel values should remain aligned across:

- recipe validation;
- deterministic simulator;
- Digital Twin INI profile;
- documentation;
- tests.

## Runtime files

The console application writes to:

```text
.runtime/operations.db
.runtime/events.jsonl
```

The WPF HMI writes under the current user's local application-data directory.

## Digital Twin profile configuration

The initial profile is located at:

```text
configs/digital-twin/xyz-3axis/
```

Relevant files:

| File | Purpose |
|---|---|
| `machine.ini` | Machine, trajectory, axis, joint, and GUI settings |
| `machine.hal` | Motion feedback loops and simulated I/O |
| `postgui.hal` | Future GUI-created HAL signals |
| `tool.tbl` | Virtual probe tool |

---

<!-- DOC-FOOTER:START -->
[Documentation index](README.md) · [Back to top](#configuration-reference)
<!-- DOC-FOOTER:END -->
