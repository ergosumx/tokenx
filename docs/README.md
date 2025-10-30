# ErgoX VecraX ML NLP Tokenizers

Production-ready .NET tokenization libraries with native interop for modern NLP applications.

---

## 🚀 Quick Start

**New to TokenX? Start here:**

- 📖 **[Getting Started Guide](getting-started.md)** - Introduction to tokenization concepts
- 🎯 **[HuggingFace Quickstart](../examples/HuggingFace/Quickstart/README.md)** - 16 comprehensive examples covering ALL HuggingFace features

**The quickstart includes:**
- ✅ Models bundled (no download required)
- ✅ Runnable code (`dotnet run`)
- ✅ Detailed documentation for every feature
- ✅ Use case examples and best practices

**Comparing libraries?**
- 📊 **[Library Comparison](comparison.md)** - Microsoft.ML vs ErgoX HuggingFace (features, performance, use cases)
- ⚡ **[Benchmarks](../benchmarks/README.md)** - Performance measurements and benchmark suite

> **Note**: For OpenAI GPT models, consider using Microsoft.ML.Tokenizers which provides optimized `TiktokenTokenizer` implementation.

---

## What Is This Project?

ErgoX VecraX ML NLP Tokenizers provides high-performance, cross-platform tokenization capabilities for .NET applications that need to integrate with modern language models and NLP pipelines. The project bridges the battle-tested HuggingFace Tokenizers library (written in Rust) with idiomatic .NET APIs through carefully designed P/Invoke layers.

## Goals

### Core Mission
Enable .NET developers to tokenize text with the **same fidelity and performance** as Python-based ML workflows, eliminating the need to maintain separate preprocessing services or port tokenization logic by hand.

### Technical Focus
- **Correctness First**: Token-exact parity with reference implementations (validated via 37+ integration tests against Python baselines)
- **Cross-Platform**: Linux, Windows, macOS (x64/ARM64) with unified native library management
- **Production-Ready**: Deterministic behavior, comprehensive error handling, resource management aligned with .NET semantics

### Non-Goals
- **Not a Training Framework**: This library **only** tokenizes; model training/inference happens elsewhere (ONNX Runtime, ML.NET, etc.)
- **Not a Model Hub Client**: Users supply their own tokenizer configs (from HuggingFace Hub downloads, local files, etc.)
- **Not a Research Platform**: Focused on stable, widely-used tokenizers—experimental algorithms belong upstream

## What We're Attempting

### The Challenge
Modern transformer models (BERT, GPT, T5, Whisper) rely on complex tokenization pipelines that:
- Split text using learned subword vocabularies (BPE, WordPiece, Unigram)
- Apply normalization, pre-tokenization, and post-processing rules
- Handle special tokens, padding, truncation, attention masks
- Support multimodal inputs (text + audio/vision tokens)

Most implementations live in Python (`transformers`). Replicating this logic in pure C# is error-prone and diverges from upstream fixes. **We provide native interop instead**.

### The Solution
Wrap production-grade native library with minimal overhead:
**HuggingFace Tokenizers** (Rust): Comprehensive support for BPE, WordPiece, Unigram models; chat templates; configurable pipelines

The binding exposes:
- Low-level FFI layer (P/Invoke) for direct native calls
- High-level managed API (AutoTokenizer) following .NET conventions
- Disposal patterns ensuring native resources are freed correctly

## Architecture

```
┌───────────────────────────────────────────────┐
│        .NET Application Code                  │
├───────────────────────────────────────────────┤
│  ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace   │  ← Managed API
├───────────────────────────────────────────────┤
│         P/Invoke Bindings (C ABI)             │
├───────────────────────────────────────────────┤
│ tokenx_bridge.{dll|so|dylib}  (Rust → C FFI)  │  ← Native Library
└───────────────────────────────────────────────┘
```

**Key Design Decisions**:
- Native libraries are **embedded as content** in NuGet packages (deployed to `runtimes/{rid}/native/`)
- **Thin wrappers**: Managed code adds safety/convenience but delegates heavy lifting to native layer
- **Parity testing**: Python reference outputs anchor correctness—new tokenizer support requires matching fixtures

