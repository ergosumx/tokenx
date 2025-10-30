# ErgoX TokenX ML NLP Tokenizers

[![Rust Tests](https://github.com/ergosumx/vecrax-hf-tokenizers/actions/workflows/test-c-bindings.yml/badge.svg)](https://github.com/ergosumx/vecrax-hf-tokenizers/actions/workflows/test-c-bindings.yml)
[![.NET Tests](https://github.com/ergosumx/vecrax-hf-tokenizers/actions/workflows/test-dotnet.yml/badge.svg)](https://github.com/ergosumx/vecrax-hf-tokenizers/actions/workflows/test-dotnet.yml)
[![codecov](https://codecov.io/gh/ergosumx/vecrax-hf-tokenizers/branch/main/graph/badge.svg)](https://codecov.io/gh/ergosumx/vecrax-hf-tokenizers)

.NET bindings for HuggingFace Tokenizers and OpenAI TikToken with comprehensive testing and multi-platform support.

## Features

✅ **Cross-platform** - Linux, Windows, macOS (x64 & ARM64)  
✅ **Extensive test coverage** across Linux, Windows, and macOS  
✅ **Rust FFI bindings** - High-performance C bindings layer  
✅ **CI/CD integration** - Automated testing and releases  
✅ **Test reports** - Published with every release  
✅ **Code coverage** - Tracked via Codecov  
✅ **Sequence decoder combinator** - Compose native decoders from .NET  

## Quick Start

### Installation

Download the latest release for your platform:

- **Windows x64**: `tokenizers-c-win-x64.zip`
- **Linux x64**: `tokenizers-c-linux-x64.tar.gz`
- **macOS x64**: `tokenizers-c-osx-x64.tar.gz`
- **macOS ARM64**: `tokenizers-c-osx-arm64.tar.gz`

Extract the native library to your project's runtime folder:

```
runtimes/
   win-x64/native/tokenx_bridge.dll
   linux-x64/native/libtokenx_bridge.so
   osx-x64/native/libtokenx_bridge.dylib
```

### Usage

```csharp
using ErgoX.TokenX.HuggingFace;

// Load tokenizer from JSON
var tokenizer = Tokenizer.FromFile("tokenizer.json");

// Encode text
var encoding = tokenizer.Encode("Hello, world!");
Console.WriteLine($"Tokens: {string.Join(", ", encoding.Tokens)}");
Console.WriteLine($"IDs: {string.Join(", ", encoding.Ids)}");

// Decode
var decoded = tokenizer.Decode(encoding.Ids);
Console.WriteLine($"Decoded: {decoded}");
```

### Hugging Face Examples

The repository ships ready-to-run console samples that exercise the `AutoTokenizer` pipeline against the archived assets in
`examples/.models`:

- `dotnet run --project examples/HuggingFace/AllMiniLmL6V2Console` – single sentence embedding with `all-minilm-l6-v2`.
- `dotnet run --project examples/HuggingFace/E5SmallV2Console` – query/passage batching with `e5-small-v2`.
- `dotnet run --project examples/HuggingFace/MultilingualE5SmallConsole` – multilingual inputs with `multilingual-e5-small`.
- `dotnet run --project examples/HuggingFace/AutoTokenizerPipelineExplorer` – inspect tokenizer metadata across the models.
- `dotnet run --project examples/HuggingFace/WhisperTinyConsole` – transcribe `.data/wav` audio snippets with the Whisper Tiny encoder and decoder ONNX pair.

Each sample resolves `tokenizer_config.json`, `tokenizer.json`, special tokens, and optional generation defaults directly from the
local model snapshot so they can be executed offline.

## Development

### Prerequisites

- **.NET SDK 8.0+**
- **Rust 1.70+**
- **Visual Studio 2022** (Windows) or equivalent C++ toolchain

### Building

```bash
# Build Rust library
cd .ext/hf_bridge
cargo build --release

# Copy to .NET runtime folder
Copy-Item target/release/tokenx_bridge.dll ../src/HuggingFace/runtimes/win-x64/native/ -Force

# Build .NET project
cd ..
cd ..
dotnet build --configuration Release
```

### Testing

```bash
# Restore sanitized tokenizer fixtures (skips network downloads)
python tests/Py/Common/restore_test_data.py --force

# Run Rust tests
cd .ext/hf_bridge
cargo test --release

# Run .NET tests
dotnet test --configuration Release

# Refresh HuggingFace parity fixtures (requires transformers/tokenizers)
python tests/Py/Huggingface/generate_benchmarks.py

# Refresh TikToken parity fixtures (requires the 'tiktoken' package)
python -m pip install -q tiktoken
python tests/Py/OpenAI/Tiktoken/generate_benchmarks.py

> Ensure the active Python environment includes the `transformers`, `tokenizers`, `huggingface_hub`, and `tiktoken` packages so the generators can materialize tokenizer pipelines directly from each model asset.

TikToken fixtures are written to `tests/_testdata_tiktoken/<encoding>` and power the .NET TikToken parity integration tests.
Current fixtures cover the OpenAI encodings `gpt2`, `r50k_base`, `p50k_base`, `p50k_edit`, `cl100k_base`, `o200k_base`, and `o200k_harmony`.
```

Running the .NET parity suite now also emits `dotnet-benchmark.json` alongside the Python fixtures in `tests/_testdata_huggingface/<model>` so you can inspect the full decoded tokens produced by the managed implementation.

**Expected Results**: 37 passed, 0 skipped, 0 failed

See [TESTING-CHECKLIST.md](.github/TESTING-CHECKLIST.md) for detailed instructions.

## CI/CD

### Automated Testing

Every push and pull request triggers:

1. **Rust C Bindings Tests** - 20 FFI layer tests on Linux, Windows, macOS
2. **.NET Integration Tests** - 185 end-to-end tests on all platforms
3. **Coverage Reports** - Uploaded to Codecov

### Releases

Create a release by tagging:

```bash
git tag c-v0.22.2
git push origin c-v0.22.2
```

The release workflow will:

1. ✅ Build binaries for 7 platforms
2. ✅ Run full test suite (205 tests)
3. ✅ Package test reports
4. ✅ Create GitHub Release with:
   - Multi-platform binaries
   - Test reports archive (`test-reports.tar.gz`)
   - Checksums
   - Release notes with test results

See [CI-CD-WORKFLOWS.md](.github/CI-CD-WORKFLOWS.md) for complete documentation.

## Test Reports

Every release includes **test-reports.tar.gz** containing:

- **TRX files** - Machine-readable test results
- **HTML reports** - Human-readable test results
- **Coverage reports** - Code coverage analysis

Download from the [Releases](https://github.com/ergosumx/vecrax-hf-tokenizers/releases) page.

## Project Structure

```
TokenX/
├── .ext/
│   ├── hf_bridge/                      # HuggingFace native bridge crate (Rust)
│   ├── tiktoken/                       # TikToken native assets
│   └── tt_bridge/                      # TikToken bridge crate (Rust)
├── .github/
│   ├── workflows/                      # CI/CD workflows
│   │   ├── test-c-bindings.yml         # Rust tests + coverage
│   │   ├── test-dotnet.yml             # .NET tests + coverage
│   │   └── release-c-bindings.yml      # Multi-platform release
│   ├── CI-CD-WORKFLOWS.md              # CI/CD documentation
│   └── TESTING-CHECKLIST.md            # Quick reference
├── src/
│   ├── Common/                         # Shared utilities and abstractions
│   ├── HuggingFace/                    # HuggingFace tokenizer bindings
│   └── OpenAI/
│       └── Tiktoken/                   # TikToken bindings
└── tests/
   ├── ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests/
   ├── ErgoX.VecraX.ML.NLP.Tokenizers.OpenAI.Tiktoken.Tests/
   └── ErgoX.VecraX.ML.NLP.Tokenizers.Testing/   # Shared testing infrastructure
```

## Test Coverage

| Component | Tests | Pass Rate |
|-----------|-------|-----------|
| Rust Bridge (tokenx_bridge) | TBD | – |
| .NET Integration | 37 | 100% ✅ |

**Known Limitation**: 1 test skipped due to Rust library limitation with complex normalizer pipelines.

## Supported Platforms

| Platform | Architecture | Status |
|----------|--------------|--------|
| Linux | x64 | ✅ Tested |
| Windows | x64 | ✅ Tested |
| macOS | x64 | ✅ Tested |
| macOS | ARM64 | ✅ Built |
| iOS | ARM64 | ✅ Built |
| Android | ARM64 | ✅ Built |
| WebAssembly | wasm32 | ✅ Built |

**Note**: "Tested" platforms run full .NET test suite in CI. "Built" platforms compile successfully but tests are not run in CI.

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make changes and add tests
4. Ensure all tests pass locally
5. Submit a pull request

CI will automatically:
- Run tests on all platforms
- Generate coverage reports
- Comment on PR with results

See [TESTING-CHECKLIST.md](.github/TESTING-CHECKLIST.md) for pre-commit checklist.

## Documentation

- **[CI/CD Workflows](.github/CI-CD-WORKFLOWS.md)** - Complete CI/CD documentation
- **[Testing Checklist](.github/TESTING-CHECKLIST.md)** - Quick reference for testing and releases
- **[Coding Standards](.github/instructions/ergox.engineering.coding.standards.instructions.md)** - Project coding standards
- **[Acceptance Criteria](.github/instructions/ergox.acceptance.instructions.md)** - Quality gates

## Troubleshooting

### Tests Failing Locally

1. **Ensure Rust library is built**:
   ```bash
   cd .ext/hf_bridge
   cargo build --release
   ```

2. **Verify DLL is in runtime folder**:
   ```powershell
   Test-Path "src/HuggingFace/runtimes/win-x64/native/tokenx_bridge.dll"
   ```

3. **Check DLL size** (should be ~4.3 MB):
   ```powershell
   (Get-Item ".ext/hf_bridge/target/release/tokenx_bridge.dll").Length / 1MB
   ```

### CI Failures

Check the [Actions](https://github.com/ergosumx/vecrax-hf-tokenizers/actions) tab for detailed logs.

Common issues:
- **Cache corruption** - Re-run workflow
- **Network timeouts** - Re-run workflow
- **Disk space** - Clear caches

See [CI-CD-WORKFLOWS.md](.github/CI-CD-WORKFLOWS.md#troubleshooting) for detailed troubleshooting.

## License

This project follows the license terms of the HuggingFace Tokenizers library.

## Acknowledgments

Built on top of [HuggingFace Tokenizers](https://github.com/huggingface/tokenizers) - an incredible fast and versatile tokenization library.

---

**Maintained by**: ErgoX TokenX Team  
**Last Updated**: October 17, 2025  
**Version**: 0.22.1

