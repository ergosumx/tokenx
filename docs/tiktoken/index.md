# OpenAI TikToken

TikToken is OpenAI's fast byte-pair encoding (BPE) tokenizer used in GPT models. It provides high-performance tokenization with special token support and deterministic encoding.

## Overview

TikToken uses byte-level BPE with regex-based pre-tokenization. Key characteristics:

- **Byte-level encoding** - Text → UTF-8 bytes → BPE tokens
- **Regex pre-tokenization** - Groups bytes before merging (words, numbers, punctuation)
- **Deterministic** - Same text always produces identical tokens
- **Fast** - Optimized Rust backend for maximum performance
- **Special tokens** - Supports reserved tokens like `<|endoftext|>`

## Key Features

- ✅ **High performance** - Optimized for speed with Rust core
- ✅ **GPT compatibility** - Matches OpenAI's Python tokenizer exactly
- ✅ **Special token handling** - Configurable special token recognition
- ✅ **Deterministic output** - Reproducible tokenization
- ✅ **Simple API** - Easy-to-use encoding and decoding

## Installation

```bash
dotnet add package ErgoX.TokenX.Tiktoken
```

Native libraries are automatically deployed to your runtime folder during build.

## Quick Start

### Loading an Encoding

```csharp
using ErgoX.TokenX.Tiktoken;

// Create GPT-2 encoding from .tiktoken file
var specialTokens = new Dictionary<string, int>
{
    ["<|endoftext|>"] = 50256
};

using var encoding = TiktokenEncodingFactory.FromTiktokenFile(
    name: "gpt2",
    pattern: "'(?:[sdmt]|ll|ve|re)| ?\\p{L}+| ?\\p{N}+| ?[^\\s\\p{L}\\p{N}]+|\\s+(?!\\S)|\\s+",
    tiktokenFilePath: "mergeable_ranks.tiktoken",
    specialTokens: specialTokens,
    explicitVocabularySize: 50257);
```

### Basic Encoding

```csharp
// Encode text to token IDs (ordinary encoding)
var tokens = encoding.EncodeOrdinary("Hello, world!");
Console.WriteLine($"Tokens: {string.Join(", ", tokens)}");

// Decode tokens back to text
var decoded = encoding.Decode(tokens.ToArray());
Console.WriteLine($"Decoded: {decoded}");
```

### Special Token Handling

```csharp
// Text with special token
var text = "Document content <|endoftext|> More content";

// Ordinary encoding: special tokens are treated as regular text
var ordinary = encoding.EncodeOrdinary(text);

// Special-aware encoding: recognizes and encodes special tokens
var allowedSpecial = new[] { "<|endoftext|>" };
var withSpecial = encoding.Encode(text, allowedSpecial);

Console.WriteLine($"Ordinary: {ordinary.Count} tokens");
Console.WriteLine($"With special: {withSpecial.Count} tokens");
```

## Core API

### Encoding Methods

```csharp
// Encode without special token recognition
IReadOnlyList<uint> EncodeOrdinary(string text);

// Encode with special token support
IReadOnlyList<uint> Encode(
    string text,
    ISet<string>? allowedSpecial = null);

// Encode single token (for testing/debugging)
uint EncodeSingleToken(ReadOnlySpan<byte> token);
```

### Decoding Methods

```csharp
// Decode token IDs to text
string Decode(uint[] tokens);

// Decode single token to bytes
byte[] DecodeSingleTokenBytes(uint token);
```

### Properties

```csharp
// Encoding name (e.g., "gpt2", "cl100k_base")
string Name { get; }

// Regex pattern used for pre-tokenization
string Pattern { get; }

// Special tokens dictionary
IReadOnlyDictionary<string, int> SpecialTokens { get; }
```

## Encoding Details

### Regex Patterns

TikToken uses regex patterns to group bytes before BPE merging:

**GPT-2 Pattern:**
```
'(?:[sdmt]|ll|ve|re)| ?\p{L}+| ?\p{N}+| ?[^\s\p{L}\p{N}]+|\s+(?!\S)|\s+
```

