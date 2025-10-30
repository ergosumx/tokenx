# HuggingFace Tokenizer Quickstart

**Comprehensive examples demonstrating ALL features of ErgoX.TokenX.HuggingFace**

This quickstart provides 16 complete, runnable examples covering every aspect of HuggingFace tokenization in .NET. All required models are included - just clone and run!

## 🚀 Quick Start

```bash
cd examples/HuggingFace/Quickstart
dotnet run
```

## 📦 What's Included

### Models (in `.models/` directory)
- **all-minilm-l6-v2** - BERT-based sentence embeddings (WordPiece tokenizer)
- **t5-small** - T5 sequence-to-sequence model (Unigram tokenizer)
- **meta-llama-3-8b-instruct** - Llama 3 instruction model (chat templates)

### 16 Comprehensive Examples

#### Core Features (Examples 1-4)
1. **Basic Tokenization** - Encode text to tokens, decode tokens to text
2. **Batch Processing** - Efficiently tokenize multiple texts
3. **Token-to-Text Conversion** - Understand WordPiece subword tokenization
4. **Special Tokens** - Learn about [CLS], [SEP], [PAD], [UNK], [MASK]

#### Advanced Features (Examples 5-10)
5. **Padding Strategies** - Right vs left padding for fixed-length sequences
6. **Truncation Strategies** - Handle texts longer than max length
7. **Text Pair Encoding** - Tokenize sentence pairs for classification
8. **Attention Masks & Type IDs** - Essential for transformer models
9. **Offset Mapping** - Map tokens back to character positions (for NER, QA)
10. **Word IDs & Sequence IDs** - Group subword tokens and distinguish sequences

#### Specialized Features (Examples 11-16)
11. **Chat Template Rendering** 🔥 - Format conversations for instruction-following models
12. **Custom Padding & Truncation** - Combine strategies for complex use cases
13. **Vocabulary Access** - Bidirectional token ↔ ID lookup
14. **Multiple Models** - Compare Word Piece (BERT), Unigram (T5), BPE (GPT)
15. **Overflowing Tokens** - Process long documents with stride/windowing
16. **Special Tokens & Vocabulary** - Inspect vocabulary and understand token handling

## 📖 Feature Documentation

### 1. Basic Tokenization
**Purpose**: Learn the fundamentals of encoding and decoding

**Key Concepts**:
- `Encode()` - Convert text to token IDs
- `Decode()` - Convert token IDs back to text
- Special tokens are automatically added (configurable)

**Example Output**:
```
Text: Hello, how are you?
Token IDs: [101, 7592, 1010, 2129, 2024, 2017, 1029, 102, ...]
Tokens: [[CLS], hello, ,, how, are, you, ?, [SEP], [PAD], ...]
Decoded: hello, how are you?
```

### 2. Batch Processing
**Purpose**: Efficiently process multiple texts simultaneously

**Key Concepts**:
- Process arrays/lists of texts in one call
- Automatic padding to uniform length
- Better performance than individual calls

**Use Cases**:
- Batch inference
- Dataset preprocessing
- High-throughput applications

### 3. Token-to-Text Conversion
**Purpose**: Understand WordPiece subword tokenization

**Key Concepts**:
- Unknown words split into subwords with `##` prefix
- `tokenization` → `[token, ##ization]`
- Access individual tokens via `encoding.Tokens`

**Why It Matters**:
- Handles out-of-vocabulary words gracefully
- Reduces vocabulary size while maintaining coverage
- Standard for BERT family models

### 4. Special Tokens
**Purpose**: Master the role of special tokens in transformer models

**Special Tokens**:
- `[CLS]` (101) - Classification token at sequence start
- `[SEP]` (102) - Separator between sequences
- `[PAD]` (0) - Padding to reach fixed length
- `[UNK]` (100) - Unknown token placeholder
- `[MASK]` (103) - Masked language modeling

**Control**: Use `addSpecialTokens` parameter to include/exclude

### 5. Padding Strategies
**Purpose**: Create fixed-length sequences for batch processing

**Strategies**:
- **Right Padding** (default) - Pad at the end: `[text][PAD][PAD]`
- **Left Padding** - Pad at the start: `[PAD][PAD][text]`

