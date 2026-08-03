# Portfolio Review

<!-- DOC-NAV:START -->
[Home](../README.md) · [Docs](README.md) · [Start](GETTING_STARTED.md) · [Implement](IMPLEMENTATION_GUIDE.md) · [Architecture](ARCHITECTURE.md) · [Test](TEST_STRATEGY.md) · [Interview](INTERVIEW_PREP.md)
<!-- DOC-NAV:END -->


This scorecard evaluates the repository as evidence for a mid-level automation or equipment-software engineering application.

The score measures demonstrated evidence in the repository, not the candidate's total professional ability.

## Current score

| Area | Weight | Score | Notes |
|---|---:|---:|---|
| Engineering problem and scope | 10 | 9 | Clear industry problem and honest simulation boundary |
| Architecture and modularity | 15 | 14 | Focused use cases, cancellation control, internal runtime, and fault isolation |
| C#/.NET implementation | 15 | 12 | Substantial implementation; authoritative build evidence still required |
| Equipment workflow and recovery | 15 | 13 | Stop, recovery targets, command exclusion, and primary-fault preservation implemented |
| HMI and operator experience | 10 | 7 | Live status, state-aware commands, safety display, warnings, and measurements; more screens pending |
| Testing and traceability | 10 | 8 | Broad reliability test source and traceability; CI execution still pending |
| Motion and Digital Twin evidence | 10 | 5 | Simulator improved; Digital Twin adapter and manual evidence remain incomplete |
| Diagnostics and persistence | 5 | 4 | Transactional SQLite, JSONL, and warning isolation; export and migrations pending |
| Documentation and open source | 5 | 5 | Comprehensive navigation, guides, ADRs, and policies |
| Demonstration and release evidence | 5 | 1 | Public CI, videos, screenshots, and tagged release are still needed |
| **Total** | **100** | **78** | Strong interview-generating portfolio; execution evidence is now the main gap |

This is a provisional repository score. It should not be presented as fully verified until the .NET and Windows workflows pass publicly.

## Interpretation

| Score | Meaning |
|---:|---|
| 90–100 | Exceptional portfolio evidence with verified integration and polished demonstrations |
| 80–89 | Strong mid-level portfolio with complete executable workflows and credible evidence |
| 70–79 | Good interview-generating project with a small number of visible completion gaps |
| 60–69 | Strong foundation, but reviewers must infer too much from documentation and scaffolding |
| Below 60 | Early-stage project or primarily conceptual evidence |

At **78/100**, the repository now demonstrates meaningful mid-level design decisions rather than only architectural intent.

The most important remaining step is no longer another refactor. It is running the code in public CI, fixing any compiler or runtime issues, and publishing concise demonstration evidence.

## Alignment with Taiwan equipment-software roles

The project directly demonstrates:

- C# and .NET equipment software;
- WPF operator-interface foundations;
- motion-control concepts;
- equipment workflows and abnormal handling;
- PLC-style permissives and I/O;
- operational logging and database records;
- automated tests;
- technical documentation;
- architecture and modularization.

Current automation and equipment-software roles also commonly request direct protocol or equipment integration, commissioning/debugging, continuous operator-interface behavior, PLC or motion-controller communication, and complete technical documentation.

The repository should therefore be presented as evidence of software architecture and virtual commissioning, while the missing integration evidence remains explicit.

## Highest-value improvements

### Priority 1 — Execute and publish evidence

- Pass public Linux CI.
- Pass the Windows WPF build.
- Pass CodeQL.
- Tag `v0.1.0`.
- Publish normal, stop/cancellation, and probe-timeout demonstrations.
- Attach test summaries and known limitations.

Expected score gain: **7–10 points**.

### Priority 2 — Complete operator diagnostics

- Alarm-history screen
- Recipe details and selection
- I/O monitor
- Diagnostic export
- Windows-only view-model tests
- Completed WPF manual matrix

Expected score gain: **3–5 points**.

### Priority 3 — Verify and integrate Digital Twin

- Complete the Digital Twin manual matrix.
- Implement `DigitalTwinMotionController`.
- Add shared adapter contract tests.
- Demonstrate normal and failure workflows using Digital Twin simulation.

Expected score gain: **7–10 points**.

### Priority 4 — Harden persistence lifecycle

- SQLite schema versioning and migrations
- backup and rollback guidance;
- retention policy;
- diagnostic package with a consistent database copy.

Expected score gain: **2–3 points**.

## Target score before active applications

The repository is already suitable for selective applications where the project is presented honestly as simulation-first and implementation evidence is shown locally.

A stronger broad-application target is **85/100 or higher**:

- public CI passes;
- normal and fault demonstrations exist;
- Windows HMI is shown running;
- manual WPF evidence is recorded;
- Digital Twin profile is verified;
- known limitations remain explicit.

The Digital Twin adapter can raise the project further, but public executable evidence should come first.

---

<!-- DOC-FOOTER:START -->
[← Previous: Acceptance Criteria](ACCEPTANCE_CRITERIA.md) · [Documentation index](README.md) · [Next: Interview Prep →](INTERVIEW_PREP.md) · [Back to top](#portfolio-review)
<!-- DOC-FOOTER:END -->
