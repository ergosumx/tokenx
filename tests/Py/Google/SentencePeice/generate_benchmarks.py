#!/usr/bin/env python3
"""Generate reference benchmarks for the Google SentencePiece bindings.

The script captures deterministic encode/decode scenarios so that the .NET
implementation can be validated against the official Python sentencepiece
package. Fixtures are emitted under ``tests/_TestData/<model>`` as
``python-sentencepiece-benchmark.json``.
"""

from __future__ import annotations

import argparse
import json
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


@dataclass(frozen=True)
class ModelSpec:
    display_name: str
    repo_id: str
    model_file: str


MODEL_SPECS: Dict[str, ModelSpec] = {
    "google-mt5-small": ModelSpec("mT5 Small", "google/mt5-small", "spiece.model"),
    "google-flan-t5-base": ModelSpec("Flan-T5 Base", "google/flan-t5-base", "spiece.model"),
    "t5-small": ModelSpec("T5 Small", "t5-small", "spiece.model"),
    "openchat-3.5-1210": ModelSpec("OpenChat 3.5 1210", "openchat/openchat-3.5-1210", "tokenizer.model"),
}


CASE_DEFINITIONS: Sequence[Dict[str, Sequence[str]]] = (
    {
        "description": "Short compliance update",
        "text": "SentencePiece enables nightly compliance summaries for analysts.",
        "batch_texts": (
            "SentencePiece enables nightly compliance summaries for analysts.",
            "SentencePiece enables nightly incident summaries for analysts.",
            "SentencePiece enables nightly compliance summaries for auditors.",
        ),
    },
    {
        "description": "Hyphenated identifiers",
        "text": "Queue-42 flagged policy-drift events during the 03:17 UTC window.",
        "batch_texts": (
            "Queue-42 flagged policy-drift events during the 03:17 UTC window.",
            "Queue-42 flagged policy-drift events during the 09:45 UTC window.",
            "Queue-42 flagged policy-drift events during the 12:05 UTC window.",
        ),
    },
    {
        "description": "Multi-sentence telemetry",
        "text": (
            "Telemetry dashboards surface multilingual traces so reviewers can verify policy coverage. "
            "Compliance leads archive the summaries for the morning briefing."
        ),
        "batch_texts": (
            "Telemetry dashboards surface multilingual traces so reviewers can verify policy coverage.",
            "Compliance leads archive the summaries for the morning briefing.",
            "Telemetry dashboards surface multilingual traces for nightly auditing responsibilities.",
        ),
    },
)


def ensure_model_path(model_root: Path, model_file: str) -> Path:
    candidate = model_root / model_file
    if not candidate.exists():
        raise RuntimeError(
            f"Model asset '{model_file}' is missing from '{model_root}'. Run tests/Py/Common/restore_test_data.py first."
        )
    return candidate


def encode_sample(
    processor: spm.SentencePieceProcessor,
    description: str,
    text: str,
    batch_texts: Sequence[str],
) -> Dict[str, object]:
    ids = [int(value) for value in processor.encode(text, out_type=int)]
    pieces = [str(value) for value in processor.encode(text, out_type=str)]
    decoded = processor.decode(ids)

    decoded_from_pieces = processor.decode(pieces)
    if decoded != decoded_from_pieces:
        raise RuntimeError("Single decode mismatch between ids and pieces pathways.")

    batch_ids: List[List[int]] = []
    batch_pieces: List[List[str]] = []
    for item in batch_texts:
        batch_ids.append([int(value) for value in processor.encode(item, out_type=int)])
        batch_pieces.append([str(value) for value in processor.encode(item, out_type=str)])

    batch_decoded = list(processor.decode(batch_ids))
    decoded_from_pieces_batch = list(processor.decode(batch_pieces))
    if batch_decoded != decoded_from_pieces_batch:
        raise RuntimeError("Batch decode mismatch between ids and pieces pathways.")

    return {
        "description": description,
        "text": text,
        "ids": ids,
        "pieces": pieces,
        "decoded": decoded,
        "batchTexts": list(batch_texts),
        "batchIds": batch_ids,
        "batchPieces": batch_pieces,
        "batchDecoded": batch_decoded,
    }


def build_payload(processor: spm.SentencePieceProcessor) -> List[Dict[str, object]]:
    payload: List[Dict[str, object]] = []
    for case in CASE_DEFINITIONS:
        payload.append(
            encode_sample(
                processor,
                str(case["description"]),
                str(case["text"]),
                tuple(case["batch_texts"]),
            )
        )
    return payload


def write_fixture(model: str, spec: ModelSpec, output_dir: Path, payload: Iterable[Dict[str, object]]) -> Path:
    destination = output_dir / model / "python-sentencepiece-benchmark.json"
    destination.parent.mkdir(parents=True, exist_ok=True)

    metadata = {
        "model": model,
        "display_name": spec.display_name,
        "repo_id": spec.repo_id,
        "generated_at": time.strftime("%Y-%m-%dT%H:%M:%SZ", time.gmtime()),
        "sentencepiece_version": getattr(spm, "__version__", "unknown"),
        "assets": {"model": spec.model_file},
    }

    content = {
        "metadata": metadata,
        "cases": list(payload),
    }

    destination.write_text(json.dumps(content, indent=2), encoding="utf-8")
    return destination


def process_model(model: str, output_dir: Path) -> Path:
    spec = MODEL_SPECS[model]
    model_root = output_dir / model
    model_path = ensure_model_path(model_root, spec.model_file)

    processor = spm.SentencePieceProcessor()
    if not processor.Load(str(model_path)):
        raise RuntimeError(f"Failed to load SentencePiece model from '{model_path}'.")

    payload = build_payload(processor)
    return write_fixture(model, spec, output_dir, payload)


def run(models: Sequence[str], output_dir: Path) -> None:
    for model in models:
        try:
            destination = process_model(model, output_dir)
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
        choices=sorted(MODEL_SPECS.keys()),
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
    models = args.models or list(MODEL_SPECS.keys())
    run(models, args.output_dir)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
