# Acceptance Criteria

<!-- DOC-NAV:START -->
[Home](../README.md) · [Docs](README.md) · [Start](GETTING_STARTED.md) · [Implement](IMPLEMENTATION_GUIDE.md) · [Architecture](ARCHITECTURE.md) · [Test](TEST_STRATEGY.md) · [Interview](INTERVIEW_PREP.md)
<!-- DOC-NAV:END -->


## `v0.1.0` — Refactored simulation core

### Source-complete criteria

- Domain, Application, Simulation, Persistence, Console, and WPF projects exist.
- Application workflows are split into lifecycle, inspection, stop, and status services.
- `MachineCoordinator` remains a thin presentation-facing facade.
- Active operations own a linked cancellation token.
- Stop cancels and waits for active-operation completion.
- Recovery policy selects `Ready`, `NotHomed`, or `Off`.
- Primary faults remain active when secondary PLC or diagnostic actions fail.
- Status streaming provides live machine snapshots.
- WPF commands are state-aware.
- WPF displays live safety, motion, alarm, warning, and measurement data.
- Domain, application, SQLite, and JSONL test source exists.
- Documentation navigation and local links pass validation.

### Verification criteria before release

- .NET restore succeeds.
- Core projects compile with warnings treated as errors.
- All .NET tests pass.
- Normal console scenario exits zero.
- Probe-timeout scenario exits non-zero and reports the correct alarm.
- Windows HMI workflow builds.
- CodeQL completes.
- No documentation claims physical performance.

## `v0.2.0` — Operator and diagnostics completion

- Alarm-history screen is implemented.
- Recipe-details and selection screen is implemented.
- I/O monitor is implemented.
- Diagnostic-export package is implemented.
- Windows-only view-model tests pass.
- WPF manual matrix is complete.
- Normal, stop, fault, and recovery videos are published.

## `v0.3.0` — HAL-first integration evidence

- grblHAL simulator launch and connection workflow is documented.
- Normal, operator-stop, and probe-fault workflows execute with published evidence.
- WPF manual matrix is complete and attached to the release.
- Shared adapter contract tests pass for deterministic simulator and grblHAL.
- Domain and application rules remain unchanged.

## `v0.4.0` — Optional Digital Twin and hardware extension

- A .NET `DigitalTwinMotionController` implements the motion contract.
- Shared adapter contract tests pass for deterministic simulator, grblHAL, and Digital Twin.
- Digital Twin behavior mapping and known limitations are documented.
- Physical-controller adapter plan is documented with verification strategy.
- Domain and application rules remain unchanged.

---

<!-- DOC-FOOTER:START -->
[← Previous: Development Plan](DEVELOPMENT_PLAN.md) · [Documentation index](README.md) · [Next: Portfolio Review →](PORTFOLIO_REVIEW.md) · [Back to top](#acceptance-criteria)
<!-- DOC-FOOTER:END -->
