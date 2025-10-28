# HuggingFace Tokenizers

Complete guide to the HuggingFace tokenizers library wrapper.

## Overview

The HuggingFace tokenizers library provides .NET bindings to the Rust-based `tokenizers` library, offering fast and flexible tokenization for modern NLP models. This implementation supports all major tokenization algorithms (BPE, WordPiece, Unigram) and advanced features like chat templates and multimodal processing.

### Key Features

- **Multiple Algorithms**: BPE (GPT, RoBERTa), WordPiece (BERT), Unigram (SentencePiece-compatible)
- **Chat Templates**: Jinja2 template rendering for multi-turn conversations
- **Multimodal Support**: Audio (Whisper), vision (CLIP), and text tokenization
- **Pre/Post Processing**: Automatic padding, truncation, attention masks, token type IDs
- **Generation Config**: Model-specific defaults for autoregressive decoding
- **Thread-Safe**: Safe for concurrent use after initialization
- **Native Performance**: Rust backend with zero-cost abstractions

### Architecture

```
AutoTokenizer (High-level API)
    ├── Tokenizer (Core tokenization)
    │   ├── Encode/Decode methods
    │   ├── Add special tokens
    │   └── Vocabulary management
    ├── TokenizerConfig (Settings)
    │   ├── Padding/Truncation
    │   ├── Chat template (Jinja2)
    │   └── Model defaults
    ├── SpecialTokensMap (Special tokens)
    │   ├── BOS, EOS, UNK, PAD
    │   └── Additional custom tokens
    └── GenerationConfig (Generation defaults)
        ├── max_length, max_new_tokens
        ├── top_k, top_p, temperature
        └── repetition_penalty
```

## Installation

```bash
dotnet add package ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace
```

See [Installation Guide](../installation.md) for detailed setup.

## Quick Start

### Basic Encoding

```csharp
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace;

// Load tokenizer from directory
using var tokenizer = AutoTokenizer.Load("bert-base-uncased");

// Encode text
var encoding = tokenizer.Encode("Hello, world!");

Console.WriteLine($"Tokens: {string.Join(", ", encoding.Tokens)}");
Console.WriteLine($"IDs: {string.Join(", ", encoding.Ids)}");

// Output:
// Tokens: [CLS], hello, ,, world, !, [SEP]
// IDs: 101, 7592, 1010, 2088, 999, 102
```

### Load from HuggingFace Hub

```csharp
// Download and cache from HuggingFace Hub
using var tokenizer = AutoTokenizer.LoadFromPretrained("bert-base-uncased");

// With authentication for private models
using var tokenizer = AutoTokenizer.LoadFromPretrained(
    "private-model",
    authToken: "hf_your_token_here");
```

### Batch Encoding

```csharp
// Encode multiple texts with padding
var texts = new[] { "First sentence.", "Second sentence is longer." };
var encodings = tokenizer.EncodeBatch(texts, addSpecialTokens: true);

foreach (var encoding in encodings)
{
    Console.WriteLine($"Tokens: {encoding.Ids.Count}");
}
```

## Core API

### AutoTokenizer Class

`AutoTokenizer` is the primary interface for loading and using tokenizers.

#### Loading Methods

```csharp
// Load from local directory
using var tokenizer = AutoTokenizer.Load("path/to/model");

// Load with options
var options = new AutoTokenizerLoadOptions
{
    ApplyTokenizerDefaults = true,
    LoadGenerationConfig = true
};
using var tokenizer = AutoTokenizer.Load("path/to/model", options);

// Load asynchronously
using var tokenizer = await AutoTokenizer.LoadAsync("path/to/model", options, cancellationToken);
```

#### Properties

```csharp
// Underlying tokenizer for encoding/decoding
public Tokenizer Tokenizer { get; }

// Configuration (padding, truncation, chat template)
public TokenizerConfig? TokenizerConfig { get; }

// Special tokens map ([CLS], [SEP], [PAD], etc.)
public SpecialTokensMap? SpecialTokens { get; }

// Generation defaults (max_length, top_k, etc.)
public GenerationConfig? GenerationConfig { get; }

// Base directory path
public string BasePath { get; }

// Check feature support
public bool SupportsChatTemplate { get; }
public bool SupportsGenerationDefaults { get; }
```

