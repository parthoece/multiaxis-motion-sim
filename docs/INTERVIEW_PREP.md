# Interview Preparation

<!-- DOC-NAV:START -->
[Home](../README.md) · [Docs](README.md) · [Start](GETTING_STARTED.md) · [Implement](IMPLEMENTATION_GUIDE.md) · [Architecture](ARCHITECTURE.md) · [Test](TEST_STRATEGY.md) · [Interview](INTERVIEW_PREP.md)
<!-- DOC-NAV:END -->


This guide helps explain the project accurately in applications, technical interviews, and portfolio demonstrations.

Only describe features that you have implemented and verified. Use the status labels in the repository to distinguish implemented work, planned work, manual verification, and physical validation.

## One-sentence description

> I built a software-in-the-loop virtual commissioning platform that verifies multi-axis equipment workflows, homing, permissives, inspection cycles, alarms, and fault recovery before physical machinery is available.

## 30-second introduction

> Industrial equipment software is often integrated late, when the mechanical and electrical systems are already assembled. That makes software defects expensive and difficult to isolate. I built a simulation-first platform in C# and .NET that separates equipment-domain logic from motion, PLC, persistence, and UI adapters. It can initialize and home a virtual XYZ machine, execute a deterministic inspection recipe, store measurements, inject faults such as probe timeout, and verify controlled recovery without physical hardware.

## Two-minute explanation

> The problem I chose is late validation of industrial machine-control software. Automated equipment combines motion, sensors, operating modes, recipes, alarms, PLC handshakes, and an operator interface. When these behaviors are first tested on unfinished hardware, software defects become mixed with mechanical and electrical problems.
>
> I designed the project as a modular monolith with ports and adapters. Domain contains state rules, recipes, alarms, tolerance behavior, and an explicit recovery policy. Application is split into lifecycle, inspection, stop, and status services behind a stable coordinator facade. The deterministic simulator implements motion and virtual PLC behavior, SQLite stores operational history, and WPF provides the operator interface.
>
> The normal workflow is initialization, Z-X-Y homing, permissive verification, a five-point probing cycle, tolerance evaluation, transactional persistence, and return to Ready. Stop owns an operation-scoped cancellation token, cancels active work, commands the motion adapter to stop, and waits for completion confirmation.
>
> Fault handling preserves the primary equipment fault before attempting diagnostic logging or PLC fault outputs. Secondary failures become warnings instead of replacing the original alarm. The HMI receives continuous status, enables commands according to machine state, and displays safety inputs, motion, alarms, warnings, and measurements.
>
> I intentionally excluded microservices, Docker, MQTT, and other distributed infrastructure because the current business problem involves one local machine. The remaining major milestones are public CI evidence, operator diagnostic screens, Digital Twin verification, and a Digital Twin motion adapter.

## Five-minute technical walkthrough

### 1. Problem

Explain:

- why late machine integration is expensive;
- why abnormal faults are difficult to reproduce on hardware;
- why deterministic simulation improves software verification;
- what simulation cannot validate.

### 2. Architecture

Open [`ARCHITECTURE.md`](ARCHITECTURE.md).

Explain the dependency direction:

```text
Presentation → Application → Domain
Adapters     → Application → Domain
```

Key point:

> The equipment workflow depends on interfaces, so the deterministic simulator can later be replaced by Digital Twin without rewriting state rules or recipes.

### 3. State machine

Open [`STATE_MACHINE.md`](STATE_MACHINE.md).

Explain:

- why automatic mode requires homing;
- why permissives are checked before cycle start;
- why reset does not automatically restart motion;
- which failures invalidate position;
- why recovery passes through a dedicated state.

### 4. Normal cycle

Show:

```text
Initialize
→ Verify startup permissives
→ Home Z, X, Y
→ Enter Ready
→ Validate recipe
→ Verify part present
→ Move and probe five points
→ Evaluate tolerances
→ Save cycle transactionally
→ Return to Ready
```

### 5. Fault cycle

Show probe timeout:

