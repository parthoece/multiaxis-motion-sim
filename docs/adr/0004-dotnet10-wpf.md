# ADR 0004: .NET 10 and WPF

<!-- DOC-NAV:START -->
[Home](../../README.md) · [Docs](../README.md) · [Start](../GETTING_STARTED.md) · [Implement](../IMPLEMENTATION_GUIDE.md) · [Architecture](../ARCHITECTURE.md) · [Test](../TEST_STRATEGY.md) · [Interview](../INTERVIEW_PREP.md)
<!-- DOC-NAV:END -->


- Status: Accepted
- Date: 2026-07-28

## Decision

Use .NET 10 LTS for core software and WPF for the Windows operator HMI.

## Rationale

C# provides strong typing, asynchronous workflows, test tooling, and common equipment-software patterns. WPF matches Windows-based PC-controlled equipment while the console app keeps the core testable on non-Windows systems.

---

<!-- DOC-FOOTER:START -->
[Documentation index](../README.md) · [Back to top](#adr-0004-net-10-and-wpf)
<!-- DOC-FOOTER:END -->
