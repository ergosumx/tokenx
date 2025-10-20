#!/usr/bin/env python3
"""Generate reference benchmark corpora using the Hugging Face tokenizers package.

The script mirrors the .NET benchmark scenarios so that Python parity data can be
produced on demand. Assets are written under ``tests/_TestData/<model>``.
"""

from __future__ import annotations

import argparse
import hashlib
import json
import os
import math
import struct
import sys
import time
import traceback
from dataclasses import dataclass, field
from enum import Enum
from pathlib import Path
from random import Random
from typing import Any, Dict, Iterable, List, Sequence, Tuple

try:
    from tokenizers import Encoding, Tokenizer, __version__ as TOKENIZERS_VERSION
except ImportError as exc:  # pragma: no cover - defensive wiring for missing deps
    raise SystemExit(
        "The 'tokenizers' package is required. Activate the workspace .venv and install dependencies."
    ) from exc

try:
    from huggingface_hub import hf_hub_download
    try:
        from huggingface_hub.errors import EntryNotFoundError, HfHubHTTPError
    except ImportError:  # pragma: no cover - compatibility shim for older hubs
        from huggingface_hub import HfHubHTTPError  # type: ignore[attr-defined]
        try:
            from huggingface_hub import EntryNotFoundError  # type: ignore[attr-defined]
        except ImportError:  # pragma: no cover - final fallback for very old hubs
            EntryNotFoundError = HfHubHTTPError  # type: ignore[assignment]
except ImportError as exc:  # pragma: no cover - defensive wiring for missing deps
    raise SystemExit(
        "The 'huggingface_hub' package is required. Activate the workspace .venv and install dependencies."
    ) from exc

try:
    from transformers import AutoTokenizer as TransformersAutoTokenizer
    from transformers import PreTrainedTokenizerFast, __version__ as TRANSFORMERS_VERSION
except ImportError as exc:  # pragma: no cover - defensive wiring for missing deps
    raise SystemExit(
        "The 'transformers' package is required. Activate the workspace .venv and install dependencies."
    ) from exc

try:
    import jinja2  # noqa: F401
except ImportError as exc:  # pragma: no cover - chat template evaluation safety
    raise SystemExit(
        "The 'jinja2' package is required to evaluate chat templates. Activate the workspace .venv and install dependencies."
    ) from exc


REPO_ROOT = Path(__file__).resolve().parents[3]
DEFAULT_OUTPUT_DIR = REPO_ROOT / "tests" / "_TestData"
DEFAULT_VENV = REPO_ROOT / ".venv"


class SequenceLength(Enum):
    TINY = "Tiny"
    SHORT = "Short"
    MEDIUM = "Medium"
    LONG = "Long"
    EXTRA_LONG = "ExtraLong"
    MASSIVE = "Massive"
    ADVANCED_PAIR = "AdvancedPair"
    ADVANCED_TRUNCATION = "AdvancedTruncation"


@dataclass(frozen=True)
class TruncationConfig:
    max_length: int
    stride: int


@dataclass(frozen=True)
class CaseOptions:
    add_special_tokens: bool = False
    decode_skip_special_tokens: bool = True
    truncation: TruncationConfig | None = None


@dataclass(frozen=True)
class BenchmarkCase:
    length: SequenceLength
    description: str
    text: str
    batch: Sequence[str]
    options: CaseOptions = field(default_factory=CaseOptions)
    pair_text: str | None = None
    batch_pair_texts: Sequence[str] | None = None


@dataclass(frozen=True)
class ModelSpec:
    name: str
    repo_id: str
    required_files: Sequence[str] = field(default_factory=tuple)
    optional_files: Sequence[str] = field(default_factory=tuple)
    revision: str | None = None
    trust_remote_code: bool = False
    supports_fast_tokenizer: bool = True


@dataclass(frozen=True)
class ChatScenario:
    description: str
    messages: List[Dict[str, Any]]
    add_generation_prompt: bool = True


