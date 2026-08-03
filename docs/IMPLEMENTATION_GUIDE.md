# Implementation Guide

<!-- DOC-NAV:START -->
[Home](../README.md) · [Docs](README.md) · [Start](GETTING_STARTED.md) · [Implement](IMPLEMENTATION_GUIDE.md) · [Architecture](ARCHITECTURE.md) · [Test](TEST_STRATEGY.md) · [Interview](INTERVIEW_PREP.md)
<!-- DOC-NAV:END -->



> **Publication note:** Keep the root README concise. Detailed recovery tables, grblHAL build instructions, implementation phases, evidence requirements, and future-adapter work belong in this guide or their dedicated technical documents.

This guide provides the recommended implementation order, the files to read, the code to change, the tests to add, and the evidence required at each stage.

Do not attempt to implement the WPF interface, Digital Twin adapter, and advanced machine profiles simultaneously. Complete each vertical workflow before adding another integration boundary.

## Implementation workflow

For every feature:

1. Identify the requirement.
2. Define normal behavior.
3. Define at least one failure behavior.
4. Implement the domain rule.
5. Implement the application workflow.
6. Implement or update adapters.
7. Add automated tests.
8. Update traceability and documentation.
9. Capture executable evidence.

```text
Requirement
    ↓
Domain invariant
    ↓
Application use case
    ↓
Adapter implementation
    ↓
Automated verification
    ↓
Demonstration evidence
```


## Current implementation status

| Area | Status |
|---|---|
| Domain state and recipe rules | Implemented |
| Explicit recovery-target policy | Implemented |
| Focused lifecycle and inspection services | Implemented |
| Command overlap rejection | Implemented |
| Operation-scoped cancellation | Implemented |
| Operator stop and completion confirmation | Implemented |
| Primary-fault and secondary-warning isolation | Implemented |
| Continuous machine-status streaming | Implemented |
| Observable deterministic motion | Implemented |
| WPF state-aware commands and measurement table | Implemented baseline; additional screens and recorded verification remain |
| grblHAL TCP motion backend | Implemented baseline; upstream simulator is built separately |
| Expanded automated test source | Implemented; CI execution remains the authoritative verification |
| SQLite runtime persistence | Implemented |
| SQLite migrations, history queries, and diagnostic export | Planned |
| Alarm-history and I/O-monitor HMI screens | Planned |
| Digital Twin adapter | Planned |

## Phase 0 — Establish a verified baseline

### Goal

Confirm the repository restores, builds, tests, and runs before changing behavior.

### Read first

- [`GETTING_STARTED.md`](GETTING_STARTED.md)
- [`CONFIGURATION_REFERENCE.md`](CONFIGURATION_REFERENCE.md)
- [`TEST_STRATEGY.md`](TEST_STRATEGY.md)

### Commands

```bash
dotnet --info
dotnet restore
dotnet build src/MotionControl.OperatorConsole --configuration Release
dotnet test tests/MotionControl.Domain.Tests --configuration Release
dotnet test tests/MotionControl.Application.Tests --configuration Release
dotnet test tests/MotionControl.IntegrationTests --configuration Release
dotnet run --project src/MotionControl.OperatorConsole -- normal
python scripts/check_docs.py
```

### Evidence

Store in a release or pull-request description:

- operating system;
- .NET SDK version;
- commit hash;
- test summary;
- normal-scenario output;
- known warnings.

### Definition of done

- Restore succeeds.
- Core projects compile.
- All existing tests pass.
- Normal scenario completes.
- `.runtime/operations.db` and `.runtime/events.jsonl` are created.

## Phase 1 — Understand and protect the domain

### Status

Implemented.

### Relevant files

```text
src/MotionControl.Domain/
├── MachineStateMachine.cs
├── MachineState.cs
├── FaultRecoveryPolicy.cs
├── SafetyInputs.cs
├── InspectionRecipe.cs
├── RecipeValidator.cs
├── AlarmRecord.cs
├── MeasurementResult.cs
└── CycleReport.cs
```

### Implemented behavior

- Inclusive lower and upper tolerance boundaries
- Startup transition from `Off` to `Faulted`
- Explicit recovery target of `Ready`, `NotHomed`, or `Off`
- Recipe range and tolerance validation
- Domain independence from adapters and presentation

### Verification source

```text
tests/MotionControl.Domain.Tests/
├── MachineStateMachineTests.cs
├── RecipeValidatorTests.cs
├── MeasurementResultTests.cs
└── FaultRecoveryPolicyTests.cs
```