**Configuration**:
```csharp
tokenizer.Tokenizer.EnablePadding(new PaddingOptions(
    direction: PaddingDirection.Right,
    length: 128
));
```

**Use Cases**:
- Batch processing (requires uniform lengths)
- Generation tasks (left padding preserves context at end)

### 6. Truncation Strategies
**Purpose**: Handle texts exceeding maximum length

**Strategies**:
- `LongestFirst` - Truncate longest sequence first (for pairs)
- `OnlyFirst` - Only truncate first sequence
- `OnlySecond` - Only truncate second sequence

**Directions**:
- `Right` - Remove from end
- `Left` - Remove from start

**Configuration**:
```csharp
tokenizer.Tokenizer.EnableTruncation(new TruncationOptions(
    maxLength: 512,
    strategy: TruncationStrategy.LongestFirst,
    direction: TruncationDirection.Right
));
```

### 7. Text Pair Encoding
**Purpose**: Tokenize sentence pairs for classification tasks

**Format**: `[CLS] text1 [SEP] text2 [SEP]`

**Use Cases**:
- Natural Language Inference (NLI)
- Paraphrase detection
- Question answering
- Sentence similarity

**Type IDs**: Distinguish sequences
- Type ID 0 - First sequence
- Type ID 1 - Second sequence

### 8. Attention Masks & Type IDs
**Purpose**: Provide essential metadata for transformer models

**Attention Mask**:
- `1` - Real token (attend to this)
- `0` - Padding token (ignore this)

**Type IDs**:
- Distinguish multiple sequences in the same input
- Essential for BERT-style models

**Special Tokens Mask**:
- `1` - Special token ([CLS], [SEP], [PAD])
- `0` - Regular token

### 9. Offset Mapping
**Purpose**: Map tokens back to character positions in original text

**Use Cases**:
- **Named Entity Recognition (NER)** - Highlight entities
- **Question Answering** - Extract answer spans
- **Text highlighting** - Show which parts contributed to prediction

**Example**:
```
Text: "Machine learning is amazing!"
Token: 'machine' → chars[0:7] = "Machine"
Token: 'learning' → chars[8:16] = "learning"
```

### 10. Word IDs & Sequence IDs
**Purpose**: Group subword tokens and distinguish sequences

**Word IDs**:
- Same word ID for tokens from the same word
- `word` and `##piece` share the same word ID
- Useful for token-level tasks (POS tagging, NER)

**Sequence IDs**:
- `0` - First sequence
- `1` - Second sequence (in pair encoding)
- `null` - Special tokens

### 11. Chat Template Rendering 🔥
**Purpose**: Format conversations for instruction-following models

**What It Does**:
- Converts structured chat messages to model-specific format
- Each model family has its own template (Llama, Mistral, Qwen, etc.)
- Templates defined in `tokenizer_config.json`

**Example**:
```csharp
var messages = new[]
{
    ChatMessage.FromText("system", "You are helpful."),
    ChatMessage.FromText("user", "Hello!")
};

string prompt = tokenizer.ApplyChatTemplate(messages, 
    new ChatTemplateOptions { AddGenerationPrompt = true });
```

**Output** (Llama 3 format):
```
<|begin_of_text|><|start_header_id|>system<|end_header_id|>
You are helpful.<|eot_id|>
<|start_header_id|>user<|end_header_id|>
Hello!<|eot_id|>
<|start_header_id|>assistant<|end_header_id|>
```

**Critical for**:
- Chat applications
- Instruction fine-tuning
- Multi-turn conversations

### 12. Custom Padding & Truncation
**Purpose**: Combine both strategies for complex scenarios

**Example**:
```csharp
// Right-pad to 32 tokens, truncate from right if longer
tokenizer.Tokenizer.EnablePadding(new PaddingOptions(
    direction: PaddingDirection.Right,
    length: 32
));
tokenizer.Tokenizer.EnableTruncation(new TruncationOptions(
    maxLength: 32,
    direction: TruncationDirection.Right
));
```

**Use Cases**:
- Exact length requirements
- Mixed-length batches
- Memory-constrained environments

### 13. Vocabulary Access
**Purpose**: Bidirectional lookup between tokens and IDs

