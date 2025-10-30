# ErgoX.TokenX.HuggingFace

.NET bindings for HuggingFace Tokenizers with native Rust performance.

[![NuGet](https://img.shields.io/nuget/v/ErgoX.TokenX.HuggingFace.svg)](https://www.nuget.org/packages/ErgoX.TokenX.HuggingFace/)

## Overview

This package provides high-performance tokenization for modern transformer models using HuggingFace's Rust-based tokenizers library. It supports all major tokenization algorithms (BPE, WordPiece, Unigram) and advanced features like chat templates and multimodal processing.

### Key Features

- **Multiple Algorithms**: BPE (GPT, RoBERTa), WordPiece (BERT), Unigram (ALBERT, XLNet)
- **Chat Templates**: Jinja2 template rendering for conversational models
- **Multimodal Support**: Text, audio (Whisper), and vision tokenization
- **Pre/Post Processing**: Automatic padding, truncation, attention masks
- **Thread-Safe**: Safe for concurrent use after initialization
- **Native Performance**: Rust backend via efficient P/Invoke layer

## Installation

### Core Package

The main package includes **Windows (x64)** and **Linux (x64)** runtimes:

```bash
dotnet add package ErgoX.TokenX.HuggingFace
```

### Additional Runtimes

For other platforms, install the corresponding runtime package:

```bash
# macOS (x64 and ARM64)
dotnet add package ErgoX.TokenX.HuggingFace.Mac

# iOS (ARM64)
dotnet add package ErgoX.TokenX.HuggingFace.iOS

# Android (ARM64, ARM, x64, x86)
dotnet add package ErgoX.TokenX.HuggingFace.Android
```

## Quick Start

> 💡 **Full working example**: [Quickstart/Program.cs](https://github.com/ergosumx/tokenx/blob/main/examples/HuggingFace/Quickstart/Program.cs)

```csharp
using ErgoX.TokenX.HuggingFace;

// Load tokenizer from local directory
var modelDirectory = "path/to/bert-base-uncased";

using var tokenizer = AutoTokenizer.Load(modelDirectory, new AutoTokenizerLoadOptions
{
    ApplyTokenizerDefaults = true
});

// Encode text
string text = "Hello, how are you?";
var encoding = tokenizer.Tokenizer.Encode(text);

Console.WriteLine($"Text: {text}");
Console.WriteLine($"Token IDs: [{string.Join(", ", encoding.Ids)}]");
Console.WriteLine($"Tokens: [{string.Join(", ", encoding.Tokens)}]");
Console.WriteLine($"Token count: {encoding.Length}");

// Decode back to text
var decoded = tokenizer.Tokenizer.Decode(encoding.Ids.ToArray());
Console.WriteLine($"Decoded: {decoded}");
```

**Output** (using `all-minilm-l6-v2` model):
```
Text: Hello, how are you?
Token IDs: [101, 7592, 1010, 2129, 2024, 2017, 1029, 102, ...]
Tokens: [[CLS], hello, ,, how, are, you, ?, [SEP], ...]
Token count: 128
Decoded: hello, how are you?
```

📄 **Source**: [examples/HuggingFace/Quickstart/Program.cs#L22-L46](https://github.com/ergosumx/tokenx/blob/main/examples/HuggingFace/Quickstart/Program.cs#L22-L46)

## Supported Models

This library works with any HuggingFace tokenizer. Common examples:

**BERT Family:**
- bert-base-uncased, bert-large-cased
- distilbert-base-uncased
- roberta-base, roberta-large
- albert-base-v2
- deberta-v3-base

**GPT Family:**
- gpt2, gpt2-medium, gpt2-large, gpt2-xl
- openai-gpt

**LLaMA & Instruct Models:**
- meta-llama-3-8b-instruct
- llama-2-7b-chat-hf
- mistral-7b-instruct-v0.2
- mixtral-8x7b-instruct-v0.1
- gemma-2-2b-it

**Multimodal:**
- openai/whisper-tiny, whisper-base, whisper-small
- microsoft/git-base
- Salesforce/blip-image-captioning-base

**Other:**
- t5-small, t5-base, google/flan-t5-base
- facebook/bart-large
- sentence-transformers/all-MiniLM-L6-v2

## Core API

### AutoTokenizer

High-level API for loading tokenizers:

```csharp
// Load from local directory
using var tokenizer = AutoTokenizer.Load("path/to/model");

// Load with options
var options = new AutoTokenizerLoadOptions
{
    ApplyTokenizerDefaults = true,
    LoadGenerationConfig = true
};
using var tokenizer = AutoTokenizer.Load("model-path", options);

// Load from HuggingFace Hub
using var tokenizer = AutoTokenizer.LoadFromPretrained("bert-base-uncased");
```

### Encoding

```csharp
// Basic encoding
var encoding = tokenizer.Encode("Hello, world!");

// Sentence pairs (for BERT-style models)
var encoding = tokenizer.Encode("Question?", "Context text");

// Batch encoding
var texts = new[] { "Text 1", "Text 2", "Text 3" };
var encodings = tokenizer.EncodeBatch(texts);
```

### EncodingResult

Tokenization outputs include:

```csharp
encoding.Ids              // Token IDs
encoding.Tokens           // Token strings
encoding.AttentionMask    // 1 for real tokens, 0 for padding
encoding.TypeIds          // Sentence type IDs (0 or 1)
encoding.SpecialTokensMask // 1 for special tokens
encoding.Offsets          // Character offsets
encoding.WordIds          // Word indices for subwords
```

### Chat Templates

Format multi-turn conversations:

```csharp
using ErgoX.TokenX.HuggingFace.Chat;

var messages = new List<ChatMessage>
{
    new("system", "You are a helpful assistant."),
    new("user", "What is machine learning?"),
    new("assistant", "Machine learning is..."),
    new("user", "Can you give an example?")
};

var options = new ChatTemplateOptions { AddGenerationPrompt = true };
string prompt = tokenizer.ApplyChatTemplate(messages, options);
```

### Special Tokens

Access special tokens:

```csharp
if (tokenizer.SpecialTokens is { } specialTokens)
{
    var bosToken = specialTokens.BosToken;  // Beginning of sequence
    var eosToken = specialTokens.EosToken;  // End of sequence
    var padToken = specialTokens.PadToken;  // Padding
    var unkToken = specialTokens.UnknownToken;  // Unknown token
}
```

## Usage Examples

> 💡 **See complete examples in**: [Quickstart/Program.cs](https://github.com/ergosumx/tokenx/blob/main/examples/HuggingFace/Quickstart/Program.cs)

### Batch Processing

```csharp
var modelDirectory = "path/to/bert-base-uncased";

using var tokenizer = AutoTokenizer.Load(modelDirectory, new AutoTokenizerLoadOptions
{
    ApplyTokenizerDefaults = true
});

var texts = new[]
{
    "Machine learning is fascinating.",
    "Natural language processing enables computers to understand text.",
    "Transformers revolutionized AI."
};

Console.WriteLine("Processing multiple texts:");
foreach (var text in texts)
{
    var encoding = tokenizer.Tokenizer.Encode(text);
    Console.WriteLine($"  '{text}' → {encoding.Length} tokens");
}
```

📄 **Source**: [examples/HuggingFace/Quickstart/Program.cs#L48-L70](https://github.com/ergosumx/tokenx/blob/main/examples/HuggingFace/Quickstart/Program.cs#L48-L70)

### Token-to-Text Conversion

```csharp
var modelDirectory = "path/to/gpt2";

using var tokenizer = AutoTokenizer.Load(modelDirectory, new AutoTokenizerLoadOptions
{
    ApplyTokenizerDefaults = true
});

string text = "Tokenization splits text into subwords.";
var encoding = tokenizer.Tokenizer.Encode(text);

Console.WriteLine($"Original: {text}");
Console.WriteLine("Individual tokens:");

for (int i = 0; i < encoding.Length; i++)
{
    var tokenId = encoding.Ids[i];
    var tokenText = encoding.Tokens[i];
    Console.WriteLine($"  Token {i}: ID={tokenId}, Text='{tokenText}'");
}
```

📄 **Source**: [examples/HuggingFace/Quickstart/Program.cs#L72-L94](https://github.com/ergosumx/tokenx/blob/main/examples/HuggingFace/Quickstart/Program.cs#L72-L94)

### Working with Special Tokens

```csharp
var modelDirectory = "path/to/bert-base-uncased";

using var tokenizer = AutoTokenizer.Load(modelDirectory, new AutoTokenizerLoadOptions
{
    ApplyTokenizerDefaults = true
});

string text = "Understanding special tokens";
var encoding = tokenizer.Tokenizer.Encode(text);

Console.WriteLine($"Text: {text}");
Console.WriteLine($"Full encoding with special tokens:");
Console.WriteLine($"  Tokens: [{string.Join(", ", encoding.Tokens)}]");
Console.WriteLine($"  IDs: [{string.Join(", ", encoding.Ids)}]");

// BERT adds [CLS] at start and [SEP] at end
Console.WriteLine($"First token (CLS): '{encoding.Tokens[0]}' (ID: {encoding.Ids[0]})");
Console.WriteLine($"Last token (SEP): '{encoding.Tokens[encoding.Length - 1]}' (ID: {encoding.Ids[encoding.Length - 1]})");
```

📄 **Source**: [examples/HuggingFace/Quickstart/Program.cs#L96-L116](https://github.com/ergosumx/tokenx/blob/main/examples/HuggingFace/Quickstart/Program.cs#L96-L116)

### Sentence Embeddings (Advanced)

For complete ONNX integration example:

```csharp
using ErgoX.TokenX.HuggingFace;
using Microsoft.ML.OnnxRuntime;

using var tokenizer = AutoTokenizer.Load("sentence-transformers/all-MiniLM-L6-v2");
using var session = new InferenceSession("model_quantized.onnx");

string text = "Machine learning enables computers to learn from data.";
var encoding = tokenizer.Tokenizer.Encode(text);

// Prepare ONNX inputs
var inputIds = CreateTensor(encoding.Ids);
var attentionMask = CreateTensor(encoding.AttentionMask);

// Run inference
using var results = session.Run(new[]
{
    NamedOnnxValue.CreateFromTensor("input_ids", inputIds),
    NamedOnnxValue.CreateFromTensor("attention_mask", attentionMask)
});

// Extract embeddings
var embeddings = results.First().AsTensor<float>();
```

📄 **Complete example**: [examples/HuggingFace/AllMiniLmL6V2Console/Program.cs](https://github.com/ergosumx/tokenx/blob/main/examples/HuggingFace/AllMiniLmL6V2Console/Program.cs)

## Examples

Complete working examples are available in the repository:

| Example | Description | Source |
|---------|-------------|--------|
| **Quickstart** | Simple examples: tokenization, batch processing, special tokens | [📄 Program.cs](https://github.com/ergosumx/tokenx/blob/main/examples/HuggingFace/Quickstart/Program.cs) |
| **AllMiniLmL6V2Console** | Sentence embeddings with ONNX Runtime integration | [📄 Program.cs](https://github.com/ergosumx/tokenx/blob/main/examples/HuggingFace/AllMiniLmL6V2Console/Program.cs) |
| **WhisperTinyConsole** | Audio transcription tokenization (Whisper) | [📄 Program.cs](https://github.com/ergosumx/tokenx/blob/main/examples/HuggingFace/WhisperTinyConsole/Program.cs) |
| **NougatBaseConsole** | Document understanding (Nougat) | [📄 Program.cs](https://github.com/ergosumx/tokenx/blob/main/examples/HuggingFace/NougatBaseConsole/Program.cs) |

Run examples:

```bash
# Quickstart examples
dotnet run --project examples/HuggingFace/Quickstart

# Sentence embeddings
dotnet run --project examples/HuggingFace/AllMiniLmL6V2Console

var texts = new[]
{
    "First document text",
    "Second document text",
    "Third document text"
};

var encodings = tokenizer.EncodeBatch(texts);

foreach (var encoding in encodings)
{
    Console.WriteLine($"Tokens: {encoding.Ids.Count}");
}
```

## Examples

Complete working examples are available in the repository:

- **AllMiniLmL6V2Console** - Sentence embeddings with ONNX
- **E5SmallV2Console** - Query/passage retrieval
- **MultilingualE5SmallConsole** - Multilingual embeddings
- **WhisperTinyConsole** - Speech-to-text tokenization
- **AutoTokenizerPipelineExplorer** - Inspect tokenizer metadata

Run examples:

```bash
dotnet run --project examples/HuggingFace/AllMiniLmL6V2Console
```

## Architecture

```
ErgoX.TokenX.HuggingFace
├── AutoTokenizer          # High-level API
├── Tokenizer              # Core tokenization
├── EncodingResult         # Tokenization outputs
├── Chat/
│   ├── ChatMessage        # Conversation messages
│   └── ChatTemplateOptions # Template rendering options
├── Options/
│   └── AutoTokenizerLoadOptions
└── Interop/               # Native bridge (P/Invoke)
    └── tokenx_bridge.{dll|so|dylib}
```

## Native Libraries

This package includes native Rust libraries:

- **Windows**: `runtimes/win-x64/native/tokenx_bridge.dll`
- **Linux**: `runtimes/linux-x64/native/libtokenx_bridge.so`
- **macOS**: Included in `ErgoX.TokenX.HuggingFace.Mac` package

Native libraries are automatically deployed to your output directory during build.

## Testing

The package is validated against Python reference implementations with 25+ integration tests ensuring token-exact parity.

## Platform Support

| Platform | Included | Runtime Package Required |
|----------|----------|--------------------------|
| Windows x64 | ✅ Yes | None |
| Linux x64 | ✅ Yes | None |
| macOS x64/ARM64 | ❌ No | `ErgoX.TokenX.HuggingFace.Mac` |
| iOS ARM64 | ❌ No | `ErgoX.TokenX.HuggingFace.iOS` |
| Android | ❌ No | `ErgoX.TokenX.HuggingFace.Android` |

## Requirements

- .NET 8.0 or later
- Windows: Visual C++ Redistributable (usually pre-installed)
- Linux: glibc 2.27+ (Ubuntu 18.04+, CentOS 8+)

## Building from Source

```bash
# Build Rust bridge
cd .ext/hf_bridge
cargo build --release

# Copy native library to runtime folder
# Windows
Copy-Item target/release/tokenx_bridge.dll ../../src/HuggingFace/runtimes/win-x64/native/

# Build .NET package
cd ../../src/HuggingFace
dotnet build --configuration Release
```

## Documentation

- **[Complete Documentation](../../docs/huggingface/index.md)** - Full API reference
- **[Installation Guide](../../docs/installation.md)** - Detailed setup
- **[Examples](../../examples/HuggingFace/)** - Working samples

## Contributing

Contributions are welcome! Please:
1. Run tests: `dotnet test --configuration Release`
2. Regenerate parity fixtures if needed: `python tests/Py/Huggingface/generate_benchmarks.py`
3. Follow coding standards in `.github/instructions/`

## License

Apache 2.0 - See [LICENSE](../../LICENSE) for details.

Built on [HuggingFace Tokenizers](https://github.com/huggingface/tokenizers) (Apache 2.0).

## Support

- **Issues**: [GitHub Issues](https://github.com/ergosumx/tokenx/issues)
- **Examples**: Check `examples/HuggingFace/` directory
- **Contact**: ErgoSum Technologies LTD

---

**Part of ErgoX TokenX ML NLP Tokenizers** | Maintained by ErgoX VecraX Team
