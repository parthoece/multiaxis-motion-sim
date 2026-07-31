# Contributing

<!-- DOC-NAV:START -->
[Home](README.md) · [Docs](docs/README.md) · [Start](docs/GETTING_STARTED.md) · [Implement](docs/IMPLEMENTATION_GUIDE.md) · [Architecture](docs/ARCHITECTURE.md) · [Test](docs/TEST_STRATEGY.md) · [Interview](docs/INTERVIEW_PREP.md)
<!-- DOC-NAV:END -->


Contributions should improve the virtual-commissioning problem, not merely add technologies.

## Before opening a pull request

1. Read the problem statement and requirements.
2. Open or reference an issue.
3. State the equipment behavior being changed.
4. Define acceptance criteria.
5. Add unit, integration, or manual verification.
6. Update documentation and traceability.
7. Run `./scripts/check.sh`.

## Technology relevance gate

A new dependency must answer:

1. Which requirement needs it?
2. Which component calls it?
3. Which workflow demonstrates it?
4. Which failure case tests it?
5. Why is the simpler alternative insufficient?

Dependencies without a concrete engineering purpose will not be accepted.

## Architecture rules

- Domain code must not depend on device, database, GUI, or simulation implementations.
- Application workflows depend on interfaces.
- Simulation and persistence implement application interfaces.
- HMI code must not contain machine business rules.
- Machine profiles remain isolated under `configs/`.
- Physical-performance claims must be labeled `physical validation required`.

## Workflow

```bash
git checkout -b feature/short-description
./scripts/check.sh
git commit -m "Add deterministic probe-timeout recovery"
```

Use focused, imperative commit messages.

## Significant decisions

Open an issue and add an ADR for:

- state-machine changes;
- safety or recovery behavior;
- public interface changes;
- new machine-profile architecture;
- incompatible persistence changes;
- new deployment boundaries;
- licensing changes.

---

<!-- DOC-FOOTER:START -->
[Documentation index](docs/README.md) · [Back to top](#contributing)
<!-- DOC-FOOTER:END -->
