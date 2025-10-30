# API Reference

Comprehensive API documentation for all tokenizer libraries.

## Navigation

- [HuggingFace API](#huggingface-api)
- [Common Interfaces](#common-interfaces)
- [Enumerations](#enumerations)

> **Note**: For OpenAI GPT models, see [Microsoft.ML.Tokenizers](https://www.nuget.org/packages/Microsoft.ML.Tokenizers/) which provides `TiktokenTokenizer`.

## HuggingFace API

### Core Classes

#### AutoTokenizer

High-level interface for loading and managing HuggingFace tokenizers.

**Namespace:** `ErgoX.TokenX.HuggingFace`

**Key Methods:**
- `Load(string location, AutoTokenizerLoadOptions? options = null)` - Load tokenizer from directory
- `LoadAsync(string location, AutoTokenizerLoadOptions? options, CancellationToken cancellationToken)` - Load asynchronously
- `Encode(string text, bool addSpecialTokens = true)` - Encode text to tokens
- `Decode(IReadOnlyList<int> ids, bool skipSpecialTokens = true)` - Decode tokens to text
- `ApplyChatTemplate(IReadOnlyList<ChatMessage> messages, ChatTemplateOptions? options)` - Apply chat template

**Properties:**
- `Tokenizer Tokenizer` - Underlying tokenizer instance
- `TokenizerConfig? TokenizerConfig` - Configuration settings
- `SpecialTokensMap? SpecialTokens` - Special tokens map
- `GenerationConfig? GenerationConfig` - Generation defaults
- `bool SupportsChatTemplate` - Chat template availability
- `bool SupportsGenerationDefaults` - Generation config availability

See [HuggingFace Documentation](../huggingface/index.md) for complete details.

#### Tokenizer

Low-level tokenization engine.

**Namespace:** `ErgoX.TokenX.HuggingFace`

**Key Methods:**
- `Encode(string text, bool addSpecialTokens = false)` - Basic encoding
- `Encode(string text, string? textPair, bool addSpecialTokens = false)` - Encode text pairs
- `EncodeBatch(IReadOnlyList<string> texts, bool addSpecialTokens = false)` - Batch encoding
- `Decode(IReadOnlyList<int> ids, bool skipSpecialTokens = true)` - Decode to text
- `AddTokens(IReadOnlyList<string> tokens)` - Add custom tokens
- `TokenToId(string token)` - Get token ID
- `IdToToken(int id)` - Get token string

**Properties:**
- `int VocabularySize` - Total vocabulary size

#### EncodingResult

Tokenization output containing IDs, tokens, and metadata.

**Namespace:** `ErgoX.TokenX.HuggingFace`

**Properties:**
- `IReadOnlyList<int> Ids` - Token IDs
- `IReadOnlyList<string> Tokens` - Token strings
- `IReadOnlyList<int> AttentionMask` - Attention mask (1 for real tokens, 0 for padding)
- `IReadOnlyList<int> TypeIds` - Token type IDs (segment IDs)
- `IReadOnlyList<int> SpecialTokensMask` - Special tokens mask
- `IReadOnlyList<(int Start, int End)> Offsets` - Character offsets
- `IReadOnlyList<int?> WordIds` - Word IDs for subword reconstruction
- `IReadOnlyList<int?> SequenceIds` - Sequence IDs for paired inputs

### Chat Classes

#### ChatMessage

Represents a single message in a conversation.

**Namespace:** `ErgoX.TokenX.HuggingFace.Chat`

**Constructor:**
- `ChatMessage(string role, string content)` - Create message

**Properties:**
- `string Role` - Message role ("system", "user", "assistant")
- `string Content` - Message text

#### ChatTemplateOptions

Options for chat template rendering.

**Namespace:** `ErgoX.TokenX.HuggingFace.Chat`

**Properties:**
- `bool AddGenerationPrompt` - Add assistant prompt at end
- `string? TemplateOverride` - Custom Jinja2 template
- `IReadOnlyDictionary<string, object> AdditionalVariables` - Custom template variables

**Methods:**
- `SetVariable(string key, object value)` - Add template variable
- `RemoveVariable(string key)` - Remove template variable

### Configuration Classes

#### TokenizerConfig

Tokenizer configuration and metadata.

**Namespace:** `ErgoX.TokenX.HuggingFace.Options`

**Properties:**
- `string? ChatTemplate` - Jinja2 chat template
- `Dictionary<string, int> Vocab` - Vocabulary snapshot
- `string? Version` - Tokenizer version

#### SpecialTokensMap

Special token definitions.

**Namespace:** `ErgoX.TokenX.HuggingFace.Options`

**Properties:**
- `SpecialToken? BosToken` - Beginning-of-sequence token
- `SpecialToken? EosToken` - End-of-sequence token
- `SpecialToken? UnknownToken` - Unknown token
- `SpecialToken? PadToken` - Padding token
- `IReadOnlyList<SpecialToken> AdditionalSpecialTokens` - Custom special tokens

#### GenerationConfig

Generation defaults for autoregressive models.

**Namespace:** `ErgoX.TokenX.HuggingFace.Generation`

**Properties:**
- `int? MaxLength` - Maximum sequence length
- `int? MaxNewTokens` - Maximum new tokens to generate
- `int? TopK` - Top-k sampling parameter
- `float? TopP` - Nucleus sampling parameter
- `float? Temperature` - Sampling temperature
- `float? RepetitionPenalty` - Repetition penalty

---

## Common Interfaces

### ITokenizer

Common interface implemented by `Tokenizer` (HuggingFace).

**Namespace:** `ErgoX.TokenX.HuggingFace.Abstractions`

**Key Methods:**
- `EncodingResult Encode(string text, bool addSpecialTokens = false)`
- `IReadOnlyList<EncodingResult> EncodeBatch(IReadOnlyList<string> texts, bool addSpecialTokens = false)`
- `string Decode(IReadOnlyList<int> ids, bool skipSpecialTokens = true)`
- `IReadOnlyList<string> DecodeBatch(IReadOnlyList<IReadOnlyList<int>> sequences, bool skipSpecialTokens = true)`

**Properties:**
- `int VocabularySize`

---

## Enumerations

### PaddingStrategy

Padding behavior for sequence alignment.

**Namespace:** `ErgoX.TokenX.HuggingFace.Options`

**Values:**
- `None` - No padding
- `Longest` - Pad to longest sequence in batch
- `MaxLength` - Pad to specified maximum length

### TruncationStrategy

Truncation behavior for long sequences.

**Namespace:** `ErgoX.TokenX.HuggingFace.Options`

**Values:**
- `None` - No truncation
- `LongestFirst` - Truncate longest sequence first
- `OnlyFirst` - Truncate only first sequence
- `OnlySecond` - Truncate only second sequence

---

## Error Handling

All libraries throw standard .NET exceptions:

- `FileNotFoundException` - Model file not found
- `InvalidOperationException` - Invalid operation or configuration
- `ArgumentException` - Invalid argument
- `ArgumentNullException` - Null argument
- `ObjectDisposedException` - Operation on disposed object

**Best Practice:**

```csharp
try
{
    using var tokenizer = AutoTokenizer.Load("model-path");
    var encoding = tokenizer.Encode("text");
}
catch (FileNotFoundException ex)
{
    Console.WriteLine($"Model not found: {ex.Message}");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Configuration error: {ex.Message}");
}
```

---

## Next Steps

- [Installation Guide](../installation.md) - Setup instructions
- [HuggingFace Documentation](../huggingface/index.md) - Complete HuggingFace guide
- [Examples](../examples.md) - Working code examples
- [Main Documentation](../index.md) - Overview

