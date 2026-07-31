# Operator Guide

<!-- DOC-NAV:START -->
[Home](../README.md) · [Docs](README.md) · [Start](GETTING_STARTED.md) · [Implement](IMPLEMENTATION_GUIDE.md) · [Architecture](ARCHITECTURE.md) · [Test](TEST_STRATEGY.md) · [Interview](INTERVIEW_PREP.md)
<!-- DOC-NAV:END -->


This guide covers the software simulator and WPF HMI. It does not authorize operation of physical machinery.

## Machine states

| State | Operator meaning |
|---|---|
| `Off` | Application has not initialized the controller |
| `Initializing` | Controller, storage, and permissives are being checked |
| `NotHomed` | Motion reference is not established |
| `Homing` | Z, X, and Y are being referenced |
| `Ready` | Machine can accept a valid automatic cycle |
| `Manual` | Manual operation is active |
| `Automatic` | Inspection recipe is running |
| `Paused` | Automatic execution is suspended |
| `Faulted` | Operation is blocked by an alarm |
| `Recovering` | Permissives and recovery rules are being evaluated |

## Normal workflow

1. Start the application.
2. Select **Initialize**.
3. Confirm the state becomes `NotHomed`.
4. Select **Home all**.
5. Confirm the state becomes `Ready`.
6. Select **Run inspection**.
7. Review the cycle result and individual measurements.
8. Confirm the system returns to `Ready`.

## Stop an active operation

1. Select **Stop** while homing or automatic motion is active.
2. The application cancels the active workflow.
3. The motion adapter receives a stop command.
4. The application waits for operation completion.
5. The machine enters `Faulted` with `OperationCancelled`.
6. Select **Reset fault**.
7. The deterministic simulator returns to `Ready`.

Stop is disabled when there is no active motion-capable workflow.

## Fault workflow

1. Arm a deterministic fault.
2. Start the affected operation.
3. Confirm the system enters `Faulted`.
4. Review the active alarm.
5. Clear the injected fault.
6. Select **Reset fault**.
7. Rehome if the alarm invalidated position.
8. Rerun the cycle.

## Operator rules

- Do not start automatic motion before homing.
- Do not assume clearing a fault automatically makes the machine ready; recovery may target `Ready`, `NotHomed`, or `Off`.
- Do not ignore an alarm that requires rehoming.
- Do not edit runtime database files while the application is running.
- Preserve `.runtime/events.jsonl` when reporting a defect.
- Treat every result as simulation evidence only.

## Console scenarios

```bash
dotnet run --project src/MotionControl.OperatorConsole -- normal
dotnet run --project src/MotionControl.OperatorConsole -- operator-stop
dotnet run --project src/MotionControl.OperatorConsole -- probe-timeout
dotnet run --project src/MotionControl.OperatorConsole -- part-missing
dotnet run --project src/MotionControl.OperatorConsole -- estop
dotnet run --project src/MotionControl.OperatorConsole -- plc-loss
dotnet run --project src/MotionControl.OperatorConsole -- out-of-tolerance
```

See [Failure Scenarios](FAILURE_SCENARIOS.md) for expected behavior.

---

<!-- DOC-FOOTER:START -->
[Documentation index](README.md) · [Back to top](#operator-guide)
<!-- DOC-FOOTER:END -->
