# Tokenizer Library Comparison

Comprehensive comparison of .NET tokenization libraries for machine learning and NLP applications.

## Executive Summary

| Library | Best For | Strengths | Limitations |
|---------|----------|-----------|-------------|
| **Microsoft.ML.Tokenizers** | Pure .NET solutions, simple use cases | No native dependencies, official Microsoft support | Limited features, fewer models |
| **ErgoX.TokenX.HuggingFace** | Production ML/NLP, open-source models | Complete feature set, battle-tested, thousands of models | Requires native binaries |
| **ErgoX.TokenX.Tiktoken** | OpenAI API integration, cost estimation | Optimized for GPT, minimal API | GPT-specific only, limited features |

---

## Feature Comparison Matrix

### ✅ = Full Support | ⚠️ = Partial Support | ❌ = Not Supported

| Feature Category | Microsoft.ML.Tokenizers | ErgoX.TokenX.HuggingFace | ErgoX.TokenX.Tiktoken |
|-----------------|------------------------|--------------------------|----------------------|
| **Tokenization Algorithms** |
| WordPiece (BERT) | ✅ | ✅ | ❌ |
| BPE (GPT-2/GPT-3) | ✅ | ✅ | ✅ |
| Unigram (T5/ALBERT) | ⚠️ Limited | ✅ | ❌ |
| SentencePiece | ⚠️ Basic | ✅ | ❌ |
| Byte-level BPE | ❌ | ✅ | ✅ |
| **Model Support** |
| BERT family | ✅ | ✅ | ❌ |
| GPT family | ✅ | ✅ | ✅ |
| T5/BART/Pegasus | ⚠️ | ✅ | ❌ |
| RoBERTa/DeBERTa | ✅ | ✅ | ❌ |
| LLaMA/Mistral/Qwen | ❌ | ✅ | ❌ |
| Whisper (audio) | ❌ | ✅ | ❌ |
| Custom models | ⚠️ Limited | ✅ | ⚠️ Custom BPE only |
| **Core Operations** |
| Single text encoding | ✅ | ✅ | ✅ |
| Text pair encoding | ✅ | ✅ | ❌ |
| Batch encoding | ✅ | ✅ | ⚠️ Manual loop |
| Decoding | ✅ | ✅ | ✅ |
| Byte-level decode | ❌ | ✅ | ✅ |
| **Advanced Features** |
| Padding (left/right) | ✅ | ✅ | ⚠️ Manual |
| Truncation strategies | ✅ | ✅ | ⚠️ Manual |
| Attention masks | ✅ | ✅ | ❌ |
| Type IDs (segment IDs) | ✅ | ✅ | ❌ |
| Offset mapping | ✅ | ✅ | ❌ |
| Word IDs | ⚠️ | ✅ | ❌ |
| Sequence IDs | ⚠️ | ✅ | ❌ |
| Special tokens mask | ⚠️ | ✅ | ❌ |
| Overflowing tokens | ⚠️ | ✅ | ❌ |
| **Chat & Generation** |
| Chat template rendering | ❌ | ✅ | ⚠️ Manual |
| Generation prompts | ❌ | ✅ | ❌ |
| Multi-turn conversations | ❌ | ✅ | ⚠️ Manual |
| **Configuration** |
| Load from directory | ⚠️ | ✅ | ⚠️ From file |
| Load from JSON | ✅ | ✅ | ❌ |
| Apply defaults | ⚠️ | ✅ | ❌ |
| Custom special tokens | ⚠️ | ✅ | ✅ |
| Vocabulary access | ✅ | ✅ | ⚠️ Limited |
| **Platform Support** |
| Windows x64 | ✅ | ✅ | ✅ |
| Windows ARM64 | ⚠️ | ⚠️ Untested | ⚠️ Untested |
| Linux x64 | ✅ | ✅ | ✅ |
| Linux ARM64 | ⚠️ | ✅ | ✅ |
| macOS x64 | ✅ | ✅ | ✅ |
| macOS ARM64 (M1/M2) | ✅ | ✅ | ✅ |
| **Deployment** |
| Pure managed code | ✅ | ❌ | ❌ |
| Native dependencies | ❌ | ✅ Required | ✅ Required |
| Self-contained publish | ✅ Easy | ✅ | ✅ |
| NuGet package | ✅ Official | ⚠️ Local | ⚠️ Local |
| **Developer Experience** |
| API documentation | ✅ Official | ✅ | ✅ |
| IntelliSense support | ✅ | ✅ | ✅ |
| Error messages | ✅ | ✅ | ✅ |
| Examples | ⚠️ Limited | ✅ 16 examples | ✅ 10 examples |
| Test coverage | ⚠️ | ✅ 37+ tests | ✅ |

