# WPF HMI Manual Test Matrix

<!-- DOC-NAV:START -->
[Home](../../README.md) · [Docs](../../docs/README.md) · [Start](../../docs/GETTING_STARTED.md) · [Implement](../../docs/IMPLEMENTATION_GUIDE.md) · [Architecture](../../docs/ARCHITECTURE.md) · [Test](../../docs/TEST_STRATEGY.md) · [Interview](../../docs/INTERVIEW_PREP.md)
<!-- DOC-NAV:END -->


Record Windows version, .NET SDK version, commit, date, and evidence.

| ID | Test | Expected result | Status |
|---|---|---|---|
| HMI-001 | Launch HMI | Main window opens without unhandled error | Not run |
| HMI-002 | Initialize | State changes from `Off` to `NotHomed` | Not run |
| HMI-003 | Home all | State changes to `Ready`; Z reports home position | Not run |
| HMI-004 | Run inspection | Cycle completes and reports PASS or FAIL | Not run |
| HMI-005 | Arm probe timeout | Next cycle enters `Faulted` with `ProbeTimeout` | Not run |
| HMI-006 | Clear fault injection | Scenario returns to normal | Not run |
| HMI-007 | Reset fault | Recovery follows rehome policy | Not run |
| HMI-008 | Stop command | Active simulated motion receives stop request | Not run |
| HMI-009 | Window resize | Controls remain visible at minimum size | Not run |
| HMI-010 | Application restart | Runtime history remains accessible | Not run |

---

<!-- DOC-FOOTER:START -->
[Documentation index](../../docs/README.md) · [Back to top](#wpf-hmi-manual-test-matrix)
<!-- DOC-FOOTER:END -->
