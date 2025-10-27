import hashlib
import json
import struct
from pathlib import Path
from typing import Any

import sentencepiece as spm  # type: ignore[import-untyped]

REPO_ROOT = Path(__file__).resolve().parents[1]
TEMPLATES_ROOT = REPO_ROOT / "tests" / "__templates"
SENTENCEPIECE_ROOT = REPO_ROOT / "tests" / "_sentencepeice"


def hash_string(value: str) -> str:
    data = value.encode("utf-8")
    digest = hashlib.sha256()
    digest.update(struct.pack("<I", len(data)))
    digest.update(data)
    return digest.hexdigest()


def hash_string_sequence(values: list[str]) -> str:
    digest = hashlib.sha256()
    for value in values:
        encoded = value.encode("utf-8")
        digest.update(struct.pack("<I", len(encoded)))
        digest.update(encoded)
    return digest.hexdigest()


def hash_int32_sequence(values: list[int]) -> str:
    digest = hashlib.sha256()
    for value in values:
        digest.update(struct.pack("<i", value))
    return digest.hexdigest()


def load_templates() -> dict[str, str]:
    templates: dict[str, str] = {}
    for template_path in sorted(TEMPLATES_ROOT.glob("tokenization-*.json")):
        payload = json.loads(template_path.read_text(encoding="utf-8"))
        targets = payload.get("targets") or []
        if targets and all(target.lower() != "sentencepiece" and target != "*" for target in targets):
            continue

        template_id = payload.get("id")
        single = payload.get("single") or {}
        text = single.get("text")
        if not template_id or text is None:
            raise RuntimeError(f"Template '{template_path}' is missing id/text")
        templates[template_id] = text
    return templates


def detect_model_file(model_dir: Path) -> str:
    candidates = sorted(model_dir.glob("*.model"))
    if not candidates:
        raise RuntimeError(f"No SentencePiece model file found in '{model_dir}'")
    # Prefer canonical naming if present.
    for preferred in ("spiece.model", "tokenizer.model"):
        candidate = model_dir / preferred
        if candidate.exists():
            return preferred
    return candidates[0].name


def regenerate_manifest(model_dir: Path, templates: dict[str, str]) -> bool:
    manifest_path = model_dir / "tokenx-tests-validation.json"
    model_file = detect_model_file(model_dir)

    processor: Any = spm.SentencePieceProcessor()
    processor.load(str(model_dir / model_file))

    if manifest_path.exists():
        manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
    else:
        manifest = {
            "model": model_dir.name,
            "modelFile": model_file,
            "cases": {},
        }

    if manifest.get("model") != model_dir.name:
        manifest["model"] = model_dir.name
    if manifest.get("modelFile") != model_file:
        manifest["modelFile"] = model_file

    cases = manifest.setdefault("cases", {})
    dirty = not manifest_path.exists()

    for case_id, text in templates.items():
        snapshot = cases.setdefault(case_id, {})
        py_snapshot = snapshot.setdefault("py", {})

        text_hash = hash_string(text)
        if "text-hash" in py_snapshot and py_snapshot["text-hash"] != text_hash:
            raise RuntimeError(
                f"Manifest '{manifest_path}' case '{case_id}' text hash mismatch:"
                f" expected={py_snapshot['text-hash']}, actual={text_hash}"
            )
        py_snapshot["text-hash"] = text_hash

        ids = processor.encode(text, out_type=int)
        pieces = processor.encode(text, out_type=str)
        decoded_from_ids = processor.decode(ids)
        decoded_from_pieces = processor.decode(pieces)

        ids_hash = hash_int32_sequence(ids)
        pieces_hash = hash_string_sequence(pieces)
        decoded_ids_hash = hash_string(decoded_from_ids)
        decoded_pieces_hash = hash_string(decoded_from_pieces)

        if py_snapshot.get("ids-hash") != ids_hash:
            py_snapshot["ids-hash"] = ids_hash
            dirty = True

        if py_snapshot.get("pieces-hash") != pieces_hash:
            py_snapshot["pieces-hash"] = pieces_hash
            dirty = True

        if py_snapshot.get("decoded-ids-hash") != decoded_ids_hash:
            py_snapshot["decoded-ids-hash"] = decoded_ids_hash
            dirty = True

        if py_snapshot.get("decoded-pieces-hash") != decoded_pieces_hash:
            py_snapshot["decoded-pieces-hash"] = decoded_pieces_hash
            dirty = True

        if py_snapshot.get("decoded-hash") != decoded_ids_hash:
            py_snapshot["decoded-hash"] = decoded_ids_hash
            dirty = True

    if dirty:
        manifest_path.write_text(
            json.dumps(manifest, indent=2, ensure_ascii=False) + "\n",
            encoding="utf-8",
        )

    return dirty


def main() -> None:
    if not SENTENCEPIECE_ROOT.is_dir():
        raise RuntimeError("SentencePiece test data directory not found")

    templates = load_templates()
    if not templates:
        raise RuntimeError("No templates applicable to SentencePiece were found")

    updated = []
    for model_dir in sorted(SENTENCEPIECE_ROOT.iterdir()):
        if not model_dir.is_dir():
            continue
        if regenerate_manifest(model_dir, templates):
            updated.append(model_dir.relative_to(REPO_ROOT / "tests"))

    if updated:
        print("Updated manifests:")
        for relative in updated:
            print(f"  tests/{relative}/tokenx-tests-validation.json")
    else:
        print("All SentencePiece manifests already up to date.")


if __name__ == "__main__":
    main()
