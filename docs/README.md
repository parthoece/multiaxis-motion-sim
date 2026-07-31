# Documentation

<!-- DOC-NAV:START -->
[Home](../README.md) · [Docs](README.md) · [Start](GETTING_STARTED.md) · [Implement](IMPLEMENTATION_GUIDE.md) · [Architecture](ARCHITECTURE.md) · [Test](TEST_STRATEGY.md) · [Interview](INTERVIEW_PREP.md)
<!-- DOC-NAV:END -->


## Choose a reading path

### Run the project

1. [Getting Started](GETTING_STARTED.md)
2. [Operator Guide](OPERATOR_GUIDE.md)
3. [Failure Scenarios](FAILURE_SCENARIOS.md)
4. [Troubleshooting](TROUBLESHOOTING.md)

### Understand the engineering design

1. [Problem Statement](PROBLEM_STATEMENT.md)
2. [Requirements](REQUIREMENTS.md)
3. [Architecture](ARCHITECTURE.md)
4. [Domain Model](DOMAIN_MODEL.md)
5. [State Machine](STATE_MACHINE.md)
6. [Technology Rationale](TECHNOLOGY_RATIONALE.md)

### Implement and extend the project

1. [Developer Guide](DEVELOPER_GUIDE.md)
2. [Implementation Guide](IMPLEMENTATION_GUIDE.md)
3. [Test Strategy](TEST_STRATEGY.md)
4. [Traceability Matrix](TRACEABILITY_MATRIX.md)
5. [Development Plan](DEVELOPMENT_PLAN.md)
6. [Acceptance Criteria](ACCEPTANCE_CRITERIA.md)

### Prepare for applications and interviews

1. [Portfolio Review](PORTFOLIO_REVIEW.md)
2. [Interview Preparation](INTERVIEW_PREP.md)
3. [Architecture Decisions](adr/README.md)
4. [Motion-Control Terms](MOTION_CONTROL_TERMS.md)

## Status labels

| Label | Meaning |
|---|---|
| **Implemented** | Source code exists |
| **Automated verification pending** | Tests exist but still need execution in .NET CI |
| **Manual verification pending** | Configuration or UI behavior requires recorded execution |
| **Planned** | Design exists but implementation is incomplete |
| **Physical validation required** | Cannot be proven by this simulation repository |

## Product and engineering documents

| Document | Purpose |
|---|---|
| [Getting Started](GETTING_STARTED.md) | Installation and first successful run |
| [Operator Guide](OPERATOR_GUIDE.md) | Normal operation, faults, stop, and recovery |
| [Developer Guide](DEVELOPER_GUIDE.md) | Architecture rules and contribution workflow |
| [Implementation Guide](IMPLEMENTATION_GUIDE.md) | File-by-file implementation order and definitions of done |
| [Configuration Reference](CONFIGURATION_REFERENCE.md) | SDK, packages, travel, runtime, and LinuxCNC settings |
| [Persistence and Logging](PERSISTENCE_AND_LOGGING.md) | SQLite schema and structured events |
| [Troubleshooting](TROUBLESHOOTING.md) | Common setup and runtime problems |
| [Problem Statement](PROBLEM_STATEMENT.md) | Industry problem, project objective, scope, and non-goals |
| [Requirements](REQUIREMENTS.md) | Functional and quality requirements |
| [Architecture](ARCHITECTURE.md) | Components, dependency direction, cancellation, and runtime flows |
| [Domain Model](DOMAIN_MODEL.md) | States, recipes, alarms, measurements, and recovery policy |
| [State Machine](STATE_MACHINE.md) | Allowed transitions, guards, faults, and recovery targets |
| [Simulation Model](SIMULATION_MODEL.md) | Virtual plant assumptions and deterministic behavior |
| [PLC I/O Map](PLC_IO_MAP.md) | Virtual inputs, outputs, and handshakes |
| [Failure Scenarios](FAILURE_SCENARIOS.md) | Injected faults and expected responses |
| [Test Strategy](TEST_STRATEGY.md) | Unit, application, integration, scenario, and manual testing |
| [Traceability Matrix](TRACEABILITY_MATRIX.md) | Requirements mapped to code and evidence |
| [Acceptance Criteria](ACCEPTANCE_CRITERIA.md) | Definition of a complete release |
| [Development Plan](DEVELOPMENT_PLAN.md) | Completed work and ordered remaining milestones |
| [LinuxCNC Integration](LINUXCNC_INTEGRATION.md) | Separate LinuxCNC profile and adapter roadmap |
| [Technology Rationale](TECHNOLOGY_RATIONALE.md) | Why technologies are included or excluded |
| [Motion-Control Terms](MOTION_CONTROL_TERMS.md) | Common software and controls vocabulary |
| [References](REFERENCES.md) | Industry, runtime, LinuxCNC, and open-source sources |
| [Portfolio Review](PORTFOLIO_REVIEW.md) | Mid-level job-portfolio scorecard and improvement priorities |
| [Interview Preparation](INTERVIEW_PREP.md) | Project pitch, technical questions, trade-offs, and demo checklist |

## Project maintenance

| Document | Purpose |
|---|---|
| [Open-Source Format](OPEN_SOURCE_FORMAT.md) | Community-health and repository structure |
| [GitHub Settings](GITHUB_SETTINGS.md) | Settings that cannot be committed as files |
| [Release Process](RELEASE_PROCESS.md) | Versioning and release evidence |
| [Maintainer Guide](MAINTAINER_GUIDE.md) | Reviews, dependencies, releases, and ownership |
| [Architecture Decisions](adr/README.md) | Recorded trade-offs and decisions |

After adding or renaming documentation:

```bash
python scripts/update_doc_navigation.py
python scripts/check_docs.py
```

---

<!-- DOC-FOOTER:START -->
[Documentation index](README.md) · [Back to top](#documentation)
<!-- DOC-FOOTER:END -->
