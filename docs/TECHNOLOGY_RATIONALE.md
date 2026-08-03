# Technology Rationale

<!-- DOC-NAV:START -->
[Home](../README.md) · [Docs](README.md) · [Start](GETTING_STARTED.md) · [Implement](IMPLEMENTATION_GUIDE.md) · [Architecture](ARCHITECTURE.md) · [Test](TEST_STRATEGY.md) · [Interview](INTERVIEW_PREP.md)
<!-- DOC-NAV:END -->


## Included now

| Technology | Requirement served | Failure case |
|---|---|---|
| .NET 10 and C# | Equipment workflow, concurrency, adapters, tests | software exception, cancellation, invalid command |
| WPF | Windows equipment operator interface | command failure and operator recovery |
| In-process deterministic simulator | Hardware-independent virtual commissioning | motion, probe, permissive, and communication faults |
| Digital Twin, HAL, G-code | Independent industrial motion simulation | homing, limit, and probe behavior |
| SQLite | Local history for one machine | transaction or storage failure |
| JSON Lines | Portable diagnostic events | malformed or missing diagnostic context |
| xUnit | Regression protection | failed invariant or workflow |
| GitHub Actions and CodeQL | Repeatable delivery checks | build, test, and security regression |

## Excluded from the core

| Technology | Reason for exclusion |
|---|---|
| Docker | The core is a desktop application plus local simulator; containers do not simplify motion or HMI execution. |
| PostgreSQL | One machine and one local application do not require a server database. |
| MQTT | There are no remote telemetry consumers. |
| OPC UA | No external SCADA or standardized asset model is currently required. |
| Grafana | There is not yet a factory-scale historical analytics requirement. |
| Microservices | Network boundaries would add failure modes without independent deployment needs. |
| ROS 2 | The system is equipment control, not a distributed robotics application. |
| Computer vision | The current process uses virtual contact probing, not visual alignment. |
| EtherCAT | No real-time fieldbus hardware or hardware-in-the-loop target exists. |

## Addition rule

A technology is introduced only when a requirement, workflow, and failure scenario demonstrate why the existing solution is insufficient.

---

<!-- DOC-FOOTER:START -->
[Documentation index](README.md) · [Back to top](#technology-rationale)
<!-- DOC-FOOTER:END -->
