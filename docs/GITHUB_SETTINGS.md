# GitHub Settings

<!-- DOC-NAV:START -->
[Home](../README.md) · [Docs](README.md) · [Start](GETTING_STARTED.md) · [Implement](IMPLEMENTATION_GUIDE.md) · [Architecture](ARCHITECTURE.md) · [Test](TEST_STRATEGY.md) · [Interview](INTERVIEW_PREP.md)
<!-- DOC-NAV:END -->


After publishing:

## General

- Replace every `parthoece` placeholder.
- Add repository topics: `motion-control`, `virtual-commissioning`, `linuxcnc`, `csharp`, `wpf`, `industrial-automation`, `simulation`.
- Enable Issues and Discussions.
- Delete merged branches automatically.

## Main branch rules

- Require pull requests.
- Require conversation resolution.
- Require CI, Windows HMI, and CodeQL checks.
- Block force pushes and deletion.
- Require linear history.
- Require one approval when a second reviewer is available.

## Security

Enable:

- dependency graph;
- Dependabot alerts;
- Dependabot security updates;
- secret scanning;
- push protection;
- private vulnerability reporting;
- CodeQL code scanning.

## Actions

Use read-only workflow permissions by default. Review workflow changes as security-sensitive code.

---

<!-- DOC-FOOTER:START -->
[Documentation index](README.md) · [Back to top](#github-settings)
<!-- DOC-FOOTER:END -->
