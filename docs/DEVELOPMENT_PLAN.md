# Development Plan

<!-- DOC-NAV:START -->
[Home](../README.md) · [Docs](README.md) · [Start](GETTING_STARTED.md) · [Implement](IMPLEMENTATION_GUIDE.md) · [Architecture](ARCHITECTURE.md) · [Test](TEST_STRATEGY.md) · [Interview](INTERVIEW_PREP.md)
<!-- DOC-NAV:END -->


## Completed implementation work

### Core architecture

- Modular monolith with ports and adapters
- Focused lifecycle, inspection, stop, and status services
- Thin `MachineCoordinator` facade
- Thread-safe runtime state
- Explicit fault recovery targets
- Internal-only application implementation components

### Reliability

- Command overlap rejection
- Operation-scoped linked cancellation
- Stop confirmation timeout
- Primary-fault preservation
- Secondary operational warnings
- Startup storage ordering correction
- Startup transition from `Off` to `Faulted`

### Simulation and HMI

- Cancellable, observable deterministic motion
- Continuous status streaming
- State-aware HMI commands
- Safety and motion display
- Warning count
- Measurement table
- Clean status-loop shutdown

### Test source

- Tolerance boundaries
- Recovery policy
- normal and failed cycles;
- stop and cancellation;
- overlap rejection;
- secondary PLC failure;
- diagnostic log failure;
- status streaming;
- SQLite and JSONL integration.

## Milestone 0 — Execute the refactored baseline

1. Install .NET 10 SDK.
2. Run `dotnet restore`.
3. Build the console application.
4. Run all test projects.
5. Build the WPF project on Windows.
6. Run normal and fault console scenarios.
7. Correct any compile or runtime defects.
8. Publish the first successful CI evidence.

Evidence: workflow links, terminal output, and test summaries.

## Milestone 1 — Complete operator-facing diagnostics

Implement:

- alarm-history query and screen;
- recipe-details and selection screen;
- I/O monitor;
- operational-warning details;
- diagnostic export;
- Windows-only view-model tests.

Evidence: WPF tests, completed manual matrix, and demonstration recording.

## Milestone 2 — Add schema migrations and retention

Implement:

- explicit SQLite schema version;
- ordered migrations;
- backup before migration;
- alarm and cycle query repositories;
- retention policy;
- diagnostic export of a consistent database copy.

Evidence: migration tests and recovery instructions.

## Milestone 3 — Verify LinuxCNC independently

1. Install LinuxCNC.
2. Launch the XYZ profile.
3. Home and jog.
4. Run rectangle G-code.
5. verify software limits;
6. test E-stop;
7. record the inactive-probe limitation;
8. complete the manual matrix.

Evidence: environment versions, logs, screenshots, and video.

## Milestone 4 — Implement the LinuxCNC adapter

Define a supported communication boundary and implement:

- connection lifecycle;
- status stream;
- initialize;
- home;
- absolute move;
- stop;
- probe result;
- timeout and communication-loss mapping.

Acceptance:

- no Domain changes;
- no equipment-rule changes in Application;
- shared adapter contract tests;
- normal and fault demonstrations.

## Milestone 5 — Add one advanced profile

Choose one business-relevant extension:

- XYZA independent rotary table;
- duplicated-joint gantry;
- XYZAC system with non-trivial kinematics.

Add only after the XYZ workflow and LinuxCNC adapter are verified.

## Milestone 6 — Produce the hiring portfolio release

- Replace `parthoece`.
- Pass public CI, Windows build, and CodeQL.
- Tag a release.
- Publish architecture image.
- Publish normal-cycle video.
- Publish stop/cancellation video.
- Publish probe-timeout and recovery video.
- Include test evidence and known limitations.

---

<!-- DOC-FOOTER:START -->
[← Previous: Test Strategy](TEST_STRATEGY.md) · [Documentation index](README.md) · [Next: Acceptance Criteria →](ACCEPTANCE_CRITERIA.md) · [Back to top](#development-plan)
<!-- DOC-FOOTER:END -->
