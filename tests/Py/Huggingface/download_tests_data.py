#!/usr/bin/env python3
"""Download Hugging Face tokenizer assets used by integration tests."""

from __future__ import annotations

import argparse
import sys
from pathlib import Path

from generate_benchmarks import (
    DEFAULT_OUTPUT_DIR,
    MODEL_SPECS,
    ensure_model_assets,
    ensure_repo_venv,
)


def parse_args(argv: list[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Download tokenizer fixtures required by integration tests.",
    )
    parser.add_argument(
        "--model",
        dest="models",
        action="append",
        choices=sorted(MODEL_SPECS.keys()),
        help="Model identifier to download (defaults to all models).",
    )
    parser.add_argument(
        "--output-dir",
        type=Path,
        default=DEFAULT_OUTPUT_DIR,
        help="Destination directory for downloaded assets (defaults to tests/_TestData).",
    )
    parser.add_argument(
        "--force-download",
        action="store_true",
        help="Re-download assets even if they already exist.",
    )
    parser.add_argument(
        "--allow-system-python",
        action="store_true",
        help="Skip enforcing that the workspace .venv interpreter is active.",
    )
    return parser.parse_args(sys.argv[1:] if argv is None else argv)


def main(argv: list[str] | None = None) -> int:
    args = parse_args(argv)
    models = args.models or sorted(MODEL_SPECS.keys())

    ensure_repo_venv(require_repo_venv=not args.allow_system_python)

    for model in models:
        spec = MODEL_SPECS[model]
        ensure_model_assets(model, spec, args.output_dir, force=args.force_download)
        print(f"Ensured assets for {model} -> {args.output_dir / model}")

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
