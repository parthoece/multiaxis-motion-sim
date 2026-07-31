# Troubleshooting

<!-- DOC-NAV:START -->
[Home](../README.md) · [Docs](README.md) · [Start](GETTING_STARTED.md) · [Implement](IMPLEMENTATION_GUIDE.md) · [Architecture](ARCHITECTURE.md) · [Test](TEST_STRATEGY.md) · [Interview](INTERVIEW_PREP.md)
<!-- DOC-NAV:END -->


## `dotnet` command not found

Install the .NET 10 SDK and confirm:

```bash
dotnet --info
```

## SDK version mismatch

The repository uses `global.json`. Install a compatible .NET 10 feature band or update the file through a reviewed change.

## NuGet restore fails

Check:

- internet connectivity;
- NuGet source configuration;
- proxy settings;
- package versions in `Directory.Packages.props`.

Then run:

```bash
dotnet nuget list source
dotnet restore --force
```

## WPF project does not run

WPF requires Windows. On Linux or macOS, build and run the console project instead:

```bash
dotnet run --project src/MotionControl.OperatorConsole -- normal
```

## Automatic cycle is rejected

Confirm:

- initialization completed;
- all axes are homed;
- machine state is `Ready`;
- E-stop is reset;
- door is closed;
- air is ready;
- part is present;
- recipe passes validation.

## Fault cannot be reset

Clear the injected fault first. Recovery rechecks permissives. Some faults return the machine to `NotHomed` and require homing again.

## Stop confirmation times out

A stop timeout means the active operation did not finish within five seconds.

Check:

- the motion adapter observes cancellation;
- `StopAsync` returns;
- the use case exits after `OperationCanceledException`;
- no adapter call ignores its cancellation token;
- operational warnings contain `OperatorStopConfirmationTimeout`.

Treat this as a controller-availability fault.

## Operational warning count increases

Warnings indicate that a secondary action failed, such as JSONL logging, status reading, or PLC fault-output writing.

The active primary alarm remains authoritative. Preserve the warning list and diagnostic files when reporting the problem.

## Runtime database is locked

Stop all application instances. Confirm no SQLite client is holding a write transaction. Do not run multiple simulators against the same `.runtime/operations.db`.

## Documentation validation fails

Run:

```bash
python scripts/check_docs.py
```

The output identifies a missing navigation marker or broken local link.

## LinuxCNC profile fails to launch

Record:

- operating system;
- LinuxCNC version;
- terminal error;
- changed INI or HAL files.

Common causes include version-specific pin names, missing AXIS GUI packages, and invalid relative program paths. Update the manual test matrix after correction.

## Probe template never completes

The LinuxCNC probing template requires `motion.probe-input` to become true. The starter HAL intentionally keeps it false until a virtual surface component is implemented.

## Reporting a defect

Attach:

- exact command;
- commit hash;
- console output;
- `.runtime/events.jsonl`;
- relevant scenario;
- expected and actual behavior.

---

<!-- DOC-FOOTER:START -->
[Documentation index](README.md) · [Back to top](#troubleshooting)
<!-- DOC-FOOTER:END -->
