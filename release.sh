#!/usr/bin/env bash
set -euo pipefail

# Usage: ./release.sh [version]
# Default version: 0.0.1

VERSION="${1:-0.0.1}"

if ! printf '%s' "$VERSION" | grep -Eq '^[0-9]+\.[0-9]+\.[0-9]+$'; then
  echo "Invalid version format: $VERSION" >&2
  exit 1
fi

TAG="rust-v${VERSION}"

echo "Preparing release with tag: ${TAG}"

echo "Creating tag ${TAG} (force)"
git tag -a "$TAG" -m "Release rust-bridge ${TAG}" -f

echo "Pushing tag ${TAG} to origin (force)"
git push origin "refs/tags/${TAG}:refs/tags/${TAG}" --force

echo "Release script completed for ${TAG}" 
