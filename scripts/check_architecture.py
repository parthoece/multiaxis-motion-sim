from __future__ import annotations

import sys
import xml.etree.ElementTree as ET
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]

PROJECT_RULES = {
    Path("src/MotionControl.Domain/MotionControl.Domain.csproj"): set(),
    Path("src/MotionControl.Application/MotionControl.Application.csproj"): {
        "../MotionControl.Domain/MotionControl.Domain.csproj",
    },
    Path("src/MotionControl.Simulation/MotionControl.Simulation.csproj"): {
        "../MotionControl.Domain/MotionControl.Domain.csproj",
        "../MotionControl.Application/MotionControl.Application.csproj",
    },
    Path("src/MotionControl.Persistence/MotionControl.Persistence.csproj"): {
        "../MotionControl.Domain/MotionControl.Domain.csproj",
        "../MotionControl.Application/MotionControl.Application.csproj",
    },
}

REQUIRED_APPLICATION_DIRECTORIES = {
    "Common",
    "Lifecycle",
    "Inspection",
    "Status",
    "Stop",
}

FORBIDDEN_DOMAIN_TEXT = {
    "MotionControl.Application",
    "MotionControl.Simulation",
    "MotionControl.Persistence",
    "Microsoft.Data.Sqlite",
    "System.Windows",
}

MAX_COORDINATOR_LINES = 140


def project_references(path: Path) -> set[str]:
    tree = ET.parse(path)
    return {
        element.attrib["Include"].replace("\\", "/")
        for element in tree.findall(".//ProjectReference")
    }


def main() -> int:
    errors: list[str] = []

    for relative, expected in PROJECT_RULES.items():
        actual = project_references(ROOT / relative)
        if actual != expected:
            errors.append(
                f"{relative}: expected project references {sorted(expected)}, "
                f"found {sorted(actual)}"
            )

    application_root = ROOT / "src" / "MotionControl.Application"
    actual_directories = {
        path.name
        for path in application_root.iterdir()
        if path.is_dir()
    }

    missing_directories = REQUIRED_APPLICATION_DIRECTORIES - actual_directories
    if missing_directories:
        errors.append(
            "MotionControl.Application: missing use-case directories: "
            + ", ".join(sorted(missing_directories))
        )

    domain_root = ROOT / "src" / "MotionControl.Domain"
    for source in sorted(domain_root.glob("*.cs")):
        text = source.read_text(encoding="utf-8")
        for forbidden in sorted(FORBIDDEN_DOMAIN_TEXT):
            if forbidden in text:
                errors.append(
                    f"{source.relative_to(ROOT)}: forbidden domain dependency "
                    f"or namespace: {forbidden}"
                )

    coordinator = (
        ROOT
        / "src"
        / "MotionControl.Application"
        / "MachineCoordinator.cs"
    )
    line_count = len(coordinator.read_text(encoding="utf-8").splitlines())
    if line_count > MAX_COORDINATOR_LINES:
        errors.append(
            f"{coordinator.relative_to(ROOT)}: {line_count} lines exceeds "
            f"the facade limit of {MAX_COORDINATOR_LINES}"
        )

    if errors:
        print("Architecture validation failed:")
        for error in errors:
            print(f"- {error}")
        return 1

    print(
        "Architecture validation passed: dependency direction, domain "
        "isolation, use-case structure, and coordinator size are valid."
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