**Methods**:
- `TokenToId(string token)` - Get ID for a token
- `IdToToken(int id)` - Get token for an ID

**Use Cases**:
- Vocabulary inspection
- Custom token handling
- Debugging tokenization
- Understanding model vocabulary

### 14. Multiple Models (WordPiece, Unigram, BPE)
**Purpose**: Understand differences between tokenization algorithms

**Comparison**:
| Algorithm | Models | Characteristics | Example |
|-----------|--------|-----------------|---------|
| **WordPiece** | BERT, RoBERTa, DistilBERT | Greedy longest-match-first, `##` prefix | `token##ization` |
| **Unigram** | T5, ALBERT, XLM-R | Probabilistic segmentation, `▁` prefix | `▁token ization` |
| **BPE** | GPT-2, GPT-3, GPT-4 | Byte-pair encoding, merge rules | `tok enization` |

**Key Insight**: Always use the tokenizer matching your model!

### 15. Overflowing Tokens
**Purpose**: Process long documents in overlapping chunks

**Stride**: Number of tokens to overlap between chunks

**Use Cases**:
- Documents exceeding max length
- Context preservation across chunks
- Long-form question answering
- Document summarization

**Configuration**:
```csharp
tokenizer.Tokenizer.EnableTruncation(new TruncationOptions(
    maxLength: 512,
    stride: 128  // 128 tokens overlap
));
```

### 16. Special Tokens & Vocabulary Inspection
**Purpose**: Understand built-in special tokens and vocabulary structure

**What You Learn**:
- How to inspect existing special tokens
- Difference between encoding with/without special tokens
- Vocabulary lookup mechanics
- How unknown words are handled (subword splitting)

**Note**: Adding truly custom tokens requires modifying `tokenizer.json` and potentially retraining/fine-tuning the model.

## 🎯 Common Use Cases

### Sentence Embeddings
```csharp
using var tokenizer = AutoTokenizer.Load("all-minilm-l6-v2");
var encoding = tokenizer.Tokenizer.Encode("Your text here");
// Feed encoding.Ids to embedding model
```

### Text Classification
```csharp
using var tokenizer = AutoTokenizer.Load("bert-base-uncased");
var encoding = tokenizer.Encode(text1, text2);  // Text pair
// Use encoding.TypeIds to distinguish sequences
```

### Named Entity Recognition (NER)
```csharp
var encoding = tokenizer.Tokenizer.Encode(text);
// Use encoding.Offsets to map predictions back to characters
// Use encoding.WordIds to group subword tokens
```

### Chat Applications
```csharp
using var tokenizer = AutoTokenizer.Load("meta-llama-3-8b-instruct");
var messages = BuildConversation();
var prompt = tokenizer.ApplyChatTemplate(messages, 
    new ChatTemplateOptions { AddGenerationPrompt = true });
```

### Document Processing
```csharp
tokenizer.Tokenizer.EnableTruncation(new TruncationOptions(
    maxLength: 512,
    stride: 128
));
var encoding = tokenizer.Tokenizer.Encode(longDocument);
// Process main + overflowing chunks
```

## 💡 Best Practices

### Performance
1. **Batch when possible** - `EncodeBatch()` is faster than multiple `Encode()` calls
2. **Reuse tokenizers** - Load once, use many times (thread-safe after initialization)
3. **Disable defaults** - Use `ApplyTokenizerDefaults = false` when you need custom settings
4. **Build in Release** - `dotnet build --configuration Release`

### Correctness
1. **Match tokenizer to model** - Never mix tokenizers and models from different families
2. **Check special tokens** - Verify model expects [CLS]/[SEP] vs other formats
3. **Validate attention masks** - Ensure padding tokens are masked correctly
4. **Test with known inputs** - Compare against Python/HuggingFace outputs

### Development
1. **Start simple** - Begin with Example 1, progress through complexity
2. **Inspect outputs** - Print tokens, IDs, and metadata to understand behavior
3. **Read documentation** - Full API docs at [../../docs/HuggingFace/](../../docs/HuggingFace/)
4. **Check tests** - Integration tests in `tests/` show real-world usage

## 🔧 API Quick Reference

