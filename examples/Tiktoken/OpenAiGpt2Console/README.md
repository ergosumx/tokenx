# TikToken OpenAI GPT-2 Tokenizer Console

A practical console app demonstrating OpenAI's TikToken byte-pair encoding (BPE) tokenizer configured for GPT-2 language models. This example showcases encoding, decoding, and special token handling for large language models.

## What Is TikToken?

**TikToken** is OpenAI's tokenizer library that encodes text into integers suitable for processing by language models (GPT-2, GPT-3, GPT-4). Key features:

- **Byte-level**: Text is converted to UTF-8 bytes first, then merged using learned BPE rules
- **Regex-aware**: Uses regex to pre-segment text (words, numbers, punctuation) before BPE merging
- **Fast**: Optimized C++ backend with Python/Rust bindings
- **Deterministic**: Same input always produces the same tokens
- **Language-aware**: Different encodings for different models (gpt2, cl100k_base for GPT-4, etc.)

**Compared to SentencePiece:**
- **TikToken**: Byte-level, regex-based pre-segmentation, optimized for English/code; lower multilingual fidelity
- **SentencePiece**: Subword-level, language-agnostic, lossless for all scripts; better multilingual support

## GPT-2 Tokenizer Details

The GPT-2 model uses a specific TikToken encoding with these properties:

### Vocabulary
- **Total tokens**: 50,257
- **BPE merges**: 50,000 (learned from Common Crawl text)
- **Raw bytes**: 256 (UTF-8 byte values 0–255)
- **Special tokens**: 1 (`<|endoftext|>`, ID 50256)

### Special Token
- **`<|endoftext|>`** (ID 50,256): Marks the end of a document or sequence
  - Used by GPT-2 to signal completion during generation
  - Can be explicitly included via the `Encode()` method with `allowedSpecial` parameter
  - Otherwise, treated as ordinary text and split across multiple byte tokens

### Regex Pattern
GPT-2 uses this regex for pre-tokenization:
```
'(?:[sdmt]|ll|ve|re)| ?\p{L}+| ?\p{N}+| ?[^\s\p{L}\p{N}]+|\s+(?!\S)|\s+
```

This pattern groups:
- Contractions: `'s`, `'d`, `'m`, `'ll`, `'ve`, `'re`, `'t`, `'ing`
- Words: sequences of letters with optional leading space
- Numbers: sequences of digits with optional leading space
- Special chars: everything else except whitespace, with optional leading space
- Whitespace: sequences of spaces, tabs, newlines

### Encoding Example
```
Text: "Hello, world!"

Step 1 (Regex): ["Hello", ",", " ", "world", "!"]
Step 2 (UTF-8): [72, 101, 108, 108, 111], [44], [32], [119, 111, 114, 108, 100], [33]
Step 3 (BPE merges): [15496], [11], [220], [1002], [0]
Result: [15496, 11, 220, 1002, 0]
```

## Running the Example

```bash
cd examples/Tiktoken/OpenAiGpt2Console
dotnet run
```

### Expected Output

The program will:
1. Load the TikToken encoding and print metadata (special tokens, vocab size)
2. For each sample text from `examples/.data/embeddings`:
   - Show the original text
   - Display token count (byte-level can produce many tokens for non-Latin text)
   - Display token IDs (first 24 before truncation)
   - Verify round-trip decoding (input == decode(encode(input)))
3. Demonstrate special token handling: compare ordinary encoding vs allowing `<|endoftext|>` as a single token

### Sample Output
```
Loaded TikToken encoding 'gpt2' from: .../examples/.models/openai-gpt2/mergeable_ranks.tiktoken
Special tokens: <|endoftext|>=50256
------------------------------------------------------------------------
Sample 'standard-tiny-en' text:
Programming language paradigms define computational models...
Token count: 70
Tokens:
15167, 2229, 3303, 11497, 328, 907, 8160, 31350, 4981, 25, 23602, 357, ...

Round-trip matches input: True

Special token handling:
Include the <|endoftext|> marker when you are done.
Ordinary encoding length: 16
Allowing <|endoftext|> collapses to length: 11
Tokens:
818, 9152, 262, 220, 50256, 18364, 618, 345, 389, 1760, 13
```

## Code Structure