MODEL_SPECS: Dict[str, ModelSpec] = {
    "gpt2": ModelSpec(
        name="GPT-2",
        repo_id="gpt2",
        required_files=("tokenizer.json",),
        optional_files=("vocab.json", "merges.txt"),
    ),
    "gpt2-medium": ModelSpec(
        name="GPT-2 Medium",
        repo_id="gpt2-medium",
        required_files=("tokenizer.json",),
        optional_files=("vocab.json", "merges.txt"),
    ),
    "gpt2-xl": ModelSpec(
        name="GPT-2 XL",
        repo_id="gpt2-xl",
        required_files=("tokenizer.json",),
        optional_files=("vocab.json", "merges.txt"),
    ),
    "bert-base-uncased": ModelSpec(
        name="BERT Base Uncased",
        repo_id="bert-base-uncased",
        required_files=("tokenizer.json",),
        optional_files=("vocab.txt",),
    ),
    "distilbert-base-uncased": ModelSpec(
        name="DistilBERT Base Uncased",
        repo_id="distilbert-base-uncased",
        required_files=("tokenizer.json",),
        optional_files=("vocab.txt",),
    ),
    "roberta-base": ModelSpec(
        name="RoBERTa Base",
        repo_id="roberta-base",
        required_files=("tokenizer.json",),
        optional_files=("vocab.json", "merges.txt"),
    ),
    "roberta-large": ModelSpec(
        name="RoBERTa Large",
        repo_id="roberta-large",
        required_files=("tokenizer.json",),
        optional_files=("vocab.json", "merges.txt"),
    ),
    "xlm-roberta-base": ModelSpec(
        name="XLM-RoBERTa Base",
        repo_id="xlm-roberta-base",
        required_files=("tokenizer.json",),
        optional_files=("sentencepiece.bpe.model",),
    ),
    "albert-base-v2": ModelSpec(
        name="ALBERT Base v2",
        repo_id="albert-base-v2",
        required_files=("tokenizer.json",),
        optional_files=("spiece.model",),
    ),
    "deberta-v3-base": ModelSpec(
        name="DeBERTa v3 Base",
        repo_id="microsoft/deberta-v3-base",
        required_files=("tokenizer.json",),
        optional_files=("vocab.json", "merges.txt"),
    ),
    "microsoft-layoutlmv3-base": ModelSpec(
        name="LayoutLMv3 Base",
        repo_id="microsoft/layoutlmv3-base",
        required_files=("tokenizer.json",),
        optional_files=("vocab.json", "merges.txt"),
    ),
    "sentence-transformers-all-MiniLM-L6-v2": ModelSpec(
        name="sentence-transformers/all-MiniLM-L6-v2",
        repo_id="sentence-transformers/all-MiniLM-L6-v2",
        required_files=("tokenizer.json",),
        optional_files=("vocab.txt",),
    ),
    # ASR tokenizers
    "openai-whisper-base": ModelSpec(
        name="OpenAI Whisper Base",
        repo_id="openai/whisper-base",
        required_files=("tokenizer.json",),
        optional_files=("vocab.json", "merges.txt", "special_tokens_map.json"),
    ),
    "openai-whisper-medium": ModelSpec(
        name="OpenAI Whisper Medium",
        repo_id="openai/whisper-medium",
        required_files=("tokenizer.json",),
        optional_files=("vocab.json", "merges.txt", "special_tokens_map.json"),
    ),
    # Computer vision OCR tokenizers
    "microsoft-trocr-base-handwritten": ModelSpec(
        name="TrOCR Base Handwritten",
        repo_id="microsoft/trocr-base-handwritten",
        required_files=("tokenizer.json",),
        optional_files=("spiece.model",),
    ),
    "microsoft-trocr-base-stage1": ModelSpec(
        name="TrOCR Base Stage1",
        repo_id="microsoft/trocr-base-stage1",
        required_files=("tokenizer.json",),
        optional_files=("spiece.model",),
    ),
    # Multimodal captioning/tokenization
    "salesforce-blip-image-captioning-base": ModelSpec(
        name="BLIP Image Captioning Base",
        repo_id="Salesforce/blip-image-captioning-base",
        required_files=("tokenizer.json",),
        optional_files=("vocab.json", "merges.txt", "special_tokens_map.json"),
    ),
    "microsoft-git-base": ModelSpec(
        name="GIT Base",
        repo_id="microsoft/git-base",
        required_files=("tokenizer.json",),
        optional_files=("vocab.json", "merges.txt", "special_tokens_map.json"),
    ),
    "microsoft-kosmos-2-patch14-224": ModelSpec(
        name="Kosmos-2 Patch14-224",
        repo_id="microsoft/kosmos-2-patch14-224",
        required_files=("tokenizer.json",),
        optional_files=("vocab.json", "merges.txt", "special_tokens_map.json"),
    ),
    "facebook-bart-large": ModelSpec(
        name="Facebook BART Large",
        repo_id="facebook/bart-large",
        required_files=("tokenizer.json",),
        optional_files=("vocab.json", "merges.txt"),
    ),
    "facebook-mbart-large-50": ModelSpec(
        name="Facebook mBART Large 50",
        repo_id="facebook/mbart-large-50",
        required_files=("tokenizer.json",),
        optional_files=("sentencepiece.bpe.model",),
    ),
    "google-mt5-small": ModelSpec(
        name="mT5 Small",
        repo_id="google/mt5-small",
        required_files=("tokenizer.json",),
        optional_files=("spiece.model",),
    ),
    "google-flan-t5-base": ModelSpec(
        name="Flan-T5 Base",
        repo_id="google/flan-t5-base",
        required_files=("tokenizer.json",),
        optional_files=("spiece.model",),
    ),
    "google-pegasus-large": ModelSpec(
        name="Pegasus Large",
        repo_id="google/pegasus-large",
        required_files=("tokenizer.json",),
        optional_files=("spiece.model",),
    ),
    "allenai-longformer-base-4096": ModelSpec(
        name="Longformer Base 4096",
        repo_id="allenai/longformer-base-4096",
        required_files=("tokenizer.json",),
        optional_files=("vocab.json", "merges.txt"),
    ),
    "google-bigbird-roberta-base": ModelSpec(
        name="BigBird RoBERTa Base",
        repo_id="google/bigbird-roberta-base",
        required_files=("tokenizer.json",),
        optional_files=("spiece.model",),
    ),
    "t5-small": ModelSpec(
        name="T5 Small",
        repo_id="t5-small",
        required_files=("tokenizer.json",),
        optional_files=("spiece.model",),
    ),
    "qwen1p5-7b-chat": ModelSpec(
        name="Qwen1.5 7B Chat",
        repo_id="Qwen/Qwen1.5-7B-Chat",
        required_files=("tokenizer.json",),
        optional_files=("tokenizer_config.json", "special_tokens_map.json", "vocab.json", "merges.txt"),
        trust_remote_code=True,
    ),
    "meta-llama-3-8b-instruct": ModelSpec(
        name="Meta LLaMA 3 8B Instruct",
        repo_id="meta-llama/Meta-Llama-3-8B-Instruct",
        required_files=("tokenizer.json",),
        optional_files=("tokenizer_config.json", "special_tokens_map.json", "vocab.json", "merges.txt"),
    ),
    "zephyr-7b-alpha": ModelSpec(
        name="Zephyr 7B Alpha",
        repo_id="HuggingFaceH4/zephyr-7b-alpha",
        required_files=("tokenizer.json",),
        optional_files=("tokenizer_config.json", "special_tokens_map.json", "tokenizer.model"),
    ),
    "mistral-7b-instruct-v0_2": ModelSpec(
        name="Mistral 7B Instruct v0.2",
        repo_id="mistralai/Mistral-7B-Instruct-v0.2",
        required_files=("tokenizer.json",),
        optional_files=("tokenizer_config.json", "special_tokens_map.json", "tokenizer.model"),
    ),
    "mixtral-8x7b-instruct-v0_1": ModelSpec(
        name="Mixtral 8x7B Instruct v0.1",
        repo_id="mistralai/Mixtral-8x7B-Instruct-v0.1",
        required_files=("tokenizer.json",),
        optional_files=("tokenizer_config.json", "special_tokens_map.json", "tokenizer.model"),
    ),
    "gemma-2-2b-it": ModelSpec(
        name="Gemma 2 2B IT",
        repo_id="google/gemma-2-2b-it",
        required_files=("tokenizer.json",),
        optional_files=("tokenizer_config.json", "special_tokens_map.json", "tokenizer.model"),
    ),
    "falcon-7b-instruct": ModelSpec(
        name="Falcon 7B Instruct",
        repo_id="tiiuae/falcon-7b-instruct",
        required_files=("tokenizer.json",),
        optional_files=("tokenizer_config.json", "special_tokens_map.json", "vocab.json", "merges.txt"),
    ),
    "openchat-3.5-1210": ModelSpec(
        name="OpenChat 3.5 1210",
        repo_id="openchat/openchat-3.5-1210",
        required_files=("tokenizer.json",),
        optional_files=("tokenizer_config.json", "special_tokens_map.json", "tokenizer.model"),
    ),
    "vicuna-7b-v1_5": ModelSpec(
        name="Vicuna 7B v1.5",
        repo_id="lmsys/vicuna-7b-v1.5",
        required_files=("tokenizer.json",),
        optional_files=("tokenizer_config.json", "special_tokens_map.json", "tokenizer.model"),
    ),
    "llava-1.5-7b": ModelSpec(
        name="LLaVA 1.5 7B",
        repo_id="liuhaotian/llava-v1.5-7b",
        required_files=("tokenizer.json",),
        optional_files=("tokenizer_config.json", "special_tokens_map.json", "tokenizer.model"),
    ),
}

