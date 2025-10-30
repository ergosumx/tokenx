# ErgoX TokenX - Getting Started

Welcome to ErgoX TokenX! This guide will help you get started with tokenization for HuggingFace transformers.

> **Note**: For OpenAI GPT models, we recommend using [Microsoft.ML.Tokenizers](https://www.nuget.org/packages/Microsoft.ML.Tokenizers/) which provides optimized `TiktokenTokenizer` implementation.

## Quick Links

- **[HuggingFace Tokenizer Quickstart](HuggingFace/quickstart.md)** - 16 comprehensive examples
- **[Main README](../README.md)** - Project overview and installation

## What is Tokenization?

Tokenization is the process of breaking text into smaller units (tokens) that machine learning models can understand. Different models use different tokenization algorithms:

- **WordPiece** (BERT, RoBERTa, DistilBERT) - Greedy subword tokenization
- **Unigram** (T5, ALBERT, XLM-RoBERTa) - Probabilistic subword segmentation
- **BPE** (GPT-2, GPT-3, GPT-4) - Byte-Pair Encoding

## Why ErgoX TokenX?

✅ **Production-ready** - Used in real-world applications  
✅ **Fast** - Native Rust/C++ backends via P/Invoke  
✅ **Accurate** - Token-exact parity with Python implementations  
✅ **Cross-platform** - Windows, Linux, macOS (x64 & ARM64)  
✅ **Comprehensive** - Support for all major model families  

## Choose Your Path

### For HuggingFace Models (BERT, T5, Llama, etc.)

**Start here:** [HuggingFace Tokenizer Quickstart](HuggingFace/quickstart.md)

Learn how to:
- Tokenize text for transformer models
- Handle padding and truncation
- Work with text pairs for classification
- Use chat templates for instruction-following models
- Compare different tokenization algorithms (WordPiece, Unigram, BPE)

**Run the examples:**
```bash
cd examples/HuggingFace/Quickstart
dotnet run
```

Models included:
- `all-minilm-l6-v2` - Sentence embeddings (WordPiece)
- `t5-small` - Sequence-to-sequence (Unigram)
- `meta-llama-3-8b-instruct` - Chat and instruction following

## Next Steps

After completing the quickstarts:

1. **Explore the API** - Both guides include detailed API references
2. **Check the examples** - Additional examples in `examples/` directory
3. **Read the tests** - Comprehensive test suite in `tests/` directory
4. **Join the community** - Contribute or ask questions on GitHub

## Key Concepts

### Encoding
Converting text to token IDs:
```csharp
var encoding = tokenizer.Encode("Hello, world!");
// Result: [101, 7592, 1010, 2088, 102]
```

### Decoding
Converting token IDs back to text:
```csharp
var text = tokenizer.Decode(encoding.Ids);
// Result: "hello, world!"
```

### Special Tokens
- `[CLS]` - Classification token (start of sequence)
- `[SEP]` - Separator token (end of sequence)
- `[PAD]` - Padding token (for fixed-length sequences)
- `[UNK]` - Unknown token (out-of-vocabulary words)

### Attention Masks
Binary masks indicating real tokens (1) vs padding (0):
```csharp
var mask = encoding.AttentionMask;
// Result: [1, 1, 1, 1, 1, 0, 0, 0]
```

### Context Windows
Maximum number of tokens a model can process:
- BERT: 512 tokens
- T5: 512 tokens (encoder/decoder)
- RoBERTa: 512 tokens
- Llama: 2048-4096 tokens (varies by version)
- Mistral: 8192 tokens

## Performance Tips

1. **Batch processing** - Tokenize multiple texts together for better performance
2. **Disable defaults** - Use `ApplyTokenizerDefaults = false` when you need custom padding/truncation
3. **Reuse tokenizers** - Load once and reuse across requests
4. **Async when possible** - Use async APIs for I/O-bound operations

## Common Use Cases

### Text Classification
Use text pair encoding with attention masks and type IDs:
```csharp
var encoding = tokenizer.Encode(premise, hypothesis);
// Use encoding.TypeIds to distinguish the two sentences
```

### Named Entity Recognition (NER)
Use offset mapping to align tokens with original text:
```csharp
var encoding = tokenizer.Encode(text);
// Use encoding.Offsets to map tokens back to character positions
```

### Question Answering
Use word IDs to group subword tokens:
```csharp
var encoding = tokenizer.Encode(context);
// Use encoding.WordIds to identify which tokens belong to the same word
```

### Chat/Instruction Following
Use chat templates to format conversations:
```csharp
var messages = new[] {
    ChatMessage.FromText("system", "You are helpful."),
    ChatMessage.FromText("user", "Hello!")
};
var prompt = tokenizer.ApplyChatTemplate(messages);
```



## Troubleshooting

### Model not found
Ensure models are in the `.models` directory relative to your executable.

### Different results from Python
Verify you're using the same tokenizer version and settings. Check special token handling.

### Memory issues with large texts
Use truncation and process in chunks for very long documents.

### Performance issues
- Enable release mode: `dotnet build --configuration Release`
- Use batch processing for multiple texts
- Reuse tokenizer instances

## Support

- **Documentation**: This directory (`docs/`)
- **Examples**: `examples/` directory
- **Issues**: [GitHub Issues](https://github.com/ergosumx/tokenx/issues)
- **Tests**: `tests/` directory for reference implementations

## License

Apache 2.0 - See [LICENSE](../LICENSE) for details.

Built on top of:
- [HuggingFace Tokenizers](https://github.com/huggingface/tokenizers) (Apache 2.0)

---

**Ready to start?** Pick a quickstart guide above and dive in! 🚀
