# ADR 0001: Simulation-First Architecture

<!-- DOC-NAV:START -->
[Home](../../README.md) · [Docs](../README.md) · [Start](../GETTING_STARTED.md) · [Implement](../IMPLEMENTATION_GUIDE.md) · [Architecture](../ARCHITECTURE.md) · [Test](../TEST_STRATEGY.md) · [Interview](../INTERVIEW_PREP.md)
<!-- DOC-NAV:END -->


- Status: Accepted
- Date: 2026-07-28

## Decision

Develop and verify machine-control workflows against deterministic software models before introducing physical equipment.

## Consequences

Benefits include repeatable faults, earlier testing, lower risk, and automated regression coverage.

The limitation is that simulation cannot prove mechanical, electrical, physical-safety, or performance characteristics.

---

<!-- DOC-FOOTER:START -->
[Documentation index](../README.md) · [Back to top](#adr-0001-simulation-first-architecture)
<!-- DOC-FOOTER:END -->
