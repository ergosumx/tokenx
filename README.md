# ErgoX VecraX ML NLP Tokenizers

[![Rust Tests](https://github.com/ergosumx/vecrax-hf-tokenizers/actions/workflows/test-c-bindings.yml/badge.svg)](https://github.com/ergosumx/vecrax-hf-tokenizers/actions/workflows/test-c-bindings.yml)
[![.NET Tests](https://github.com/ergosumx/vecrax-hf-tokenizers/actions/workflows/test-dotnet.yml/badge.svg)](https://github.com/ergosumx/vecrax-hf-tokenizers/actions/workflows/test-dotnet.yml)
[![codecov](https://codecov.io/gh/ergosumx/vecrax-hf-tokenizers/branch/main/graph/badge.svg)](https://codecov.io/gh/ergosumx/vecrax-hf-tokenizers)

.NET bindings for the HuggingFace Tokenizers library with comprehensive testing and multi-platform support.

## Features

✅ **Cross-platform** - Linux, Windows, macOS (x64 & ARM64)  
✅ **185 comprehensive tests** - 184 passing, 1 skipped  
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
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace;

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
Copy-Item target/release/tokenx_bridge.dll ../src/ErgoX.VecraX.ML.NLP.Tokenizers/HuggingFace/runtimes/win-x64/native/ -Force

# Build .NET project
cd ..
cd ..
dotnet build --configuration Release
```

### Testing

```bash
# Restore sanitized tokenizer fixtures (skips network downloads)
python tests/Py/Huggingface/restore_test_data.py --force

# Run Rust tests
cd .ext/hf_bridge
cargo test --release

# Run .NET tests
dotnet test --configuration Release

# Refresh Python parity fixtures (required when tokenizer assets change)
# This regenerates the benchmark JSON output using the archived assets.
\.\.venv\Scripts\python.exe tests\Py\Huggingface\generate_benchmarks.py

# Refresh SentencePiece parity fixtures
\.\.venv\Scripts\python.exe tests\Py\Google\SentencePeice\generate_benchmarks.py

> Ensure the workspace virtual environment includes the `transformers`, `tokenizers`, `huggingface_hub`, and `sentencepiece` packages so the generators can materialize tokenizer pipelines directly from each model asset.
```

Running the .NET parity suite now also emits `dotnet-benchmark.json` alongside the Python fixtures in `tests/_TestData/<model>` so you can inspect the full decoded tokens produced by the managed implementation.

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
ErgoX.VecraX.ML.NLP.Tokenizers/
├── .ext/tokenizers/              # HuggingFace tokenizers submodule
│   └── bindings/c/               # Rust FFI bindings
│       ├── src/                  # Rust source code
│       └── tests/                # 20 Rust decoder tests
├── .github/
│   ├── workflows/                # CI/CD workflows
│   │   ├── test-c-bindings.yml   # Rust tests + coverage
│   │   ├── test-dotnet.yml       # .NET tests + coverage
│   │   └── release-c-bindings.yml # Multi-platform release
│   ├── CI-CD-WORKFLOWS.md        # CI/CD documentation
│   └── TESTING-CHECKLIST.md      # Quick reference
├── .ext/
│   └── hf_bridge/             # Native bridge crate (Rust)
│       ├── Cargo.toml
│       └── src/lib.rs
└── src/
   ├── ErgoX.VecraX.ML.NLP.Tokenizers/
   │   ├── HuggingFace/
   │   │   ├── Tokenizer.cs      # Main tokenizer class
   │   │   ├── NativeMethods.cs  # P/Invoke declarations
   │   │   └── runtimes/         # Native libraries
   │   ├── Google/
   │   └── Tiktoken/
   └── ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests/
      └── Encoding/             # Integration tests
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
   cd .ext/tokenizers/bindings/c
   cargo build --release
   ```

2. **Verify DLL is in runtime folder**:
   ```powershell
   Test-Path "src/.../runtimes/win-x64/native/tokenx_bridge.dll"
   ```

3. **Check DLL size** (should be ~4.3 MB):
   ```powershell
   (Get-Item "src/_hf_bridge/target/release/tokenx_bridge.dll").Length / 1MB
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

**Maintained by**: ErgoX VecraX Team  
**Last Updated**: October 17, 2025  
**Version**: 0.22.1
