import hashlib
import json
import struct
from pathlib import Path

import tiktoken
import tiktoken_ext.openai_public as openai_public

REPO_ROOT = Path(__file__).resolve().parents[1]
TEMPLATES_ROOT = REPO_ROOT / "tests" / "__templates"
TIKTOKEN_ROOT = REPO_ROOT / "tests" / "_tiktoken"


def hash_string(value: str) -> str:
    data = value.encode("utf-8")
    digest = hashlib.sha256()
    digest.update(struct.pack("<I", len(data)))
    digest.update(data)
    return digest.hexdigest()


def hash_uint32_sequence(tokens: list[int]) -> str:
    digest = hashlib.sha256()
    for token in tokens:
        digest.update(struct.pack("<I", token))
    return digest.hexdigest()


def load_templates() -> dict[str, str]:
    templates: dict[str, str] = {}
    for template_path in sorted(TEMPLATES_ROOT.glob("tokenization-*.json")):
        payload = json.loads(template_path.read_text(encoding="utf-8"))
        template_id = payload.get("id")
        single = payload.get("single") or {}
        text = single.get("text")
        if not template_id or text is None:
            raise RuntimeError(f"Template '{template_path}' is missing id/text")
        templates[template_id] = text
    return templates


def regenerate_manifest(manifest_path: Path, templates: dict[str, str]) -> bool:
    manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
    encoding_name = manifest["encoding"]
    pattern = manifest["pattern"]
    mergeable_path = manifest_path.with_name("mergeable_ranks.tiktoken")
    mergeable_ranks = openai_public.load_tiktoken_bpe(str(mergeable_path))
    special_tokens = manifest.get("specialTokens", {})

    encoding = tiktoken.Encoding(
        encoding_name,
        pat_str=pattern,
        mergeable_ranks=mergeable_ranks,
        special_tokens=special_tokens,
        explicit_n_vocab=None,
    )

    cases = manifest.get("cases")
    if not isinstance(cases, dict) or not cases:
        raise RuntimeError(f"Manifest '{manifest_path}' does not contain cases")

    updated = False
    for case_id, snapshot in cases.items():
        template_text = templates.get(case_id)
        if template_text is None:
            raise RuntimeError(f"Template '{case_id}' not found for manifest '{manifest_path}'")

        py_snapshot = snapshot.get("py")
        if not isinstance(py_snapshot, dict):
            raise RuntimeError(f"Manifest '{manifest_path}' case '{case_id}' missing python snapshot")

        expected_text_hash = py_snapshot.get("text-hash")
        actual_text_hash = hash_string(template_text)
        if expected_text_hash != actual_text_hash:
            raise RuntimeError(
                f"Manifest '{manifest_path}' case '{case_id}' text hash mismatch:"
                f" expected={expected_text_hash}, actual={actual_text_hash}"
            )

        tokens = encoding.encode(template_text)
        new_hash = hash_uint32_sequence(tokens)
        if py_snapshot.get("encoding-hash") != new_hash:
            py_snapshot["encoding-hash"] = new_hash
            updated = True

    if updated:
        manifest_path.write_text(
            json.dumps(manifest, indent=2, ensure_ascii=False) + "\n",
            encoding="utf-8",
        )

    return updated


def main() -> None:
    templates = load_templates()
    manifest_paths = sorted(TIKTOKEN_ROOT.glob("*/tokenx-tests-validation.json"))
    if not manifest_paths:
        raise RuntimeError("No TikToken manifests found")

    updated_files = []
    for manifest_path in manifest_paths:
        if regenerate_manifest(manifest_path, templates):
            updated_files.append(manifest_path.relative_to(REPO_ROOT))

    if updated_files:
        print("Updated manifests:")
        for path in updated_files:
            print(f"  {path}")
    else:
        print("All manifests already up to date.")


if __name__ == "__main__":
    main()
