# ADR 0005: Operation Lifecycle and Fault Isolation

<!-- DOC-NAV:START -->
[Home](../../README.md) · [Docs](../README.md) · [Start](../GETTING_STARTED.md) · [Implement](../IMPLEMENTATION_GUIDE.md) · [Architecture](../ARCHITECTURE.md) · [Test](../TEST_STRATEGY.md) · [Interview](../INTERVIEW_PREP.md)
<!-- DOC-NAV:END -->


- Status: Accepted
- Date: 2026-07-28

## Context

The original coordinator owned state transitions, inspection execution, stop, recovery, PLC outputs, persistence, and logging.

Operator Stop called the motion adapter but did not own the cancellation token used by the active workflow. A secondary PLC output or diagnostic failure could also mask the original machine fault.

## Decision

- Preserve `MachineCoordinator` as a stable facade.
- Split lifecycle, inspection, stop, and status responsibilities into focused internal services.
- Reject overlapping commands with `MachineCommandGate`.
- Give each active command a linked token owned by `ActiveOperationController`.
- Let Stop cancel the active operation, command the adapter to stop, and await completion.
- Install primary fault state and alarm before attempting secondary actions.
- Treat diagnostic logging and fault-output failures as operational warnings when the primary fault is already established.
- Choose recovery target through `FaultRecoveryPolicy`.

## Consequences

### Positive

- Stop has deterministic ownership of active work.
- Use cases are independently understandable and testable.
- Presentation applications keep one stable API.
- Secondary failures cannot replace the primary machine alarm.
- Status streaming is separated from state-changing commands.
- Internal implementation types do not unnecessarily expand the public API.

### Negative

- More application classes and composition code are required.
- Stop semantics must be implemented consistently by future adapters.
- Best-effort diagnostics require a separate warning-reporting path.
- Recovery policy remains conservative until physical behavior is known.

## Verification

- Operator-stop cancellation test
- Overlapping-command test
- Probe timeout with secondary PLC output failure
- Diagnostic event sink failure during a normal cycle
- Status-stream observation test
- Fault-recovery policy tests

## Revisit conditions

Revisit when:

- Digital Twin adapter semantics require a different stop-confirmation model;
- a real controller cannot guarantee cancellation using the current contract;
- multiple concurrent non-motion operations become necessary;
- warning retention requires persistent storage.

---

<!-- DOC-FOOTER:START -->
[Documentation index](../README.md) · [Back to top](#adr-0005-operation-lifecycle-and-fault-isolation)
<!-- DOC-FOOTER:END -->