### Remaining improvement

Add exhaustive theory data for every state transition and every fault-code recovery target after the first CI run confirms the baseline.

## Phase 2 — Split application workflows into use cases

### Status

Implemented while preserving the public `MachineCoordinator` facade.

### Current structure

```text
src/MotionControl.Application/
├── Common/
│   ├── ActiveOperationController.cs
│   ├── MachineCommandGate.cs
│   ├── MachineControlContext.cs
│   ├── MachineRuntime.cs
│   ├── OperationalWarning.cs
│   └── PlcOutputPolicy.cs
├── Lifecycle/
│   └── MachineLifecycleService.cs
├── Inspection/
│   └── InspectionCycleService.cs
├── Status/
│   └── MachineStatusService.cs
├── Stop/
│   └── MachineStopService.cs
└── MachineCoordinator.cs
```

### Responsibility split

- Lifecycle owns initialize, home, reset, and recovery transitions.
- Inspection owns recipe validation, movement, probing, evaluation, and cycle persistence.
- Stop owns active-token cancellation, adapter stop, and completion confirmation.
- Status owns snapshots and asynchronous observation.
- Common components own runtime state, command exclusion, fault isolation, and PLC output mapping.
- `MachineCoordinator` remains a stable facade for presentation applications.

### Definition of done

- Presentation code calls one stable facade.
- Use-case classes do not contain unrelated workflows.
- Internal implementation classes do not expand the public API.
- Application tests run without WPF or SQLite.

## Phase 3 — Implement reliable cancellation and stop

### Status

Implemented.

### Current design

```text
Start command
→ acquire command gate
→ create linked active-operation token
→ execute use case
→ operator Stop cancels active token
→ motion adapter receives Stop
→ use case handles OperationCanceledException
→ machine enters Faulted with OperationCancelled
→ active-operation scope completes
→ Stop confirms termination
```

### Implemented safeguards

- Stop bypasses the state-changing command gate.
- Cancellation callbacks are not invoked while holding the operation-controller lock.
- Stop waits up to five seconds for active-operation completion.
- Cancellation does not persist a completed cycle.
- Recovery policy returns an operator-cancelled simulation to `Ready`.

### Verification source

`OperatorStopCancelsCycleAndRecoveryReturnsToReady` in the application tests.

### Remaining improvement

Define adapter-specific stop-confirmation criteria for the future Digital Twin implementation.

## Phase 4 — Harden fault handling

### Status

Core isolation implemented.

### Current fault-handling order

1. Capture the primary fault.
2. Stop motion best effort.
3. enter `Faulted` internally;
4. install the active alarm;
5. persist the fault transition best effort;
6. persist the primary alarm best effort;
7. write the diagnostic event best effort;
8. write fault outputs best effort;
9. retain secondary failures as operational warnings.

### Implemented scenarios

- Probe timeout followed by PLC alarm-output failure
- JSONL diagnostic sink unavailable during a normal cycle
- Startup failure can enter `Faulted`
- Repeated faults while already faulted preserve valid state-machine behavior

### Remaining scenarios

- SQLite unavailable while entering a fault
- stop adapter failure during fault handling
- failed alarm acknowledgement persistence
- maximum warning retention and warning export

### Recovery targets

Reset never restarts motion automatically.

| Fault condition | Recovery target |
|---|---|
| Missing part | `Ready` |
| Air pressure unavailable | `Ready` |
| Operator cancellation with the deterministic backend | `Ready` |
| External-controller abort | `NotHomed` |
| Probe timeout | `NotHomed` |
| Homing failure | `NotHomed` |
| Software-limit violation | `NotHomed` |
| E-stop activation | `NotHomed` |
| Motion controller unavailable | `Off` |
| Unexpected startup failure | `Off` |

A controller adapter may apply a stricter target when its reported position can no longer be trusted.

## Phase 5 — Complete the deterministic virtual plant

### Status

Core model implemented.

### Current behavior

- Deterministic surface variation from coordinates and seed
- Configurable execution time scale
- Cancellable motion
- Observable intermediate positions
- Z-X-Y homing
- virtual software limits;
- probe timeout, pre-active probe, positive limit, and out-of-tolerance scenarios;
- virtual PLC permissives and communication loss.

### Remaining improvements

- Separate surface geometry into its own component.
- Add stale-input timestamps when a protocol adapter is introduced.
- Add adapter contract tests shared by simulator and Digital Twin.
- Add configurable probe timing only when a requirement needs it.

