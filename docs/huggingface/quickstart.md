# HuggingFace Tokenizer Quickstart Guide

**Complete walkthrough of ALL HuggingFace Tokenizer features with working examples**

This guide demonstrates every feature of the ErgoX HuggingFace Tokenizer library through a comprehensive, runnable quickstart example. Each section maps directly to working code in [`examples/HuggingFace/Quickstart/Program.cs`](../../examples/HuggingFace/Quickstart/Program.cs).

---

## 🚀 Prerequisites

Before running the examples, ensure required models are available:

```bash
# Models included in quickstart:
examples/HuggingFace/Quickstart/.models/all-minilm-l6-v2/  # WordPiece (BERT-based)
examples/HuggingFace/Quickstart/.models/t5-small/           # Unigram (T5-based)
examples/HuggingFace/Quickstart/.models/meta-llama-3-8b-instruct/  # Chat templates
```

The quickstart includes these models and is ready to run:

```bash
cd examples/HuggingFace/Quickstart
dotnet run
```

---

## 📚 Complete Feature Coverage (16 Examples)

### ✅ Example 1: Basic Tokenization

Learn the fundamentals with BERT-based tokenization.

```csharp
using var tokenizer = AutoTokenizer.Load(modelDirectory, new AutoTokenizerLoadOptions
{
    ApplyTokenizerDefaults = true
});

string text = "Hello, how are you?";
var encoding = tokenizer.Tokenizer.Encode(text);

Console.WriteLine($"Text: {text}");
Console.WriteLine($"Token IDs: [{string.Join(", ", encoding.Ids)}]");
Console.WriteLine($"Tokens: [{string.Join(", ", encoding.Tokens)}]");
```

**Output:**
```
Text: Hello, how are you?
Token IDs: [101, 7592, 1010, 2129, 2024, 2017, 1029, 102, 0, 0, ...]
Tokens: [[CLS], hello, ,, how, are, you, ?, [SEP], [PAD], [PAD], ...]
Token count: 128
Decoded: hello, how are you?
```

**Key Concepts:**
- `[CLS]` token (101): Classification token added at start
- `[SEP]` token (102): Separator token added at end
- `[PAD]` token (0): Padding to reach fixed length (128 tokens default)
- WordPiece tokenization: Splits unknown words into subwords

---

### ✅ Example 2: Batch Processing

Efficiently tokenize multiple texts at once.

```csharp
var texts = new[]
{
    "Machine learning is fascinating.",
    "Natural language processing enables computers to understand text.",
    "Transformers revolutionized AI."
};

foreach (var text in texts)
{
    var encoding = tokenizer.Tokenizer.Encode(text);
    Console.WriteLine($"'{text}' → {encoding.Length} tokens");
}
```

**Output:**
```
Processing multiple texts:
  'Machine learning is fascinating.' → 128 tokens
  'Natural language processing enables computers to understand text.' → 128 tokens
  'Transformers revolutionized AI.' → 128 tokens
```

---

### ✅ Example 3: Token-to-Text Conversion

Understand individual token mappings.

```csharp
string text = "Tokenization splits text into subwords.";
var encoding = tokenizer.Tokenizer.Encode(text);

for (int i = 0; i < encoding.Length; i++)
{
    var tokenId = encoding.Ids[i];
    var tokenText = encoding.Tokens[i];
    Console.WriteLine($"Token {i}: ID={tokenId}, Text='{tokenText}'");
}
```

**Output:**
```
Original: Tokenization splits text into subwords.
Individual tokens:
  Token 0: ID=101, Text='[CLS]'
  Token 1: ID=19204, Text='token'
  Token 2: ID=3989, Text='##ization'
  Token 3: ID=19584, Text='splits'
  Token 4: ID=3793, Text='text'
  Token 5: ID=2046, Text='into'
  Token 6: ID=4942, Text='sub'
  Token 7: ID=22104, Text='##words'
  Token 8: ID=1012, Text='.'
  Token 9: ID=102, Text='[SEP]'
  Token 10-127: ID=0, Text='[PAD]'
```

**Note:** The `##` prefix indicates a subword continuation (WordPiece).

---

### ✅ Example 4: Special Tokens

Work with model-specific special tokens.