---

## Detailed Comparisons

### 1. Microsoft.ML.Tokenizers

**Official Microsoft Implementation**

#### Pros ✅
- **Pure C#** - No native dependencies
- **Official support** - Part of ML.NET ecosystem
- **NuGet availability** - Easy installation from official feed
- **Cross-platform** - Works everywhere .NET runs
- **Good for basics** - Handles common BERT/GPT-2 use cases

#### Cons ❌
- **Limited model coverage** - Missing many modern models (LLaMA, Mistral, etc.)
- **Fewer features** - No chat templates, limited offset mapping
- **Less battle-tested** - Newer than HuggingFace tokenizers
- **Performance** - May be slower than optimized native code
- **Incomplete parity** - Missing some HuggingFace tokenizer features

#### Best Use Cases
1. **Pure .NET environments** where native dependencies are prohibited
2. **Simple tokenization** for BERT/GPT-2 models
3. **Quick prototyping** without model downloads
4. **Education** - Learning tokenization concepts

#### Example Usage
```csharp
using Microsoft.ML.Tokenizers;

var tokenizer = new BertTokenizer("vocab.txt", lowerCase: true);
var tokens = tokenizer.Encode("Hello, world!");
Console.WriteLine($"Tokens: {string.Join(", ", tokens.Tokens)}");
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

#### Cons ❌
- **Native dependencies** - Requires `tokenx_bridge.dll/.so/.dylib`
- **P/Invoke overhead** - Small latency per call (negligible for batch)
- **Deployment complexity** - Must bundle native libraries
- **Local packages** - Not yet on official NuGet (TODO)

#### Best Use Cases
1. **Production ML/NLP** - Real-world applications with diverse models
2. **Open-source models** - BERT, T5, LLaMA, Mistral, Gemma, etc.
3. **Chat applications** - Instruction-following models with templates
4. **Advanced tokenization** - Offset mapping for NER, QA
5. **Research** - Exploring new models from HuggingFace Hub

#### Example Usage
```csharp
using ErgoX.TokenX.HuggingFace;

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
var encoding = tokenizer.Tokenizer.Encode("Machine learning");
Console.WriteLine($"Offsets: {string.Join(", ", encoding.Offsets)}");
Console.WriteLine($"Attention mask: {string.Join(", ", encoding.AttentionMask)}");
```

---

### 3. ErgoX.TokenX.Tiktoken

**Native Bindings to OpenAI TikToken**

#### Pros ✅
- **Optimized for GPT** - Fast byte-level BPE
- **Official OpenAI code** - Same tokenization as OpenAI API
- **Minimal API** - Simple, focused interface
- **Cost estimation** - Accurate token counts for billing
- **UTF-8 byte support** - Handles any valid UTF-8 text

#### Cons ❌
- **GPT-specific only** - Only BPE encodings (gpt2, cl100k_base, o200k_base)
- **Limited features** - No padding, truncation, attention masks
- **No chat templates** - Must format chat manually
- **Manual batching** - No built-in batch encoding

#### Best Use Cases
1. **OpenAI API integration** - Token counting before API calls
2. **Cost estimation** - Calculate GPT-3.5/GPT-4 usage costs
3. **GPT applications** - When using only OpenAI models
4. **Minimal dependencies** - Lightweight tokenization

#### Example Usage
```csharp
using ErgoX.TokenX.Tiktoken;

var specialTokens = new Dictionary<string, int> { ["<|endoftext|>"] = 50256 };
using var encoding = TiktokenEncodingFactory.FromTiktokenFile(
    name: "gpt2",
    pattern: "...",
    tiktokenFilePath: "mergeable_ranks.tiktoken",
    specialTokens: specialTokens);

var tokens = encoding.EncodeOrdinary("Hello, world!");
int tokenCount = tokens.Count;