#### Encoding Methods

```csharp
// Basic encoding
EncodingResult Encode(string text, bool addSpecialTokens = true);

// Encode with type IDs (for sentence pairs)
EncodingResult Encode(string text, string? textPair, bool addSpecialTokens = true);

// Batch encoding
IReadOnlyList<EncodingResult> EncodeBatch(
    IReadOnlyList<string> texts,
    bool addSpecialTokens = true);

// Encode to token strings
IReadOnlyList<string> EncodeAsTokens(string text, bool addSpecialTokens = true);
```

#### Decoding Methods

```csharp
// Decode token IDs to text
string Decode(IReadOnlyList<int> ids, bool skipSpecialTokens = true);

// Batch decoding
IReadOnlyList<string> DecodeBatch(
    IReadOnlyList<IReadOnlyList<int>> sequences,
    bool skipSpecialTokens = true);
```

#### Chat Template Methods

```csharp
// Apply chat template to conversation
string ApplyChatTemplate(
    IReadOnlyList<ChatMessage> messages,
    ChatTemplateOptions? options = null);

// Custom Jinja2 template
string ApplyChatTemplate(
    IReadOnlyList<ChatMessage> messages,
    string customTemplate,
    ChatTemplateOptions? options = null);
```

#### Token Management

```csharp
// Get vocabulary size
int VocabularySize { get; }

// Add new tokens
int AddTokens(IReadOnlyList<string> tokens);

// Add special tokens
int AddSpecialTokens(IReadOnlyList<string> specialTokens);

// Get token ID
int? TokenToId(string token);

// Get token from ID
string? IdToToken(int id);
```

### Tokenizer Class

Low-level tokenization API (accessed via `AutoTokenizer.Tokenizer`).

```csharp
// Direct encoding (no special tokens by default)
var encoding = tokenizer.Tokenizer.Encode("text");

// With special tokens
var encoding = tokenizer.Tokenizer.Encode("text", addSpecialTokens: true);

// Encoding with pair (for models like BERT)
var encoding = tokenizer.Tokenizer.Encode("text A", "text B", addSpecialTokens: true);
```

### EncodingResult Class

Result object containing tokenization outputs.

```csharp
public sealed class EncodingResult
{
    // Token IDs
    public IReadOnlyList<int> Ids { get; }
    
    // Token strings
    public IReadOnlyList<string> Tokens { get; }
    
    // Attention mask (1 for real tokens, 0 for padding)
    public IReadOnlyList<int> AttentionMask { get; }
    
    // Type IDs (0 for first sequence, 1 for second in pairs)
    public IReadOnlyList<int> TypeIds { get; }
    
    // Special tokens mask (1 for special tokens, 0 for normal)
    public IReadOnlyList<int> SpecialTokensMask { get; }
    
    // Character-level offsets for each token
    public IReadOnlyList<(int Start, int End)> Offsets { get; }
    
    // Word IDs (for subword reconstruction)
    public IReadOnlyList<int?> WordIds { get; }
    
    // Sequence IDs (0 or 1 for paired inputs)
    public IReadOnlyList<int?> SequenceIds { get; }
}
```

## Chat Templates

### Overview

Chat templates format multi-turn conversations for LLM input using Jinja2 templates.

```csharp
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Chat;

// Define conversation
var messages = new List<ChatMessage>
{
    new("system", "You are a helpful assistant."),
    new("user", "What is the capital of France?"),
    new("assistant", "The capital of France is Paris."),
    new("user", "What is its population?")
};

// Apply template
string prompt = tokenizer.ApplyChatTemplate(messages);
```

### ChatMessage Structure

```csharp
public sealed class ChatMessage
{
    public ChatMessage(string role, string content);
    
    public string Role { get; }      // "system", "user", "assistant"
    public string Content { get; }   // Message text
}
```

### ChatTemplateOptions

