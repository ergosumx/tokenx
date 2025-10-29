# Google SentencePiece

Google SentencePiece is a language-agnostic subword tokenizer that provides lossless, reversible text encoding. It's particularly well-suited for multilingual NLP applications and seq2seq models.

## Overview

SentencePiece tokenizes text into subword pieces using either Unigram or BPE algorithms. Unlike word-based tokenization, it:

- **Treats text as raw bytes** - No language-specific preprocessing required
- **Provides lossless round-trip** - `text == decode(encode(text))` always holds
- **Handles all languages equally** - No special handling for spaces or punctuation
- **Uses ▁ symbol** - Marks word boundaries explicitly in the token sequence

## Key Features

- ✅ **Language-agnostic** - Works with any language including CJK, Arabic, Indic scripts
- ✅ **Lossless encoding** - Perfect round-trip encoding/decoding
- ✅ **Subword tokenization** - Handles out-of-vocabulary words naturally
- ✅ **Sampling support** - Stochastic tokenization for data augmentation
- ✅ **Normalization** - Built-in text normalization capabilities

## Installation

```bash
dotnet add package ErgoX.TokenX.SentencePiece
```

Native libraries are automatically deployed to your runtime folder during build.

## Quick Start

### Basic Usage

```csharp
using ErgoX.TokenX.SentencePiece.Processing;

// Load SentencePiece model
using var processor = new SentencePieceProcessor();
processor.Load("spiece.model");

// Encode text to token IDs
var ids = processor.EncodeIds("Hello, world!");
Console.WriteLine($"Token IDs: {string.Join(", ", ids)}");

// Encode text to pieces (subword strings)
var pieces = processor.EncodePieces("Hello, world!");
Console.WriteLine($"Pieces: {string.Join(" | ", pieces)}");

// Decode IDs back to text
var decoded = processor.DecodeIds(ids);
Console.WriteLine($"Decoded: {decoded}");
```

### Loading Models

You can load SentencePiece models from files or serialized data:

```csharp
using var processor = new SentencePieceProcessor();

// Load from file path
processor.Load("path/to/spiece.model");

// Load from byte array (serialized proto)
byte[] modelData = File.ReadAllBytes("spiece.model");
processor.Load(modelData);
```

### Encoding Options

Control tokenization behavior with `EncodeOptions`:

```csharp
using ErgoX.TokenX.SentencePiece.Options;

var options = new EncodeOptions
{
    AddBos = false,          // Add beginning-of-sequence token
    AddEos = true,           // Add end-of-sequence token
    Reverse = false,         // Reverse token sequence
    EnableSampling = false,  // Enable stochastic sampling
    NBestSize = -1,          // N-best size (-1 = greedy, >0 = N-best)
    Alpha = 0.0f            // Smoothing parameter for sampling
};

var ids = processor.EncodeIds("Text to encode", options);
```

### Decoding

Decode token IDs or pieces back to text:

```csharp
// Decode IDs
var ids = new[] { 123, 456, 789 };
var text = processor.DecodeIds(ids);

// Decode pieces
var pieces = new[] { "▁Hello", "▁world" };
var text2 = processor.DecodePieces(pieces);
```

## Advanced Features

### Stochastic Sampling

Generate multiple tokenization variants for data augmentation:

```csharp
using ErgoX.TokenX.SentencePiece.Options;

var options = new SampleEncodeAndScoreOptions
{
    NumSamples = 10,           // Generate 10 samples
    Alpha = 0.1f,              // Smoothing parameter (higher = more diverse)
    WithoutReplacement = true, // Each sample is unique
    IncludeBest = true         // Include greedy tokenization
};

var samples = processor.SampleEncodeAndScore("Text to sample", options);
foreach (var sample in samples)
{
    Console.WriteLine($"Pieces: {string.Join(" | ", sample.Pieces)}");
    Console.WriteLine($"Score: {sample.Score}");
}
```

### Text Normalization

Apply normalization rules before tokenization:

```csharp
// Normalize text using the model's normalization rules
var normalized = processor.Normalize("Tëxt with àccents");
Console.WriteLine($"Normalized: {normalized.Text}");
Console.WriteLine($"Offsets: {string.Join(", ", normalized.Offsets)}");
```

### Model Information

Access model metadata and vocabulary:

```csharp
// Get vocabulary size
Console.WriteLine($"Vocab size: {processor.VocabSize}");

// Get special token IDs
Console.WriteLine($"PAD token ID: {processor.PadId}");
Console.WriteLine($"BOS token ID: {processor.BosId}");
Console.WriteLine($"EOS token ID: {processor.EosId}");
Console.WriteLine($"UNK token ID: {processor.UnknownId}");

// Check if token ID is unknown token
bool isUnknown = processor.IsUnknown(tokenId);

// Check if token ID or piece is control token
bool isControl = processor.IsControl(tokenId);
bool isControl2 = processor.IsControlPiece("▁");
```

### Vocabulary Management

Set or reset custom vocabularies:

```csharp
// Set custom vocabulary (filter token space)
var customVocab = new[] { "▁Hello", "▁world", "▁!" };
processor.SetVocabulary(customVocab);

// Reset to original vocabulary
processor.ResetVocabulary();
```

### ID/Piece Conversion

Convert between token IDs and piece strings:

