# ErgoX TokenX ML NLP Tokenizers

[![Rust Tests](https://github.com/ergosumx/vecrax-hf-tokenizers/actions/workflows/test-c-bindings.yml/badge.svg)](https://github.com/ergosumx/vecrax-hf-tokenizers/actions/workflows/test-c-bindings.yml)
[![.NET Tests](https://github.com/ergosumx/vecrax-hf-tokenizers/actions/workflows/test-dotnet.yml/badge.svg)](https://github.com/ergosumx/vecrax-hf-tokenizers/actions/workflows/test-dotnet.yml)
[![codecov](https://codecov.io/gh/ergosumx/vecrax-hf-tokenizers/branch/main/graph/badge.svg)](https://codecov.io/gh/ergosumx/vecrax-hf-tokenizers)

.NET bindings for HuggingFace Tokenizers with comprehensive testing and multi-platform support.

## Why ErgoX.TokenX?

**TL;DR**: Microsoft.ML.Tokenizers requires manual configuration per model. ErgoX.TokenX provides seamless `AutoTokenizer.Load()` with HuggingFace ecosystem compatibility — **2,500+ Python-.NET tests verified** with byte-to-byte Python parity.

### The Problem with Microsoft.ML.Tokenizers

While Microsoft.ML.Tokenizers offers **exceptional raw performance** for GPT models ([see benchmarks](benchmarks/BENCHMARK_REPORT.md)), working with it reveals significant friction:

#### 🔍 **HuggingFace Tokenizer Structure**

When you download a HuggingFace tokenizer (`AutoTokenizer.from_pretrained` in Python), you typically get:

```
model-name/
├── tokenizer.json          # Serialized tokenizer (model, pre-tokenizer, normalizer, post-processor)
├── tokenizer_config.json   # Metadata (model type, special tokens, casing, padding, truncation)
├── vocab.json              # BPE-based tokenizers (GPT-2, RoBERTa)
├── merges.txt              # BPE merge rules
└── special_tokens_map.json # Maps [CLS], [SEP], [PAD], [BOS], [EOS], etc.
```

**HuggingFace's Python library merges these automatically**. In Microsoft.ML.Tokenizers, **you must be explicit** — manually loading vocabulary files, configuring special tokens, and handling model-specific quirks.

#### ⚠️ **Pain Points Found**

1. **Missing Special Tokens**: Special tokens required by models are not automatically configured. You need manual attention to get it right.
2. **No AutoTokenizer**: The `AutoTokenizer(...)` pattern was missing, while HuggingFace's ecosystem was rapidly growing. .NET lagged behind.
3. **No Chat Templates**: Instruction-tuned models (Llama, Mistral, Qwen) require chat templates — outside tokenizer scope in Microsoft.ML, but **essential** for real-world use.
4. **Limited Pre/Post-Processing**: Advanced tokenization pipelines (preprocessing, postprocessing) are difficult to work with.
5. **Complex Overflow Handling**: Token overflow scenarios and advanced use cases require significant boilerplate.

#### 📉 **Real-World Impact**

After replacing HuggingFace's tokenizers with Microsoft.ML.Tokenizers in Wave2Vec and Whisper models, **word error ratio decreased** — subtle special token handling differences caused accuracy improvement.

### The ErgoX.TokenX Solution

**Simple approach**: HuggingFace's **Rust implementation** of Tokenizers is ported via **C FFI bindings** into C#.

#### ✅ **What You Get**

- **One-Line Loading**: `AutoTokenizer.Load("model-name")` — works like Python
- **2,500+ Tests Verified**: Byte-to-byte parity with Python HuggingFace Transformers
- **SHA256 Hash Verification**: Every token output verified against Python reference (2,500+ test cases)
- **Chat Templates**: Built-in support for Llama 3, Mistral, Qwen, and custom formats
- **Multi-Modal Support**: Whisper (ASR), CLIP (vision), LayoutLM (documents), TrOCR (OCR)
- **Advanced Features**:
  - Token offsets for NER/question answering
  - Truncation and padding strategies
  - Attention masks, type IDs, special token handling
  - Pre-tokenization and post-processing pipelines
- **Production-Proven**: Internally used since **May 2024** without revisiting alternatives

```csharp
// Microsoft.ML.Tokenizers - Manual configuration required
var vocab = File.ReadAllText("vocab.json");
var merges = File.ReadAllText("merges.txt");
var tokenizer = /* ...manual setup... */;

// ErgoX.TokenX - One line
var tokenizer = AutoTokenizer.Load("bert-base-uncased");
```

#### ⏱️ **When My Observations May Be Outdated**

This project was developed internally in **May 2025** for key implementations. Microsoft.ML.Tokenizers may have evolved since then, but I never revisited alternatives — **ErgoX.TokenX met all my needs**.

If you're choosing today:
- ✅ **High-throughput GPT-only services** → Consider Microsoft.ML (accept manual config)
- ✅ **HuggingFace ecosystem compatibility** → Use ErgoX.TokenX (for productivity)
- ✅ **Multi-modal models (Whisper, CLIP, etc.)** → Use ErgoX.TokenX (only option)

## Features

✅ **Cross-platform** - Linux, Windows, macOS (x64 & ARM64)  
✅ **Extensive test coverage** across Linux, Windows, and macOS  
✅ **Rust FFI bindings** - High-performance C bindings layer  
✅ **CI/CD integration** - Automated testing and releases  
✅ **Test reports** - Published with every release  
✅ **Code coverage** - Tracked via Codecov  
✅ **Sequence decoder combinator** - Compose native decoders from .NET  
✅ **Performance benchmarks** - [Comprehensive comparison vs Microsoft.ML.Tokenizers](benchmarks/BENCHMARK_REPORT.md)  

## Quick Start

### Installation

#### Option 1: NuGet Package (Recommended)

```bash
dotnet add package ErgoX.TokenX.HuggingFace
```

The package includes pre-built native libraries for all supported platforms (Windows, Linux, macOS x64/ARM64).

#### Option 2: Manual Installation from Releases

Download the latest release from [GitHub Releases](https://github.com/ergosumx/vecrax-hf-tokenizers/releases):

- **Windows x64**: `tokenizers-c-win-x64.zip`
- **Linux x64**: `tokenizers-c-linux-x64.tar.gz`
- **macOS x64**: `tokenizers-c-osx-x64.tar.gz`
- **macOS ARM64**: `tokenizers-c-osx-arm64.tar.gz`

Extract and place native libraries in your project:

```
YourProject/
└── runtimes/
    ├── win-x64/native/tokenx_bridge.dll
    ├── linux-x64/native/libtokenx_bridge.so
    └── osx-x64/native/libtokenx_bridge.dylib
```

### Basic Usage

#### HuggingFace Tokenizers

```csharp
using ErgoX.TokenX.HuggingFace;

// Load tokenizer automatically (like Python's AutoTokenizer)
using var tokenizer = AutoTokenizer.Load("bert-base-uncased");

// Encode text
var encoding = tokenizer.Tokenizer.Encode("Hello, world!", addSpecialTokens: true);
Console.WriteLine($"Tokens: {string.Join(", ", encoding.Tokens)}");
Console.WriteLine($"IDs: {string.Join(", ", encoding.Ids)}");

// Decode
var decoded = tokenizer.Tokenizer.Decode(encoding.Ids, skipSpecialTokens: true);
Console.WriteLine($"Decoded: {decoded}");
```

> **Note**: For OpenAI GPT models, consider using Microsoft.ML.Tokenizers which provides `TiktokenTokenizer` class.

### Running Examples

The repository includes ready-to-run examples with pre-configured models:

```bash
# Clone repository
git clone https://github.com/ergosumx/vecrax-hf-tokenizers
cd vecrax-hf-tokenizers

# HuggingFace comprehensive quickstart (16 examples)
cd examples/HuggingFace/Quickstart
dotnet run

# Other examples (require model downloads)
dotnet run --project examples/HuggingFace/AllMiniLmL6V2Console
dotnet run --project examples/HuggingFace/E5SmallV2Console
dotnet run --project examples/HuggingFace/AutoTokenizerPipelineExplorer
```

### Quickstart Examples Overview

#### 📗 HuggingFace Tokenizer Quickstart
**16 comprehensive examples** demonstrating:
- Basic tokenization (WordPiece, Unigram, BPE)
- Padding and truncation strategies
- Text pair encoding for classification
- Attention masks, type IDs, offset mapping
- **Chat template rendering** with Llama 3
- Vocabulary access, special tokens, batch processing

**Models included:** `all-minilm-l6-v2`, `t5-small`, `meta-llama-3-8b-instruct`

**Documentation**: [Quickstart README](examples/HuggingFace/Quickstart/README.md) | [Full Docs](docs/HuggingFace/quickstart.md)

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

> Ensure the active Python environment includes the `transformers`, `tokenizers`, and `huggingface_hub` packages so the generators can materialize tokenizer pipelines directly from each model asset.

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
│   └── hf_bridge/                      # HuggingFace native bridge crate (Rust)
├── .github/
│   ├── workflows/                      # CI/CD workflows
│   │   ├── test-c-bindings.yml         # Rust tests + coverage
│   │   ├── test-dotnet.yml             # .NET tests + coverage
│   │   └── release-c-bindings.yml      # Multi-platform release
│   ├── CI-CD-WORKFLOWS.md              # CI/CD documentation
│   └── TESTING-CHECKLIST.md            # Quick reference
├── src/
│   ├── Common/                         # Shared utilities and abstractions
│   └── HuggingFace/                    # HuggingFace tokenizer bindings
└── tests/
   ├── ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests/
   └── ErgoX.VecraX.ML.NLP.Tokenizers.Testing/   # Shared testing infrastructure
```

## Performance Benchmarks

**Comprehensive benchmark suite** comparing ErgoX.TokenX libraries against Microsoft.ML.Tokenizers and measuring performance across different algorithms and text sizes.

### Benchmark Reports

| Report | Focus | Key Findings |
|--------|-------|--------------|
| **[Algorithms](benchmarks/ALGORITHM_BENCHMARK.md)** | Microsoft.ML vs ErgoX.HF | ⚠️ MSML ZeroMeasurement warnings; ErgoX broader support |

### Algorithm Benchmark: Microsoft.ML vs ErgoX.HuggingFace

**Latest benchmark** (October 30, 2025) compares BERT WordPiece and RoBERTa BPE tokenization across text sizes (0.5KB - 5KB).

#### Key Results

| Library | BERT Support | RoBERTa Support | Performance | Ecosystem |
|---------|--------------|-----------------|-------------|-----------|
| **Microsoft.ML v1.0.3** | ✅ (manual config) | ❌ No compatible API | Sub-nanosecond ⚠️ | Limited |
| **ErgoX.HuggingFace** | ✅ (auto-load) | ✅ Full support | Nanosecond-level | 2,500+ models |

⚠️ **Critical**: Microsoft.ML produced **ZeroMeasurement warnings** (sub-nanosecond results may be measurement artifacts)

**Recommendation**: Use **ErgoX.HuggingFace** for production — broader model support, reliable measurements, HuggingFace ecosystem compatibility. For OpenAI models, consider **Microsoft.ML.Tokenizers** which provides optimized `TiktokenTokenizer` implementation.

**Full details**: [Algorithm Benchmark Report](benchmarks/ALGORITHM_BENCHMARK.md)

### Running Benchmarks

```bash
cd benchmarks
dotnet build -c Release
dotnet run -c Release --no-build
```

**Runtime**: ~12-20 minutes depending on benchmark suite

### Documentation

- **[Benchmark README](benchmarks/README.md)** - Setup, running instructions, interpretation guide
- **[Algorithm Comparison](benchmarks/ALGORITHM_BENCHMARK.md)** - Microsoft.ML vs ErgoX.HuggingFace (BERT, RoBERTa)

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