This pattern matches:
- Contractions: `'s`, `'d`, `'m`, `'ll`, `'ve`, `'re`, `'t`
- Words: sequences of letters (with optional leading space)
- Numbers: sequences of digits (with optional leading space)
- Punctuation: sequences of special characters (with optional leading space)
- Whitespace: sequences of spaces, tabs, newlines

**GPT-3.5/GPT-4 Pattern (cl100k_base):**
```
'(?i:[sdmt]|ll|ve|re)|[^\r\n\p{L}\p{N}]?+\p{L}+|\p{N}{1,3}| ?[^\s\p{L}\p{N}]++[\r\n]*|\s*[\r\n]|\s+(?!\S)|\s+
```

This pattern is more sophisticated and handles:
- Case-insensitive contractions
- Better whitespace handling
- Optimized number grouping (1-3 digits)

### Vocabulary Sizes

Common OpenAI encodings:

| Encoding | Vocabulary Size | Models |
|----------|----------------|--------|
| `gpt2` | 50,257 | GPT-2 |
| `r50k_base` | 50,257 | GPT-3 (ada, babbage) |
| `p50k_base` | 50,281 | GPT-3 (curie, davinci), Codex |
| `cl100k_base` | 100,277 | GPT-3.5, GPT-4, text-embedding-ada-002 |

## Advanced Usage

### Creating from Stream

```csharp
// Load from stream instead of file
using var stream = File.OpenRead("mergeable_ranks.tiktoken");
using var encoding = TiktokenEncodingFactory.FromTiktokenStream(
    "gpt2",
    pattern,
    stream,
    specialTokens);
```

### Custom Vocabulary

```csharp
// Create encoding with custom mergeable ranks
var mergeableRanks = new List<TiktokenMergeableRank>
{
    new TiktokenMergeableRank(token: Encoding.UTF8.GetBytes("hello"), rank: 0),
    new TiktokenMergeableRank(token: Encoding.UTF8.GetBytes("world"), rank: 1),
    // ... more ranks
};

using var encoding = TiktokenEncoding.Create(
    "custom",
    pattern,
    mergeableRanks,
    specialTokens);
```

### Disallowing Special Tokens

```csharp
// Throw exception if special tokens appear in text
try
{
    var tokens = encoding.Encode(
        "Text with <|endoftext|>",
        allowedSpecial: new HashSet<string>());  // Empty set = disallow all
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Special token not allowed: {ex.Message}");
}
```

## Use Cases

### GPT Model Input Preparation

```csharp
using var encoding = TiktokenEncodingFactory.FromTiktokenFile(
    "cl100k_base",
    pattern,
    "cl100k_base.tiktoken",
    specialTokens);

// Prepare text for GPT-3.5/GPT-4
var prompt = "Once upon a time";
var tokens = encoding.EncodeOrdinary(prompt);

// Check token count for API limits
if (tokens.Count > maxTokens)
{
    throw new InvalidOperationException($"Prompt exceeds {maxTokens} tokens");
}

// Send to GPT API...
```

### Token Counting

```csharp
// Count tokens for cost estimation
var texts = new[]
{
    "Short text",
    "A longer piece of text that will use more tokens",
    "Text with special characters: 你好世界"
};

foreach (var text in texts)
{
    var count = encoding.EncodeOrdinary(text).Count;
    Console.WriteLine($"Text: {text}");
    Console.WriteLine($"Tokens: {count}\n");
}
```

### Context Window Management

```csharp
// Truncate text to fit context window
string TruncateToTokenLimit(string text, int maxTokens)
{
    var tokens = encoding.EncodeOrdinary(text);
    
    if (tokens.Count <= maxTokens)
        return text;
    
    // Truncate tokens and decode
    var truncated = tokens.Take(maxTokens).ToArray();
    return encoding.Decode(truncated);
}

var longText = "Very long text...";
var truncated = TruncateToTokenLimit(longText, maxTokens: 100);
```

### Batch Processing

