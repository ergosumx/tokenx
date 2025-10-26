#!/usr/bin/env python3
"""Restore sanitized Hugging Face test assets from the archived fixture bundle."""

from __future__ import annotations

import argparse
import shutil
import tarfile
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parents[2]
ARCHIVE_PATH = REPO_ROOT / "_fixtures" / "tokenizer-testdata.tar.gz"
DESTINATION = REPO_ROOT / "_testdata_huggingface"
ARCHIVE_ROOT_NAME = "_TestData"


def extract(force: bool) -> None:
    if not ARCHIVE_PATH.exists():
        raise SystemExit(f"Fixture archive not found at '{ARCHIVE_PATH}'.")

    extracted_path = DESTINATION.parent / ARCHIVE_ROOT_NAME

    if DESTINATION.exists():
        if not force:
            print(f"Destination '{DESTINATION}' already exists; use --force to overwrite.")
            return
        shutil.rmtree(DESTINATION)

    if extracted_path.exists():
        shutil.rmtree(extracted_path)

    DESTINATION.parent.mkdir(parents=True, exist_ok=True)

    with tarfile.open(ARCHIVE_PATH, mode="r:gz") as archive:
        archive.extractall(path=DESTINATION.parent)

    if not extracted_path.exists():
        raise SystemExit(
            f"Archive extraction did not produce '{extracted_path}'. Ensure the archive layout matches {ARCHIVE_ROOT_NAME}."
        )

    extracted_path.rename(DESTINATION)

    print(f"Restored fixtures to '{DESTINATION}'.")


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Extract the tokenizer fixture archive into tests/_testdata_huggingface.")
    parser.add_argument(
        "--force",
        action="store_true",
        help="Overwrite existing data if the destination already exists.",
    )
    return parser.parse_args()


def main() -> int:
    args = parse_args()
    extract(force=args.force)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
