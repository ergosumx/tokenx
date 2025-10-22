from __future__ import annotations

import json
import hashlib
import struct
from pathlib import Path
from typing import Any, Dict, Iterable, List, Sequence

REPO_ROOT = Path(__file__).resolve().parents[3]
DEFAULT_CASES_PATH = REPO_ROOT / "tests" / "_testdata_templates" / "tokenization-cases.json"
SENTINEL_NONE = -0x80000000


def _hash_bytes(value: bytes) -> str:
    return hashlib.sha256(value).hexdigest()


def hash_string(value: str) -> str:
    encoded = value.encode("utf-8")
    payload = struct.pack("<I", len(encoded)) + encoded
    return _hash_bytes(payload)


def hash_string_sequence(values: Sequence[str]) -> str:
    payload = bytearray()
    for item in values:
        encoded = item.encode("utf-8")
        payload.extend(struct.pack("<I", len(encoded)))
        payload.extend(encoded)
    return _hash_bytes(payload)


def hash_int32_sequence(values: Sequence[int]) -> str:
    payload = bytearray()
    for value in values:
        payload.extend(struct.pack("<i", int(value)))
    return _hash_bytes(payload)


def hash_uint32_sequence(values: Sequence[int]) -> str:
    payload = bytearray()
    for value in values:
        payload.extend(struct.pack("<I", int(value)))
    return _hash_bytes(payload)


def hash_optional_int32_sequence(values: Sequence[int | None]) -> str:
    payload = bytearray()
    for value in values:
        encoded = SENTINEL_NONE if value is None else int(value)
        payload.extend(struct.pack("<i", encoded))
    return _hash_bytes(payload)


def hash_offsets(values: Sequence[tuple[int, int]]) -> str:
    payload = bytearray()
    for start, end in values:
        payload.extend(struct.pack("<i", int(start)))
        payload.extend(struct.pack("<i", int(end)))
    return _hash_bytes(payload)


def load_tokenization_cases(target: str, path: Path | None = None) -> List[Dict[str, Any]]:
    cases_path = path or DEFAULT_CASES_PATH
    try:
        raw = json.loads(cases_path.read_text(encoding="utf-8"))
    except FileNotFoundError as exc:  # pragma: no cover - surfaced to CLI
        raise RuntimeError(f"Tokenization case contract not found at '{cases_path}'.") from exc
    except json.JSONDecodeError as exc:  # pragma: no cover - surfaced to CLI
        raise RuntimeError(f"Tokenization case contract at '{cases_path}' is not valid JSON.") from exc

    cases = raw.get("cases")
    if not isinstance(cases, list):  # pragma: no cover - contract validation
        raise RuntimeError("Tokenization case contract must contain a 'cases' array.")

    filtered: List[Dict[str, Any]] = []
    for entry in cases:
        if not isinstance(entry, dict):  # pragma: no cover - contract validation
            continue
        targets = entry.get("targets")
        if not targets:
            filtered.append(entry)
            continue
        if not isinstance(targets, Iterable):  # pragma: no cover - contract validation
            continue
        if target in targets or "*" in targets:
            filtered.append(entry)

    if not filtered:
        raise RuntimeError(f"No tokenization cases registered for target '{target}'.")

    return filtered
