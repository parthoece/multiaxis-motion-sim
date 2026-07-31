# Problem Statement

<!-- DOC-NAV:START -->
[Home](../README.md) · [Docs](README.md) · [Start](GETTING_STARTED.md) · [Implement](IMPLEMENTATION_GUIDE.md) · [Architecture](ARCHITECTURE.md) · [Test](TEST_STRATEGY.md) · [Interview](INTERVIEW_PREP.md)
<!-- DOC-NAV:END -->


## Industry problem

Industrial machine-control software is commonly integrated and validated only after mechanical, electrical, motion, sensing, and operator-interface subsystems become available.

At that stage, defects in motion sequences, homing, interlocks, equipment states, recipes, sensor handling, PLC handshakes, probing, and recovery behavior are expensive and risky to correct.

A defect in one subsystem can affect several others:

- incorrect homing can create unsafe movement;
- stale or missing permissives can start an invalid cycle;
- probe timeout can leave the system in an undefined state;
- communication loss can make operator status inaccurate;
- recipe errors can command motion outside permitted travel;
- cancellation can interrupt persistence or recovery logic.

Physical commissioning makes software defects difficult to isolate because machine time is limited, abnormal conditions are difficult to reproduce, unfinished hardware introduces unrelated failures, and every change may require another physical test.

## Project objective

Provide an open-source, software-in-the-loop environment for developing and validating industrial equipment-control behavior without physical machinery.

## Primary engineering question

How can a machine-control software team verify motion behavior, equipment workflows, interlocks, communication handling, and fault recovery before the physical machine is assembled?

## Intended outcome

Developers can execute the same workflow repeatedly under normal and abnormal conditions, observe deterministic results, and collect verification evidence before migrating software to real equipment.

## In scope

- coordinated multi-axis commands;
- joint and coordinate concepts;
- homing order and software limits;
- operator commands;
- manual and automatic state rules;
- virtual PLC inputs and outputs;
- inspection recipes and probing;
- alarms and recovery;
- cancellation and concurrency;
- persistent operational records;
- deterministic fault injection;
- independent LinuxCNC simulation profile.

## Out of scope

- physical accuracy, repeatability, or backlash;
- structural stiffness or vibration;
- motor and drive sizing;
- electrical design and noise immunity;
- physical sensors;
- collision physics;
- hardware E-stop performance;
- machinery-safety compliance;
- production MES or fleet-level analytics.

## Success definition

The platform is useful when it exposes software and integration defects earlier than physical commissioning and preserves enough evidence to reproduce and diagnose them.

## Source context

See the [reference list](REFERENCES.md) for industry and technical sources used to frame the problem.

---

<!-- DOC-FOOTER:START -->
[← Previous: Getting Started](GETTING_STARTED.md) · [Documentation index](README.md) · [Next: Requirements →](REQUIREMENTS.md) · [Back to top](#problem-statement)
<!-- DOC-FOOTER:END -->
