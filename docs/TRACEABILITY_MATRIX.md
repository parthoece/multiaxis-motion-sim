# Traceability Matrix

<!-- DOC-NAV:START -->
[Home](../README.md) · [Docs](README.md) · [Start](GETTING_STARTED.md) · [Implement](IMPLEMENTATION_GUIDE.md) · [Architecture](ARCHITECTURE.md) · [Test](TEST_STRATEGY.md) · [Interview](INTERVIEW_PREP.md)
<!-- DOC-NAV:END -->


| Requirement | Implementation | Verification source | Evidence status |
|---|---|---|---|
| FR-001 | `MachineLifecycleService.InitializeAsync` | SQLite integration test and normal scenario | Automated execution pending |
| FR-002 | State guards and motion adapter | State-machine and application tests | Automated execution pending |
| FR-003 | `DeterministicMotionController.HomeAllAsync` | Application flow and Digital Twin manual matrix | Automated source + manual pending |
| FR-004 | Simulator target validation and Digital Twin limits | Limit scenarios and manual matrix | Partial |
| FR-005 | `InspectionRecipe` and `RecipeValidator` | Recipe tests and normal cycle | Automated execution pending |
| FR-006 | `SafetyInputs` and `VirtualPlcGateway` | Missing-part application test | Automated execution pending |
| FR-007 | `ProbeZAsync` | Normal-cycle application test | Automated execution pending |
| FR-008 | `MeasurementResult` | Boundary and out-of-tolerance tests | Automated execution pending |
| FR-009 | `SqliteOperationsStore` | SQLite integration test | Automated execution pending |
| FR-010 | `JsonLineEventLog` | JSONL integration test | Automated execution pending |
| FR-011 | `SimulationFault` and `SimulationScenario` | Console scenarios and application tests | Partial |
| FR-012 | `MachineControlContext.EnterFaultAsync` | Probe-timeout and cancellation tests | Automated execution pending |
| FR-013 | `MachineLifecycleService.ResetFaultAsync` | Probe and cancellation recovery tests | Automated execution pending |
| FR-014 | `FaultRecoveryPolicy` | Recovery-policy and reset tests | Automated execution pending |
| FR-015 | `MachineCommandGate` | Overlapping-command test | Automated execution pending |
| FR-016 | `ActiveOperationController` and `MachineStopService` | Operator-stop test | Automated execution pending |
| FR-017 | Operator console | CI normal and fault scenarios | Workflow defined |
| FR-018 | WPF HMI | Windows build and manual matrix | Build/manual pending |
| FR-019 | Digital Twin profile | Digital Twin manual matrix | Manual pending |
| FR-020 | Application interfaces and adapter boundaries | Architecture review | Implemented design |
| FR-021 | `MachineStatusService.ObserveAsync` | Status-stream application test | Automated execution pending |
| FR-022 | `ActiveOperationController` and `MachineStopService` | Operator-stop application test | Automated execution pending |
| FR-023 | `FaultRecoveryPolicy.GetRecoveryTarget` | Recovery-policy and reset tests | Automated execution pending |
| FR-024 | `MachineControlContext.EnterFaultAsync` and warnings | Secondary PLC and logging failure tests | Automated execution pending |
| QR-001 | Project references and internal application components | Solution review | Implemented |
| QR-002 | Seeded surface model | Normal and repeated scenario execution | Automated execution pending |
| QR-003 | Failure scenario document and tests | Fault tests | Partial |
| QR-004 | Domain and workflow test projects | Test source | Implemented source |
| QR-005 | SQLite transaction in `SaveCycleAsync` | SQLite integration test | Automated execution pending |
| QR-006 | Navigation generator and docs checker | `scripts/check_docs.py` | Verified locally |
| QR-007 | Linux CI workflow | `.github/workflows/ci.yml` | Public run pending |
| QR-008 | Windows workflow | `.github/workflows/windows-hmi.yml` | Public run pending |
| QR-009 | Scope and evidence labels | README and documentation | Implemented |
| QR-010 | Technology rationale and contribution gate | Documentation and PR template | Implemented |
| QR-011 | Internal application services | Source accessibility review | Implemented |
| QR-012 | Best-effort JSONL diagnostics | Diagnostic-log failure test | Automated execution pending |

---

<!-- DOC-FOOTER:START -->
[Documentation index](README.md) · [Back to top](#traceability-matrix)
<!-- DOC-FOOTER:END -->
