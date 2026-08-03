# Requirements

<!-- DOC-NAV:START -->
[Home](../README.md) · [Docs](README.md) · [Start](GETTING_STARTED.md) · [Implement](IMPLEMENTATION_GUIDE.md) · [Architecture](ARCHITECTURE.md) · [Test](TEST_STRATEGY.md) · [Interview](INTERVIEW_PREP.md)
<!-- DOC-NAV:END -->


## Functional requirements

| ID | Requirement |
|---|---|
| FR-001 | The system shall initialize the controller and verify startup permissives. |
| FR-002 | The system shall require homing before coordinated automatic motion. |
| FR-003 | The XYZ profile shall home Z before X and Y. |
| FR-004 | The system shall enforce XYZ software travel limits. |
| FR-005 | The system shall execute versioned inspection recipes. |
| FR-006 | The system shall verify part-present and safety permissives before cycle start. |
| FR-007 | The system shall perform deterministic virtual Z probing. |
| FR-008 | The system shall evaluate measurements against lower and upper limits. |
| FR-009 | The system shall persist cycles, measurements, alarms, and state transitions. |
| FR-010 | The system shall create structured diagnostic events. |
| FR-011 | The system shall inject deterministic motion, probe, PLC, and permissive faults. |
| FR-012 | The system shall stop motion and enter `FAULTED` for unrecoverable cycle faults. |
| FR-013 | The system shall require deliberate alarm acknowledgement and recovery. |
| FR-014 | The system shall require rehoming after faults marked as position-invalidating. |
| FR-015 | The system shall reject overlapping machine commands. |
| FR-016 | The system shall support cancellation of an active cycle. |
| FR-017 | The system shall expose a cross-platform console demo. |
| FR-018 | The system shall expose a Windows operator HMI. |
| FR-019 | The repository shall include an independent Digital Twin XYZ simulation profile. |
| FR-020 | New machine profiles shall not require changes to equipment-domain rules. |
| FR-021 | The system shall stream machine status without blocking the operator interface. |
| FR-022 | Operator stop shall cancel the active workflow and wait for completion confirmation. |
| FR-023 | Recovery shall select `Ready`, `NotHomed`, or `Off` according to the fault policy. |
| FR-024 | A secondary logging or PLC-output failure shall not replace the primary machine fault. |

## Quality requirements

| ID | Requirement |
|---|---|
| QR-001 | Domain code shall not depend on GUI, persistence, Digital Twin, or simulation implementations. |
| QR-002 | Simulation results shall be reproducible with the same scenario and seed. |
| QR-003 | Every fault scenario shall define trigger, detection, response, and recovery. |
| QR-004 | Core state transitions and recipes shall have automated tests. |
| QR-005 | SQLite writes for a completed cycle shall be transactional. |
| QR-006 | Documentation shall contain navigation and valid local links. |
| QR-007 | CI shall build, test, run a normal scenario, and verify an expected fault. |
| QR-008 | Windows CI shall build the WPF HMI. |
| QR-009 | The repository shall distinguish simulation evidence from physical validation. |
| QR-010 | Dependencies shall have a direct requirement and failure case. |
| QR-011 | Internal application implementation classes shall not unnecessarily expand the public API. |
| QR-012 | Diagnostic-event failure shall be reported as an operational warning when control behavior can continue safely. |

---

<!-- DOC-FOOTER:START -->
[← Previous: Problem Statement](PROBLEM_STATEMENT.md) · [Documentation index](README.md) · [Next: Architecture →](ARCHITECTURE.md) · [Back to top](#requirements)
<!-- DOC-FOOTER:END -->
