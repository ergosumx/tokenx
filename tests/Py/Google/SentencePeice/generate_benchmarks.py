#!/usr/bin/env python3
"""Generate parity benchmarks for the managed SentencePiece bindings.

The emitted fixtures mirror the structure produced by the Hugging Face parity
tooling so the .NET test suite can validate hashes, decoded payloads, and batch
behaviour across every model that ships a SentencePiece vocabulary inside
``tests/_TestData``.
"""

from __future__ import annotations

import argparse
import hashlib
import json
import struct
import sys
import time
from dataclasses import dataclass
from pathlib import Path
from typing import Dict, Iterable, List, Sequence

try:
    import sentencepiece as spm
except ImportError as exc:  # pragma: no cover - dependency guard
    raise SystemExit(
        "The 'sentencepiece' package is required. Activate the workspace .venv and install dependencies."
    ) from exc

REPO_ROOT = Path(__file__).resolve().parents[4]
DEFAULT_OUTPUT_DIR = REPO_ROOT / "tests" / "_TestData"
SENTINEL_NONE = -0x80000000


@dataclass(frozen=True)
class ModelSpec:
    display_name: str
    repo_id: str
    model_file: str


@dataclass(frozen=True)
class CaseDefinition:
    length: str
    description: str
    text: str
    batch_texts: Sequence[str]


CASE_DEFINITIONS: Sequence[CaseDefinition] = (
    CaseDefinition(
        length="Short",
        description="Short compliance update",
        text="SentencePiece enables nightly compliance summaries for analysts.",
        batch_texts=(
            "SentencePiece enables nightly compliance summaries for analysts.",
            "SentencePiece enables nightly incident summaries for analysts.",
            "SentencePiece enables nightly compliance summaries for auditors.",
        ),
    ),
    CaseDefinition(
        length="Identifiers",
        description="Hyphenated identifiers",
        text="Queue-42 flagged policy-drift events during the 03:17 UTC window.",
        batch_texts=(
            "Queue-42 flagged policy-drift events during the 03:17 UTC window.",
            "Queue-42 flagged policy-drift events during the 09:45 UTC window.",
            "Queue-42 flagged policy-drift events during the 12:05 UTC window.",
        ),
    ),
    CaseDefinition(
        length="Telemetry",
        description="Multi-sentence telemetry",
        text=(
            "Telemetry dashboards surface multilingual traces so reviewers can verify policy coverage. "
            "Compliance leads archive the summaries for the morning briefing."
        ),
        batch_texts=(
            "Telemetry dashboards surface multilingual traces so reviewers can verify policy coverage.",
            "Compliance leads archive the summaries for the morning briefing.",
            "Telemetry dashboards surface multilingual traces for nightly auditing responsibilities.",
        ),
    ),
)


def _hash_bytes(value: bytes) -> str:
    return hashlib.sha256(value).hexdigest()


def _hash_string(value: str) -> str:
    encoded = value.encode("utf-8")
    return _hash_bytes(struct.pack("<I", len(encoded)) + encoded)


def _hash_string_sequence(values: Sequence[str]) -> str:
    payload = bytearray()
    for item in values:
        encoded = item.encode("utf-8")
        payload.extend(struct.pack("<I", len(encoded)))
        payload.extend(encoded)
    return _hash_bytes(payload)


def _hash_int32_sequence(values: Sequence[int]) -> str:
    payload = bytearray()
    for value in values:
        payload.extend(struct.pack("<i", int(value)))
    return _hash_bytes(payload)


def _hash_uint32_sequence(values: Sequence[int]) -> str:
    payload = bytearray()
    for value in values:
        payload.extend(struct.pack("<I", int(value)))
    return _hash_bytes(payload)


def _hash_optional_int32_sequence(values: Sequence[int | None]) -> str:
    payload = bytearray()
    for value in values:
        encoded = SENTINEL_NONE if value is None else int(value)
        payload.extend(struct.pack("<i", encoded))
    return _hash_bytes(payload)


def _hash_offsets(values: Sequence[tuple[int, int]]) -> str:
    payload = bytearray()
    for start, end in values:
        payload.extend(struct.pack("<i", int(start)))
        payload.extend(struct.pack("<i", int(end)))
    return _hash_bytes(payload)


def discover_model_specs(output_dir: Path) -> Dict[str, ModelSpec]:
    specs: Dict[str, ModelSpec] = {}
    if not output_dir.exists():
        return specs

    for directory in sorted(output_dir.iterdir(), key=lambda item: item.name):
        if not directory.is_dir():
            continue

        asset = None
        for candidate in ("spiece.model", "sentencepiece.bpe.model", "tokenizer.model"):
            if (directory / candidate).exists():
                asset = candidate
                break

        if asset is None:
            continue

        display_name = directory.name
        repo_id = directory.name
        metadata_path = directory / "python-benchmark.json"
        if metadata_path.exists():
            try:
                metadata = json.loads(metadata_path.read_text(encoding="utf-8")).get("metadata", {})
                display_name = metadata.get("display_name", display_name)
                repo_id = metadata.get("repo_id", repo_id)
            except (OSError, json.JSONDecodeError):  # pragma: no cover - defensive
                pass

        specs[directory.name] = ModelSpec(display_name=display_name, repo_id=repo_id, model_file=asset)

    return specs


