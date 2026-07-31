# ADR 0002: Modular Monolith

<!-- DOC-NAV:START -->
[Home](../../README.md) · [Docs](../README.md) · [Start](../GETTING_STARTED.md) · [Implement](../IMPLEMENTATION_GUIDE.md) · [Architecture](../ARCHITECTURE.md) · [Test](../TEST_STRATEGY.md) · [Interview](../INTERVIEW_PREP.md)
<!-- DOC-NAV:END -->


- Status: Accepted
- Date: 2026-07-28

## Decision

Use one deployable equipment application with explicit Domain, Application, Simulation, Persistence, and Presentation modules.

## Rationale

One local machine does not require network services. In-process boundaries simplify debugging, transactions, cancellation, and tests while preserving adapter replacement.

---

<!-- DOC-FOOTER:START -->
[Documentation index](../README.md) · [Back to top](#adr-0002-modular-monolith)
<!-- DOC-FOOTER:END -->
