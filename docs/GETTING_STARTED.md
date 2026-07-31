# Getting Started

<!-- DOC-NAV:START -->
[Home](../README.md) · [Docs](README.md) · [Start](GETTING_STARTED.md) · [Implement](IMPLEMENTATION_GUIDE.md) · [Architecture](ARCHITECTURE.md) · [Test](TEST_STRATEGY.md) · [Interview](INTERVIEW_PREP.md)
<!-- DOC-NAV:END -->


## Supported development environments

| Task | Environment |
|---|---|
| Domain, application, simulation, persistence, and console | Windows, Linux, or macOS with .NET 10 |
| WPF operator HMI | Windows 10/11 with .NET 10 |
| LinuxCNC profile | Debian/LinuxCNC environment |
| Documentation validation | Python 3.10 or newer |

## 1. Clone and inspect

```bash
git clone https://github.com/parthoece/multiaxis-motion-sim.git
cd multiaxis-motion-sim
```

Before publishing a fork, replace the `parthoece` placeholders:

```bash
python scripts/replace_owner.py YOUR_GITHUB_USERNAME
```

## 2. Verify prerequisites

```bash
dotnet --info
python --version
git --version
```

The repository pins the expected SDK family in `global.json`.

## 3. Restore dependencies

```bash
dotnet restore
```

## 4. Run automated tests

```bash
dotnet test tests/MotionControl.Domain.Tests
dotnet test tests/MotionControl.Application.Tests
dotnet test tests/MotionControl.IntegrationTests
```

## 5. Run the normal simulation

```bash
dotnet run --project src/MotionControl.OperatorConsole -- normal
```

Expected high-level sequence:

```text
Off → Initializing → NotHomed → Homing → Ready → Automatic → Ready
```

Runtime artifacts are written under `.runtime/`:

```text
.runtime/
├── operations.db
└── events.jsonl
```

## 6. Run one failure scenario

```bash
dotnet run --project src/MotionControl.OperatorConsole -- probe-timeout
```

The process should return a non-zero exit code and report a `ProbeTimeout` alarm.

## 7. Run the operator-stop scenario

```bash
dotnet run --project src/MotionControl.OperatorConsole -- operator-stop
```

Expected behavior:

```text
Automatic
→ operator stop
→ active token cancelled
→ OperationCancelled alarm
→ Faulted
→ deliberate reset
→ Ready
```

The scenario exits zero only when cancellation and recovery behave as expected.

## 8. Run the Windows HMI

From Windows:

```powershell
dotnet run --project src/MotionControl.Hmi.Wpf
```

Use the commands in this order:

1. Initialize
2. Home all
3. Run inspection
4. Arm probe timeout
5. Run inspection again
6. Clear the injected fault
7. Reset fault
8. Home again when required

## 9. Run the repository check

```bash
./scripts/check.sh
```

On Windows, run the equivalent commands from `scripts/check.sh` or use WSL for the shell script.

## 10. Continue development

Follow the ordered milestones in [Development Plan](DEVELOPMENT_PLAN.md).

---

<!-- DOC-FOOTER:START -->
[Documentation index](README.md) · [Next: Problem Statement →](PROBLEM_STATEMENT.md) · [Back to top](#getting-started)
<!-- DOC-FOOTER:END -->