```csharp
public sealed class ChatTemplateOptions
{
    // Add generation prompt (e.g., "assistant:" at end)
    public bool AddGenerationPrompt { get; set; }
    
    // Override default template with custom Jinja2 template
    public string? TemplateOverride { get; set; }
    
    // Additional variables for template rendering
    public IReadOnlyDictionary<string, object> AdditionalVariables { get; }
    
    // Add custom variable
    public void SetVariable(string key, object value);
    
    // Remove custom variable
    public bool RemoveVariable(string key);
}
```

### Example: Llama 3 Format

```csharp
var messages = new List<ChatMessage>
{
    new("system", "You are a helpful AI."),
    new("user", "Hello!")
};

var options = new ChatTemplateOptions
{
    AddGenerationPrompt = true
};

string prompt = tokenizer.ApplyChatTemplate(messages, options);

// Output:
// <|begin_of_text|><|start_header_id|>system<|end_header_id|>
//
// You are a helpful AI.<|eot_id|><|start_header_id|>user<|end_header_id|>
//
// Hello!<|eot_id|><|start_header_id|>assistant<|end_header_id|>
```

### Custom Templates

```csharp
// Use custom Jinja2 template
string customTemplate = @"
{% for message in messages %}
{{ message.role }}: {{ message.content }}
{% endfor %}
assistant: ";

string prompt = tokenizer.ApplyChatTemplate(messages, customTemplate);
```

## Model Types

### BPE (Byte Pair Encoding)

Used by GPT-2, GPT-3, RoBERTa, BART, and similar models.

**Features:**
- Subword tokenization based on byte pairs
- No out-of-vocabulary tokens
- Handles any Unicode text

**Example Models:**
- `gpt2`, `gpt2-medium`, `gpt2-large`
- `roberta-base`, `roberta-large`
- `facebook/bart-large`

```csharp
using var tokenizer = AutoTokenizer.Load("gpt2");
var encoding = tokenizer.Encode("tokenization");

// Output: token, ization (subword split)
```

### WordPiece

Used by BERT, DistilBERT, and ELECTRA.

**Features:**
- Subword tokenization with `##` prefix
- Vocabulary-constrained (unknown tokens mapped to [UNK])
- Case-sensitive or case-insensitive variants

**Example Models:**
- `bert-base-uncased`, `bert-large-cased`
- `distilbert-base-uncased`
- `google/electra-base-discriminator`

```csharp
using var tokenizer = AutoTokenizer.Load("bert-base-uncased");
var encoding = tokenizer.Encode("tokenization");

// Output: token, ##ization (WordPiece prefix)
```

### Unigram

Used by T5, mT5, ALBERT, and XLNet (via SentencePiece).

**Features:**
- Probabilistic subword segmentation
- Multiple tokenization paths
- Effective for multilingual models

**Example Models:**
- `albert-base-v2`
- `xlnet-base-cased`
- Note: T5/mT5 typically use SentencePiece library directly

```csharp
using var tokenizer = AutoTokenizer.Load("albert-base-v2");
var encoding = tokenizer.Encode("tokenization");

// Output: ▁token, ization (SentencePiece style)
```

## Advanced Usage

### Padding and Truncation

```csharp
// Pad to fixed length
var options = new EncodeOptions
{
    MaxLength = 512,
    Padding = PaddingStrategy.MaxLength,
    Truncation = TruncationStrategy.LongestFirst
};

var encoding = tokenizer.Encode("text", options);
```

### Special Token Handling

```csharp
// Access special tokens
if (tokenizer.SpecialTokens is { } specialTokens)
{
    Console.WriteLine($"BOS: {specialTokens.BosToken?.Content} (ID: {specialTokens.BosToken?.Id})");
    Console.WriteLine($"EOS: {specialTokens.EosToken?.Content} (ID: {specialTokens.EosToken?.Id})");
    Console.WriteLine($"PAD: {specialTokens.PadToken?.Content} (ID: {specialTokens.PadToken?.Id})");
    Console.WriteLine($"UNK: {specialTokens.UnknownToken?.Content} (ID: {specialTokens.UnknownToken?.Id})");
}

// Encode without special tokens
var encoding = tokenizer.Encode("text", addSpecialTokens: false);

// Decode without special tokens
string text = tokenizer.Decode(encoding.Ids, skipSpecialTokens: true);
```

