# Getting Started

<!-- DOC-NAV:START -->
[Home](../README.md) · [Docs](README.md) · [Start](GETTING_STARTED.md) · [Implement](IMPLEMENTATION_GUIDE.md) · [Architecture](ARCHITECTURE.md) · [Test](TEST_STRATEGY.md) · [Interview](INTERVIEW_PREP.md)
<!-- DOC-NAV:END -->

Use this guide to verify a clean checkout, run the deterministic console workflows, launch the Windows HMI, and optionally connect the HMI to the grblHAL software backend.

## Supported development environments

| Task | Environment |
|---|---|
| Domain, application, simulation, persistence, and console | Windows, Linux, or macOS with the SDK selected by `global.json` |
| WPF operator HMI | Windows 10 or 11 with .NET 10 |
| grblHAL software backend | Windows with a separately built grblHAL Simulator executable |
| Documentation and repository validation | Python 3.10 or newer |
| Digital Twin work | Planned extension; not required for the current baseline |

## 1. Clone the repository

```bash
git clone https://github.com/parthoece/multiaxis-motion-sim.git
cd multiaxis-motion-sim
```

## 2. Verify prerequisites

```bash
dotnet --info
git --version
python --version
```

The repository pins the expected .NET SDK family in [`global.json`](../global.json). Python is needed only for the optional repository and documentation checks.

## 3. Restore and build

```bash
dotnet restore
dotnet build --configuration Release
```

A clean build should not require the grblHAL Simulator or a Digital Twin runtime.

## 4. Run automated tests

Run the complete suite:

```bash
dotnet test --configuration Release
```

Or run the projects separately:

```bash
dotnet test tests/MotionControl.Domain.Tests --configuration Release
dotnet test tests/MotionControl.Application.Tests --configuration Release
dotnet test tests/MotionControl.IntegrationTests --configuration Release
```

The standard automated suite does not require a locally running grblHAL process.

## 5. Run the normal deterministic scenario

```bash
dotnet run --project src/MotionControl.OperatorConsole -- normal
```

Expected workflow:

```text
Off
→ Initializing
→ NotHomed
→ Homing
→ Ready
→ Automatic
→ Ready
```

The scenario initializes the virtual machine, homes Z-X-Y, executes a five-point inspection recipe, evaluates the measurements, and persists the cycle result.

Runtime artifacts are written under `.runtime/`:

```text
.runtime/
├── operations.db
└── events.jsonl
```

## 6. Exercise abnormal behavior

### Probe timeout

```bash
dotnet run --project src/MotionControl.OperatorConsole -- probe-timeout
```

Expected result:

- active motion is cancelled;
- `ProbeTimeout` remains the primary alarm;
- the machine enters `Faulted`;
- reset returns the machine to `NotHomed`;
- rehoming is required before another automatic cycle.

The scenario is expected to return a non-zero exit code when the injected fault is detected. Verify this behavior against the current release before documenting it as part of public evidence.

### Operator Stop

```bash
dotnet run --project src/MotionControl.OperatorConsole -- operator-stop
```

Expected workflow:

```text
Automatic
→ operator Stop
→ active workflow cancelled
→ OperationCancelled alarm
→ Faulted
→ deliberate reset
→ Ready
```

With the deterministic backend, the scenario exits successfully only when cancellation and recovery match the expected policy. External-controller backends may require rehoming after an abort because position is no longer trusted.

Other deterministic scenarios are listed in the [root README](../README.md#deterministic-scenarios).

## 7. Run the Windows HMI

From Windows:

```powershell
dotnet run --project src/MotionControl.Hmi.Wpf
```

Suggested verification flow:

1. Select **Initialize**.
2. Select **Home All**.
3. Run a normal inspection.
4. Review the five measurements and cycle result.
5. Enable probe-timeout injection.
6. Run another inspection.
7. Observe cancellation and the preserved primary alarm.
8. Clear the injected condition.
9. Reset the fault.
10. Rehome when required.

WPF operational data is stored in the current user's local application-data directory rather than the repository `.runtime/` directory.

## 8. Optionally run the grblHAL software backend

The grblHAL executable is not committed to this repository. Build the upstream simulator separately and place it at:

```text
tools/grblhal-sim/bin/grblHAL_sim.exe
```

The full build procedure, behavior boundary, and tested-evidence requirements are documented in [Implementation Guide — Validate the grblHAL software backend](IMPLEMENTATION_GUIDE.md#phase-8--validate-the-grblhal-software-backend).

Start the simulator:

```powershell
.\tools\grblhal-sim\bin\grblHAL_sim.exe -p 23000
```

In another terminal, run the protocol smoke test:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass `
    -File scripts/Test-GrblHalSimulator.ps1
```

Then launch the HMI with the grblHAL backend:

```powershell
$env:MOTION_BACKEND = "grblhal"
$env:GRBLHAL_HOST = "127.0.0.1"
$env:GRBLHAL_PORT = "23000"

dotnet run --project src/MotionControl.Hmi.Wpf
```

Return to the default deterministic backend:

```powershell
Remove-Item Env:MOTION_BACKEND -ErrorAction SilentlyContinue
Remove-Item Env:GRBLHAL_HOST -ErrorAction SilentlyContinue
Remove-Item Env:GRBLHAL_PORT -ErrorAction SilentlyContinue
```

This path validates software communication with the controller core. It does not validate electrical I/O, motors, mechanics, positioning accuracy, collision behavior, or functional safety.

## 9. Run repository checks

On a compatible shell:

```bash
./scripts/check.sh
```

The individual documentation and architecture checks can also be run directly:

```bash
python scripts/check_architecture.py
python scripts/check_docs.py
```

On Windows, run the equivalent commands from `scripts/check.sh` or use WSL for the shell script.

## 10. Record verification evidence

For a release, pull request, or portfolio capture, record:

- operating system;
- .NET SDK version;
- commit hash;
- selected motion backend;
- test summary;
- scenario output;
- grblHAL Simulator revision when applicable;
- known warnings or unverified behavior.

Do not commit generated databases, diagnostic files, the simulator executable, `EEPROM.DAT`, credentials, usernames, or machine-specific paths.

## Fork maintainers

Before publishing a fork under a different GitHub account, update repository-owner references using the supplied helper:

```bash
python scripts/replace_owner.py YOUR_GITHUB_USERNAME
```

Review the resulting badge URLs, clone commands, documentation links, and package metadata before committing the replacements.

## Continue development

Follow the ordered milestones in the [Implementation Guide](IMPLEMENTATION_GUIDE.md) and [Development Plan](DEVELOPMENT_PLAN.md).

---

<!-- DOC-FOOTER:START -->
[Documentation index](README.md) · [Next: Problem Statement →](PROBLEM_STATEMENT.md) · [Back to top](#getting-started)
<!-- DOC-FOOTER:END -->