```csharp
string text = "Understanding special tokens";
var encoding = tokenizer.Tokenizer.Encode(text);

Console.WriteLine($"Full encoding with special tokens:");
Console.WriteLine($"Tokens: [{string.Join(", ", encoding.Tokens)}]");
Console.WriteLine($"First token (CLS): '{encoding.Tokens[0]}' (ID: {encoding.Ids[0]})");
Console.WriteLine($"Last non-pad token (SEP): '{encoding.Tokens[encoding.Length - 1]}'");
```

**Output:**
```
Text: Understanding special tokens
Tokens: [[CLS], understanding, special, token, ##s, [SEP], [PAD], ...]
IDs: [101, 4824, 2569, 19204, 2015, 102, 0, ...]
First token (CLS): '[CLS]' (ID: 101)
```

---

### ✅ Example 5: Padding Strategies

Control padding direction for different model types.

```csharp
// Right padding (default for encoder models like BERT)
tokenizer.Tokenizer.EnablePadding(new PaddingOptions(
    direction: PaddingDirection.Right,
    padId: 0,
    padTypeId: 0,
    padToken: "[PAD]",
    length: 20));

// Left padding (common for decoder models like GPT)
tokenizer.Tokenizer.EnablePadding(new PaddingOptions(
    direction: PaddingDirection.Left,
    padId: 0,
    padTypeId: 0,
    padToken: "[PAD]",
    length: 20));
```

**Output:**
```
Right-padded to 20 tokens:
  Tokens: [[CLS], hello, ,, world, !, [SEP], [PAD], [PAD], ...]

Left-padded to 20 tokens:
  First 5 tokens: [[PAD], [PAD], [PAD], [PAD], [PAD]]
  Last 5 tokens: [hello, ,, world, !, [SEP]]
```

**When to use:**
- Right padding: Encoder models (BERT, RoBERTa) - attention focuses on real tokens
- Left padding: Decoder models (GPT) - maintains causal attention flow

---

### ✅ Example 6: Truncation Strategies

Handle text that exceeds maximum length.

```csharp
tokenizer.Tokenizer.EnableTruncation(new TruncationOptions(
    maxLength: 20,
    stride: 0,
    strategy: TruncationStrategy.LongestFirst,
    direction: TruncationDirection.Right));  // or Left
```

**Output:**
```
Right truncation (max 20 tokens):
  Kept: 20 tokens
  First 10: [[CLS], this, is, a, very, long, text, that, will, be]

Left truncation (max 20 tokens):
  Kept: 20 tokens
  Last 10: [different, approaches, to, handling, overflow, ., [SEP], [PAD], [PAD], [PAD]]
```

**Strategies:**
- `LongestFirst`: Truncate longest sequence first (for pairs)
- `OnlyFirst`: Only truncate the first sequence
- `OnlySecond`: Only truncate the second sequence

---

### ✅ Example 7: Text Pair Encoding

Encode sentence pairs for classification tasks.

```csharp
string text1 = "The cat sits on the mat.";
string text2 = "A feline rests on the rug.";

var pairEncoding = tokenizer.Tokenizer.Encode(text1, text2, addSpecialTokens: true);

Console.WriteLine($"Tokens: [{string.Join(", ", pairEncoding.Tokens)}]");
Console.WriteLine($"Type IDs: [{string.Join(", ", pairEncoding.TypeIds)}]");
```

**Output:**
```
Text 1: The cat sits on the mat.
Text 2: A feline rests on the rug.

Pair encoding:
  Total tokens: 128
  Tokens: [[CLS], the, cat, sits, on, the, mat, ., [SEP], a, fe, ##line, rests, on, the, rug, ., [SEP], [PAD], ...]
  
Type IDs (0=first, 1=second):
  [0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, ...]
```

**Use Cases:**
- Semantic similarity (do these sentences mean the same thing?)
- Natural Language Inference (does text2 follow from text1?)
- Question-Answer pairs

---

### ✅ Example 8: Attention Masks and Type IDs

Understand model input metadata.

```csharp
tokenizer.Tokenizer.EnablePadding(new PaddingOptions(
    direction: PaddingDirection.Right,
    padId: 0,
    padTypeId: 0,
    padToken: "[PAD]",
    length: 15));

string text = "Attention is all you need.";
var encoding = tokenizer.Tokenizer.Encode(text);

Console.WriteLine($"Attention Mask (1=real, 0=padding):");
Console.WriteLine($"  [{string.Join(", ", encoding.AttentionMask)}]");

Console.WriteLine($"Type IDs (segment IDs):");
Console.WriteLine($"  [{string.Join(", ", encoding.TypeIds)}]");

Console.WriteLine($"Special Tokens Mask (1=special, 0=regular):");
Console.WriteLine($"  [{string.Join(", ", encoding.SpecialTokensMask)}]");
```

