# Domain Model

<!-- DOC-NAV:START -->
[Home](../README.md) · [Docs](README.md) · [Start](GETTING_STARTED.md) · [Implement](IMPLEMENTATION_GUIDE.md) · [Architecture](ARCHITECTURE.md) · [Test](TEST_STRATEGY.md) · [Interview](INTERVIEW_PREP.md)
<!-- DOC-NAV:END -->


## Domain concepts

| Concept | Responsibility |
|---|---|
| `MachineStateMachine` | Allowed machine states and transitions |
| `FaultRecoveryPolicy` | Chooses recovery target: `Ready`, `NotHomed`, or `Off` |
| `InspectionRecipe` | Versioned motion and tolerance parameters |
| `InspectionPoint` | XY location and Z tolerance |
| `MeasurementResult` | Measured value, error, and inclusive pass/fail limits |
| `CycleReport` | Immutable inspection-cycle result |
| `AlarmRecord` | Fault, message, acknowledgement, and rehome indication |
| `SafetyInputs` | E-stop, door, part, and air permissives |
| `PlcOutputs` | Ready, cycle, alarm, and stack-light state |
| `AxisVector` | XYZ position value |

## Application runtime concepts

| Concept | Responsibility |
|---|---|
| `MachineRuntime` | Thread-safe state, alarm, and last-known safety inputs |
| `MachineCommandGate` | Rejects overlapping machine commands |
| `ActiveOperationController` | Owns cancellation and completion for the active workflow |
| `OperationalWarning` | Records a secondary diagnostic or output failure |
| `PlcOutputPolicy` | Maps machine state to logical outputs |

## Application ports

| Interface | Purpose |
|---|---|
| `IMotionController` | Initialize, home, move, probe, stop, and status |
| `IPlcGateway` | Read permissives and write machine outputs |
| `IOperationsStore` | Persist transitions, alarms, and cycles |
| `IOperationEventLog` | Write structured diagnostic events |
| `IClock` | Provide testable time |

The domain does not know whether motion is implemented by an in-process simulator, LinuxCNC, a PLC, or a commercial motion card.

---

<!-- DOC-FOOTER:START -->
[← Previous: Architecture](ARCHITECTURE.md) · [Documentation index](README.md) · [Next: State Machine →](STATE_MACHINE.md) · [Back to top](#domain-model)
<!-- DOC-FOOTER:END -->
