#!/usr/bin/env python3
"""Generate TikToken parity fixtures from the shared tokenization contract.

The output mirrors the structure consumed by the .NET Tiktoken parity
integration tests.
"""

# pylint: disable=protected-access

from __future__ import annotations

import argparse
import base64
import json
import sys
from dataclasses import dataclass
from datetime import datetime, timezone
from functools import lru_cache
from pathlib import Path
from typing import Sequence

try:
    import tiktoken
except ModuleNotFoundError as exc:  # pragma: no cover - helpful runtime guidance
    raise SystemExit(
        "The 'tiktoken' package is required. Install it with 'pip install tiktoken'."
    ) from exc

REPO_ROOT = Path(__file__).resolve().parents[4]
DEFAULT_OUTPUT_ROOT = REPO_ROOT / "tests" / "_TestData_Tiktoken"
CONTRACT_TARGET = "tiktoken"

if str(REPO_ROOT) not in sys.path:
    sys.path.insert(0, str(REPO_ROOT))

from tests.Py.Common.tokenization_contract import (  # pylint: disable=wrong-import-position
    hash_string,
    hash_string_sequence,
    hash_uint32_sequence,
    load_tokenization_cases,
)


@dataclass(frozen=True)
class ModelSpec:
    folder: str
    encoding_name: str
    display_name: str
    model_name: str | None = None


MODEL_SPECS: Sequence[ModelSpec] = (
    ModelSpec(
        folder="openai-gpt2",
        encoding_name="gpt2",
        model_name="gpt2",
        display_name="OpenAI GPT-2",
    ),
    ModelSpec(
        folder="openai-r50k_base",
        encoding_name="r50k_base",
        model_name="text-ada-001",
        display_name="OpenAI r50k_base",
    ),
    ModelSpec(
        folder="openai-p50k_base",
        encoding_name="p50k_base",
        model_name="text-davinci-003",
        display_name="OpenAI p50k_base",
    ),
    ModelSpec(
        folder="openai-p50k_edit",
        encoding_name="p50k_edit",
        model_name="text-davinci-edit-001",
        display_name="OpenAI p50k_edit",
    ),
    ModelSpec(
        folder="openai-cl100k_base",
        encoding_name="cl100k_base",
        model_name="gpt-3.5-turbo",
        display_name="OpenAI cl100k_base",
    ),
    ModelSpec(
        folder="openai-o200k_base",
        encoding_name="o200k_base",
        model_name="gpt-4o",
        display_name="OpenAI o200k_base",
    ),
    ModelSpec(
        folder="openai-o200k_harmony",
        encoding_name="o200k_harmony",
        display_name="OpenAI o200k_harmony",
    ),
)


def build_batch_inputs(case: dict) -> Sequence[str]:
    batch = case.get("batch", {})
    texts: list[str] = list(batch.get("texts") or [])
    if texts:
        return texts

    count = int(batch.get("count", 0))
    base = case["single"]["text"]
    return [f"{base} [sample:{index:02}]" for index in range(count)]


def write_mergeable_ranks(encoding: tiktoken.Encoding, destination: Path) -> int:
    items = sorted(encoding._mergeable_ranks.items(), key=lambda pair: pair[1])  # type: ignore[attr-defined]
    with destination.open("w", encoding="ascii", newline="\n") as stream:
        for token_bytes, rank in items:
            encoded = base64.b64encode(token_bytes).decode("ascii")
            stream.write(f"{encoded} {rank}\n")
    return items[-1][1] if items else 0


def create_case_payload(encoding: tiktoken.Encoding, case: dict) -> dict:
    single_spec = case["single"]
    single_text = single_spec["text"]
    single_tokens = encoding.encode(single_text)
    single_decoded = encoding.decode(single_tokens)

    batch_texts = list(build_batch_inputs(case))
    batch_encodings: list[dict] = []
    for text in batch_texts:
        tokens = encoding.encode(text)
        decoded = encoding.decode(tokens)
        batch_encodings.append(
            {
                "tokenIds": tokens,
                "idsHash": hash_uint32_sequence(tokens),
                "decodedHash": hash_string(decoded),
                "length": len(tokens),
            }
        )

    batch_decoded_hash = hash_string_sequence([encoding.decode(encoding.encode(text)) for text in batch_texts])

    payload = {
        "contractId": case.get("id"),
        "length": case.get("length"),
        "description": case.get("description"),
        "options": case.get("options", {}),
        "single": {
            "text": single_text,
            "textHash": single_spec.get("textHash"),
            "tokenIds": single_tokens,
            "idsHash": hash_uint32_sequence(single_tokens),
            "decodedHash": hash_string(single_decoded),
            "length": len(single_tokens),
        },
        "batch": {
            "count": len(batch_texts),
            "texts": batch_texts,
            "textsHash": hash_string_sequence(batch_texts),
            "encodings": batch_encodings,
            "decodedHash": batch_decoded_hash,
        },
    }

    pair_text = single_spec.get("pairText")
    if pair_text:
        payload["single"]["pairText"] = pair_text
        payload["single"]["pairTextHash"] = single_spec.get("pairTextHash")

    return payload


