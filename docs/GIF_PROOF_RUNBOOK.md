# GIF Proof Runbook

<!-- DOC-NAV:START -->
[Home](../README.md) · [Docs](README.md) · [Start](GETTING_STARTED.md) · [Implement](IMPLEMENTATION_GUIDE.md) · [Architecture](ARCHITECTURE.md) · [Test](TEST_STRATEGY.md) · [Interview](INTERVIEW_PREP.md)
<!-- DOC-NAV:END -->

Use this runbook to generate small GIF clips that prove repository execution for hiring review and release evidence.

This version is HMI-first so the demo is more appealing than command-line-only evidence.

## Objective

Create short, reproducible GIF clips that show:

1. Git traceability to a commit or tag.
2. Successful build and test execution.
3. Expected normal and fault behavior.
4. grblHAL simulator integration proof.
5. Operator-focused HMI workflow and response quality.

## GIF constraints for GitHub

1. Keep each GIF to 15 to 45 seconds.
2. Capture terminal only, not full desktop.
3. Use 1280x720 or smaller.
4. Use 8 to 12 fps.
5. Compress after capture to stay under repository-friendly size.

Recommended clip structure:

- 4 to 6 GIF files.
- 5 MB to 20 MB each after compression.

## Streamlined HMI-first flow

Use this reduced flow if you want fewer terminal commands and stronger visual storytelling.

Required terminal commands:

1. git status
2. git log --oneline -n 5
3. dotnet test multiaxis-motion-sim.sln
4. dotnet run --project src/MotionControl.Hmi.Wpf

The rest of the proof is captured inside the HMI with guided actions.

## Recording preparation

1. Open a PowerShell terminal at repository root.
2. Increase terminal font size so command output is readable.
3. Run each command once before recording to avoid setup delays.
4. Start with a clean working tree unless the clip is explicitly about pending changes.

## Clip 1: Git identity proof

Show these commands in order:

```powershell
git status
git branch --show-current
git log --oneline -n 5
git show --no-patch --decorate
```

What must be visible in the GIF:

1. Current branch name.
2. Latest commit hash.
3. Commit subject lines.
4. Clean or explainable workspace state.

## Clip 2: Build and test proof

Show these commands in order:

```powershell
dotnet test multiaxis-motion-sim.sln
```

What must be visible in the GIF:

1. Build succeeded.
2. Test summary total and failed count.
3. No hidden terminal scroll at the final summary line.

## Clip 3: Launch HMI proof

Show this command:

```powershell
dotnet run --project src/MotionControl.Hmi.Wpf
```

What must be visible in the GIF:

1. HMI window launches successfully.
2. Guided demonstration panel is visible.
3. Backend label and next-action hint are visible.
4. KPI cards and state quality signal are visible.

## Clip 4: HMI guided normal flow

Inside the HMI, click:

1. Run guided flow

What must be visible in the GIF:

1. State transitions from Off to Ready.
2. Inspection cycle runs and measurements populate.
3. Response log entries update with timestamps.
4. Status panel shows cycle completion.
5. KPI cards update total/pass/fail/pass-rate and last-cycle duration.

## Clip 5: HMI fault-response proof

Inside the HMI, perform:

1. Select ProbeTimeout in the fault selector.
2. Click Arm selected fault.
3. Click Run inspection.
4. Click Recover to ready.

What must be visible in the GIF:

1. Active injection label changes.
2. Active alarm reflects the injected fault.
3. Response log shows fault and recovery sequence.
4. Next-action hint updates after fault.

## Clip 6: grblHAL simulator proof

Show this command:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Test-GrblHalSimulator.ps1
```

What must be visible in the GIF:

1. Script runs from repository path.
2. Simulator workflow completes without script failure.
3. Final success output or zero exit result.

## Optional Clip 7: console non-zero fault exit proof

If you still want explicit non-zero process evidence, record:

```powershell
dotnet run --project src/MotionControl.OperatorConsole -- probe-timeout
$LASTEXITCODE
```

## Optional release tag clip

If you are preparing a release evidence pack, record:

```powershell
git tag -a v0.3.0 -m "HAL-first integration evidence"
git show v0.3.0 --no-patch --decorate
git push
git push origin v0.3.0
```

## Evidence naming convention

Use file names that include date and short commit hash.

Example pattern:

```text
2026-08-03_abc1234_git-proof.gif
2026-08-03_abc1234_build-test.gif
2026-08-03_abc1234_normal-scenario.gif
2026-08-03_abc1234_probe-timeout.gif
```

## Final checklist before upload

1. Every GIF shows the full command line before execution.
2. Every GIF shows the final success or failure summary line.
3. At least one GIF shows commit hash and branch.
4. Fault GIF shows non-zero exit code.
5. GIF names include date and commit hash.
6. Evidence is referenced from release notes or portfolio document.

---

<!-- DOC-FOOTER:START -->
[← Previous: Release Process](RELEASE_PROCESS.md) · [Documentation index](README.md) · [Next: Portfolio Review →](PORTFOLIO_REVIEW.md) · [Back to top](#gif-proof-runbook)
<!-- DOC-FOOTER:END -->