BENCHMARK_MODEL_IDS: Tuple[str, ...] = tuple(
    sorted(name for name, spec in MODEL_SPECS.items() if spec.supports_fast_tokenizer)
)

CHAT_SYSTEM_PROMPT = (
    "You are a compliance co-pilot guiding investigators through telemetry anomalies and policy drift."
)
CHAT_SUMMARY_PROMPT = "Summarize the overnight telemetry anomalies in two bullet points."
CHAT_SUMMARY_RESPONSE = (
    "- Streaming pipeline flagged delayed encrypted payload batches.\n"
    "- Access policy drift triggered an audit event across the regulated region."
)
CHAT_FOLLOW_UP_PROMPT = (
    "Draft a follow-up question referencing threshold drift and the redaction policy checks."
)
CHAT_ACTION_PROMPT = "Outline two action items for the remediation stand-up."
CHAT_ACTION_RESPONSE = (
    "1. Validate the threshold drift monitors against yesterday's baseline.\n"
    "2. Circulate the redaction policy diff with compliance stakeholders."
)
CHAT_GEMMA_INITIAL = "Catalogue the telemetry noise filters applied overnight."
CHAT_GEMMA_RESPONSE = (
    "We applied rate limiting, de-duplication for stale IDs, and redaction on audit summaries."
)
CHAT_GEMMA_FOLLOW_UP = (
    "Add a note about enabling drift monitors before tomorrow's compliance briefing."
)
CHAT_GEMMA_STATUS_PROMPT = "Provide a one sentence status update on the redaction pipeline."
CHAT_GEMMA_STATUS_RESPONSE = (
    "Redaction pipeline cleared all batches and logged zero policy deviations."
)


