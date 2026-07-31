# ADR 0003: Exclude Distributed Infrastructure from the Core

<!-- DOC-NAV:START -->
[Home](../../README.md) · [Docs](../README.md) · [Start](../GETTING_STARTED.md) · [Implement](../IMPLEMENTATION_GUIDE.md) · [Architecture](../ARCHITECTURE.md) · [Test](../TEST_STRATEGY.md) · [Interview](../INTERVIEW_PREP.md)
<!-- DOC-NAV:END -->


- Status: Accepted
- Date: 2026-07-28

## Decision

Do not add Docker, MQTT, PostgreSQL, Grafana, or microservices to the core release.

## Rationale

The business problem is local virtual commissioning of one machine. Distributed infrastructure becomes relevant only for multiple machines, remote clients, centralized manufacturing data, or independent deployment.

---

<!-- DOC-FOOTER:START -->
[Documentation index](../README.md) · [Back to top](#adr-0003-exclude-distributed-infrastructure-from-the-core)
<!-- DOC-FOOTER:END -->