```csharp
// Process multiple texts efficiently
var documents = LoadDocuments();
var tokenCounts = new List<int>();

foreach (var doc in documents)
{
    var tokens = encoding.EncodeOrdinary(doc.Text);
    tokenCounts.Add(tokens.Count);
    
    // Store tokens for later use
    doc.TokenIds = tokens.ToArray();
}

Console.WriteLine($"Total tokens: {tokenCounts.Sum()}");
Console.WriteLine($"Average tokens: {tokenCounts.Average():F2}");
```

## API Reference

### Core Classes

- **[TiktokenEncoding](api/tiktokenencoding.md)** - Main encoding interface
- **[TiktokenEncodingFactory](api/tiktokenencodingfactory.md)** - Factory methods
- **[TiktokenBpeLoader](api/tiktokenbpeloader.md)** - Load .tiktoken files
- **[TiktokenMergeableRank](api/tiktokenmergeablerank.md)** - BPE merge entry

### Exceptions

- **[TiktokenInteropException](api/exception.md)** - TikToken errors

## Best Practices

### Encoding Selection

```csharp
// Choose encoding based on your model
string GetEncodingForModel(string modelName)
{
    return modelName switch
    {
        "gpt-4" or "gpt-3.5-turbo" => "cl100k_base",
        "text-davinci-003" or "text-davinci-002" => "p50k_base",
        "davinci" or "curie" => "r50k_base",
        "gpt2" => "gpt2",
        _ => throw new ArgumentException($"Unknown model: {modelName}")
    };
}
```

### Resource Management

```csharp
// Always dispose encodings
using (var encoding = TiktokenEncodingFactory.FromTiktokenFile(...))
{
    // Use encoding
    var tokens = encoding.EncodeOrdinary(text);
}
// Native resources automatically cleaned up

// Or use using declaration (C# 8+)
using var encoding = TiktokenEncodingFactory.FromTiktokenFile(...);
var tokens = encoding.EncodeOrdinary(text);
// Disposed at end of scope
```

### Special Token Security

```csharp
// For user-provided text, use ordinary encoding
var userText = GetUserInput();
var tokens = encoding.EncodeOrdinary(userText);

// Only use Encode() with allowedSpecial for trusted content
var systemPrompt = "System: <|endoftext|>";
var allowedSpecial = new HashSet<string> { "<|endoftext|>" };
var systemTokens = encoding.Encode(systemPrompt, allowedSpecial);
```

## Performance Tips

1. **Reuse encoding instances** - Creating encodings is expensive
2. **Use EncodeOrdinary** - Faster than special-aware encoding
3. **Batch processing** - Process multiple texts with one encoding instance
4. **Avoid string concatenation** - Encode separately and combine token arrays

## Examples

Complete working examples are available in the repository:

- [OpenAI GPT-2 Console](../../examples/Tiktoken/OpenAiGpt2Console/) - GPT-2 tokenization
- [Token Counting](examples.md) - Token count examples
- [Special Token Handling](examples.md) - Special token examples

## Troubleshooting

### Common Issues

**File not found**
```csharp
// Use absolute paths or verify file location
var path = Path.GetFullPath("mergeable_ranks.tiktoken");
if (!File.Exists(path))
    throw new FileNotFoundException($"File not found: {path}");
```

**Vocabulary size mismatch**
```csharp
// Ensure vocabulary size matches encoding
// GPT-2: 50,257
// cl100k_base: 100,277
using var encoding = TiktokenEncodingFactory.FromTiktokenFile(
    name,
    pattern,
    path,
    specialTokens,
    explicitVocabularySize: 50257);  // Must match model
```

**Special token errors**
```csharp
// Verify special tokens are configured correctly
var specialTokens = new Dictionary<string, int>
{
    ["<|endoftext|>"] = 50256  // Must match model's special tokens
};
```

## Additional Resources

- [TikToken Repository](https://github.com/openai/tiktoken)
- [OpenAI Tokenizer Tool](https://platform.openai.com/tokenizer)
- [OpenAI API Documentation](https://platform.openai.com/docs/guides/embeddings/what-are-embeddings)

## See Also

- [Installation Guide](../installation.md)
- [API Reference](api/index.md)
- [Examples](examples.md)