def _build_standard_chat_scenarios(include_metadata: bool = False) -> List[ChatScenario]:
    primary_messages: List[Dict[str, Any]] = [
        {"role": "system", "content": CHAT_SYSTEM_PROMPT},
        {"role": "user", "content": CHAT_SUMMARY_PROMPT},
        {"role": "assistant", "content": CHAT_SUMMARY_RESPONSE},
        {"role": "user", "content": CHAT_FOLLOW_UP_PROMPT},
    ]
    if include_metadata:
        primary_messages[-1] = {
            "role": "user",
            "content": CHAT_FOLLOW_UP_PROMPT,
            "name": "audit-ops",
        }

    closure_messages: List[Dict[str, Any]] = [
        {"role": "system", "content": CHAT_SYSTEM_PROMPT},
        {"role": "user", "content": CHAT_ACTION_PROMPT},
        {"role": "assistant", "content": CHAT_ACTION_RESPONSE},
    ]

    return [
        ChatScenario(
            description="System priming with follow-up request",
            messages=primary_messages,
            add_generation_prompt=True,
        ),
        ChatScenario(
            description="Assistant finalizes remediation guidance",
            messages=closure_messages,
            add_generation_prompt=False,
        ),
    ]


def _build_gemma_chat_scenarios() -> List[ChatScenario]:
    primary_messages: List[Dict[str, Any]] = [
        {"role": "user", "content": CHAT_GEMMA_INITIAL},
        {"role": "assistant", "content": CHAT_GEMMA_RESPONSE},
        {"role": "user", "content": CHAT_GEMMA_FOLLOW_UP},
    ]
    closure_messages: List[Dict[str, Any]] = [
        {"role": "user", "content": CHAT_GEMMA_STATUS_PROMPT},
        {"role": "assistant", "content": CHAT_GEMMA_STATUS_RESPONSE},
    ]

    return [
        ChatScenario(
            description="User initiated compliance review",
            messages=primary_messages,
            add_generation_prompt=True,
        ),
        ChatScenario(
            description="Assistant provides direct status update",
            messages=closure_messages,
            add_generation_prompt=False,
        ),
    ]


CHAT_MODEL_SCENARIOS: Dict[str, List[ChatScenario]] = {
    "qwen1p5-7b-chat": _build_standard_chat_scenarios(include_metadata=True),
    "meta-llama-3-8b-instruct": _build_standard_chat_scenarios(),
    "zephyr-7b-alpha": _build_standard_chat_scenarios(),
    "mistral-7b-instruct-v0_2": _build_standard_chat_scenarios(),
    "mixtral-8x7b-instruct-v0_1": _build_standard_chat_scenarios(),
    "gemma-2-2b-it": _build_gemma_chat_scenarios(),
    "falcon-7b-instruct": _build_standard_chat_scenarios(),
    "openchat-3.5-1210": _build_standard_chat_scenarios(),
}

BASE_SENTENCE = (
    "In a distant research lab, engineers tuned transformers while monitoring streaming telemetry "
    "from multilingual datasets."
)
SUPPLEMENTARY_SENTENCES = [
    "The models adapted to sudden domain shifts as analysts injected noisy transcripts into the queue.",
    "Meanwhile, the observability stack captured latency spikes and surfaced alerts to the on-call rotation.",
    "A compliance auditor reviewed masked payloads to ensure privacy constraints remained intact.",
    "After midnight, an A/B experiment introduced synthetic dialogue to probe robustness against adversarial prompts.",
    "Statisticians summarized the nightly run, documenting throughput, token distributions, and error trends.",
]

CASE_SPECS = (
    (SequenceLength.TINY, "Tiny (~16 tokens)", 1, 2, 16),
    (SequenceLength.SHORT, "Short (~32 tokens)", 1, 4, 32),
    (SequenceLength.MEDIUM, "Medium (~128 tokens)", 4, 6, 32),
    (SequenceLength.LONG, "Long (~512 tokens)", 16, 8, 32),
    (SequenceLength.EXTRA_LONG, "Extra Long (~1024 tokens)", 32, 12, 32),
    (SequenceLength.MASSIVE, "Massive (~2048 tokens)", 64, 16, 16),
)


def ensure_repo_venv(require_repo_venv: bool) -> None:
    if not require_repo_venv:
        return

    if DEFAULT_VENV.exists():
        current_prefix = Path(sys.prefix).resolve()
        expected = DEFAULT_VENV.resolve()
        if current_prefix != expected:
            raise SystemExit(
                f"Activate the workspace virtual environment at '{expected}' before running this script."
            )


def build_cases() -> List[BenchmarkCase]:
    cases: List[BenchmarkCase] = []
    for length, description, base_repeats, shuffle_seed, batch_size in CASE_SPECS:
        random = Random(shuffle_seed)
        segments: List[str] = []
        for _ in range(base_repeats):
            segments.append(BASE_SENTENCE)
            shuffled = list(SUPPLEMENTARY_SENTENCES)
            random.shuffle(shuffled)
            segments.extend(shuffled)
        text = " ".join(segments)
        batch = [f"{text} [sample:{index:02d}]" for index in range(batch_size)]
        cases.append(
            BenchmarkCase(
                length=length,
                description=description,
                text=text,
                batch=batch,
            )
        )
    cases.extend(build_advanced_cases())
    return cases