### Main Workflow
```csharp
// 1. Load encoding from mergeable ranks file
using var encoding = TiktokenEncodingFactory.FromTiktokenFile(
    "gpt2",
    pattern,
    ranksPath,
    specialTokens,
    vocabularySize);

// 2. Encode: text → token IDs (ordinary)
var tokens = encoding.EncodeOrdinary("Hello world");  // [15496, 11, 220, 1002]

// 3. Decode: token IDs → text (reverse operation)
var text = encoding.Decode(tokens.ToArray());  // "Hello world"

// 4. Encode with special token awareness
var withSpecial = encoding.Encode(
    "Stop: <|endoftext|>",
    allowedSpecial: new[] { "<|endoftext|>" });  // [20467, 25, 50256]

// 5. Decode with special tokens
var decodedSpecial = encoding.Decode(withSpecial.ToArray());  // "Stop: <|endoftext|>"
```

### Encoding Pipeline
1. **Text preprocessing**: UTF-8 encoding (Unicode → bytes)
2. **Regex segmentation**: Pre-tokenize by pattern (words, numbers, punctuation, spaces)
3. **BPE merging**: Apply learned merge operations (50,000 rules)
4. **Special token handling** (if Encode() used): Preserve reserved tokens as single IDs

### Decoding Pipeline
1. **Token → bytes mapping**: Look up each token ID in BPE merge table
2. **Byte sequence assembly**: Concatenate all bytes in order
3. **UTF-8 reconstruction**: Convert byte array to valid Unicode string
4. **Character normalization**: Handle edge cases (spaces, combining characters)

### EncodeOrdinary vs Encode
- **EncodeOrdinary**: Treats all text as raw bytes; special tokens are split character-by-character
- **Encode**: Checks for allowed special tokens and preserves them as single tokens

Example:
```
Text: "<|endoftext|>"

EncodeOrdinary(): [27, 91, 58, 91, 101, 110, 100, 111, 102, 116, 101, 120, 116, 91, 93]
                  (splits into 15 byte tokens)

Encode(allowedSpecial: ["<|endoftext|>"]): [50256]
                  (recognizes as single special token)

Decode both: "<|endoftext|>"
             (both decode to identical text)
```

## Key Observations

### Multilingual Encoding
GPT-2 was trained primarily on English and code, so:
- **ASCII/Latin**: 1–1.3 tokens per character (highly efficient)
- **Accented Latin** (é, ñ, ü): 1–2 tokens per character (still efficient)
- **Non-Latin scripts** (CJK, Indic, Arabic): 3–5+ tokens per character (very verbose)

Example token counts from the example:
- English: 70 tokens for ~200 characters (0.35 tokens/char)
- Spanish: 123 tokens for ~280 characters (0.44 tokens/char)
- French: 128 tokens for ~290 characters (0.44 tokens/char)
- **Hindi: 536 tokens for ~200 characters (2.68 tokens/char)** ⚠ Large expansion!

### Byte-Level Peculiarities
Since TikToken works at the byte level:
- **Unicode normalization**: The same character represented differently (e.g., é as single byte vs e+accent) tokenizes differently
- **Spaces as tokens**: Leading spaces often become separate tokens (ID ~220)
- **Punctuation**: Most punctuation has its own token ID
- **Newlines**: Encoded as specific tokens; affect token count

### Special Token Handling
The `<|endoftext|>` token:
- **By default**: Ordinary encoding splits it into ~15 byte tokens
- **With allowedSpecial**: Single token (ID 50256)
- **Use case**: Mark document boundaries during generation to prevent model confusion

## Troubleshooting

| Issue | Cause | Solution |
|-------|-------|----------|
| Model file not found | Path resolution error | Ensure running from `examples/Tiktoken/OpenAiGpt2Console` directory |
| Token count very high | Multilingual text | Expected for non-Latin scripts; GPT-2 was trained on English/code |
| Special token not collapsed | `allowedSpecial` not set | Pass `new[] { "<\|endoftext\|>" }` to `Encode()` method |
| Round-trip decode mismatch | Corrupted token IDs | Verify token IDs are in valid range (0–50256) |

## Performance Notes

### Encoding Speed
TikToken achieves ~10M tokens/second on modern CPUs (C++ backend). For typical text:
- English: ~0.1ms per 100 tokens
- Multilingual: ~0.3–1ms per 100 tokens (due to higher token counts)

### Memory Usage
- Mergeable ranks table: ~100 KB (cached in memory once loaded)
- Token buffers: Dynamic based on text length; ~1 token ≈ 4 bytes

## References

- **TikToken GitHub**: https://github.com/openai/tiktoken
- **TikToken Python Docs**: https://github.com/openai/tiktoken/blob/main/README.md
- **GPT-2 Paper**: https://d4mucfpksywv.cloudfront.net/better-language-models/language-models.pdf
- **BPE Algorithm**: https://arxiv.org/abs/1508.07909

