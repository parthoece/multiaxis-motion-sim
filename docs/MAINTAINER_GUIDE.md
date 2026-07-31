# Maintainer Guide

<!-- DOC-NAV:START -->
[Home](../README.md) · [Docs](README.md) · [Start](GETTING_STARTED.md) · [Implement](IMPLEMENTATION_GUIDE.md) · [Architecture](ARCHITECTURE.md) · [Test](TEST_STRATEGY.md) · [Interview](INTERVIEW_PREP.md)
<!-- DOC-NAV:END -->


## Routine responsibilities

- Triage issues.
- Keep requirements and traceability current.
- Review dependency updates.
- Review safety- and recovery-related changes carefully.
- Keep CI and CodeQL passing.
- Prepare releases with evidence.
- Remove misleading claims or badges.

## Reviewing a change

Confirm:

1. The engineering requirement is real.
2. The design respects dependency direction.
3. Normal and failure paths are tested.
4. Cancellation and recovery are considered.
5. New dependencies pass the technology relevance gate.
6. Documentation and traceability are updated.
7. Simulation results are not described as physical validation.

## Dependency updates

Dependabot proposes NuGet and GitHub Actions updates.

Review:

- release notes;
- target-framework support;
- security implications;
- breaking changes;
- generated lock or restore behavior;
- all automated checks.

## Release preparation

Follow [Release Process](RELEASE_PROCESS.md). A release must include:

- changelog;
- test results;
- known limitations;
- LinuxCNC manual evidence when the profile is claimed;
- migration notes;
- corrected citation version;
- no unresolved critical security findings.

## Ownership

Replace `parthoece` in:

- badges;
- repository URLs;
- CODEparthoeceS;
- issue contact links;
- citation metadata.

Use:

```bash
python scripts/replace_owner.py GITHUB_USERNAME
```

## Backup and recovery

Before a schema or release change:

- tag the current commit;
- preserve representative runtime data;
- document migration behavior;
- keep release artifacts reproducible.

## Community health

Periodically check GitHub's Community Standards view and verify that policies remain discoverable.

---

<!-- DOC-FOOTER:START -->
[Documentation index](README.md) · [Back to top](#maintainer-guide)
<!-- DOC-FOOTER:END -->