def build_advanced_cases() -> List[BenchmarkCase]:
    advanced: List[BenchmarkCase] = []

    pair_text = (
        "Summary: Compliance auditors consolidate redacted findings while moderators "
        "triage multilingual incidents across distributed queues."
    )
    pair_base = " ".join([BASE_SENTENCE, SUPPLEMENTARY_SENTENCES[0], SUPPLEMENTARY_SENTENCES[2]])
    pair_batch = [f"{pair_base} [pair-doc:{index:02d}]" for index in range(8)]
    pair_batch_pairs = [f"{pair_text} [pair-summary:{index:02d}]" for index in range(8)]
    advanced.append(
        BenchmarkCase(
            length=SequenceLength.ADVANCED_PAIR,
            description="Pair encoding with special tokens for document and summary inputs.",
            text=pair_base,
            batch=pair_batch,
            options=CaseOptions(add_special_tokens=True, decode_skip_special_tokens=False),
            pair_text=pair_text,
            batch_pair_texts=pair_batch_pairs,
        )
    )

    overflow_segments: List[str] = []
    for _ in range(12):
        overflow_segments.append(BASE_SENTENCE)
        overflow_segments.extend(SUPPLEMENTARY_SENTENCES)
    overflow_text = " ".join(overflow_segments)
    overflow_batch = [f"{overflow_text} [overflow:{index:02d}]" for index in range(6)]
    advanced.append(
        BenchmarkCase(
            length=SequenceLength.ADVANCED_TRUNCATION,
            description="Truncation with stride generates deterministic overflow encodings.",
            text=overflow_text,
            batch=overflow_batch,
            options=CaseOptions(
                add_special_tokens=True,
                decode_skip_special_tokens=False,
                truncation=TruncationConfig(max_length=256, stride=48),
            ),
        )
    )

    return advanced


COMMON_OPTIONAL_FILES: Tuple[str, ...] = (
    "special_tokens_map.json",
    "added_tokens.json",
)


def ensure_model_assets(model: str, spec: ModelSpec, output_root: Path, force: bool) -> Dict[str, Path]:
    assets_dir = output_root / model
    assets_dir.mkdir(parents=True, exist_ok=True)

    resolved: Dict[str, Path] = {}

    def download(filename: str, required: bool, allow_missing: bool = False) -> bool:
        destination = assets_dir / filename
        if destination.exists() and not force:
            resolved[filename] = destination
            return True

        try:
            path = hf_hub_download(
                repo_id=spec.repo_id,
                filename=filename,
                revision=spec.revision,
                token=os.environ.get("HF_TOKEN"),
                local_dir=str(assets_dir),
                local_dir_use_symlinks=False,
                force_download=force,
            )
        except HfHubHTTPError as exc:
            status = getattr(getattr(exc, "response", None), "status_code", None)
            is_missing = status == 404 or isinstance(exc, EntryNotFoundError)
            is_unauthorized = status in {401, 403}
            if (is_missing or (is_unauthorized and not required)) and (not required or allow_missing):
                return False

            raise RuntimeError(
                f"Failed to download required asset '{filename}' for model '{model}' from repo '{spec.repo_id}'."
            ) from exc

        resolved[filename] = Path(path)
        return True

    has_config = download("tokenizer_config.json", required=True, allow_missing=True)

    required_files = list(dict.fromkeys(spec.required_files))
    if "tokenizer.json" not in required_files:
        required_files.append("tokenizer.json")

    has_tokenizer_json = False
    for filename in required_files:
        allow_missing = filename == "tokenizer.json"
        success = download(filename, required=True, allow_missing=allow_missing)
        if filename == "tokenizer.json":
            has_tokenizer_json = success

    optional_candidates = list(dict.fromkeys((*spec.optional_files, *COMMON_OPTIONAL_FILES)))
    for filename in optional_candidates:
        download(filename, required=False)

    if not has_config or not has_tokenizer_json:
        synthesize_tokenizer_assets(model, spec, assets_dir, resolved)

    sanitize_tokenizer_config(assets_dir / "tokenizer_config.json")
    normalize_special_tokens_map(assets_dir / "special_tokens_map.json")

    return resolved
def synthesize_tokenizer_assets(model: str, spec: ModelSpec, assets_dir: Path, resolved: Dict[str, Path]) -> None:
    token = os.environ.get("HF_TOKEN")
    try:
        tokenizer = TransformersAutoTokenizer.from_pretrained(
            spec.repo_id,
            revision=spec.revision,
            use_fast=True,
            local_files_only=False,
            trust_remote_code=spec.trust_remote_code,
            token=token,
        )
    except Exception as exc:  # pragma: no cover - external dependency failure surface
        raise RuntimeError(
            f"Failed to materialize tokenizer assets for model '{model}' using transformers."
        ) from exc

    tokenizer.save_pretrained(str(assets_dir))

    generated_files = (
        "tokenizer_config.json",
        "tokenizer.json",
        "special_tokens_map.json",
        "added_tokens.json",
    )

    for filename in generated_files:
        candidate = assets_dir / filename
        if candidate.exists():
            resolved.setdefault(filename, candidate)


