#!/usr/bin/env bash
set -euo pipefail

# Usage: ./release.sh [version]
# Default version: 0.0.1

PREFIX="rust"
VERSION="${1:-0.0.1}"
DRY_RUN=0

# Parse named arguments --prefix/-p and --version/-v while also supporting positional
while [[ $# -gt 0 ]]; do
  case "$1" in
    --prefix|-p)
      PREFIX="$2"
      shift 2
      ;;
    --version|-v)
      VERSION="$2"
      _VERSION_FLAG_SET=1
      shift 2
      ;;
    --dry-run|-n)
      DRY_RUN=1
      shift
      ;;
    --)
      shift
      break
      ;;
    *)
      # If first arg doesn't match flags and we still have at least one arg, keep positional
      if [[ -z "${POSITIONAL+x}" ]]; then
        POSITIONAL="$1"
      fi
      shift
      ;;
  esac
done

# If a positional argument was provided and no explicit --version flag, use it
if [[ -n "${POSITIONAL+x}" && -z "${_VERSION_FLAG_SET+x}" ]]; then
  VERSION="$POSITIONAL"
fi

if ! printf '%s' "$VERSION" | grep -Eq '^[0-9]+\.[0-9]+\.[0-9]+$'; then
  echo "Invalid version format: $VERSION" >&2
  exit 1
fi

TAG="${PREFIX}-v${VERSION}"

echo "Preparing release with tag: ${TAG}"

echo "Creating tag ${TAG} (force)"
if [[ "$DRY_RUN" -eq 1 ]]; then
  echo "DRY RUN: git tag -a \"${TAG}\" -m \"Release ${PREFIX} bridge ${TAG}\" -f"
  echo "DRY RUN: git push origin \"refs/tags/${TAG}:refs/tags/${TAG}\" --force"
else
  git tag -a "$TAG" -m "Release ${PREFIX} bridge ${TAG}" -f
  echo "Pushing tag ${TAG} to origin (force)"
  git push origin "refs/tags/${TAG}:refs/tags/${TAG}" --force
fi

echo "Release script completed for ${TAG}"
