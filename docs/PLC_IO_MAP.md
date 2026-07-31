# Virtual PLC I/O Map

<!-- DOC-NAV:START -->
[Home](../README.md) · [Docs](README.md) · [Start](GETTING_STARTED.md) · [Implement](IMPLEMENTATION_GUIDE.md) · [Architecture](ARCHITECTURE.md) · [Test](TEST_STRATEGY.md) · [Interview](INTERVIEW_PREP.md)
<!-- DOC-NAV:END -->


## Inputs to the equipment application

| Logical input | Type | Meaning |
|---|---|---|
| `EmergencyStopReset` | Boolean | E-stop chain is reset |
| `DoorClosed` | Boolean | Access door permissive is satisfied |
| `PartPresent` | Boolean | Workpiece is detected |
| `AirPressureReady` | Boolean | Pneumatic permissive is satisfied |

## Outputs from the equipment application

| Logical output | Type | Meaning |
|---|---|---|
| `MachineReady` | Boolean | Machine can accept an automatic start |
| `CycleActive` | Boolean | Automatic cycle is executing |
| `CycleComplete` | Boolean | Cycle-complete pulse or state |
| `AlarmActive` | Boolean | Unacknowledged machine fault exists |
| `GreenLight` | Boolean | Ready indication |
| `YellowLight` | Boolean | Cycle or attention indication |
| `RedLight` | Boolean | Fault indication |

## Current implementation

`VirtualPlcGateway` implements the application interface in process. A Modbus adapter is not included because no external PLC process is required to solve the current problem.

A Modbus adapter becomes justified when the project needs protocol timing, register mapping, stale-data detection, and reconnection testing against a separate PLC simulator.

---

<!-- DOC-FOOTER:START -->
[← Previous: Simulation Model](SIMULATION_MODEL.md) · [Documentation index](README.md) · [Next: Failure Scenarios →](FAILURE_SCENARIOS.md) · [Back to top](#virtual-plc-io-map)
<!-- DOC-FOOTER:END -->