def load_transformers_tokenizer(assets_dir: Path, spec: ModelSpec) -> PreTrainedTokenizerFast:
    tokenizer = TransformersAutoTokenizer.from_pretrained(
        str(assets_dir),
        use_fast=True,
        local_files_only=True,
        trust_remote_code=spec.trust_remote_code,
    )

    if not isinstance(tokenizer, PreTrainedTokenizerFast):
        raise RuntimeError(
            "The selected tokenizer does not expose a fast backend. Ensure a fast tokenizer is available for parity generation."
        )

    return tokenizer


def load_tokenizers(assets_dir: Path, spec: ModelSpec) -> Tuple[PreTrainedTokenizerFast, Tokenizer]:
    transformers_tokenizer = load_transformers_tokenizer(assets_dir, spec)
    backend = transformers_tokenizer.backend_tokenizer
    backend.no_padding()
    backend.no_truncation()
    return transformers_tokenizer, backend


INT32_MAX = 2_147_483_647


def sanitize_tokenizer_config(config_path: Path) -> None:
    if not config_path.exists():
        return

    try:
        payload = json.loads(config_path.read_text(encoding="utf-8"))
    except (OSError, json.JSONDecodeError):  # pragma: no cover - defensive fallback
        return

    value = payload.get("model_max_length")
    replacement = _normalize_model_max_length(value)
    if replacement is None:
        return

    if replacement == value:
        return

    payload["model_max_length"] = replacement
    config_path.write_text(json.dumps(payload, indent=2), encoding="utf-8")


def _normalize_model_max_length(value: Any) -> int | None:
    if isinstance(value, int):
        if value <= 0 or value > INT32_MAX:
            return INT32_MAX
        return None

    if isinstance(value, float):
        if not math.isfinite(value):
            return INT32_MAX
        candidate = int(value)
        if candidate <= 0 or candidate > INT32_MAX:
            return INT32_MAX
        return candidate

    if isinstance(value, str):
        candidate: int | None = None
        try:
            candidate = int(value)
        except ValueError:
            try:
                candidate_float = float(value)
            except ValueError:
                candidate_float = math.inf

            if math.isfinite(candidate_float):
                candidate = int(candidate_float)

        if candidate is None or candidate <= 0 or candidate > INT32_MAX:
            return INT32_MAX

        return candidate

    if value is None:
        return None

    return INT32_MAX


def normalize_special_tokens_map(map_path: Path) -> None:
    if not map_path.exists():
        return

    try:
        payload = json.loads(map_path.read_text(encoding="utf-8"))
    except (OSError, json.JSONDecodeError):  # pragma: no cover - defensive fallback
        return

    changed = False

    for key in ("bos_token", "eos_token", "pad_token", "unk_token"):
        token = payload.get(key)
        normalized = _normalize_token_definition(token)
        if normalized is token:
            continue

        if normalized is None:
            if key in payload:
                del payload[key]
                changed = True
        else:
            payload[key] = normalized
            changed = True

    tokens = payload.get("additional_special_tokens")
    if isinstance(tokens, list):
        normalized_tokens: List[str] = []
        for item in tokens:
            normalized = _normalize_token_definition(item)
            if normalized is None:
                continue
            normalized_tokens.append(normalized)
            if normalized is not item:
                changed = True

        if normalized_tokens != tokens:
            payload["additional_special_tokens"] = normalized_tokens
            changed = True

    if changed:
        map_path.write_text(json.dumps(payload, indent=2), encoding="utf-8")


def _normalize_token_definition(node: Any) -> str | None:
    if node is None:
        return None

    if isinstance(node, dict):
        content = node.get("content")
        return content if isinstance(content, str) else None

    if isinstance(node, str):
        return node

    return None


SENTINEL_NONE = -0x80000000


def _hash_bytes(buffer: bytes) -> str:
    return hashlib.sha256(buffer).hexdigest()


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


def _hash_optional_int32_sequence(values: Sequence[Any]) -> str:
    payload = bytearray()
    for value in values:
        encoded = SENTINEL_NONE if value is None else int(value)
        payload.extend(struct.pack("<i", encoded))
    return _hash_bytes(payload)


def _hash_offsets(offsets: Sequence[Sequence[int]]) -> str:
    payload = bytearray()
    for start, end in offsets:
        payload.extend(struct.pack("<i", int(start)))
        payload.extend(struct.pack("<i", int(end)))
    return _hash_bytes(payload)


def _hash_string(value: str) -> str:
    data = value.encode("utf-8")
    payload = struct.pack("<I", len(data)) + data
    return _hash_bytes(payload)


def _hash_string_sequence(values: Sequence[str]) -> str:
    payload = bytearray()
    for value in values:
        data = value.encode("utf-8")
        payload.extend(struct.pack("<I", len(data)))
        payload.extend(data)
    return _hash_bytes(payload)


def _clone_chat_messages(messages: Sequence[Dict[str, Any]]) -> List[Dict[str, Any]]:
    return json.loads(json.dumps(messages))


