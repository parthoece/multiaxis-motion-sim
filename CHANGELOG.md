# Changelog

<!-- DOC-NAV:START -->
[Home](README.md) · [Docs](docs/README.md) · [Start](docs/GETTING_STARTED.md) · [Implement](docs/IMPLEMENTATION_GUIDE.md) · [Architecture](docs/ARCHITECTURE.md) · [Test](docs/TEST_STRATEGY.md) · [Interview](docs/INTERVIEW_PREP.md)
<!-- DOC-NAV:END -->


All notable changes are documented here.

## [Unreleased]

### Added

- Rewritten industry problem and simulation-only project scope.
- .NET 10 domain, application, simulation, persistence, console, and WPF projects.
- Explicit equipment-state machine and command serialization.
- Deterministic XYZ motion and virtual PLC simulation.
- Inspection recipes, probing, measurements, alarms, and recovery.
- SQLite operational history and JSON Lines diagnostic events.
- Unit and integration tests.
- LinuxCNC XYZ simulation profile and G-code examples.
- Engineering requirements, architecture, traceability, and acceptance documentation.
- Open-source governance, support, security, citation, ownership, and CI files.

---

---


- Role-based documentation reading paths and generated previous/next navigation.
- Detailed file-by-file implementation guide.
- Mid-level job-portfolio scorecard and improvement priorities.
- Interview-preparation guide with project pitches, technical questions, trade-offs, and resume bullets.

---


- Split application orchestration into lifecycle, inspection, stop, and status services.
- Added operation-scoped cancellation and stop confirmation.
- Added continuous status streaming and observable intermediate simulator positions.
- Added explicit recovery targets for Ready, NotHomed, and Off.
- Preserved primary faults when diagnostic or PLC output actions fail.
- Upgraded the WPF HMI with state-aware commands, live safety and motion status, warnings, and measurements.
- Added cancellation, recovery, status, fault-isolation, tolerance, SQLite, and JSONL test coverage.
- Corrected SQLite initialization ordering before the first persisted transition.
- Added ADR 0005 for operation lifecycle and fault isolation.

---


- Updated the pinned .NET 10 SDK feature band to 10.0.302.

- Added CI-enforced architecture boundary validation.

---


- Added an executable operator-stop cancellation and recovery scenario to the console and CI.

---

<!-- DOC-FOOTER:START -->
[Documentation index](docs/README.md) · [Back to top](#changelog)
<!-- DOC-FOOTER:END -->