def ensure_model_path(model_root: Path, model_file: str) -> Path:
    candidate = model_root / model_file
    if not candidate.exists():
        raise RuntimeError(
            f"Model asset '{model_file}' is missing from '{model_root}'. Run tests/Py/Common/restore_test_data.py first."
        )
    return candidate


def summarize_encoding(ids: Sequence[int], pieces: Sequence[str]) -> Dict[str, object]:
    length = len(ids)
    type_ids = [0] * length
    attention_mask = [1] * length
    special_tokens_mask = [0] * length
    offsets = [(0, 0)] * length
    word_ids: List[int | None] = [None] * length
    sequence_ids = [0] * length

    return {
        "length": length,
        "idsHash": _hash_int32_sequence(ids),
        "tokensHash": _hash_string_sequence(pieces),
        "typeIdsHash": _hash_uint32_sequence(type_ids),
        "attentionMaskHash": _hash_uint32_sequence(attention_mask),
        "specialTokensMaskHash": _hash_uint32_sequence(special_tokens_mask),
        "offsetsHash": _hash_offsets(offsets),
        "wordIdsHash": _hash_optional_int32_sequence(word_ids),
        "sequenceIdsHash": _hash_optional_int32_sequence(sequence_ids),
        "overflowing": [],
    }


def generate_case_payload(processor: spm.SentencePieceProcessor, case: CaseDefinition) -> Dict[str, object]:
    single_ids = [int(value) for value in processor.encode(case.text, out_type=int)]
    single_pieces = [str(value) for value in processor.encode(case.text, out_type=str)]
    single_decoded = processor.decode(single_ids)

    batch_texts = list(case.batch_texts)
    batch_ids = [[int(value) for value in processor.encode(item, out_type=int)] for item in batch_texts]
    batch_pieces = [[str(value) for value in processor.encode(item, out_type=str)] for item in batch_texts]
    batch_decoded = list(processor.decode(batch_ids))

    single_payload = {
        "text": case.text,
        "textHash": _hash_string(case.text),
        "encoding": summarize_encoding(single_ids, single_pieces),
        "decodedHash": _hash_string(single_decoded),
    }

    batch_payload = {
        "count": len(batch_texts),
        "texts": batch_texts,
        "textsHash": _hash_string_sequence(batch_texts),
        "encodings": [summarize_encoding(ids, pieces) for ids, pieces in zip(batch_ids, batch_pieces)],
        "decodedHash": _hash_string_sequence(batch_decoded),
    }

    return {
        "length": case.length,
        "description": case.description,
        "options": {
            "addSpecialTokens": False,
            "decodeSkipSpecialTokens": True,
        },
        "single": single_payload,
        "batch": batch_payload,
    }


def build_payload(processor: spm.SentencePieceProcessor) -> List[Dict[str, object]]:
    return [generate_case_payload(processor, case) for case in CASE_DEFINITIONS]


def write_fixture(model: str, spec: ModelSpec, output_dir: Path, payload: Iterable[Dict[str, object]]) -> Path:
    destination = output_dir / model / "python-sentencepiece-benchmark.json"
    destination.parent.mkdir(parents=True, exist_ok=True)

    sentencepiece_version = getattr(spm, "__version__", "unknown")
    metadata = {
        "model": model,
        "display_name": spec.display_name,
        "repo_id": spec.repo_id,
        "generated_at": time.strftime("%Y-%m-%dT%H:%M:%SZ", time.gmtime()),
        "tokenizers_version": sentencepiece_version,
        "transformers_version": None,
        "sentencepiece_version": sentencepiece_version,
        "assets": {"model": spec.model_file},
    }

    content = {
        "metadata": metadata,
        "cases": list(payload),
    }

    destination.write_text(json.dumps(content, indent=2), encoding="utf-8")
    return destination


def process_model(model: str, spec: ModelSpec, output_dir: Path) -> Path:
    model_root = output_dir / model
    model_path = ensure_model_path(model_root, spec.model_file)

    processor = spm.SentencePieceProcessor()
    if not processor.Load(str(model_path)):
        raise RuntimeError(f"Failed to load SentencePiece model from '{model_path}'.")

    payload = build_payload(processor)
    return write_fixture(model, spec, output_dir, payload)


def run(models: Sequence[str], output_dir: Path, specs: Dict[str, ModelSpec]) -> None:
    for model in models:
        spec = specs[model]
        try:
            destination = process_model(model, spec, output_dir)
        except Exception as exc:  # pragma: no cover - surfaced to CLI
            raise SystemExit(f"Failed to generate benchmark for model '{model}': {exc}") from exc
        else:
            print(f"Generated {destination}")


def parse_args(argv: Sequence[str]) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Generate SentencePiece benchmark fixtures for parity testing.")
    parser.add_argument(
        "--model",
        dest="models",
        action="append",
        help="Optional model identifier to process. Can be specified multiple times.",
    )
    parser.add_argument(
        "--output-dir",
        type=Path,
        default=DEFAULT_OUTPUT_DIR,
        help="Destination directory for generated assets (defaults to tests/_TestData).",
    )
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(sys.argv[1:] if argv is None else argv)
    specs = discover_model_specs(Path(args.output_dir))
    if not specs:
        raise SystemExit(f"No SentencePiece assets discovered under '{args.output_dir}'.")

    if args.models:
        missing = [model for model in args.models if model not in specs]
        if missing:
            raise SystemExit(f"Requested model(s) not found: {', '.join(missing)}")
        models = args.models
    else:
        models = sorted(specs.keys())

    run(models, Path(args.output_dir), specs)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