```csharp
// Get piece from ID
var piece = processor.IdToPiece(123);

// Get ID from piece
var id = processor.PieceToId("▁Hello");

// Get piece score (log probability)
var score = processor.GetScore(123);
```

## Use Cases

### Seq2Seq Models (T5, mT5)

```csharp
using var processor = new SentencePieceProcessor();
processor.Load("t5-small-spiece.model");

// Encode with EOS for T5 encoder input
var options = new EncodeOptions { AddEos = true };
var inputIds = processor.EncodeIds("translate English to French: Hello", options);

// Process through model...
var outputIds = model.Generate(inputIds);

// Decode output
var translation = processor.DecodeIds(outputIds);
```

### Multilingual Text Processing

```csharp
using var processor = new SentencePieceProcessor();
processor.Load("multilingual-spiece.model");

// SentencePiece handles all languages uniformly
var languages = new[]
{
    "Hello, world!",           // English
    "Bonjour le monde!",       // French
    "你好世界！",               // Chinese
    "مرحبا بالعالم!",          // Arabic
    "नमस्ते दुनिया!"           // Hindi
};

foreach (var text in languages)
{
    var pieces = processor.EncodePieces(text);
    Console.WriteLine($"Text: {text}");
    Console.WriteLine($"Pieces: {string.Join(" | ", pieces)}");
    
    var decoded = processor.DecodePieces(pieces);
    Console.WriteLine($"Lossless: {text == decoded}\n");
}
```

### Data Augmentation

```csharp
// Generate multiple tokenization variants for robustness
var options = new SampleEncodeAndScoreOptions
{
    NumSamples = 5,
    Alpha = 0.2f,
    IncludeBest = true
};

var samples = processor.SampleEncodeAndScore("Training text", options);
foreach (var sample in samples)
{
    // Use different tokenization for each training epoch
    var augmented = processor.DecodePieces(sample.Pieces);
    // Train model with augmented text...
}
```

## API Reference

### Core Classes

- **[SentencePieceProcessor](api/sentencepieceprocessor.md)** - Main tokenization interface
- **[EncodeOptions](api/encodeoptions.md)** - Encoding configuration
- **[SampleEncodeAndScoreOptions](api/sampleencodeandscore.md)** - Sampling configuration

### Model Types

- **[NormalizedText](api/normalizedtext.md)** - Normalized text with offsets
- **[ScoredIdSequence](api/scoredidsequence.md)** - Token IDs with score
- **[ScoredPieceSequence](api/scoredpiecesequence.md)** - Token pieces with score

### Exceptions

- **[SentencePieceException](api/exception.md)** - SentencePiece errors
- **[SentencePieceStatusCode](api/statuscode.md)** - Error status codes

## Best Practices

### Model Loading

```csharp
// Load model once and reuse
using var processor = new SentencePieceProcessor();
processor.Load("spiece.model");

// Process many texts with same instance
foreach (var text in texts)
{
    var ids = processor.EncodeIds(text);
    // ... process ids
}
// Automatically disposed when leaving scope
```

### Special Tokens

```csharp
// Check model configuration for special tokens
Console.WriteLine($"PAD: {processor.PadId}");    // Usually 0
Console.WriteLine($"UNK: {processor.UnknownId}"); // Usually 1 or 2
Console.WriteLine($"BOS: {processor.BosId}");     // -1 if not used
Console.WriteLine($"EOS: {processor.EosId}");     // Usually 1

// Only add special tokens if model supports them
var options = new EncodeOptions
{
    AddBos = processor.BosId >= 0,  // Only if defined
    AddEos = processor.EosId >= 0   // Only if defined
};
```

### Round-trip Verification

```csharp
var original = "Test text with 日本語 and émojis 🎉";
var ids = processor.EncodeIds(original);
var decoded = processor.DecodeIds(ids);

// Should always be true for SentencePiece
Debug.Assert(original == decoded, "Lossless round-trip failed!");
```

## Performance Tips

1. **Reuse processor instances** - Creating a processor is expensive
2. **Batch processing** - Process multiple texts in sequence with one processor
3. **Disable sampling** - Use greedy encoding unless you need stochastic behavior
4. **Appropriate N-best** - Keep NBestSize at -1 (greedy) for production

## Examples

Complete working examples are available in the repository:

- [T5 Small Console](../../examples/SentencePeice/T5SmallConsole/) - T5 model tokenization
- [Multilingual Examples](examples.md) - Various language samples

## Troubleshooting

### Common Issues

**Model file not found**
```csharp
// Use absolute paths or verify relative paths
var fullPath = Path.GetFullPath("spiece.model");
processor.Load(fullPath);
```

**Special token errors**
```csharp
// Check if model defines special tokens before using
if (processor.BosId >= 0)
{
    // Safe to use AddBos
}
```

**Decode mismatches**
```csharp
// Ensure you're decoding with same processor that encoded
var ids = processor.EncodeIds(text);
var decoded = processor.DecodeIds(ids);  // Use same processor
```

## Additional Resources

- [SentencePiece Paper](https://arxiv.org/abs/1808.06226)
- [Official Repository](https://github.com/google/sentencepiece)
- [Model Training Guide](https://github.com/google/sentencepiece#train-sentencepiece-model)

## See Also

- [Installation Guide](../installation.md)
- [API Reference](api/index.md)
- [Examples](examples.md)

