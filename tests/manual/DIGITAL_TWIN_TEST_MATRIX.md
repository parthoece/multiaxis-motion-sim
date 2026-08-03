# Digital Twin Manual Test Matrix

<!-- DOC-NAV:START -->
[Home](../../README.md) · [Docs](../../docs/README.md) · [Start](../../docs/GETTING_STARTED.md) · [Implement](../../docs/IMPLEMENTATION_GUIDE.md) · [Architecture](../../docs/ARCHITECTURE.md) · [Test](../../docs/TEST_STRATEGY.md) · [Interview](../../docs/INTERVIEW_PREP.md)
<!-- DOC-NAV:END -->


Record operating system, Digital Twin version, profile commit, date, and evidence.

| ID | Test | Expected result | Status |
|---|---|---|---|
| LC-001 | Launch profile | AXIS GUI opens without fatal error | Not run |
| LC-002 | Enable machine | E-stop clears and machine enables | Not run |
| LC-003 | Home all | Z homes before X and Y | Not run |
| LC-004 | Jog XYZ | Each coordinate moves in the expected direction | Not run |
| LC-005 | X soft limit | Command beyond 500 mm is rejected | Not run |
| LC-006 | Y soft limit | Command beyond 400 mm is rejected | Not run |
| LC-007 | Z soft limit | Command beyond 150 mm is rejected | Not run |
| LC-008 | Rectangle program | Program completes inside limits | Not run |
| LC-009 | Probe inactive | G38.2 fails when no trigger occurs | Not run |
| LC-010 | E-stop during motion | Motion stops and machine disables | Not run |

---

<!-- DOC-FOOTER:START -->
[Documentation index](../../docs/README.md) · [Back to top](#digital-twin-manual-test-matrix)
<!-- DOC-FOOTER:END -->
