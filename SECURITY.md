# Security and Safety Policy

<!-- DOC-NAV:START -->
[Home](README.md) · [Docs](docs/README.md) · [Start](docs/GETTING_STARTED.md) · [Implement](docs/IMPLEMENTATION_GUIDE.md) · [Architecture](docs/ARCHITECTURE.md) · [Test](docs/TEST_STRATEGY.md) · [Interview](docs/INTERVIEW_PREP.md)
<!-- DOC-NAV:END -->


## Supported versions

The latest `main` branch and latest tagged release are supported on a best-effort basis.

## Private reporting

Enable GitHub private vulnerability reporting after publishing. Use it for:

- command injection;
- unsafe input handling;
- unexpected-motion paths;
- limit, homing, or interlock bypass;
- unsafe recovery;
- exposed secrets;
- vulnerabilities that could affect a future physical machine.

Include the affected commit, reproduction steps, impact, mitigation ideas, and whether the result was observed in simulation or hardware.

## Simulation boundary

No configuration in this repository is certified for physical machinery. Physical deployment requires independent electrical, mechanical, controls, and machinery-safety review.

---

<!-- DOC-FOOTER:START -->
[Documentation index](docs/README.md) · [Back to top](#security-and-safety-policy)
<!-- DOC-FOOTER:END -->