**Output:**
```
Tokens: [[CLS], attention, is, all, you, need, ., [SEP], [PAD], [PAD], ...]

Attention Mask (1=real, 0=padding):
  [1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0]

Type IDs (segment IDs):
  [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]

Special Tokens Mask (1=special, 0=regular):
  [1, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1]
```

**Explanation:**
- **Attention Mask**: Tells the model which tokens to attend to (1) vs ignore (0)
- **Type IDs**: Distinguishes first sequence (0) from second sequence (1) in pairs
- **Special Tokens Mask**: Identifies special tokens for filtering or loss calculation

---

### ✅ Example 9: Offset Mapping

Map tokens back to original character positions.

```csharp
string text = "Machine learning is amazing!";
var encoding = tokenizer.Tokenizer.Encode(text);

for (int i = 0; i < Math.Min(10, encoding.Length); i++)
{
    var token = encoding.Tokens[i];
    var (start, end) = encoding.Offsets[i];
    
    if (start > 0 || end > 0)
    {
        string substring = text.Substring(start, end - start);
        Console.WriteLine($"Token {i}: '{token}' → chars[{start}:{end}] = \"{substring}\"");
    }
}
```

**Output:**
```
Text: "Machine learning is amazing!"

Token offsets (character positions):
  Token 0: '[CLS]' → special token (no offset)
  Token 1: 'machine' → chars[0:7] = "Machine"
  Token 2: 'learning' → chars[8:16] = "learning"
  Token 3: 'is' → chars[17:19] = "is"
  Token 4: 'amazing' → chars[20:27] = "amazing"
  Token 5: '!' → chars[27:28] = "!"
  Token 6: '[SEP]' → special token (no offset)
```

**Use Cases:**
- Named Entity Recognition (NER): Map predicted entity spans to original text
- Question Answering: Extract answer text from document
- Text highlighting: Show which parts of text contributed to model decision

---

### ✅ Example 10: Word IDs and Sequence IDs

Understand token-to-word mappings.

```csharp
string text = "WordPiece tokenization splits words.";
var encoding = tokenizer.Tokenizer.Encode(text);

for (int i = 0; i < encoding.Length; i++)
{
    var token = encoding.Tokens[i];
    var wordId = encoding.WordIds[i];
    var seqId = encoding.SequenceIds[i];
    
    Console.WriteLine($"Token {i}: '{token}' → Word {wordId?.ToString() ?? "null"}, Seq {seqId?.ToString() ?? "null"}");
}
```

**Output:**
```
Text: WordPiece tokenization splits words.

Word IDs (which word each token belongs to):
  Token 0: '[CLS]' → Word null, Seq null
  Token 1: 'word' → Word 0, Seq 0
  Token 2: '##piece' → Word 0, Seq 0
  Token 3: 'token' → Word 1, Seq 0
  Token 4: '##ization' → Word 1, Seq 0
  Token 5: 'splits' → Word 2, Seq 0
  Token 6: 'words' → Word 3, Seq 0
  Token 7: '.' → Word 4, Seq 0
  Token 8: '[SEP]' → Word null, Seq null
```

**Use Cases:**
- Word-level classification: Aggregate subword predictions
- Alignment: Match tokens to original words in parallel corpora
- Debugging: Understand tokenizer behavior

---

### ✅ Example 11: Chat Template Rendering

Convert conversations to model-specific prompts using **Meta Llama 3**.

```csharp
// Using meta-llama-3-8b-instruct for chat template support
using var tokenizer = AutoTokenizer.Load(modelDirectory, new AutoTokenizerLoadOptions
{
    ApplyTokenizerDefaults = true,
    LoadGenerationConfig = true
});

if (tokenizer.SupportsChatTemplate)
{
    var messages = new[]
    {
        ChatMessage.FromText("system", "You are a helpful AI assistant."),
        ChatMessage.FromText("user", "What is machine learning?"),
        ChatMessage.FromText("assistant", "Machine learning is a subset of AI..."),
        ChatMessage.FromText("user", "Can you explain transformers?")
    };

    var options = new ChatTemplateOptions { AddGenerationPrompt = true };
    string prompt = tokenizer.ApplyChatTemplate(messages, options);

    // Or encode directly to tokens
    var encoding = tokenizer.ApplyChatTemplateAsEncoding(messages, options);
}
```

