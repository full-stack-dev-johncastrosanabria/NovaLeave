#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Independent invariant checker for the derived tasks/ tree (DR-002, item 3).

Run from the repository root:  python3 tools/validate_tasks_tree.py

Checks (all must pass):
  1. No unexpanded template artifacts ("$(@{", "System.Object[]", "{{") and no
     UTF-8 BOM in any file under tasks/ or docs/adr/.
  2. Exactly 153 canonical tasks (T001-T153) in specs/001-leave-management-mvp/tasks.md,
     and every tasks/EPIC-*/TASK-nnn.md round-trips: its Objective line embeds the
     canonical text of its Tnnn verbatim.
  3. Every active FR identifier defined in spec.md is referenced by at least one
     TASK file (traceability floor).

This validator is deliberately independent of any generator that produces the
tree (see docs/adr/DR-002-generated-artifact-review-rule.md).
"""
import codecs
import glob
import re
import sys

CANON = "specs/001-leave-management-mvp/tasks.md"
SPEC = "specs/001-leave-management-mvp/spec.md"
ARTIFACTS = ("$(@{", "System.Object[]", "{{")
# Files allowed to mention artifact patterns as documentation:
CITATION_ALLOWED = ("docs/adr/DR-002-generated-artifact-review-rule.md",)


def main() -> int:
    fails = []

    # 1) artifacts + BOM
    for path in glob.glob("tasks/**/*.md", recursive=True) + glob.glob("docs/adr/*.md"):
        raw = open(path, "rb").read()
        if raw.startswith(codecs.BOM_UTF8):
            fails.append(f"BOM present: {path}")
        text = raw.decode("utf-8")
        if path.replace("\\", "/") in CITATION_ALLOWED:
            continue
        for bad in ARTIFACTS:
            if bad in text:
                fails.append(f"template artifact {bad!r} in {path}")

    # 2) canonical round-trip
    canon = dict(
        re.findall(
            r"^- \[ \] (T\d{3}) (.+?)\r?$",
            open(CANON, encoding="utf-8").read(),
            re.M,
        )
    )
    if len(canon) != 153:
        fails.append(f"expected 153 canonical tasks, found {len(canon)}")
    task_files = glob.glob("tasks/EPIC-*/TASK-*.md")
    if len(task_files) != 153:
        fails.append(f"expected 153 TASK files, found {len(task_files)}")
    for path in task_files:
        tid = "T" + re.search(r"TASK-(\d{3})\.md$", path).group(1)
        if tid not in canon:
            fails.append(f"{path}: {tid} not in canonical tasks.md")
            continue
        if f"canonical task {tid}: {canon[tid]}" not in open(path, encoding="utf-8").read():
            fails.append(f"{path}: Objective does not embed canonical text for {tid}")

    # 3) FR traceability floor
    spec = open(SPEC, encoding="utf-8").read()
    active = set(re.findall(r"\*\*(FR-\d+)", spec))
    covered = set()
    for path in task_files:
        covered |= set(re.findall(r"\bFR-\d+\b", open(path, encoding="utf-8").read()))
    missing = sorted(active - covered)
    if missing:
        fails.append(f"active FRs with no TASK reference: {missing}")

    if fails:
        print(f"FAIL ({len(fails)}):")
        for f in fails:
            print("  -", f)
        return 1
    print(
        f"PASS: {len(task_files)} TASK files round-trip against {len(canon)} canonical tasks; "
        f"{len(active)} active FRs all referenced; no template artifacts; no BOM."
    )
    return 0


if __name__ == "__main__":
    sys.exit(main())