### Constraint

Do not add detailed physics unless a software requirement depends on that behavior.

## Phase 6 — Strengthen persistence and diagnostics

### Goal

Make a fault diagnosable without attaching a debugger.

### Tasks

1. Add explicit schema migrations.
2. Add repository queries for cycle and alarm history.
3. Add a diagnostic-export service.
4. Include operation, cycle, recipe, and correlation identifiers.
5. Add retention and database-backup guidance.
6. Test disk and database failures.

### Diagnostic package

```text
diagnostics-<timestamp>/
├── summary.json
├── events.jsonl
├── active-alarm.json
├── machine-snapshot.json
├── recipe.json
└── database-copy.db
```

### Definition of done

- A reviewer can reconstruct the state sequence.
- A cycle and its measurements are transactionally consistent.
- Export excludes credentials and personal information.
- Persistence failures have defined behavior.

## Phase 7 — Complete the WPF operator workflow

### Status

Partially implemented.

### Implemented

- State-aware command enablement
- Continuous status observation
- Live XYZ and moving state
- Safety-permissive summary
- Active alarm display
- Operational-warning count
- Latest measurement table
- Simulation-only probe-timeout controls
- Clean shutdown of status monitoring

### Remaining screens

```text
HMI/
├── Alarm History
├── Recipe Details and Selection
├── I/O Monitor
└── Diagnostic Export
```

### Remaining verification

- Add Windows-only view-model tests.
- Complete the WPF manual test matrix.
- Record normal, stop, fault, and recovery demonstrations.
- Verify accessibility, keyboard operation, and resizing.

### Public proof capture

Capture at least one screenshot and one short GIF:

```text
docs/assets/
├── hmi-overview.png
└── hmi-demo.gif
```

The GIF should show:

1. initialization and Z-X-Y homing;
2. a successful five-point inspection;
3. measurement and cycle-result visibility;
4. probe-timeout injection;
5. cancellation and preserved primary alarm;
6. reset, rehoming, and return to `Ready`.

Do not publish a generated mockup as runtime evidence. Crop usernames, local paths, tokens, and unrelated desktop content from the capture.


## Phase 8 — Validate the grblHAL software backend

### Status

Implemented baseline; repeatable transport and contract-test coverage remain.

### Goal

Validate the controller-protocol boundary without claiming physical-machine behavior.

### Runtime path

```text
WPF HMI
    ↓
MachineCoordinator
    ↓
IMotionController
    ↓
GrblHalMotionController
    ↓ TCP 127.0.0.1:23000
grblHAL Simulator
```

### Genuine and modeled behavior

| Behavior | Source |
|---|---|
| G-code parsing and planning | grblHAL Simulator |
| XYZ movement execution | grblHAL Simulator |
| Controller state and `MPos` reports | grblHAL Simulator |
| Status query, feed hold, and soft reset | grblHAL real-time protocol |
| Home-switch activation | Modeled by the .NET adapter |
| Probe contact | Modeled by the .NET adapter |
| Machine workflow and recovery | .NET application and domain rules |
| Persistence and diagnostics | SQLite and JSON Lines |

### Prepare the simulator

The executable is not committed to the repository. Build the upstream grblHAL Simulator separately and place the Windows executable at:

```text
tools/grblhal-sim/bin/grblHAL_sim.exe
```

Example MSYS2 UCRT64 build:

```bash
pacman -S --needed \
    git \
    mingw-w64-ucrt-x86_64-gcc \
    mingw-w64-ucrt-x86_64-cmake \
    mingw-w64-ucrt-x86_64-ninja

mkdir -p /c/src
cd /c/src

git clone --recurse-submodules \
    https://github.com/grblHAL/Simulator.git \
    grblhal-simulator

cd grblhal-simulator
git submodule update --init --recursive

cmake -S . -B build -G Ninja -DCMAKE_BUILD_TYPE=Release
cmake --build build --parallel
```

Copy the executable:

```powershell
New-Item -ItemType Directory -Force `
    ".\tools\grblhal-sim\bin"

Copy-Item `
    "C:\src\grblhal-simulator\build\grblHAL_sim.exe" `
    ".\tools\grblhal-sim\bin\grblHAL_sim.exe" `
    -Force
```

Pin a tested upstream tag or commit in the release evidence. The executable and `EEPROM.DAT` must remain ignored by Git.

### Smoke test