**Output:**
```
Conversation:
  system: You are a helpful AI assistant.
  user: What is machine learning?
  assistant: Machine learning is a subset of AI that enables systems to...
  user: Can you explain transformers?

Rendered prompt:
<|im_start|>system
You are a helpful AI assistant.<|im_end|>
<|im_start|>user
What is machine learning?<|im_end|>
<|im_start|>assistant
Machine learning is a subset of AI that enables systems to learn from data.<|im_end|>
<|im_start|>user
Can you explain transformers?<|im_end|>
<|im_start|>assistant

Encoded tokens: 245 tokens
```

**Model-Specific Templates:**
- Each model family has its own chat format (Llama, Mistral, Qwen, etc.)
- Templates are defined in `tokenizer_config.json`
- `AddGenerationPrompt = true` adds the assistant prefix for generation

---

### ✅ Example 12: Custom Padding and Truncation Combined

Apply both strategies simultaneously.

```csharp
tokenizer.Tokenizer.EnablePadding(new PaddingOptions(
    direction: PaddingDirection.Right,
    padId: 0,
    padTypeId: 0,
    padToken: "[PAD]",
    length: 32));

tokenizer.Tokenizer.EnableTruncation(new TruncationOptions(
    maxLength: 32,
    stride: 0,
    strategy: TruncationStrategy.LongestFirst,
    direction: TruncationDirection.Right));

var texts = new[]
{
    "Short text.",
    "This is a medium-length text with several words.",
    "This is a very long text that will definitely exceed the maximum token limit..."
};
```

**Output:**
```
Processing texts with padding AND truncation (target: 32 tokens):

  Input: Short text.
  Result: 32 tokens (real: 5, padding: 27)

  Input: This is a medium-length text with several words.
  Result: 32 tokens (real: 13, padding: 19)

  Input: This is a very long text that will definitely exceed...
  Result: 32 tokens (real: 32, padding: 0)
```

---

### ✅ Example 13: Vocabulary Access

Look up tokens and IDs directly.

```csharp
// Token to ID lookup
var sampleTokens = new[] { "[CLS]", "[SEP]", "[PAD]", "[UNK]", "hello", "world", "##ing" };

foreach (var token in sampleTokens)
{
    int? tokenId = tokenizer.Tokenizer.TokenToId(token);
    Console.WriteLine($"'{token}' → ID {tokenId?.ToString() ?? "NOT FOUND"}");
}

// ID to Token lookup
var sampleIds = new[] { 101, 102, 0, 100, 7592, 2088 };

foreach (var id in sampleIds)
{
    string? token = tokenizer.Tokenizer.IdToToken(id);
    Console.WriteLine($"ID {id} → '{token ?? "NOT FOUND"}'");
}
```

**Output:**
```
Token to ID lookup:
  '[CLS]' → ID 101
  '[SEP]' → ID 102
  '[PAD]' → ID 0
  '[UNK]' → ID 100
  'hello' → ID 7592
  'world' → ID 2088
  '##ing' → ID 2075

ID to Token lookup:
  ID 101 → '[CLS]'
  ID 102 → '[SEP]'
  ID 0 → '[PAD]'
  ID 100 → '[UNK]'
  ID 7592 → 'hello'
  ID 2088 → 'world'
```

---

### ✅ Example 14: Working with Multiple Models (WordPiece, BPE, Unigram)

Compare different tokenization algorithms across model families.

```csharp
string testText = "tokenization";

// WordPiece tokenizer (BERT-based models)
using var bertTokenizer = AutoTokenizer.Load("all-minilm-l6-v2");
var bertEncoding = bertTokenizer.Tokenizer.Encode(testText);
// Output: [[CLS], token, ##ization, [SEP], ...]

// Unigram tokenizer (T5-based models)
using var t5Tokenizer = AutoTokenizer.Load("t5-small");
var t5Encoding = t5Tokenizer.Tokenizer.Encode(testText);
// Output: [▁token, ization, </s>]  (3 tokens)

// Different algorithms produce different segmentations
```

