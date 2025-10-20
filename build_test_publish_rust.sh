#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="${SCRIPT_DIR}"

RUST_BRIDGE="${REPO_ROOT}/src/_hf_bridge"
MANIFEST="${RUST_BRIDGE}/Cargo.toml"
TARGET_DIR="${RUST_BRIDGE}/target/release"
RUNTIME_ROOT="${REPO_ROOT}/src/ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace/runtimes"

SKIP_TESTS=${SKIP_TESTS:-0}

echo "Repository root: ${REPO_ROOT}"
echo "===> Building Rust bridge (release configuration)"
cargo build --manifest-path "${MANIFEST}" --release

if [[ "${SKIP_TESTS}" != "1" ]]; then
  echo "===> Running Rust tests"
  cargo test --manifest-path "${MANIFEST}"
fi

unameOut="$(uname -s)"
case "${unameOut}" in
  Linux*)   RUNTIME_DIR="linux-x64"; ARTIFACTS=("libtokenx_bridge.so");;
  Darwin*)  RUNTIME_DIR="osx-x64"; ARTIFACTS=("libtokenx_bridge.dylib");;
  CYGWIN*|MINGW*|MSYS*) RUNTIME_DIR="win-x64"; ARTIFACTS=("tokenx_bridge.dll" "tokenx_bridge.pdb");;
  *)        echo "Unsupported operating system: ${unameOut}" >&2; exit 1;;
 esac

DESTINATION="${RUNTIME_ROOT}/${RUNTIME_DIR}/native"
mkdir -p "${DESTINATION}"

echo "===> Publishing artifacts to ${DESTINATION}"
for artifact in "${ARTIFACTS[@]}"; do
  source_path="${TARGET_DIR}/${artifact}"
  if [[ ! -f "${source_path}" ]]; then
    echo "Expected artifact not found: ${source_path}" >&2
    exit 1
  fi
  cp "${source_path}" "${DESTINATION}/"
done

echo "Rust bridge build/test/publish complete."
