#!/usr/bin/env python3
"""Initialize per-model tokenization validation manifests.

Each Hugging Face model directory under ``tests/_huggingface`` receives a
``tokenx-tests-validation.json`` file that mirrors the set of contract cases
under ``tests/__templates``. Existing manifests are merged so that any tracked
results remain in place while newly added cases are appended automatically.
"""

from __future__ import annotations

import json
from dataclasses import dataclass
from pathlib import Path
from typing import Dict

REPO_ROOT = Path(__file__).resolve().parents[3]
TEMPLATES_DIR = REPO_ROOT / "tests" / "__templates"
MODELS_DIR = REPO_ROOT / "tests" / "_huggingface"

MANIFEST_FILE_NAME = "tokenx-tests-validation.json"
MANIFEST_VERSION = 1


def load_template_cases() -> Dict[str, Dict[str, object]]:
    cases: Dict[str, Dict[str, object]] = {}
    for template_path in sorted(TEMPLATES_DIR.glob("tokenization-*.json")):
        data = json.loads(template_path.read_text(encoding="utf-8"))
        case_id = data.get("id")
        if not case_id:
            continue
        cases.setdefault(case_id, {"py": {}, "tokenx": {}})
    return cases


@dataclass
class Manifest:
    version: int
    model: str
    cases: Dict[str, Dict[str, object]]

    @classmethod
    def load(cls, path: Path, model: str) -> "Manifest":
        if not path.exists():
            return cls(MANIFEST_VERSION, model, {})
        payload = json.loads(path.read_text(encoding="utf-8"))
        version = int(payload.get("version", MANIFEST_VERSION))
        cases = {
            str(case_id): dict(case_payload)
            for case_id, case_payload in payload.get("cases", {}).items()
        }
        return cls(version, payload.get("model", model), cases)

    def ensure_case(self, case_id: str) -> None:
        self.cases.setdefault(case_id, {"py": {}, "tokenx": {}})

    def prune_missing(self, valid_case_ids: set[str]) -> None:
        removed = [case_id for case_id in self.cases if case_id not in valid_case_ids]
        for case_id in removed:
            self.cases.pop(case_id, None)

    def write(self, path: Path) -> None:
        payload = {
            "version": MANIFEST_VERSION,
            "model": self.model,
            "cases": dict(sorted(self.cases.items())),
        }
        path.write_text(json.dumps(payload, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def main() -> None:
    if not MODELS_DIR.exists():
        raise SystemExit(f"Hugging Face data directory missing: {MODELS_DIR}")

    template_cases = load_template_cases()
    valid_case_ids = set(template_cases.keys())

    for model_dir in sorted(MODELS_DIR.iterdir()):
        if not model_dir.is_dir():
            continue

        if not (model_dir / "tokenizer.json").exists():
            continue
        manifest_path = model_dir / MANIFEST_FILE_NAME
        manifest = Manifest.load(manifest_path, model_dir.name)
        for case_id, skeleton in template_cases.items():
            if case_id not in manifest.cases:
                manifest.cases[case_id] = dict(skeleton)
        manifest.prune_missing(valid_case_ids)
        manifest.write(manifest_path)
        print(f"Initialized {manifest_path.relative_to(REPO_ROOT)}")


if __name__ == "__main__":
    main()
