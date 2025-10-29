# ErgoX TokenX ML NLP Tokenizers

High-performance .NET tokenizer libraries with native interop for HuggingFace Tokenizers, Google SentencePiece, and OpenAI TikToken.

## Overview

This repository provides three comprehensive tokenization libraries for .NET applications:

- **[HuggingFace Tokenizers](huggingface/index.md)** - Full-featured tokenization supporting BPE, WordPiece, and Unigram models with multimodal chat capabilities
- **[Google SentencePiece](sentencepiece/index.md)** - Language-agnostic subword tokenization with lossless round-trip encoding
- **[OpenAI TikToken](tiktoken/index.md)** - Fast byte-pair encoding optimized for GPT models

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

| Feature | HuggingFace | SentencePiece | TikToken |
|---------|-------------|---------------|----------|
| **Best For** | Transformer models, multimodal chat | Multilingual NLP, seq2seq | GPT models, code generation |
| **Language Support** | Excellent | Excellent | Good (English-focused) |
| **Model Types** | BPE, WordPiece, Unigram | Unigram, BPE | BPE (byte-level) |
| **Special Features** | Chat templates, generation | Sampling, normalization | Fast encoding, special tokens |
| **Performance** | High | High | Very High |

## Getting Started

Each library can be used independently. Install the NuGet package for your chosen tokenizer:

```bash
# HuggingFace Tokenizers
dotnet add package ErgoX.TokenX.HuggingFace

# Google SentencePiece
dotnet add package ErgoX.TokenX.SentencePiece

# OpenAI TikToken
dotnet add package ErgoX.TokenX.Tiktoken
```

See the [Installation Guide](installation.md) for complete setup instructions including native library deployment.

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

### Google SentencePiece

```csharp
using ErgoX.TokenX.SentencePiece.Processing;

// Load SentencePiece model
using var processor = new SentencePieceProcessor();
processor.Load("spiece.model");

// Encode to IDs
var ids = processor.EncodeIds("Tokenization example");
Console.WriteLine($"IDs: {string.Join(", ", ids)}");

// Encode to pieces (subwords)
var pieces = processor.EncodePieces("Tokenization example");
Console.WriteLine($"Pieces: {string.Join(" | ", pieces)}");

// Decode back to text
var decoded = processor.DecodeIds(ids);
Console.WriteLine($"Decoded: {decoded}");
```

### OpenAI TikToken

```csharp
using ErgoX.TokenX.Tiktoken;

// Load GPT-2 encoding
using var encoding = TiktokenEncodingFactory.FromTiktokenFile(
    "gpt2",
    "'(?:[sdmt]|ll|ve|re)| ?\\p{L}+| ?\\p{N}+| ?[^\\s\\p{L}\\p{N}]+|\\s+(?!\\S)|\\s+",
    "mergeable_ranks.tiktoken",
    new Dictionary<string, int> { ["<|endoftext|>"] = 50256 });

// Encode text
var tokens = encoding.EncodeOrdinary("Hello, GPT!");
Console.WriteLine($"Tokens: {string.Join(", ", tokens)}");

// Decode back to text
var decoded = encoding.Decode(tokens.ToArray());
Console.WriteLine($"Decoded: {decoded}");
```

## Documentation Structure

- **[Installation](installation.md)** - Setup and deployment guide
- **[HuggingFace](huggingface/index.md)** - Complete HuggingFace Tokenizers documentation
- **[SentencePiece](sentencepiece/index.md)** - Complete Google SentencePiece documentation
- **[TikToken](tiktoken/index.md)** - Complete OpenAI TikToken documentation
- **[Examples](examples.md)** - Practical examples and tutorials
- **[API Reference](api/index.md)** - Detailed API documentation

## Support

- [Report Issues](https://github.com/ergosumx/tokenx/issues)
- [Discussions](https://github.com/ergosumx/tokenx/discussions)

## License

See [LICENSE](https://github.com/ergosumx/tokenx/blob/main/LICENSE) for details.

