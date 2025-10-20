#!/usr/bin/env python3
"""Utility for managing Hugging Face authentication during CI runs."""

from __future__ import annotations

import argparse
import sys
from pathlib import Path

from huggingface_hub import HfFolder


def login(token: str) -> None:
    if not token:
        raise SystemExit("A Hugging Face token is required for login.")

    HfFolder.save_token(token)
    print("Saved Hugging Face token to cache.")


def logout() -> None:
    HfFolder.delete_token()

    cache_home = Path.home() / ".cache" / "huggingface"
    stored_tokens = cache_home / "stored_tokens"
    if stored_tokens.exists():
        if stored_tokens.is_file():
            stored_tokens.unlink()
        else:
            for child in sorted(stored_tokens.glob("**/*"), reverse=True):
                if child.is_file():
                    child.unlink()
                else:
                    child.rmdir()
            stored_tokens.rmdir()
        print("Removed stored token metadata.")

    print("Cleared Hugging Face token cache.")


def parse_args(argv: list[str]) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Manage Hugging Face authentication cache.")
    subparsers = parser.add_subparsers(dest="command", required=True)

    login_parser = subparsers.add_parser("login", help="Persist a token to the local cache.")
    login_parser.add_argument("token", type=str, help="Hugging Face access token")

    subparsers.add_parser("logout", help="Remove any persisted tokens from the local cache.")

    return parser.parse_args(argv)


def main(argv: list[str] | None = None) -> int:
    args = parse_args(sys.argv[1:] if argv is None else argv)

    if args.command == "login":
        login(args.token)
    elif args.command == "logout":
        logout()
    else:  # pragma: no cover - defensive fallback for future commands
        raise SystemExit(f"Unknown command: {args.command}")

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
