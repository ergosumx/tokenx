# ErgoX VecraX ML NLP Tokenizers

High-performance .NET tokenizer library with native interop for HuggingFace Tokenizers.

> **⚠️ Beta Status**: This project is currently in beta. Documentation is in progress. However, the packages are production-proven and have been used internally for many months. Please see our [examples](examples.md) for guidance while we complete the documentation.

## Overview

This repository provides comprehensive tokenization library for .NET applications:

- **[HuggingFace Tokenizers](huggingface/index.md)** - Full-featured tokenization supporting BPE, WordPiece, and Unigram models with multimodal chat capabilities

> **Note**: For OpenAI GPT models, we recommend using Microsoft.ML.Tokenizers which provides optimized `TiktokenTokenizer` implementation.

## Key Features

✅ **Cross-platform** - Windows, Linux, macOS (x64 & ARM64)  
✅ **High performance** - Native Rust/C++ backends with efficient .NET bindings  
✅ **Comprehensive testing** - 3,600+ tests ensuring reliability  
✅ **Production-ready** - Used in real-world ML/NLP applications  
✅ **Well-documented** - Complete XML documentation and examples  

## Quick Links

- [Installation Guide](installation.md)
- [API Reference](api/index.md)
- [Examples](examples.md)
- [GitHub Repository](https://github.com/ergosumx/tokenx)

## Choosing a Tokenizer

### Benchmark Results Summary

✅ **[Complete Performance Benchmarks Available](../benchmarks/ALGORITHM_BENCHMARK.md)**

**Key Findings:**
- ✅ **ErgoX.HuggingFace**: Broader model support (BPE, WordPiece, Unigram)
- ✅ **AutoTokenizer**: Seamless loading from HuggingFace Hub
- ✅ **Chat Templates**: Instruction-tuned model support (Llama, Mistral, Qwen)
- ✅ **Production-Ready**: 2,500+ tests with byte-to-byte Python parity

[View Full Benchmark Report →](../benchmarks/ALGORITHM_BENCHMARK.md)

## Getting Started

### Installation

Install the core NuGet package for your chosen tokenizer. Core packages include Windows and Linux x64 runtimes:

```bash
# HuggingFace Tokenizers (win-x64 + linux-x64 included)
dotnet add package ErgoX.TokenX.HuggingFace
```

**Additional Runtimes**: For macOS, iOS, or Android, install the corresponding runtime package:
- `ErgoX.TokenX.HuggingFace.Mac`
- `ErgoX.TokenX.HuggingFace.iOS`  
- `ErgoX.TokenX.HuggingFace.Android`

See the [Installation Guide](installation.md) for complete setup instructions.

## Quick Start Examples

### HuggingFace Tokenizers

```csharp
using ErgoX.TokenX.HuggingFace;

// Load tokenizer from pretrained model
using var tokenizer = AutoTokenizer.Load("bert-base-uncased");

// Encode text
var encoding = tokenizer.Encode("Hello, world!");
Console.WriteLine($"Tokens: {string.Join(", ", encoding.Tokens)}");
Console.WriteLine($"IDs: {string.Join(", ", encoding.Ids)}");

// Decode back to text
var decoded = tokenizer.Decode(encoding.Ids);
Console.WriteLine($"Decoded: {decoded}");
```



## Documentation Structure

- **[Installation](installation.md)** - Setup and deployment guide
- **[HuggingFace](huggingface/index.md)** - Complete HuggingFace Tokenizers documentation
- **[Examples](examples.md)** - Practical examples and tutorials
- **[Performance Benchmarks](../benchmarks/README.md)** - Algorithm performance comparison
- **[API Reference](api/index.md)** - Detailed API documentation

## Support

- [Report Issues](https://github.com/ergosumx/tokenx/issues)
- [Discussions](https://github.com/ergosumx/tokenx/discussions)

## License

See [LICENSE](https://github.com/ergosumx/tokenx/blob/main/LICENSE) for details.