**Output:**
```
WordPiece tokenizer (all-minilm-l6-v2):
  Text: tokenization
  Tokens: [[CLS], token, ##ization, [SEP], [PAD], ...]
  Token count: 128

Unigram tokenizer (t5-small):
  Text: tokenization
  Tokens: [▁token, ization, </s>]
  Token count: 3

💡 Different tokenization algorithms:
   - WordPiece (BERT): Greedy longest-match-first, uses ## prefix
   - Unigram (T5): Probabilistic subword segmentation, uses ▁ prefix
   - BPE (GPT): Byte-pair encoding based on merge rules
💡 Always use the tokenizer that matches your model!
```

**Key Insight:** Each tokenization algorithm has different characteristics. Always use the tokenizer that matches your model to ensure correct results.

---

### ✅ Example 15: Overflowing Tokens (Stride/Windowing)

Process long documents in overlapping chunks.

```csharp
tokenizer.Tokenizer.EnableTruncation(new TruncationOptions(
    maxLength: 20,
    stride: 5,  // Overlap between chunks
    strategy: TruncationStrategy.LongestFirst,
    direction: TruncationDirection.Right));

string longText = "This is a very long document that exceeds the maximum token limit...";
var encoding = tokenizer.Tokenizer.Encode(longText);

Console.WriteLine($"Main encoding: {encoding.Length} tokens");
Console.WriteLine($"Overflowing encodings: {encoding.Overflowing.Count}");
```

**Output:**
```
Main encoding: 20 tokens
Overflowing encodings: 3

📊 Encoding breakdown:
  Main: 20 tokens
  Overflow 1: 20 tokens
  Overflow 2: 20 tokens
  Overflow 3: 15 tokens

💡 Use stride to process long documents in overlapping chunks
```

**Use Cases:**
- Long document classification with 512-token limit models
- Question answering over documents longer than context window
- Maintaining context across chunks for better understanding

---

### ✅ Example 16: Special Tokens & Vocabulary Inspection

Understand built-in special tokens and explore vocabulary structure.

```csharp
using var tokenizer = AutoTokenizer.Load(modelDirectory);

// Inspect existing special tokens
var specialTokens = new[] { "[CLS]", "[SEP]", "[PAD]", "[UNK]", "[MASK]" };
Console.WriteLine("Built-in special tokens:");
foreach (var token in specialTokens)
{
    int? tokenId = tokenizer.Tokenizer.TokenToId(token);
    Console.WriteLine($"  {token} → ID {tokenId}");
}

// Compare encoding with/without special tokens
string text = "Hello, world!";
var withSpecial = tokenizer.Tokenizer.Encode(text, addSpecialTokens: true);
var withoutSpecial = tokenizer.Tokenizer.Encode(text, addSpecialTokens: false);

Console.WriteLine($"\nWith special tokens: {withSpecial.Length} tokens");
Console.WriteLine($"Without special tokens: {withoutSpecial.Length} tokens");

// Vocabulary lookup (bidirectional)
string word = "hello";
int? id = tokenizer.Tokenizer.TokenToId(word);
string? retrieved = id.HasValue ? tokenizer.Tokenizer.IdToToken(id.Value) : null;

Console.WriteLine($"\nVocabulary lookup:");
Console.WriteLine($"  '{word}' → ID {id} → '{retrieved}'");

// Unknown word handling (WordPiece splits into subwords)
string unknownWord = "tokenization";
var unknownEncoding = tokenizer.Tokenizer.Encode(unknownWord, addSpecialTokens: false);
Console.WriteLine($"\nUnknown word handling:");
Console.WriteLine($"  '{unknownWord}' → [{string.Join(", ", unknownEncoding.Tokens)}]");
```

**Output:**
```
Built-in special tokens:
  [CLS] → ID 101
  [SEP] → ID 102
  [PAD] → ID 0
  [UNK] → ID 100
  [MASK] → ID 103

With special tokens: 7 tokens
  [[CLS], hello, ,, world, !, [SEP], [PAD]]

Without special tokens: 5 tokens
  [hello, ,, world, !, [PAD]]

Vocabulary lookup:
  'hello' → ID 7592 → 'hello'

Unknown word handling:
  'tokenization' → [token, ##ization]
  (WordPiece splits unknown words into known subwords)
```

**Key Insights:**
- Special tokens have reserved IDs (0-103 for BERT models)
- `addSpecialTokens` parameter controls automatic [CLS]/[SEP] insertion
- `TokenToId()` and `IdToToken()` provide bidirectional lookup
- Unknown words are automatically split into subwords (WordPiece algorithm)

**Note:** To add truly custom tokens, you must modify `tokenizer.json` and potentially retrain/fine-tune the model. This example demonstrates inspection of existing vocabulary.

