# Motion-Control Terms

<!-- DOC-NAV:START -->
[Home](../README.md) · [Docs](README.md) · [Start](GETTING_STARTED.md) · [Implement](IMPLEMENTATION_GUIDE.md) · [Architecture](ARCHITECTURE.md) · [Test](TEST_STRATEGY.md) · [Interview](INTERVIEW_PREP.md)
<!-- DOC-NAV:END -->


## Coordinates and mechanisms

| Term | Software meaning |
|---|---|
| Axis | Commanded coordinate such as X, Y, Z, A, B, or C |
| Joint | Physical or simulated actuator |
| Degree of freedom | Independent motion available to the mechanism |
| Machine coordinates | Coordinate system established by homing |
| Work coordinates | Coordinate system attached to a part or fixture |
| Work offset | Transform between machine and work coordinates |
| Tool center point | Effective point positioned by the controller |
| Kinematics | Mapping between joints and task-space coordinates |
| Forward kinematics | Joint positions to machine pose |
| Inverse kinematics | Desired pose to required joint positions |
| Identity kinematics | One joint directly maps to one coordinate |
| Singularity | Configuration where kinematic control becomes indeterminate or extreme |

## Motion planning

| Term | Software meaning |
|---|---|
| Path | Geometric route through space |
| Trajectory | Path plus timing, velocity, and acceleration |
| Interpolation | Generation of intermediate motion commands |
| Coordinated motion | Multiple axes move as one planned path |
| Feed rate | Programmed process velocity |
| Rapid move | Positioning move, commonly G0 |
| Acceleration | Rate of velocity change |
| Jerk | Rate of acceleration change |
| Look-ahead | Planner analysis of future path segments |
| Blending | Passing through connected segments without a full stop |
| Path tolerance | Allowed deviation during blending |
| Cycle time | Time required for one repeated operation |

## Referencing and safety logic

| Term | Software meaning |
|---|---|
| Homing | Establishing a repeatable machine reference |
| Home switch | Sensor used during referencing |
| Home offset | Coordinate assigned at or relative to home detection |
| Home sequence | Safe order or grouping of joint homing |
| Hard limit | Physical overtravel input |
| Soft limit | Software coordinate boundary after homing |
| Permissive | Condition required before an operation may start |
| Interlock | Logic that prevents or stops an unsafe or invalid action |
| Emergency stop | Safety function for stopping hazardous motion; software alone is insufficient |
| Fault | Condition preventing normal operation |
| Alarm | Operator-visible fault information |
| Recovery | Controlled return from a fault to a known state |
| Fail-safe state | State intended to minimize risk after failure |

## Feedback and real time

| Term | Software meaning |
|---|---|
| Command | Requested controlled value |
| Feedback | Measured or simulated actual value |
| Following error | Difference between commanded and measured position |
| Open loop | Control without corrective measured feedback |
| Closed loop | Control using feedback to reduce error |
| Servo loop | Periodic real-time control calculation |
| Resolution | Smallest commandable or observable increment |
| Accuracy | Closeness to the true value |
| Repeatability | Ability to return to the same value |
| Backlash | Lost motion after direction reversal |
| Real-time | Correctness depends on bounded completion time |
| Determinism | Timing and outcomes are repeatable or bounded |
| Latency | Delay between an event and response |
| Jitter | Variation in periodic timing |

## LinuxCNC and HAL

| Term | Software meaning |
|---|---|
| HAL | Hardware Abstraction Layer connecting LinuxCNC components |
| Component | HAL module exposing pins, parameters, and functions |
| Pin | Typed input or output connection |
| Signal | Named value connecting compatible pins |
| Parameter | Configurable component value |
| Thread | Periodic execution context for HAL functions |
| Servo period | Interval between motion-control updates |
| G-code | Program language for motion and machine commands |
| M-code | Program command for machine functions |
| MDI | Interactive execution of individual commands |
| Probe move | Motion that ends when probe input changes |
| Joint mode | Direct actuator-space operation |
| World mode | Cartesian coordinate operation |

## Software-in-the-loop

| Term | Meaning |
|---|---|
| Virtual plant | Software model of machine-observable behavior |
| Virtual commissioning | Testing controls against a simulated machine before hardware |
| Software-in-the-loop | Controller software and plant model both run in software |
| Hardware-in-the-loop | Real controller hardware runs against a simulated plant |
| Fault injection | Deliberate failure used to verify detection and recovery |
| Deterministic seed | Fixed input producing repeatable simulated variation |
| Acceptance criterion | Observable condition required for completion |
| Traceability | Link between requirement, code, test, and evidence |
| Regression test | Test preventing previously fixed behavior from breaking |

---

<!-- DOC-FOOTER:START -->
[Documentation index](README.md) · [Back to top](#motion-control-terms)
<!-- DOC-FOOTER:END -->