def generate_chat_template_fixture(
    tokenizer: PreTrainedTokenizerFast,
    model: str,
    spec: ModelSpec,
    output_root: Path,
) -> Path | None:
    scenarios = CHAT_MODEL_SCENARIOS.get(model)
    if not scenarios:
        return None

    chat_template = getattr(tokenizer, "chat_template", None)
    if not chat_template:
        return None

    cases: List[Dict[str, Any]] = []
    for scenario in scenarios:
        messages = _clone_chat_messages(scenario.messages)
        try:
            rendered = tokenizer.apply_chat_template(
                messages,
                tokenize=False,
                add_generation_prompt=scenario.add_generation_prompt,
            )
        except Exception as exc:  # pragma: no cover - diagnostic surface
            raise RuntimeError(
                f"Failed to render chat template for model '{model}' scenario '{scenario.description}'."
            ) from exc

        try:
            token_ids_raw = tokenizer.apply_chat_template(
                messages,
                tokenize=True,
                add_generation_prompt=scenario.add_generation_prompt,
            )
        except Exception as exc:  # pragma: no cover - diagnostic surface
            raise RuntimeError(
                f"Failed to tokenize chat template for model '{model}' scenario '{scenario.description}'."
            ) from exc

        if not isinstance(token_ids_raw, list):
            raise RuntimeError(
                f"Unexpected token sequence type '{type(token_ids_raw).__name__}' for model '{model}'."
            )

        token_ids = [int(value) for value in token_ids_raw]

        cases.append(
            {
                "description": scenario.description,
                "addGenerationPrompt": scenario.add_generation_prompt,
                "messages": messages,
                "rendered": rendered,
                "renderedHash": _hash_string(rendered),
                "tokenIds": token_ids,
                "tokenIdsHash": _hash_int32_sequence(token_ids),
            }
        )

    fixture = {
        "model": model,
        "repo_id": spec.repo_id,
        "cases": cases,
    }

    destination = output_root / model / "chat-template.json"
    destination.write_text(json.dumps(fixture, indent=2), encoding="utf-8")
    return destination


def summarize_encoding(encoding: Encoding) -> Dict[str, Any]:
    word_ids = encoding.word_ids or []
    sequence_ids = encoding.sequence_ids or []
    overflowing = getattr(encoding, "overflowing", None) or []

    summary: Dict[str, Any] = {
        "length": len(encoding.ids),
        "idsHash": _hash_int32_sequence(encoding.ids),
        "tokensHash": _hash_string_sequence(encoding.tokens),
        "typeIdsHash": _hash_uint32_sequence(encoding.type_ids),
        "attentionMaskHash": _hash_uint32_sequence(encoding.attention_mask),
        "specialTokensMaskHash": _hash_uint32_sequence(encoding.special_tokens_mask),
        "offsetsHash": _hash_offsets(encoding.offsets),
        "wordIdsHash": _hash_optional_int32_sequence(word_ids),
        "sequenceIdsHash": _hash_optional_int32_sequence(sequence_ids),
    }

    if overflowing:
        summary["overflowing"] = [summarize_encoding(item) for item in overflowing]
    else:
        summary["overflowing"] = []

    return summary


def generate_payload(tokenizer: Tokenizer, cases: Iterable[BenchmarkCase]) -> List[Dict[str, object]]:
    payload: List[Dict[str, object]] = []
    for case in cases:
        try:
            payload.append(_generate_case_payload(tokenizer, case))
        except Exception as exc:  # pragma: no cover - diagnostics only
            traceback.print_exc()
            raise RuntimeError(f"Failed to generate case '{case.length.value}' ({case.description})") from exc
    return payload


def _generate_case_payload(tokenizer: Tokenizer, case: BenchmarkCase) -> Dict[str, object]:
        batch_texts = list(case.batch)
        pair_batch: List[str] | None = list(case.batch_pair_texts) if case.batch_pair_texts else None

        if pair_batch is not None and len(pair_batch) != len(batch_texts):
            raise ValueError(
                f"Batch pair count mismatch for case '{case.description}': {len(pair_batch)} != {len(batch_texts)}"
            )

        if case.options.truncation:
            tokenizer.no_truncation()
            tokenizer.enable_truncation(
                max_length=case.options.truncation.max_length,
                stride=case.options.truncation.stride,
            )
        else:
            tokenizer.no_truncation()

        try:
            if case.pair_text is not None:
                single_encoding = tokenizer.encode(
                    case.text,
                    case.pair_text,
                    add_special_tokens=case.options.add_special_tokens,
                )
            else:
                single_encoding = tokenizer.encode(
                    case.text,
                    add_special_tokens=case.options.add_special_tokens,
                )

            if pair_batch:
                batch_inputs: List[Tuple[str, str]] = list(zip(batch_texts, pair_batch))
                batch_encodings = tokenizer.encode_batch(
                    batch_inputs, add_special_tokens=case.options.add_special_tokens
                )
            else:
                batch_encodings = tokenizer.encode_batch(
                    batch_texts, add_special_tokens=case.options.add_special_tokens
                )

            decoded_single = tokenizer.decode(
                single_encoding.ids, skip_special_tokens=case.options.decode_skip_special_tokens
            )
            decoded_batch = list(
                tokenizer.decode_batch(
                    [encoding.ids for encoding in batch_encodings],
                    skip_special_tokens=case.options.decode_skip_special_tokens,
                )
            )
        finally:
            tokenizer.no_truncation()

        single_payload: Dict[str, Any] = {
            "text": case.text,
            "textHash": _hash_string(case.text),
            "encoding": summarize_encoding(single_encoding),
            "decodedHash": _hash_string(decoded_single),
        }
        if case.pair_text is not None:
            single_payload["pairText"] = case.pair_text
            single_payload["pairTextHash"] = _hash_string(case.pair_text)

        batch_payload: Dict[str, Any] = {
            "count": len(batch_texts),
            "texts": batch_texts,
            "textsHash": _hash_string_sequence(batch_texts),
            "encodings": [summarize_encoding(encoding) for encoding in batch_encodings],
            "decodedHash": _hash_string_sequence(decoded_batch),
        }
        if pair_batch:
            batch_payload["pairTexts"] = pair_batch
            batch_payload["pairTextsHash"] = _hash_string_sequence(pair_batch)

        return {
            "length": case.length.value,
            "description": case.description,
            "options": serialize_case_options(case.options),
            "single": single_payload,
            "batch": batch_payload,
        }