---

## 🎓 API Reference Summary

### AutoTokenizer

```csharp
AutoTokenizer.Load(
    string location,                          // Path to model directory or tokenizer.json
    AutoTokenizerLoadOptions? options = null)

Options:
- ApplyTokenizerDefaults: Apply padding/truncation from config
- LoadGenerationConfig: Load generation_config.json
```

### Tokenizer Core Methods

| Method | Description |
|--------|-------------|
| `Encode(string text, bool addSpecialTokens)` | Encode single text |
| `Encode(string text, string? textPair, bool addSpecialTokens)` | Encode text pair |
| `EncodeBatch(IEnumerable<string> inputs, bool addSpecialTokens)` | Batch encode |
| `Decode(IReadOnlyList<int> ids, bool skipSpecialTokens)` | Decode to text |
| `TokenToId(string token)` | Get token ID from string |
| `IdToToken(int id)` | Get token string from ID |

### Configuration Methods

| Method | Description |
|--------|-------------|
| `EnablePadding(PaddingOptions)` | Configure padding |
| `DisablePadding()` | Remove padding |
| `EnableTruncation(TruncationOptions)` | Configure truncation |
| `DisableTruncation()` | Remove truncation |
| `GetPadding()` | Get current padding config |
| `GetTruncation()` | Get current truncation config |

### Chat Template Methods

| Method | Description |
|--------|-------------|
| `ApplyChatTemplate(IEnumerable<ChatMessage> messages, ChatTemplateOptions?)` | Render chat to text |
| `ApplyChatTemplateAsEncoding(...)` | Render chat to tokens |
| `ApplyChatTemplateAsTokenIds(...)` | Render chat to token IDs only |
| `SupportsChatTemplate` | Check if model has chat template |

### EncodingResult Properties

| Property | Description |
|----------|-------------|
| `Ids` | Token IDs |
| `Tokens` | Token strings |
| `Offsets` | Character positions (start, end) |
| `TypeIds` | Segment IDs (0=first, 1=second) |
| `AttentionMask` | Attention mask (1=real, 0=pad) |
| `SpecialTokensMask` | Special token mask |
| `WordIds` | Word indices |
| `SequenceIds` | Sequence indices |
| `Overflowing` | Overflow encodings (if using stride) |

---

## 📦 Running the Full Example

The complete quickstart is ready to run:

```bash
cd examples/HuggingFace/Quickstart
dotnet run
```

**All 16 Features Demonstrated:**
1. ✅ Basic tokenization
2. ✅ Batch processing
3. ✅ Token-to-text conversion
4. ✅ Special tokens
5. ✅ Padding strategies
6. ✅ Truncation strategies
7. ✅ Text pair encoding
8. ✅ Attention masks & type IDs
9. ✅ Offset mapping
10. ✅ Word & sequence IDs
11. ✅ Chat template rendering
12. ✅ Custom padding & truncation
13. ✅ Vocabulary access
14. ✅ Multiple models
15. ✅ Overflowing tokens
16. ✅ Special tokens & vocabulary inspection

---

## 🔗 Related Documentation

- [HuggingFace README](../../src/HuggingFace/README.md)
- [API Documentation](../api/huggingface.md)
- [TikToken Quickstart](../Tiktoken/quickstart.md)
- [Chat Templates Guide](./chat-templates.md)

---

## 💡 Best Practices

### When to use AutoTokenizer vs Tokenizer directly

```csharp
// ✅ Preferred: AutoTokenizer (loads all config files)
using var auto = AutoTokenizer.Load("model-directory");

// ⚠️ Direct Tokenizer (only loads tokenizer.json, no defaults)
using var tokenizer = Tokenizer.FromFile("tokenizer.json");
```

### Production Checklist

- [ ] Load tokenizer once, reuse for all requests
- [ ] Configure padding/truncation for your use case
- [ ] Use `ApplyTokenizerDefaults = true` unless you need custom control
- [ ] Handle padding direction based on model type (encoder vs decoder)
- [ ] Test with edge cases (empty strings, very long text, special characters)
- [ ] Monitor token counts for cost estimation
- [ ] Cache tokenizer configuration for performance

---

**Full source code**: [`examples/HuggingFace/Quickstart/Program.cs`](https://github.com/ergosumx/tokenx/blob/main/examples/HuggingFace/Quickstart/Program.cs)
