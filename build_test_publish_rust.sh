#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="${SCRIPT_DIR}"

RUST_BRIDGE="${REPO_ROOT}/.ext/hf_bridge"
MANIFEST="${RUST_BRIDGE}/Cargo.toml"
TARGET_ROOT="${RUST_BRIDGE}/target"
TARGET_DIR="${TARGET_ROOT}/release"
RUNTIME_ROOT="${REPO_ROOT}/src/ErgoX.VecraX.ML.NLP.Tokenizers/HuggingFace/runtimes"

SKIP_TESTS=${SKIP_TESTS:-0}

echo "Repository root: ${REPO_ROOT}"
echo "===> Building Rust bridge (release configuration)"
cargo build --manifest-path "${MANIFEST}" --release

if [[ "${SKIP_TESTS}" != "1" ]]; then
  echo "===> Running Rust tests"
  cargo test --manifest-path "${MANIFEST}"
fi

if [[ ! -d "${TARGET_DIR}" ]]; then
  mapfile -t RELEASE_CANDIDATES < <(find "${TARGET_ROOT}" -maxdepth 2 -type d -name release -print0 | xargs -0 -r ls -dt)
  if [[ ${#RELEASE_CANDIDATES[@]} -gt 0 ]]; then
    TARGET_DIR="${RELEASE_CANDIDATES[0]}"
  else
    echo "Unable to locate release artifacts under ${TARGET_ROOT} after build." >&2
    exit 1
  fi
fi

unameOut="$(uname -s)"
machineArch="$(uname -m)"
case "${unameOut}" in
  Linux*)
    if [[ "${machineArch}" == "aarch64" || "${machineArch}" == "arm64" ]]; then
      RUNTIME_DIR="linux-arm64"
    else
      RUNTIME_DIR="linux-x64"
    fi
    ARTIFACTS=("libtokenx_bridge.so")
    ;;
  Darwin*)
    if [[ "${machineArch}" == "arm64" ]]; then
      RUNTIME_DIR="osx-arm64"
    else
      RUNTIME_DIR="osx-x64"
    fi
    ARTIFACTS=("libtokenx_bridge.dylib")
    ;;
  CYGWIN*|MINGW*|MSYS*)
    if [[ "${machineArch}" == "aarch64" || "${machineArch}" == "arm64" ]]; then
      RUNTIME_DIR="win-arm64"
    elif [[ "${machineArch}" == "i686" ]]; then
      RUNTIME_DIR="win-x86"
    else
      RUNTIME_DIR="win-x64"
    fi
    ARTIFACTS=("tokenx_bridge.dll" "tokenx_bridge.pdb")
    ;;
  Android*)
    RUNTIME_DIR="android-arm64"
    ARTIFACTS=("libtokenx_bridge.so")
    ;;
  iOS*)
    RUNTIME_DIR="ios-arm64"
    ARTIFACTS=()
    for candidate in "libtokenx_bridge.a" "libtokenx_bridge.dylib"; do
      if [[ -f "${TARGET_DIR}/${candidate}" ]]; then
        ARTIFACTS+=("${candidate}")
      fi
    done
    if [[ ${#ARTIFACTS[@]} -eq 0 ]]; then
      echo "No iOS bridge artifacts were produced." >&2
      exit 1
    fi
    ;;
  *)
    echo "Unsupported operating system: ${unameOut}" >&2
    exit 1
    ;;
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