def serialize_case_options(options: CaseOptions) -> Dict[str, Any]:
    payload: Dict[str, Any] = {
        "addSpecialTokens": options.add_special_tokens,
        "decodeSkipSpecialTokens": options.decode_skip_special_tokens,
    }
    if options.truncation is not None:
        payload["truncation"] = {
            "maxLength": options.truncation.max_length,
            "stride": options.truncation.stride,
        }
    return payload


def write_output(model: str, spec: ModelSpec, resolved_assets: Dict[str, Path], payload: Sequence[Dict[str, Any]], output_root: Path) -> Path:
    metadata = {
        "model": model,
        "display_name": spec.name,
        "repo_id": spec.repo_id,
        "generated_at": time.strftime("%Y-%m-%dT%H:%M:%SZ", time.gmtime()),
        "tokenizers_version": TOKENIZERS_VERSION,
        "transformers_version": TRANSFORMERS_VERSION,
        "assets": {name: os.path.relpath(path, output_root / model) for name, path in resolved_assets.items()},
    }
    content = {
        "metadata": metadata,
        "cases": payload,
    }
    destination = output_root / model / "python-benchmark.json"
    destination.write_text(json.dumps(content, indent=2), encoding="utf-8")
    return destination


def run(models: Sequence[str], output_dir: Path, force_download: bool, require_repo_venv: bool) -> None:
    ensure_repo_venv(require_repo_venv)
    cases = build_cases()
    for model in models:
        spec = MODEL_SPECS[model]
        if not spec.supports_fast_tokenizer:
            print(
                f"Skipping {model}: fast tokenizer assets are not available for benchmark generation."
            )
            continue
        resolved_assets = ensure_model_assets(model, spec, output_dir, force_download)
        if "tokenizer.json" not in resolved_assets:
            raise FileNotFoundError(
                f"Missing tokenizer.json for model '{model}'. Ensure the Hugging Face assets are available."
            )

        transformers_tokenizer, backend_tokenizer = load_tokenizers(output_dir / model, spec)
        try:
            payload = generate_payload(backend_tokenizer, cases)
        except Exception as exc:  # pragma: no cover - diagnostic surface for CI
            traceback.print_exc()
            raise RuntimeError(f"Failed to generate payload for model '{model}'") from exc
        destination = write_output(model, spec, resolved_assets, payload, output_dir)
        print(f"Generated {destination}")

        chat_destination = generate_chat_template_fixture(
            transformers_tokenizer,
            model,
            spec,
            output_dir,
        )
        if chat_destination is not None:
            print(f"Generated {chat_destination}")


def parse_args(argv: Sequence[str]) -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Generate Hugging Face benchmark parity fixtures (runs inside the workspace .venv)."
    )
    parser.add_argument(
        "--model",
        dest="models",
        action="append",
        choices=sorted(MODEL_SPECS.keys()),
        help=(
            "Model identifier to process. Can be supplied multiple times (defaults to models with fast tokenizer support)."
        ),
    )
    parser.add_argument(
        "--output-dir",
        type=Path,
        default=DEFAULT_OUTPUT_DIR,
        help="Destination directory for generated assets (defaults to Benchmarks/data).",
    )
    parser.add_argument(
        "--force-download",
        action="store_true",
        help="Re-download model assets even if they already exist locally.",
    )
    parser.add_argument(
        "--allow-system-python",
        action="store_true",
        help="Skip enforcing that the workspace .venv interpreter is active.",
    )
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(sys.argv[1:] if argv is None else argv)
    models = args.models or list(BENCHMARK_MODEL_IDS)
    try:
        run(
            models=models,
            output_dir=args.output_dir,
            force_download=args.force_download,
            require_repo_venv=not args.allow_system_python,
        )
    except KeyboardInterrupt:  # pragma: no cover - user abort
        return 130
    except Exception as exc:  # pragma: no cover - bubble up with friendly message
        raise SystemExit(str(exc)) from exc
    return 0


if __name__ == "__main__":
    sys.exit(main())