### Sentence Pairs

BERT and similar models support sentence pair inputs for tasks like question answering:

```csharp
string question = "What is machine learning?";
string context = "Machine learning is a field of AI that focuses on data-driven algorithms.";

var encoding = tokenizer.Encode(question, context, addSpecialTokens: true);

// Output format:
// [CLS] question tokens [SEP] context tokens [SEP]

// Type IDs distinguish sentences:
// 0 = first sentence, 1 = second sentence
Console.WriteLine($"Type IDs: {string.Join(", ", encoding.TypeIds)}");
```

### Multimodal: Whisper (Audio)

Whisper tokenizer includes language and task tokens:

```csharp
using var tokenizer = AutoTokenizer.Load("openai/whisper-tiny");

// Whisper special tokens control transcription behavior
// Language token: <|en|>, <|es|>, <|de|>, etc.
// Task token: <|transcribe|>, <|translate|>

// These are typically set in the decoder initial tokens
int langToken = tokenizer.TokenToId("<|en|>") ?? throw new Exception("Language token not found");
int taskToken = tokenizer.TokenToId("<|transcribe|>") ?? throw new Exception("Task token not found");

// Used during decoding: [BOS] [lang] [task] ... generated tokens ...
```

### Multimodal: CLIP (Vision-Text)

CLIP uses separate text and image encoders:

```csharp
// Text tokenizer is standard BPE
using var tokenizer = AutoTokenizer.Load("openai/clip-vit-base-patch32");

string[] captions = {
    "a photo of a cat",
    "a photo of a dog",
    "a diagram of neural networks"
};

foreach (var caption in captions)
{
    var encoding = tokenizer.Encode(caption);
    // Encode to tensor for CLIP text encoder
}
```

## Use Cases

### Text Classification

```csharp
using var tokenizer = AutoTokenizer.Load("distilbert-base-uncased");

string text = "This movie was fantastic! Highly recommend it.";
var encoding = tokenizer.Encode(text, addSpecialTokens: true);

// Prepare for ONNX inference
var inputIds = encoding.Ids.ToArray();
var attentionMask = encoding.AttentionMask.ToArray();

// Run model, get classification logits
// ...
```

### Question Answering

```csharp
using var tokenizer = AutoTokenizer.Load("bert-base-uncased");

string question = "Who invented the telephone?";
string context = "Alexander Graham Bell invented the telephone in 1876.";

var encoding = tokenizer.Encode(question, context, addSpecialTokens: true);

// Type IDs help model distinguish question from context
// Run model to predict answer span
// ...
```

### Named Entity Recognition

```csharp
using var tokenizer = AutoTokenizer.Load("bert-base-cased");

string text = "Apple Inc. is headquartered in Cupertino, California.";
var encoding = tokenizer.Encode(text, addSpecialTokens: true);

// Preserve word IDs for entity reconstruction
var wordIds = encoding.WordIds;

// Run model, map predicted labels back to words using word IDs
// ...
```

### Embedding Generation

```csharp
using var tokenizer = AutoTokenizer.Load("sentence-transformers/all-MiniLM-L6-v2");

string[] sentences = {
    "The weather is sunny today.",
    "Today has clear skies and sunshine.",
    "It's raining heavily outside."
};

foreach (var sentence in sentences)
{
    var encoding = tokenizer.Encode(sentence, addSpecialTokens: true);
    
    // Run model to generate embedding
    // Compute similarity between embeddings
    // ...
}
```

### Summarization

```csharp
using var tokenizer = AutoTokenizer.Load("facebook/bart-large-cnn");

string article = "... long news article text ...";

// Encode article
var encoding = tokenizer.Encode(article, addSpecialTokens: true);

// Generate summary with decoder
// Decode token IDs back to text
string summary = tokenizer.Decode(generatedIds, skipSpecialTokens: true);
```

## Best Practices

### Resource Management

```csharp
// ✓ Always dispose tokenizers
using var tokenizer = AutoTokenizer.Load("model");

// ✓ Reuse tokenizer instances (thread-safe after load)
private static readonly AutoTokenizer _tokenizer = AutoTokenizer.Load("model");
```