// Estimate GPT-4 cost
decimal cost = (tokenCount / 1000m) * 0.03m;  // $0.03 per 1k tokens
Console.WriteLine($"Estimated cost: ${cost:F4}");
```

---

## Performance Characteristics

| Library | Encoding Speed | Memory Usage | Startup Time | Deployment Size |
|---------|---------------|--------------|--------------|-----------------|
| **Microsoft.ML** | Medium | Low (managed) | Fast | Small (~100KB) |
| **ErgoX HuggingFace** | Fast (native) | Medium | Medium (load native) | Large (~10-50MB) |
| **ErgoX TikToken** | Very Fast (BPE) | Low | Fast | Medium (~5MB) |

### Benchmarking Notes
- **Encoding speed**: ErgoX libraries win on throughput due to optimized native code
- **Memory**: Microsoft.ML wins on GC pressure (pure managed)
- **Startup**: Microsoft.ML wins (no native library loading)
- **Deployment**: Microsoft.ML wins (smallest package size)

> Run benchmarks on your hardware: `dotnet run -c Release --project benchmarks/`

---

## Decision Matrix

### Choose **Microsoft.ML.Tokenizers** if:
- ✅ You need pure .NET (no native dependencies allowed)
- ✅ You're tokenizing simple BERT or GPT-2 models
- ✅ You want official Microsoft support
- ✅ Deployment size and startup speed are critical
- ❌ You don't need advanced features (chat templates, offsets)

### Choose **ErgoX.TokenX.HuggingFace** if:
- ✅ You're using open-source models (LLaMA, Mistral, T5, etc.)
- ✅ You need chat template support for instruction-following
- ✅ You require rich metadata (offsets, word IDs, attention masks)
- ✅ You're building production ML/NLP pipelines
- ✅ You want compatibility with Python transformers library
- ❌ You can deploy native dependencies

### Choose **ErgoX.TokenX.Tiktoken** if:
- ✅ You're using only OpenAI GPT models
- ✅ You need accurate token counting for OpenAI API
- ✅ You want minimal API surface
- ✅ You prioritize encoding speed over features
- ❌ You don't need advanced tokenization features

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
var encoding = tokenizer.Tokenizer.Encode("Hello, world!");
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
var encoding = tokenizer.Tokenizer.Encode("Hello, world!");
Console.WriteLine(string.Join(", ", encoding.Ids));
Console.WriteLine(string.Join(", ", encoding.Offsets));
```

### From Python TikToken to ErgoX TikToken

**Python:**
```python
import tiktoken

enc = tiktoken.get_encoding("gpt2")
tokens = enc.encode("Hello, world!")
print(len(tokens))
```

**C# (ErgoX):**
```csharp
using ErgoX.TokenX.Tiktoken;

using var encoding = TiktokenEncodingFactory.FromTiktokenFile(...);
var tokens = encoding.EncodeOrdinary("Hello, world!");
Console.WriteLine(tokens.Count);
```

---

## Compatibility Matrix

### Model Formats

| Format | Microsoft.ML | ErgoX HuggingFace | ErgoX TikToken |
|--------|-------------|-------------------|----------------|
| `tokenizer.json` | ⚠️ Partial | ✅ | ❌ |
| `vocab.txt` (BERT) | ✅ | ✅ | ❌ |
| `merges.txt` (GPT-2) | ✅ | ✅ | ❌ |
| `.tiktoken` files | ❌ | ❌ | ✅ |
| `tokenizer_config.json` | ❌ | ✅ | ❌ |
| SentencePiece `.model` | ⚠️ | ✅ | ❌ |

### .NET Versions

| .NET Version | Microsoft.ML | ErgoX HuggingFace | ErgoX TikToken |
|--------------|-------------|-------------------|----------------|
| .NET 8.0 | ✅ | ✅ | ✅ |
| .NET 7.0 | ✅ | ✅ | ✅ |
| .NET 6.0 | ✅ | ✅ | ✅ |
| .NET Framework | ❌ | ❌ | ❌ |

---

## Conclusion

**No single library is best for every use case.** Choose based on your requirements:

1. **Simplicity + No native deps** → Microsoft.ML.Tokenizers
2. **Production ML with diverse models** → ErgoX.TokenX.HuggingFace
3. **OpenAI API integration** → ErgoX.TokenX.Tiktoken

For most **production ML/NLP applications**, **ErgoX.TokenX.HuggingFace** offers the best balance of features, compatibility, and performance.

---

## Further Reading

- **[Getting Started Guide](getting-started.md)** - Introduction to tokenization
- **[HuggingFace Quickstart](../examples/HuggingFace/Quickstart/README.md)** - 16 comprehensive examples
- **[TikToken Quickstart](../examples/Tiktoken/Quickstart/README.md)** - 10 comprehensive examples
- **[Benchmarks](../benchmarks/README.md)** - Performance comparison

---

**Questions?** Open an issue on [GitHub](https://github.com/ergosumx/tokenx/issues)  
**Maintained by**: ErgoX VecraX Team  
**Last Updated**: October 30, 2025