```text
Probe command
→ No trigger
→ Motion stop
→ Fault creation
→ State becomes Faulted
→ Alarm and diagnostics persisted
→ Restart rejected
→ Deliberate recovery
```

### 6. Trade-offs

Explain:

- modular monolith rather than microservices;
- SQLite rather than a server database;
- in-process virtual PLC rather than Modbus in the first release;
- deterministic model rather than high-fidelity physics;
- WPF for equipment HMI and console for cross-platform testing.

### 7. Limitations

State honestly:

- authoritative .NET compilation and test execution still need to pass in public CI;
- WPF behavior has not yet been recorded through the complete manual matrix;
- alarm history, recipe selection, I/O monitor, and diagnostic export are not complete;
- Digital Twin profile requires environment verification;
- .NET and Digital Twin integration is not complete;
- physical accuracy, hardware safety, and machinery compliance remain out of scope.

## Architecture questions and answers

### Why use ports and adapters?

It protects business rules from device-specific APIs. The application can use an in-process simulator during automated tests and a Digital Twin adapter later while preserving the same workflow and state rules.

### Why not use microservices?

There is one machine, one operator application, and local operational data. Network services would add deployment, consistency, and failure-handling complexity without an independent scaling or ownership requirement.

### Why use SQLite?

The current product boundary is one local simulated machine. SQLite supports transactions and portable inspection history without operating a database server. A server database becomes relevant only for concurrent clients or centralized multi-machine data.

### Why use deterministic simulation?

A fault must be repeatable to become a regression test. Deterministic seeds and explicit scenarios make measurements and failures reproducible.

### Why keep Digital Twin separate initially?

It allows the equipment-domain and workflow software to be developed and tested on any .NET machine. Digital Twin can then be integrated through a dedicated adapter after its profile is independently verified.

### Why WPF?

Many PC-based equipment systems run on Windows, and WPF supports desktop HMI patterns, XAML binding, asynchronous commands, and testable view models. The cross-platform console keeps core verification independent of Windows.

## Software-engineering questions and answers

### How do you prevent conflicting commands?

The presentation layer calls a stable coordinator facade. A `MachineCommandGate` rejects overlapping state-changing commands immediately rather than silently queueing them. The active workflow also owns an `ActiveOperationController` scope, which gives Stop a specific operation to cancel and await.

### How do you handle cancellation?

Every state-changing workflow receives a linked operation-scoped cancellation token. Operator Stop bypasses the command gate, cancels that token, calls the motion adapter's stop method, and waits up to five seconds for operation completion. The cancelled use case enters `Faulted` with `OperationCancelled`, does not save a completed cycle, and can recover to `Ready` in the deterministic simulator.

### How do you preserve diagnostic information?

SQLite stores state transitions, alarms, cycles, and measurements. JSON Lines provides portable diagnostic events. The primary fault is installed in runtime state before persistence, event logging, or PLC fault-output actions. If a secondary action fails, the original alarm remains active and the secondary failure becomes an `OperationalWarning`.

### How do you test faults?

Faults are explicit simulation scenarios, not unpredictable random events. Tests activate a scenario such as probe timeout, execute the workflow, and assert the state, alarm, persistence effects, and recovery requirement.

### How do you avoid coupling UI to hardware?

The WPF view model uses application workflows. It does not call Digital Twin, SQLite, or HAL directly. Adapter implementations are composed at startup.

### What would you refactor first?

The coordinator has already been reduced to a thin facade with lifecycle, inspection, stop, and status services. The next refinements would be explicit SQLite migrations, a diagnostic-export service, Windows-only HMI view-model tests, and shared motion-adapter contract tests before implementing Digital Twin integration.

## Motion-control questions and answers

### What is the difference between an axis and a joint?

An axis is a commanded coordinate. A joint is an actuator. A gantry can have one Y axis but two Y joints.

### Why home Z first?

For the conceptual inspection machine, raising or referencing Z first reduces the chance that XY movement drags the probe across the workpiece. This is a machine-specific safety assumption and must be reviewed for each profile.