### Performance

```csharp
// ✓ Batch processing for efficiency
var encodings = tokenizer.EncodeBatch(texts);

// ✓ Parallel processing (tokenizer is thread-safe)
Parallel.ForEach(texts, text =>
{
    var encoding = tokenizer.Encode(text);
    // Process encoding...
});

// ✗ Avoid repeated loading
// DON'T: Load tokenizer in a loop
foreach (var text in texts)
{
    using var tokenizer = AutoTokenizer.Load("model");  // Expensive!
    // ...
}
```

### Token Limits

```csharp
// Check token count before encoding
string text = GetUserInput();
var precheck = tokenizer.Encode(text);

if (precheck.Ids.Count > 512)
{
    // Truncate or split text
    text = text.Substring(0, text.Length / 2);
}

var encoding = tokenizer.Encode(text);
```

### Special Token Security

```csharp
// ✓ Validate user input doesn't contain special tokens
string userInput = GetUserInput();

if (tokenizer.SpecialTokens is { } specialTokens)
{
    var specialTokenStrings = new[] {
        specialTokens.BosToken?.Content,
        specialTokens.EosToken?.Content,
        specialTokens.PadToken?.Content
    }.Where(t => t != null);
    
    foreach (var token in specialTokenStrings)
    {
        if (userInput.Contains(token!))
        {
            // Sanitize or reject input
            userInput = userInput.Replace(token!, "");
        }
    }
}

var encoding = tokenizer.Encode(userInput);
```

## Troubleshooting

### Issue: FileNotFoundException for tokenizer.json

**Solution:** Verify file path and use absolute paths:

```csharp
var modelPath = Path.GetFullPath("models/bert-base-uncased");
if (!Directory.Exists(modelPath))
{
    throw new DirectoryNotFoundException($"Model not found: {modelPath}");
}
using var tokenizer = AutoTokenizer.Load(modelPath);
```

### Issue: Native library not loaded

**Solution:** Ensure native libraries are in runtime folder. See [Installation Guide](../installation.md).

### Issue: Incorrect tokenization

**Solution:** Check model type and special tokens:

```csharp
// Verify special tokens loaded
if (tokenizer.SpecialTokens is null)
{
    Console.WriteLine("Warning: No special tokens map loaded");
}

// Check token ID mapping
int? bosId = tokenizer.TokenToId("<s>");
if (bosId is null)
{
    Console.WriteLine("BOS token '<s>' not in vocabulary");
}
```

### Issue: Chat template not supported

**Solution:** Check if model provides chat template:

```csharp
if (!tokenizer.SupportsChatTemplate)
{
    Console.WriteLine("Model does not provide chat template");
    // Use custom template or manual formatting
}
```

## API Reference

### Key Classes

- `AutoTokenizer` - High-level tokenizer loader
- `Tokenizer` - Core encoding/decoding
- `EncodingResult` - Tokenization output
- `ChatMessage` - Chat message structure
- `ChatTemplateOptions` - Chat template configuration
- `TokenizerConfig` - Model configuration
- `SpecialTokensMap` - Special token definitions
- `GenerationConfig` - Generation defaults

### Key Methods

| Method | Description |
|--------|-------------|
| `AutoTokenizer.Load()` | Load tokenizer from directory |
| `AutoTokenizer.LoadAsync()` | Load tokenizer asynchronously |
| `Encode()` | Convert text to token IDs |
| `Decode()` | Convert token IDs to text |
| `EncodeBatch()` | Encode multiple texts |
| `DecodeBatch()` | Decode multiple sequences |
| `ApplyChatTemplate()` | Format multi-turn chat |
| `AddTokens()` | Add custom tokens |
| `TokenToId()` | Get token ID |
| `IdToToken()` | Get token string |

## Next Steps

- [Installation Guide](../installation.md) - Setup and deployment
- [Examples](../examples.md) - Complete working examples
- [SentencePiece Documentation](../sentencepiece/index.md) - Alternative tokenizer
- [TikToken Documentation](../tiktoken/index.md) - OpenAI tokenizer
- [Main Documentation](../index.md) - Overview and comparison
