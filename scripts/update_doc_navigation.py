from __future__ import annotations

import os
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]

NAV_START = "<!-- DOC-NAV:START -->"
NAV_END = "<!-- DOC-NAV:END -->"
FOOTER_START = "<!-- DOC-FOOTER:START -->"
FOOTER_END = "<!-- DOC-FOOTER:END -->"

EXCLUDED = {
    Path(".github/PULL_REQUEST_TEMPLATE.md"),
}

LEARNING_PATH = [
    Path("docs/GETTING_STARTED.md"),
    Path("docs/PROBLEM_STATEMENT.md"),
    Path("docs/REQUIREMENTS.md"),
    Path("docs/ARCHITECTURE.md"),
    Path("docs/DOMAIN_MODEL.md"),
    Path("docs/STATE_MACHINE.md"),
    Path("docs/IMPLEMENTATION_GUIDE.md"),
    Path("docs/SIMULATION_MODEL.md"),
    Path("docs/PLC_IO_MAP.md"),
    Path("docs/FAILURE_SCENARIOS.md"),
    Path("docs/TEST_STRATEGY.md"),
    Path("docs/DEVELOPMENT_PLAN.md"),
    Path("docs/ACCEPTANCE_CRITERIA.md"),
    Path("docs/PORTFOLIO_REVIEW.md"),
    Path("docs/INTERVIEW_PREP.md"),
]


def relative_link(source: Path, target: Path) -> str:
    return Path(os.path.relpath(ROOT / target, source.parent)).as_posix()


def strip_block(text: str, start: str, end: str) -> str:
    return re.sub(
        rf"\n?{re.escape(start)}.*?{re.escape(end)}\n?",
        "\n",
        text,
        flags=re.S,
    )


def slugify(title: str) -> str:
    value = title.lstrip("#").strip().lower()
    value = re.sub(r"[^\w\s-]", "", value)
    return re.sub(r"[\s_]+", "-", value).strip("-")


def clean_footer_separators(lines: list[str]) -> list[str]:
    while lines and not lines[-1].strip():
        lines.pop()

    while lines and lines[-1].strip() == "---":
        lines.pop()
        while lines and not lines[-1].strip():
            lines.pop()

    return lines


def learning_neighbors(relative: Path) -> tuple[Path | None, Path | None]:
    if relative not in LEARNING_PATH:
        return None, None

    index = LEARNING_PATH.index(relative)
    previous = LEARNING_PATH[index - 1] if index > 0 else None
    following = LEARNING_PATH[index + 1] if index + 1 < len(LEARNING_PATH) else None
    return previous, following


def update(path: Path) -> None:
    relative = path.relative_to(ROOT)
    text = path.read_text(encoding="utf-8")
    text = strip_block(text, NAV_START, NAV_END)
    text = strip_block(text, FOOTER_START, FOOTER_END)

    lines = text.splitlines()
    h1_index = next(
        (index for index, line in enumerate(lines) if line.startswith("# ")),
        None,
    )
    if h1_index is None:
        raise RuntimeError(f"Missing H1: {relative}")

    top_links = [
        ("Home", Path("README.md")),
        ("Docs", Path("docs/README.md")),
        ("Start", Path("docs/GETTING_STARTED.md")),
        ("Implement", Path("docs/IMPLEMENTATION_GUIDE.md")),
        ("Architecture", Path("docs/ARCHITECTURE.md")),
        ("Test", Path("docs/TEST_STRATEGY.md")),
        ("Interview", Path("docs/INTERVIEW_PREP.md")),
    ]

    top_navigation = " · ".join(
        f"[{label}]({relative_link(path, target)})"
        for label, target in top_links
    )

    nav_lines = [
        "",
        NAV_START,
        top_navigation,
        NAV_END,
        "",
    ]
    lines[h1_index + 1:h1_index + 1] = nav_lines
    lines = clean_footer_separators(lines)

    previous, following = learning_neighbors(relative)
    footer_links: list[str] = []

    if previous is not None:
        footer_links.append(
            f"[← Previous: {previous.stem.replace('_', ' ').title()}]"
            f"({relative_link(path, previous)})"
        )

    footer_links.append(
        f"[Documentation index]({relative_link(path, Path('docs/README.md'))})"
    )

    if following is not None:
        footer_links.append(
            f"[Next: {following.stem.replace('_', ' ').title()} →]"
            f"({relative_link(path, following)})"
        )

    footer_links.append(f"[Back to top](#{slugify(lines[h1_index])})")

    lines.extend([
        "",
        "---",
        "",
        FOOTER_START,
        " · ".join(footer_links),
        FOOTER_END,
        "",
    ])

    # Normalize excessive blank lines.
    output = "\n".join(lines)
    output = re.sub(r"\n{4,}", "\n\n\n", output)
    path.write_text(output, encoding="utf-8")


def main() -> int:
    files = sorted(
        path
        for path in ROOT.rglob("*.md")
        if path.relative_to(ROOT) not in EXCLUDED
        and ".git" not in path.parts
    )

    for path in files:
        update(path)

    print(f"Updated navigation in {len(files)} Markdown files.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