Start the simulator:

```powershell
.\tools\grblhal-sim\bin\grblHAL_sim.exe -p 23000
```

Run the protocol check in another terminal:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass `
    -File scripts/Test-GrblHalSimulator.ps1
```

The check should verify TCP connectivity, startup identification, `$I`, real-time `?` status, an `Idle` report, and XYZ `MPos`.

### Run the HMI with grblHAL

```powershell
$env:MOTION_BACKEND = "grblhal"
$env:GRBLHAL_HOST = "127.0.0.1"
$env:GRBLHAL_PORT = "23000"

dotnet run --project src/MotionControl.Hmi.Wpf
```

### Validation boundary

This phase validates:

- TCP communication with the controller core;
- G-code command formatting and acknowledgement;
- controller-state and `MPos` parsing;
- motion-completion monitoring;
- feed-hold and reset behavior;
- backend selection;
- integration with machine states, persistence, diagnostics, and the WPF HMI.

It does not validate electrical switches or probes, microcontroller step timing, motors, drives, encoders, mechanics, collision behavior, positioning accuracy, functional safety, or machinery compliance.

### Remaining verification

- Pin and record the tested upstream simulator revision.
- Add fake-transport tests for partial, delayed, malformed, and disconnected responses.
- Add shared `IMotionController` contract tests.
- Define reconnect and position-trust behavior.
- Record a grblHAL-backed HMI demonstration.


## Phase 9 — Verify Digital Twin independently

### Goal

Prove the Digital Twin machine profile before connecting it to .NET.

### Required evidence

- Linux distribution and version;
- Digital Twin version;
- profile commit;
- successful launch;
- homing order;
- XYZ jog;
- soft limits;
- rectangle program;
- E-stop behavior;
- probe-input limitation.

Use [`../tests/manual/DIGITAL_TWIN_TEST_MATRIX.md`](../tests/manual/DIGITAL_TWIN_TEST_MATRIX.md).

### Definition of done

- All claimed Digital Twin features have recorded evidence.
- Version-specific changes are documented.
- Unimplemented probing is clearly labeled.

## Phase 10 — Implement the Digital Twin adapter

### Goal

Replace the in-process motion simulator without changing Domain or use-case rules.

### Adapter responsibilities

- connection lifecycle;
- status observation;
- initialization;
- homing;
- absolute movement;
- program execution;
- probe result;
- stop;
- timeout;
- communication-loss mapping.

### Integration tests

Use a Digital Twin simulation environment to verify:

- adapter startup;
- command/status agreement;
- stop response;
- lost connection;
- Digital Twin alarm mapping;
- normal inspection path.

### Definition of done

- Domain and Application projects remain unchanged.
- Adapter failure maps to known fault codes.
- Both simulator and Digital Twin satisfy the same contract tests.

## Phase 11 — Produce portfolio evidence

### Required evidence set

```text
portfolio/
├── architecture-diagram.svg
├── hmi-demo.gif
├── hmi-overview.png
├── normal-cycle-demo.mp4
├── probe-timeout-demo.mp4
├── grblhal-smoke-test.txt
├── test-results.txt
├── digital-twin-evidence/
├── screenshots/
└── release-notes.md
```

### Demonstration order

1. Explain the commissioning problem.
2. Show architecture boundaries.
3. Run initialization and homing.
4. Run a successful inspection.
5. Inspect stored measurements.
6. Inject a probe timeout.
7. Show fault entry and diagnostics.
8. Recover deliberately.
9. Explain Digital Twin migration.

### README publication gate

Before replacing the illustrative README overview with runtime media:

- commit `docs/assets/hmi-demo.gif` or `docs/assets/hmi-overview.png`;
- confirm the media was captured from the current release commit;
- record the operating system, .NET SDK, selected backend, simulator revision, and commit hash;
- verify that the README status table matches this guide;
- run documentation and architecture checks;
- confirm every badge and relative link resolves;
- confirm no generated database, simulator executable, `EEPROM.DAT`, credentials, usernames, or local paths are committed.

### Definition of done

A reviewer can understand the problem, architecture, executable behavior, failure handling, evidence, and limitations in less than ten minutes.

---

<!-- DOC-FOOTER:START -->
[← Previous: State Machine](STATE_MACHINE.md) · [Documentation index](README.md) · [Next: Simulation Model →](SIMULATION_MODEL.md) · [Back to top](#implementation-guide)
<!-- DOC-FOOTER:END -->
