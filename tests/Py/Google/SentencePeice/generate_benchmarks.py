#!/usr/bin/env python3
"""Generate parity benchmarks for the managed SentencePiece bindings.

The emitted fixtures mirror the structure produced by the Hugging Face parity
tooling so the .NET test suite can validate hashes, decoded payloads, and batch
behaviour across every model that ships a SentencePiece vocabulary inside
``tests/_testdata_sentencepeice``.
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
if str(REPO_ROOT) not in sys.path:
    sys.path.insert(0, str(REPO_ROOT))

from tests.Py.Common.tokenization_contract import (
    hash_int32_sequence,
    hash_offsets,
    hash_optional_int32_sequence,
    hash_string,
    hash_string_sequence,
    hash_uint32_sequence,
    load_tokenization_cases,
)

DEFAULT_OUTPUT_DIR = REPO_ROOT / "tests" / "_testdata_sentencepeice"


@dataclass(frozen=True)
class ModelSpec:
    display_name: str
    repo_id: str
    model_file: str


@dataclass(frozen=True)
class CaseDefinition:
    contract_id: str
    length: str
    description: str
    text: str
    batch_texts: Sequence[str]
    options: Dict[str, object]
    pair_text: str | None = None
    batch_pair_texts: Sequence[str] | None = None


def _load_case_definitions() -> Sequence[CaseDefinition]:
    raw_cases = load_tokenization_cases("sentencepiece")
    definitions: List[CaseDefinition] = []
    for entry in raw_cases:
        case_id = entry.get("id")
        if not isinstance(case_id, str) or not case_id:
            raise RuntimeError("Tokenization contract entry is missing an identifier.")
        single = entry.get("single") or {}
        batch = entry.get("batch") or {}
        text = single.get("text")
        batch_texts = batch.get("texts")
        length = entry.get("length")
        description = entry.get("description")
        if not isinstance(text, str) or not isinstance(batch_texts, list):
            raise RuntimeError("Tokenization contract entry is missing required sentencepiece fields.")
        if not isinstance(length, str) or not isinstance(description, str):
            raise RuntimeError("Tokenization contract entry is missing metadata for SentencePiece.")

        text_hash = single.get("textHash")
        if not isinstance(text_hash, str) or hash_string(text) != text_hash:
            raise RuntimeError(f"SentencePiece contract case '{case_id}' single text hash mismatch.")

        pair_text_value = single.get("pairText")
        pair_text_hash = single.get("pairTextHash")
        if pair_text_value is not None:
            if not isinstance(pair_text_value, str) or not isinstance(pair_text_hash, str):
                raise RuntimeError(f"SentencePiece contract case '{case_id}' pair payload is invalid.")
            if hash_string(pair_text_value) != pair_text_hash:
                raise RuntimeError(f"SentencePiece contract case '{case_id}' pair text hash mismatch.")
            pair_text: str | None = pair_text_value
        else:
            pair_text = None

        batch_count = batch.get("count")
        if not isinstance(batch_count, int):
            raise RuntimeError(f"SentencePiece contract case '{case_id}' batch count is invalid.")

        batch_text_list = [str(value) for value in batch_texts]
        if len(batch_text_list) != batch_count:
            raise RuntimeError(
                f"SentencePiece contract case '{case_id}' batch length mismatch: declared {batch_count}, actual {len(batch_text_list)}."
            )

        texts_hash = batch.get("textsHash")
        if not isinstance(texts_hash, str) or hash_string_sequence(batch_text_list) != texts_hash:
            raise RuntimeError(f"SentencePiece contract case '{case_id}' batch text hash mismatch.")

        pair_batch_raw = batch.get("pairTexts")
        pair_batch: Sequence[str] | None = None
        if isinstance(pair_batch_raw, list) and pair_batch_raw:
            pair_batch_list = [str(value) for value in pair_batch_raw]
            if len(pair_batch_list) != batch_count:
                raise RuntimeError(
                    f"SentencePiece contract case '{case_id}' batch pair length mismatch: declared {batch_count}, actual {len(pair_batch_list)}."
                )
            pair_hash = batch.get("pairTextsHash")
            if not isinstance(pair_hash, str) or hash_string_sequence(pair_batch_list) != pair_hash:
                raise RuntimeError(f"SentencePiece contract case '{case_id}' batch pair hash mismatch.")
            pair_batch = tuple(pair_batch_list)

        options: Dict[str, object] = {
            "addSpecialTokens": False,
            "decodeSkipSpecialTokens": True,
        }
        options.update(entry.get("options") or {})
        definitions.append(
            CaseDefinition(
                contract_id=case_id,
                length=length,
                description=description,
                text=text,
                batch_texts=tuple(batch_text_list),
                options=options,
                pair_text=pair_text,
                batch_pair_texts=pair_batch,
            )
        )
    return definitions


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
        "idsHash": hash_int32_sequence(ids),
        "tokensHash": hash_string_sequence(pieces),
        "typeIdsHash": hash_uint32_sequence(type_ids),
        "attentionMaskHash": hash_uint32_sequence(attention_mask),
        "specialTokensMaskHash": hash_uint32_sequence(special_tokens_mask),
        "offsetsHash": hash_offsets(offsets),
        "wordIdsHash": hash_optional_int32_sequence(word_ids),
        "sequenceIdsHash": hash_optional_int32_sequence(sequence_ids),
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
        "textHash": hash_string(case.text),
        "encoding": summarize_encoding(single_ids, single_pieces),
        "decodedHash": hash_string(single_decoded),
    }
    if case.pair_text is not None:
        single_payload["pairText"] = case.pair_text
        single_payload["pairTextHash"] = hash_string(case.pair_text)

    batch_payload = {
        "count": len(batch_texts),
        "texts": batch_texts,
        "textsHash": hash_string_sequence(batch_texts),
        "encodings": [summarize_encoding(ids, pieces) for ids, pieces in zip(batch_ids, batch_pieces)],
        "decodedHash": hash_string_sequence(batch_decoded),
    }
    if case.batch_pair_texts:
        pair_batch = list(case.batch_pair_texts)
        batch_payload["pairTexts"] = pair_batch
        batch_payload["pairTextsHash"] = hash_string_sequence(pair_batch)

    return {
        "contractId": case.contract_id,
        "length": case.length,
        "description": case.description,
        "options": case.options,
        "single": single_payload,
        "batch": batch_payload,
    }


def build_payload(processor: spm.SentencePieceProcessor) -> List[Dict[str, object]]:
    return [generate_case_payload(processor, case) for case in _load_case_definitions()]


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
    help="Destination directory for generated assets (defaults to tests/_testdata_sentencepeice).",
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