### Why are software limits valid only after homing?

Software limits depend on a known machine coordinate reference. Before homing, the controller cannot reliably map current position to the configured travel range.

### What does virtual probing validate?

It validates the command sequence, trigger handling, recorded position, timeout logic, tolerance evaluation, alarms, and recovery. It does not validate physical probe repeatability or surface accuracy.

## Behavioral questions using the project

### Tell me about a design trade-off

Use the modular-monolith decision:

> I considered adding services, MQTT, and a server database because they appear in manufacturing job descriptions. I rejected them for the core release because they did not solve the single-machine commissioning problem. I used explicit in-process boundaries so those integrations can be added later when there is a real remote consumer.

### Tell me about preventing a difficult bug

Use deterministic fault injection:

> Probe failures are difficult and potentially risky to reproduce on real equipment. I represented probe timeout as a deterministic scenario and tested the expected stop, fault state, alarm, and recovery requirement. This turns a rare commissioning failure into a repeatable regression test.

### Tell me about an incomplete design

Use the Digital Twin boundary honestly:

> The deterministic simulator and equipment application are implemented, but the Digital Twin profile is still independent and the adapter is not complete. I intentionally separated those milestones so the domain and workflow software could be verified first. Before claiming Digital Twin integration, I will complete the versioned manual profile tests and then make both motion implementations pass the same contract tests.

### Tell me about cross-functional communication

Explain how the repository separates software-verifiable behavior from physical validation, and how the requirements, I/O map, state diagram, and test matrices create shared artifacts for software, controls, electrical, and mechanical engineers.

## Resume bullet examples

Use only after the related features are verified.

- Designed a C#/.NET software-in-the-loop virtual commissioning platform for an XYZ automated inspection machine, separating domain workflows from motion, PLC, persistence, and HMI adapters.
- Implemented deterministic homing, coordinated movement, five-point probing, recipe validation, alarms, fault injection, and transactional SQLite inspection history.
- Built unit and integration tests for equipment state transitions, permissives, abnormal probe behavior, tolerance evaluation, and persistence.
- Developed a WPF operator-interface starter and Digital Twin simulation profile with documented homing, travel limits, G-code, and manual verification plans.
- Established CI, CodeQL, dependency updates, requirements traceability, architecture decisions, and open-source maintenance policies.

## Statements to avoid

Do not say:

- “This is a production-ready machine controller.”
- “The project validates machinery safety.”
- “I implemented EtherCAT, OPC UA, or Modbus” unless those adapters exist.
- “Digital Twin is fully integrated with .NET” before the adapter is complete.
- “The simulator proves physical accuracy.”
- “All code is verified” before CI and manual matrices pass.

Use:

- “simulation-only”;
- “software-in-the-loop”;
- “starter profile”;
- “planned adapter”;
- “physical validation required”;
- “verified by automated test”;
- “manual verification pending.”

## Portfolio demonstration checklist

Before an interview:

- Replace all `parthoece` placeholders.
- Ensure CI and Windows HMI workflows pass.
- Tag a release.
- Record a two- to four-minute normal-cycle demonstration.
- Record a probe-timeout and recovery demonstration.
- Include one architecture diagram.
- Include test results.
- Complete the Digital Twin and WPF manual matrices for any claimed behavior.
- Prepare one design trade-off and one known limitation.
- Practice the 30-second and two-minute explanations.

## Questions to ask the interviewer

- How are equipment state machines and abnormal recovery implemented in your current platform?
- Which motion controllers, PLCs, and industrial protocols are used?
- How much software can be developed before physical equipment is available?
- How are commissioning defects reproduced and converted into regression tests?
- How are HMI, motion, vision, and PLC responsibilities divided?
- What evidence is required before software is released to a production line?

---

<!-- DOC-FOOTER:START -->
[← Previous: Portfolio Review](PORTFOLIO_REVIEW.md) · [Documentation index](README.md) · [Back to top](#interview-preparation)
<!-- DOC-FOOTER:END -->
