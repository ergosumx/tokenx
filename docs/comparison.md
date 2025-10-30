# Tokenizer Library Comparison

Comprehensive comparison of .NET tokenization libraries for machine learning and NLP applications.

## Executive Summary

| Library | Best For | Strengths | Limitations |
|---------|----------|-----------|-------------|
| **Microsoft.ML.Tokenizers** | Pure .NET solutions, OpenAI GPT models | No native dependencies, official Microsoft support, optimized TiktokenTokenizer | Limited HuggingFace model support |
| **ErgoX.TokenX.HuggingFace** | Production ML/NLP, open-source models | Complete feature set, battle-tested, thousands of models | Requires native binaries |

> **Note**: For OpenAI GPT models (GPT-2, GPT-3, GPT-4), we recommend using [Microsoft.ML.Tokenizers](https://www.nuget.org/packages/Microsoft.ML.Tokenizers/) which provides optimized `TiktokenTokenizer` implementation.

---

## Feature Comparison Matrix

### ✅ = Full Support | ⚠️ = Partial Support | ❌ = Not Supported

| Feature Category | Microsoft.ML.Tokenizers | ErgoX.TokenX.HuggingFace |
|-----------------|------------------------|--------------------------|
| **Tokenization Algorithms** |
| WordPiece (BERT) | ✅ | ✅ |
| BPE (GPT-2/GPT-3) | ✅ | ✅ |
| Tiktoken (GPT-3.5/4) | ✅ Optimized | ❌ Use Microsoft.ML |
| Unigram (T5/ALBERT) | ⚠️ Limited | ✅ |
| SentencePiece | ⚠️ Basic | ✅ |
| Byte-level BPE | ✅ | ✅ |
| **Model Support** |
| BERT family | ✅ | ✅ |
| GPT family | ✅ TiktokenTokenizer | ✅ |
| T5/BART/Pegasus | ⚠️ | ✅ |
| RoBERTa/DeBERTa | ✅ | ✅ |
| LLaMA/Mistral/Qwen | ❌ | ✅ |
| Whisper (audio) | ❌ | ✅ |
| Custom models | ⚠️ Limited | ✅ |
| **Core Operations** |
| Single text encoding | ✅ | ✅ |
| Text pair encoding | ✅ | ✅ |
| Batch encoding | ✅ | ✅ |
| Decoding | ✅ | ✅ |
| Byte-level decode | ✅ | ✅ |
| **Advanced Features** |
| Padding (left/right) | ✅ | ✅ |
| Truncation strategies | ✅ | ✅ |
| Attention masks | ✅ | ✅ |
| Type IDs (segment IDs) | ✅ | ✅ |
| Offset mapping | ✅ | ✅ |
| Word IDs | ⚠️ | ✅ |
| Sequence IDs | ⚠️ | ✅ |
| Special tokens mask | ⚠️ | ✅ |
| Overflowing tokens | ⚠️ | ✅ |
| **Chat & Generation** |
| Chat template rendering | ❌ | ✅ |
| Generation prompts | ❌ | ✅ |
| Multi-turn conversations | ❌ | ✅ |
| **Configuration** |
| Load from directory | ⚠️ | ✅ |
| Load from JSON | ✅ | ✅ |
| Apply defaults | ⚠️ | ✅ |
| Custom special tokens | ⚠️ | ✅ |
| Vocabulary access | ✅ | ✅ |
| **Platform Support** |
| Windows x64 | ✅ | ✅ |
| Windows ARM64 | ⚠️ | ⚠️ Untested |
| Linux x64 | ✅ | ✅ |
| Linux ARM64 | ⚠️ | ✅ |
| macOS x64 | ✅ | ✅ |
| macOS ARM64 (M1/M2) | ✅ | ✅ |
| **Deployment** |
| Pure managed code | ✅ | ❌ |
| Native dependencies | ❌ | ✅ Required |
| Self-contained publish | ✅ Easy | ✅ |
| NuGet package | ✅ Official | ✅ |
| **Developer Experience** |
| API documentation | ✅ Official | ✅ |
| IntelliSense support | ✅ | ✅ |
| Error messages | ✅ | ✅ |
| Examples | ⚠️ Limited | ✅ 16 examples |
| Test coverage | ⚠️ | ✅ 2,836+ tests |

---

## Detailed Comparisons

### 1. Microsoft.ML.Tokenizers

**Official Microsoft Implementation**

#### Pros ✅
- **Pure C#** - No native dependencies
- **Official support** - Part of ML.NET ecosystem
- **NuGet availability** - Easy installation from official feed
- **Cross-platform** - Works everywhere .NET runs
- **TiktokenTokenizer** - Optimized for OpenAI GPT models
- **Good for basics** - Handles common BERT/GPT use cases

#### Cons ❌
- **Limited HuggingFace model coverage** - Missing many modern models (LLaMA, Mistral, etc.)
- **Fewer features** - No chat templates, limited offset mapping
- **Less battle-tested** - Newer than HuggingFace tokenizers
- **Performance** - May be slower than optimized native code for some tasks

#### Best Use Cases
1. **Pure .NET environments** where native dependencies are prohibited
2. **OpenAI GPT models** (GPT-2, GPT-3, GPT-4) via TiktokenTokenizer
3. **Simple tokenization** for BERT models
4. **Quick prototyping** without model downloads
5. **Education** - Learning tokenization concepts

#### Example Usage
```csharp
using Microsoft.ML.Tokenizers;

// For BERT models
var tokenizer = new BertTokenizer("vocab.txt", lowerCase: true);
var tokens = tokenizer.Encode("Hello, world!");

// For GPT models (recommended)
var tiktokenizer = TiktokenTokenizer.CreateForModel("gpt-4");
var tokens = tiktokenizer.EncodeToIds("Hello, world!");
Console.WriteLine($"Token count: {tokens.Count}");
```

---

### 2. ErgoX.TokenX.HuggingFace

**Rust FFI Bindings to HuggingFace Tokenizers**

#### Pros ✅
- **Feature-complete** - All HuggingFace tokenizer features
- **Battle-tested** - Same Rust code used by Python transformers library
- **Thousands of models** - Compatible with entire HuggingFace Hub
- **Chat templates** - First-class support for instruction-following models
- **Rich metadata** - Offsets, word IDs, attention masks, etc.
- **Production-ready** - Used in real-world ML pipelines
- **High performance** - Native Rust backend

#### Cons ❌
- **Native dependencies** - Requires `tokenx_bridge.dll/.so/.dylib`
- **P/Invoke overhead** - Small latency per call (negligible for batch)
- **Deployment complexity** - Must bundle native libraries
- **Not for OpenAI GPT** - Use Microsoft.ML.Tokenizers instead

#### Best Use Cases
1. **Production ML/NLP** - Real-world applications with diverse models
2. **Open-source models** - BERT, T5, LLaMA, Mistral, Gemma, etc.
3. **Chat applications** - Instruction-following models with templates
4. **Advanced tokenization** - Offset mapping for NER, QA
5. **Research** - Exploring new models from HuggingFace Hub

#### Example Usage
```csharp
using ErgoX.TokenX.HuggingFace;

// Load from local model directory
using var tokenizer = AutoTokenizer.Load("meta-llama-3-8b-instruct");

// Chat template support
var messages = new[]
{
    ChatMessage.FromText("system", "You are helpful."),
    ChatMessage.FromText("user", "Hello!")
};
string prompt = tokenizer.ApplyChatTemplate(messages, 
    new ChatTemplateOptions { AddGenerationPrompt = true });

// Rich encoding metadata
var encoding = tokenizer.Encode("Machine learning");
Console.WriteLine($"Offsets: {string.Join(", ", encoding.Offsets)}");
Console.WriteLine($"Attention mask: {string.Join(", ", encoding.AttentionMask)}");
```

---

## Performance Characteristics

| Library | Encoding Speed | Memory Usage | Startup Time | Deployment Size |
|---------|---------------|--------------|--------------|-----------------|
| **Microsoft.ML** | Medium | Low (managed) | Fast | Small (~100KB) |
| **ErgoX HuggingFace** | Fast (native) | Medium | Medium (load native) | Large (~10-50MB) |

### Benchmarking Notes
- **Encoding speed**: ErgoX wins on throughput due to optimized native Rust code
- **Memory**: Microsoft.ML wins on GC pressure (pure managed)
- **Startup**: Microsoft.ML wins (no native library loading)
- **Deployment**: Microsoft.ML wins (smallest package size)

> Run benchmarks on your hardware: `dotnet run -c Release --project benchmarks/`

---

## Decision Matrix

### Choose **Microsoft.ML.Tokenizers** if:
- ✅ You need pure .NET (no native dependencies allowed)
- ✅ You're using OpenAI GPT models (GPT-2, GPT-3, GPT-4)
- ✅ You're tokenizing simple BERT models
- ✅ You want official Microsoft support
- ✅ Deployment size and startup speed are critical
- ❌ You don't need advanced features (chat templates, rich metadata)

### Choose **ErgoX.TokenX.HuggingFace** if:
- ✅ You're using open-source models (LLaMA, Mistral, T5, etc.)
- ✅ You need chat template support for instruction-following
- ✅ You require rich metadata (offsets, word IDs, attention masks)
- ✅ You're building production ML/NLP pipelines
- ✅ You want compatibility with Python transformers library
- ❌ You can deploy native dependencies

---

## Migration Guide

### From Microsoft.ML to ErgoX HuggingFace

**Before:**
```csharp
using Microsoft.ML.Tokenizers;

var tokenizer = new BertTokenizer("vocab.txt");
var tokens = tokenizer.Encode("Hello, world!");
foreach (var id in tokens.Ids)
{
    Console.WriteLine(id);
}
```

**After:**
```csharp
using ErgoX.TokenX.HuggingFace;

using var tokenizer = AutoTokenizer.Load("bert-base-uncased");
var encoding = tokenizer.Encode("Hello, world!");
foreach (var id in encoding.Ids)
{
    Console.WriteLine(id);
}
```

### From Python Transformers to ErgoX HuggingFace

**Python:**
```python
from transformers import AutoTokenizer

tokenizer = AutoTokenizer.from_pretrained("bert-base-uncased")
encoding = tokenizer("Hello, world!", return_offsets_mapping=True)
print(encoding["input_ids"])
print(encoding["offset_mapping"])
```

**C# (ErgoX):**
```csharp
using ErgoX.TokenX.HuggingFace;

using var tokenizer = AutoTokenizer.Load("bert-base-uncased");
var encoding = tokenizer.Encode("Hello, world!");
Console.WriteLine(string.Join(", ", encoding.Ids));
Console.WriteLine(string.Join(", ", encoding.Offsets));
```

---

## Compatibility Matrix

### Model Formats

| Format | Microsoft.ML | ErgoX HuggingFace |
|--------|-------------|-------------------|
| `tokenizer.json` | ⚠️ Partial | ✅ |
| `vocab.txt` (BERT) | ✅ | ✅ |
| `merges.txt` (GPT-2) | ✅ | ✅ |
| `.tiktoken` files | ✅ Via TiktokenTokenizer | ❌ Use Microsoft.ML |
| `tokenizer_config.json` | ❌ | ✅ |
| SentencePiece `.model` | ⚠️ | ✅ |

### .NET Versions

| .NET Version | Microsoft.ML | ErgoX HuggingFace |
|--------------|-------------|-------------------|
| .NET 8.0 | ✅ | ✅ |
| .NET 7.0 | ✅ | ✅ |
| .NET 6.0 | ✅ | ✅ |
| .NET Framework | ❌ | ❌ |

---

## Conclusion

**No single library is best for every use case.** Choose based on your requirements:

1. **OpenAI GPT models** → Microsoft.ML.Tokenizers (TiktokenTokenizer)
2. **Pure .NET + simple BERT** → Microsoft.ML.Tokenizers
3. **Production ML with diverse HuggingFace models** → ErgoX.TokenX.HuggingFace

For most **production ML/NLP applications using HuggingFace models**, **ErgoX.TokenX.HuggingFace** offers the best balance of features, compatibility, and performance.

---

## Further Reading

- **[Getting Started Guide](getting-started.md)** - Introduction to tokenization
- **[HuggingFace Quickstart](huggingface/quickstart.md)** - 16 comprehensive examples
- **[Installation Guide](installation.md)** - Setup and deployment
- **[Benchmarks](../benchmarks/README.md)** - Performance comparison

---

**Questions?** Open an issue on [GitHub](https://github.com/ergosumx/tokenx/issues)  
**Maintained by**: ErgoX VecraX Team  
**Last Updated**: October 30, 2025