def resolve_encoding(model: ModelSpec) -> tiktoken.Encoding:
    try:
        return tiktoken.get_encoding(model.encoding_name)
    except ValueError:
        if model.encoding_name != "o200k_harmony":
            raise
        return build_o200k_harmony_encoding()  # pragma: no cover - executes only when plugin missing


@lru_cache(maxsize=1)
def build_o200k_harmony_encoding() -> tiktoken.Encoding:
    base_encoding = tiktoken.get_encoding("o200k_base")

    harmony_tokens = {
        **base_encoding._special_tokens,
        "<|startoftext|>": 199998,
        "<|endoftext|>": 199999,
        "<|reserved_200000|>": 200000,
        "<|reserved_200001|>": 200001,
        "<|return|>": 200002,
        "<|constrain|>": 200003,
        "<|reserved_200004|>": 200004,
        "<|channel|>": 200005,
        "<|start|>": 200006,
        "<|end|>": 200007,
        "<|message|>": 200008,
        "<|reserved_200009|>": 200009,
        "<|reserved_200010|>": 200010,
        "<|reserved_200011|>": 200011,
        "<|call|>": 200012,
    }
    harmony_tokens.update({f"<|reserved_{value}|>": value for value in range(200013, 201088)})

    # We rehydrate the encoding using the known base parameters so downstream consumers see the
    # expected name and token inventory even when the plugin has not registered the harmony variant.
    return tiktoken.Encoding(
        name="o200k_harmony",
        pat_str=base_encoding._pat_str,
        mergeable_ranks=base_encoding._mergeable_ranks,
        special_tokens=harmony_tokens,
    )


def generate_model(model: ModelSpec, output_root: Path) -> None:
    cases = load_tokenization_cases(CONTRACT_TARGET)

    encoding = resolve_encoding(model)
    if model.model_name:
        alias_encoding = tiktoken.encoding_for_model(model.model_name)
        if alias_encoding.name != encoding.name:
            raise RuntimeError(
                f"Model '{model.model_name}' resolves to '{alias_encoding.name}' instead of '{encoding.name}'."
            )

    model_root = output_root / model.folder
    model_root.mkdir(parents=True, exist_ok=True)

    mergeable_ranks_path = model_root / "mergeable_ranks.tiktoken"
    write_mergeable_ranks(encoding, mergeable_ranks_path)

    special_tokens = dict(encoding._special_tokens.items())
    explicit_vocab_size = len(encoding._mergeable_ranks) + len(special_tokens)
    python_vocab_size = getattr(encoding, "n_vocab", None)

    payload = {
        "metadata": {
            "encoding": encoding.name,
            "displayName": model.display_name,
            "model": model.model_name or encoding.name,
            "aliases": [model.model_name] if model.model_name else [],
            "generatedAt": datetime.now(timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ"),
            "tiktokenVersion": getattr(tiktoken, "__version__", "unknown"),
            "mergeableRanksFile": mergeable_ranks_path.name,
            "specialTokens": special_tokens,
            "pattern": getattr(encoding, "_pat_str", "(?s)."),
            "explicitVocabularySize": explicit_vocab_size,
            "pythonVocabularySize": python_vocab_size,
        },
        "cases": [create_case_payload(encoding, case) for case in cases],
    }

    destination = model_root / "python-benchmark.json"
    with destination.open("w", encoding="utf-8") as stream:
        json.dump(payload, stream, indent=2)
        stream.write("\n")


def parse_arguments() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Generate TikToken parity fixtures")
    parser.add_argument(
        "--output",
        type=Path,
        default=DEFAULT_OUTPUT_ROOT,
        help=f"Output root directory (default: {DEFAULT_OUTPUT_ROOT})",
    )
    return parser.parse_args()


def main() -> None:
    args = parse_arguments()
    for model in MODEL_SPECS:
        generate_model(model, args.output)


if __name__ == "__main__":
    main()
