# Developer Guide

<!-- DOC-NAV:START -->
[Home](../README.md) · [Docs](README.md) · [Start](GETTING_STARTED.md) · [Implement](IMPLEMENTATION_GUIDE.md) · [Architecture](ARCHITECTURE.md) · [Test](TEST_STRATEGY.md) · [Interview](INTERVIEW_PREP.md)
<!-- DOC-NAV:END -->


## Solution structure

```text
src/
├── MotionControl.Domain
├── MotionControl.Application/
│   ├── Common
│   ├── Lifecycle
│   ├── Inspection
│   ├── Status
│   └── Stop
├── MotionControl.Simulation
├── MotionControl.Persistence
├── MotionControl.OperatorConsole
└── MotionControl.Hmi.Wpf
```

`MachineCoordinator` is the stable presentation-facing facade. Application implementation classes are internal unless they are intentional ports, support types, or public results.

## Dependency rule

Dependencies point inward:

```text
Presentation → Application → Domain
Adapters     → Application → Domain
```

The Domain project must never reference:

- WPF;
- SQLite;
- Digital Twin;
- simulation implementations;
- file-system or network libraries.

## Adding domain behavior

1. Define the invariant or state rule.
2. Add or modify a domain type.
3. Add unit tests.
4. Update requirements and traceability.
5. Avoid adapter-specific terminology inside the domain.

## Adding a state-changing use case

1. Define the required starting state.
2. Implement the use case in an appropriate Application folder.
3. Execute it through `MachineCoordinator.ExecuteOperationAsync`.
4. Accept and propagate the active operation token.
5. Define cancellation behavior.
6. Map adapter exceptions to fault codes.
7. Add normal, invalid-state, adapter-failure, and cancellation tests.
8. Update requirements and traceability.

Stop is different: it must bypass the command gate so it can cancel active work.

## Adding an application workflow

1. Define the required ports as interfaces.
2. Orchestrate domain behavior in Application.
3. Handle cancellation explicitly.
4. Define fault conversion and recovery.
5. Add application tests using deterministic fakes or simulation adapters.

## Adding an adapter

Adapters implement Application interfaces.

Examples:

- deterministic motion simulator;
- virtual PLC;
- SQLite operations store;
- Digital Twin motion adapter.

An adapter must document:

- connection and initialization;
- timeout behavior;
- cancellation behavior;
- error mapping;
- stale-data behavior;
- recovery expectations.

## Adding a machine profile

Create a new isolated directory:

```text
configs/digital-twin/<profile-name>/
```

Each profile requires:

- profile README;
- INI file;
- HAL files;
- coordinate and joint map;
- travel limits;
- homing sequence;
- known limitations;
- manual test evidence.

## Code-quality expectations

- Nullable reference types remain enabled.
- Compiler warnings are treated as errors.
- Public behavior is tested.
- Async methods accept cancellation where meaningful.
- Commands must not block the HMI thread.
- Exceptions crossing adapter boundaries are mapped to meaningful fault codes.
- Simulation values are deterministic unless a test explicitly controls randomness.

## Automated architecture checks

Run:

```bash
python scripts/check_architecture.py
```

The check enforces:

- Domain has no project references.
- Application references only Domain.
- Simulation and Persistence reference only allowed inward projects.
- Domain source does not use WPF, SQLite, or adapter namespaces.
- Required application use-case directories exist.
- `MachineCoordinator` remains a small facade.

## Local verification

```bash
dotnet restore
dotnet build src/MotionControl.OperatorConsole --configuration Release
dotnet test tests/MotionControl.Domain.Tests --configuration Release
dotnet test tests/MotionControl.Application.Tests --configuration Release
dotnet test tests/MotionControl.IntegrationTests --configuration Release
python scripts/check_docs.py
```

## Pull-request readiness

A change is ready when:

- the requirement is clear;
- implementation boundaries remain intact;
- normal and failure behavior are tested;
- documentation and traceability are updated;
- no physical-performance claim is implied.

---

<!-- DOC-FOOTER:START -->
[Documentation index](README.md) · [Back to top](#developer-guide)
<!-- DOC-FOOTER:END -->