### AutoTokenizer
```csharp
// Load tokenizer
using var tokenizer = AutoTokenizer.Load(modelDirectory, options);

// Check capabilities
bool hasChat = tokenizer.SupportsChatTemplate;

// Apply chat template
string prompt = tokenizer.ApplyChatTemplate(messages, options);
var encoding = tokenizer.ApplyChatTemplateAsEncoding(messages, options);
```

### Tokenizer
```csharp
// Encoding
var encoding = tokenizer.Tokenizer.Encode(text, addSpecialTokens: true);
var encoding = tokenizer.Tokenizer.Encode(text, textPair, addSpecialTokens: true);
var encodings = tokenizer.Tokenizer.EncodeBatch(texts, addSpecialTokens: true);

// Decoding
string text = tokenizer.Tokenizer.Decode(ids, skipSpecialTokens: true);
var texts = tokenizer.Tokenizer.DecodeBatch(sequences, skipSpecialTokens: true);

// Vocabulary
int? id = tokenizer.Tokenizer.TokenToId(token);
string? token = tokenizer.Tokenizer.IdToToken(id);

// Configuration
tokenizer.Tokenizer.EnablePadding(paddingOptions);
tokenizer.Tokenizer.DisablePadding();
tokenizer.Tokenizer.EnableTruncation(truncationOptions);
tokenizer.Tokenizer.DisableTruncation();
```

### EncodingResult
```csharp
// Properties
encoding.Ids           // Token IDs (List<int>)
encoding.Tokens        // Token strings (List<string>)
encoding.Offsets       // Character positions (List<(int, int)>)
encoding.AttentionMask // 1=real, 0=padding (List<uint>)
encoding.TypeIds       // Sequence IDs (List<uint>)
encoding.SpecialTokensMask  // 1=special, 0=regular (List<uint>)
encoding.WordIds       // Word grouping (List<uint?>)
encoding.SequenceIds   // Sequence membership (List<uint?>)
encoding.Overflowing   // Overflow chunks (List<EncodingResult>)
encoding.Length        // Token count (int)
```

## 📚 Additional Resources

- **Full Documentation**: [../../docs/HuggingFace/quickstart.md](../../docs/HuggingFace/quickstart.md)
- **API Reference**: [../../docs/HuggingFace/](../../docs/HuggingFace/)
- **Getting Started**: [../../docs/getting-started.md](../../docs/getting-started.md)
- **Source Code**: [Program.cs](Program.cs) - Read the full implementation

## 🐛 Troubleshooting

### Model not found
**Error**: `DirectoryNotFoundException: Model directory 'xxx' not found`

**Solution**: Models should be in `.models/` directory. Verify:
```bash
ls .models/all-minilm-l6-v2
ls .models/t5-small
ls .models/meta-llama-3-8b-instruct
```

### Different results from Python
**Issue**: Token IDs don't match HuggingFace Python

**Checklist**:
1. Same tokenizer version?
2. Same padding/truncation settings?
3. Same special token handling?
4. Check `ApplyTokenizerDefaults` setting

### Out of memory
**Issue**: Processing very long documents

**Solutions**:
1. Enable truncation with reasonable max length
2. Process in chunks with stride
3. Use batch processing for multiple docs

## 🎓 Learning Path

### Beginner (Examples 1-4)
Start here to understand basics:
1. Basic Tokenization
2. Batch Processing
3. Token-to-Text Conversion
4. Special Tokens

### Intermediate (Examples 5-10)
Learn advanced configuration:
5. Padding Strategies
6. Truncation Strategies
7. Text Pair Encoding
8. Attention Masks & Type IDs
9. Offset Mapping
10. Word IDs & Sequence IDs

### Advanced (Examples 11-16)
Master specialized features:
11. Chat Template Rendering
12. Custom Padding & Truncation
13. Vocabulary Access
14. Multiple Models
15. Overflowing Tokens
16. Special Tokens & Vocabulary

## 📄 License

Apache 2.0 - See [../../../LICENSE](../../../LICENSE)

---

**Questions?** Open an issue on [GitHub](https://github.com/ergosumx/tokenx/issues)  
**Maintained by**: ErgoX VecraX Team  
**Last Updated**: October 30, 2025
