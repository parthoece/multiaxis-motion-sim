from __future__ import annotations

import re
import sys
from pathlib import Path
from urllib.parse import unquote

ROOT = Path(__file__).resolve().parents[1]
NAV_START = "<!-- DOC-NAV:START -->"
NAV_END = "<!-- DOC-NAV:END -->"
FOOTER_START = "<!-- DOC-FOOTER:START -->"
FOOTER_END = "<!-- DOC-FOOTER:END -->"

LINK_RE = re.compile(r"(?<!!)\[[^\]]+\]\(([^)]+)\)")

REQUIRED_INDEX_LINKS = {
    "GETTING_STARTED.md",
    "IMPLEMENTATION_GUIDE.md",
    "ARCHITECTURE.md",
    "TEST_STRATEGY.md",
    "DEVELOPMENT_PLAN.md",
    "PORTFOLIO_REVIEW.md",
    "INTERVIEW_PREP.md",
}

EXCLUDED = {
    Path(".github/PULL_REQUEST_TEMPLATE.md"),
}


def markdown_files() -> list[Path]:
    return sorted(
        path
        for path in ROOT.rglob("*.md")
        if path.relative_to(ROOT) not in EXCLUDED
        and ".git" not in path.parts
    )


def validate_navigation(path: Path, text: str) -> list[str]:
    errors: list[str] = []
    relative = path.relative_to(ROOT)

    if NAV_START not in text or NAV_END not in text:
        errors.append(f"{relative}: missing top navigation")

    if FOOTER_START not in text or FOOTER_END not in text:
        errors.append(f"{relative}: missing footer navigation")

    return errors


def validate_links(path: Path, text: str) -> list[str]:
    errors: list[str] = []

    for raw in LINK_RE.findall(text):
        target = raw.strip().split(maxsplit=1)[0].strip("<>")
        if not target or target.startswith(("#", "http://", "https://", "mailto:")):
            continue

        target = unquote(target).split("#", 1)[0]
        if not target:
            continue

        resolved = (path.parent / target).resolve()

        try:
            resolved.relative_to(ROOT.resolve())
        except ValueError:
            errors.append(f"{path.relative_to(ROOT)}: link escapes repository: {raw}")
            continue

        if not resolved.exists():
            errors.append(
                f"{path.relative_to(ROOT)}: missing local link target: {raw}"
            )

    return errors


def main() -> int:
    errors: list[str] = []
    files = markdown_files()

    index_text = (ROOT / "docs" / "README.md").read_text(encoding="utf-8")
    for required in sorted(REQUIRED_INDEX_LINKS):
        if f"({required})" not in index_text:
            errors.append(f"docs/README.md: missing required index link: {required}")

    for path in files:
        text = path.read_text(encoding="utf-8")
        errors.extend(validate_navigation(path, text))
        errors.extend(validate_links(path, text))

    if errors:
        print("Documentation validation failed:")
        for error in errors:
            print(f"- {error}")
        return 1

    print(f"Documentation validation passed for {len(files)} Markdown files.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
