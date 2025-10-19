#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="${SCRIPT_DIR}"

PROJECT="${REPO_ROOT}/src/ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace/ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.csproj"
TEST_PROJECT="${REPO_ROOT}/tests/ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests/ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests.csproj"
DOWNLOAD_SCRIPT="${REPO_ROOT}/tests/Py/Huggingface/download_tests_data.py"
BENCHMARK_SCRIPT="${REPO_ROOT}/tests/Py/Huggingface/generate_benchmarks.py"

resolve_python() {
  local root="$1"
  if [[ -x "${root}/.venv/bin/python" ]]; then
    echo "${root}/.venv/bin/python"
    return
  fi
  if [[ -x "${root}/.venv/Scripts/python.exe" ]]; then
    echo "${root}/.venv/Scripts/python.exe"
    return
  fi
  command -v python >/dev/null 2>&1 && echo "python" && return
  command -v python3 >/dev/null 2>&1 && echo "python3" && return
  echo "python" # fallback
}

PYTHON="$(resolve_python "${REPO_ROOT}")"

echo "Using python interpreter: ${PYTHON}"
echo "===> Building ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace project"
dotnet build "${PROJECT}"

if [[ "${SKIP_UNIT_TESTS:-0}" != "1" ]]; then
  echo "===> Running unit tests (Category=Unit)"
  dotnet test "${TEST_PROJECT}" --filter "Category=Unit" --no-build
else
  echo "Skipping unit tests"
fi

if [[ "${SKIP_DOWNLOADS:-0}" != "1" ]]; then
  echo "===> Downloading integration test data"
  "${PYTHON}" "${DOWNLOAD_SCRIPT}"
else
  echo "Skipping test data download"
fi

if [[ "${SKIP_BENCHMARKS:-0}" != "1" ]]; then
  echo "===> Generating benchmark fixtures"
  "${PYTHON}" "${BENCHMARK_SCRIPT}"
else
  echo "Skipping benchmark generation"
fi

if [[ "${SKIP_INTEGRATION_TESTS:-0}" != "1" ]]; then
  echo "===> Running integration tests (Category=Integration)"
  dotnet test "${TEST_PROJECT}" --filter "Category=Integration" --no-build
else
  echo "Skipping integration tests"
fi

echo "Dotnet build/test workflow complete."