## Current Status

### Supported Tokenizers
- ✅ **HuggingFace Tokenizers**: BERT, GPT-2, RoBERTa, ALBERT, T5, BART, DistilBERT, DeBERTa, Whisper, LLaMA, Mistral, Gemma, and more

### Test Coverage
- **Integration tests** (HuggingFace parity with Python)
- **Linux, Windows, macOS** CI validation on every commit
- **Codecov tracking** for managed code coverage

### Production Use
This library powers real-world ML/NLP applications that:
- Preprocess text for embedding models (sentence-transformers, E5)
- Tokenize inputs for ONNX-hosted inference (Whisper transcription, GPT generation)
- Calculate token counts for OpenAI API cost estimation

## Limitations

### What Works
- Encoding text → token IDs
- Decoding token IDs → text
- Special tokens, padding, truncation
- Chat templates (HuggingFace)
- Batch encoding
- Thread-safe tokenization (after initialization)

### Known Gaps
- **No Training**: Cannot train new tokenizers from scratch (use HuggingFace `tokenizers` library in Python)
- **No Incremental Decoding**: Full decode only (no streaming partial outputs)
- **Limited Normalizer Introspection**: Complex normalizer chains treated as opaque (1 test skipped for this reason)

### Platform Support
| Platform       | Status           | Notes                          |
|----------------|------------------|--------------------------------|
| Windows x64    | ✅ Tested in CI   | Requires VC++ Redistributable  |
| Linux x64      | ✅ Tested in CI   | glibc 2.27+ recommended        |
| macOS x64      | ✅ Tested in CI   | Works on Intel Macs            |
| macOS ARM64    | ✅ Built          | Tests not automated (no CI runner) |
| iOS/Android    | ✅ Built          | Native libs compile; no CI     |
| WebAssembly    | ✅ Built          | Experimental; no CI            |

## Quick Start

**New to ErgoX TokenX?** Start with **[Getting Started Guide](getting-started.md)** for a friendly introduction!

### Comprehensive Quickstart Examples

**Self-contained examples with models included - ready to run after cloning!**

- 📗 **[HuggingFace Quickstart](HuggingFace/quickstart.md)** - 16 examples covering WordPiece, Unigram, chat templates

```bash
# HuggingFace (BERT, T5, Llama, etc.)
cd examples/HuggingFace/Quickstart && dotnet run
```

### Installation

```bash
# Install NuGet package
dotnet add package ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace
```

See [Installation Guide](installation.md) for detailed setup.

## Documentation

- **[Installation](installation.md)** – NuGet setup, native library deployment, troubleshooting
- **[HuggingFace](huggingface/index.md)** – AutoTokenizer API, chat templates, special tokens
- **[Examples](examples.md)** – Working console apps for common scenarios

## Contributing

Contributions that improve **correctness**, **cross-platform support**, or **testing** are welcome. Before submitting:

1. **Run Tests**: `dotnet test --configuration Release` (all must pass)
2. **Verify Parity**: Regenerate fixtures if changing tokenizer behavior:
   ```bash
   python tests/Py/Huggingface/generate_benchmarks.py
   ```
3. **Follow Coding Standards**: See `.github/instructions/ergox.engineering.coding.standards.instructions.md`

**Priority Areas**:
- Adding support for new HuggingFace tokenizers (with Python baseline)
- Improving error messages for common misconfiguration scenarios
- macOS ARM64 CI automation

## License

This project follows the license terms of the HuggingFace Tokenizers library (Apache 2.0). See [LICENSE](../LICENSE) for details.

## Acknowledgments

Built on:
- [HuggingFace Tokenizers](https://github.com/huggingface/tokenizers) – Rust implementation of modern tokenization

For OpenAI GPT models, see [Microsoft.ML.Tokenizers](https://www.nuget.org/packages/Microsoft.ML.Tokenizers/) which provides `TiktokenTokenizer`.

Maintained by **ErgoX VecraX Team** | Last Updated: October 30, 2025

