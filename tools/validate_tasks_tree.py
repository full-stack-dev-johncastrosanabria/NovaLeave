#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Independent invariant checker for the derived tasks/ tree (DR-002, item 3).

Run from the repository root:  python3 tools/validate_tasks_tree.py

Checks (all must pass):
  1. No unexpanded template artifacts ("$(@{", "System.Object[]", "{{") and no
     UTF-8 BOM in any file under tasks/ or docs/adr/.
  2. Canonical tasks in specs/001-leave-management-mvp/tasks.md are contiguous
     from T001 to the final Tnnn, every tasks/EPIC-*/TASK-nnn.md exists, and each
     generated task round-trips: its Objective line embeds the canonical text of
     its Tnnn verbatim.
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
    if not canon:
        fails.append("expected at least one canonical task, found 0")

    canonical_numbers = sorted(int(tid[1:]) for tid in canon)
    expected_numbers = list(range(1, len(canonical_numbers) + 1))
    if canonical_numbers != expected_numbers:
        expected_last = expected_numbers[-1] if expected_numbers else 0
        fails.append(
            f"canonical task IDs must be contiguous T001-T{expected_last:03d}; "
            f"found {[f'T{n:03d}' for n in canonical_numbers]}"
        )

    task_files = glob.glob("tasks/EPIC-*/TASK-*.md")
    if len(task_files) != len(canon):
        fails.append(f"expected {len(canon)} TASK files, found {len(task_files)}")

    generated_ids = set()
    for path in task_files:
        tid = "T" + re.search(r"TASK-(\d{3})\.md$", path).group(1)
        if tid in generated_ids:
            fails.append(f"duplicate generated TASK file for {tid}")
        generated_ids.add(tid)
        if tid not in canon:
            fails.append(f"{path}: {tid} not in canonical tasks.md")
            continue
        if f"canonical task {tid}: {canon[tid]}" not in open(path, encoding="utf-8").read():
            fails.append(f"{path}: Objective does not embed canonical text for {tid}")

    missing_generated = sorted(set(canon) - generated_ids)
    if missing_generated:
        fails.append(f"missing generated TASK files: {missing_generated}")

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
