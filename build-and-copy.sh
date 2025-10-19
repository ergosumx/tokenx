#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="${SCRIPT_DIR}"

RUST_BRIDGE_DIR="${ROOT_DIR}/src/ErgoX.VecraX.ML.NLP.Tokenizers.Rust.Bridge"
MANIFEST_PATH="${RUST_BRIDGE_DIR}/Cargo.toml"
SOLUTION_PATH="${ROOT_DIR}/TokenX.HF.sln"
RELEASE_DIR="${RUST_BRIDGE_DIR}/target/release"
DEST_DIR="${ROOT_DIR}/src/ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace/runtimes/win-x64/native"
ARTIFACTS=("tokenx_bridge.dll" "tokenx_bridge.pdb")

echo "Building Rust bridge (release)..."
cargo build --release --manifest-path "${MANIFEST_PATH}"

if [[ "${SKIP_CARGO_TESTS:-0}" != "1" ]]; then
    echo "Running Rust tests..."
    cargo test --manifest-path "${MANIFEST_PATH}"
fi

if [[ "${SKIP_DOTNET_TESTS:-0}" != "1" ]]; then
    echo "Running .NET tests..."
    dotnet test "${SOLUTION_PATH}"
fi

echo "Copying artifacts to ${DEST_DIR}"
mkdir -p "${DEST_DIR}"

for artifact in "${ARTIFACTS[@]}"; do
    source_path="${RELEASE_DIR}/${artifact}"
    if [[ ! -f "${source_path}" ]]; then
        echo "Missing artifact: ${source_path}" >&2
        exit 1
    fi
    cp "${source_path}" "${DEST_DIR}/"
done

echo "Done."
