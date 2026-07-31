# Failure Scenarios

<!-- DOC-NAV:START -->
[Home](../README.md) · [Docs](README.md) · [Start](GETTING_STARTED.md) · [Implement](IMPLEMENTATION_GUIDE.md) · [Architecture](ARCHITECTURE.md) · [Test](TEST_STRATEGY.md) · [Interview](INTERVIEW_PREP.md)
<!-- DOC-NAV:END -->


| Scenario | Trigger | Detection | Expected response | Recovery |
|---|---|---|---|---|
| Startup storage failure | SQLite initialization fails | Initialization use case | Enter `Faulted`; preserve warning if alarm persistence also fails | Repair storage and recover to `Off` |
| Motion controller unavailable | Controller initialization or stop confirmation fails | Adapter exception or timeout | Enter `Faulted` | Recover to `Off` and initialize again |
| Emergency stop | E-stop input false | Permissive check | Reject operation and fault | Clear input, reset, rehome |
| Door open | Door input false | Permissive check | Reject operation and fault | Close door, reset, rehome under conservative policy |
| Part missing | Part input false | Pre-cycle check | Do not begin automatic motion | Restore part, reset to `Ready` |
| Air not ready | Air input false | Permissive check | Reject operation | Restore air, reset to `Ready` |
| PLC communication lost | Gateway exception | Read or output write | Stop workflow and preserve primary fault | Restore communication, reset, rehome |
| Homing failure | Home sensor never activates | Homing adapter | Stop and enter `Faulted` | Clear fault, reset, repeat homing |
| Probe already active | Probe active before move | Probe precondition | Stop cycle and alarm | Clear probe, reset, rehome |
| Probe timeout | Surface not detected | Probe move | Stop cycle; preserve probe fault even if alarm output fails | Clear fault, reset, rehome |
| Positive limit | Injected boundary crossing | Motion adapter | Reject move and fault | Clear fault, reset, rehome |
| Out-of-tolerance part | Surface offset | Measurement evaluation | Complete cycle with FAIL result | No machine recovery required |
| Operator stop | Stop button | Active-operation cancellation | Cancel workflow, stop adapter, await confirmation, enter `Faulted` | Reset to `Ready` in simulator |
| Diagnostic log unavailable | JSONL write throws | Best-effort diagnostic action | Continue valid workflow and record operational warning | Repair log sink; no machine reset required |
| Secondary PLC fault-output failure | Alarm output write throws after primary fault | Best-effort output action | Keep original primary alarm and add warning | Repair PLC after primary fault is handled |

Every scenario must define trigger, detection, machine response, persistence behavior, and recovery target.

---

<!-- DOC-FOOTER:START -->
[← Previous: Plc Io Map](PLC_IO_MAP.md) · [Documentation index](README.md) · [Next: Test Strategy →](TEST_STRATEGY.md) · [Back to top](#failure-scenarios)
<!-- DOC-FOOTER:END -->
