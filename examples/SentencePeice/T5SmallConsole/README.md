# SentencePiece T5 Small Tokenizer Console

A practical console app demonstrating multilingual subword tokenization using Google's SentencePiece library with the T5 small model. This example showcases encoding, decoding, and special token handling for seq2seq tasks.

## What Is SentencePiece?

**SentencePiece** is a language-independent subword tokenizer that treats text as a sequence of bytes and learns merge rules to build a vocabulary. Key advantages:

- **Language agnostic**: Works with any Unicode script (Latin, CJK, Indic, Arabic, emoji, etc.)
- **Deterministic**: Same input always produces the same tokens
- **Lossless round-trip**: Original text is perfectly recoverable from token IDs (unless OOV occurs)
- **No preprocessing needed**: Handles raw text; no need for manual space/punctuation handling
- **Byte-level robustness**: Rare characters map to a fallback `<unk>` token instead of crashing

**Compared to other tokenizers:**
- **WordPiece** (BERT): Requires initial word splitting; language-specific
- **BPE** (GPT): Byte-pair encoding; can produce rare-character artifacts
- **SentencePiece**: Unified encoding/decoding for all languages; T5, mBART, XLM-R all use it

## T5 Tokenizer Details

The `t5-small` model includes:
- **Vocabulary size**: 32,000 tokens
- **Special tokens**:
  - `<pad>` (ID 0): Padding token for batching
  - `</s>` (ID 1): End-of-sequence marker for generation
  - `<unk>` (ID 2): Unknown/out-of-vocabulary fallback
- **Word marker**: Uses `▁` (underscore) to denote word boundaries; preserved in piece output, converted to spaces on decode
- **Encoding scheme**: Sentence-pair compatible; can handle up to 512 tokens per sequence

## Running the Example

```bash
cd examples/SentencePeice/T5SmallConsole
dotnet run
```

### Expected Output

The program will:
1. Load the model and print vocabulary metadata (special token IDs, vocab size)
2. For each sample text from `examples/.data/embeddings`:
   - Show the original text
   - Display token IDs (first 32 before truncation)
   - Display piece representation (first 16 before truncation)
   - Verify round-trip decoding (input == decode(encode(input)))
3. Demonstrate seq2seq usage with EOS token injection

### Sample Output
```
Loaded SentencePiece model from: .../examples/.models/t5-small/spiece.model
Vocabulary size: 32,000
Special IDs -> unk:2, bos:-1, eos:1, pad:0
------------------------------------------------------------------------
Sample 'standard-tiny-en' text:
Programming language paradigms define computational models...
Token IDs:
7106, 53, 1612, 20491, 7, 6634, 25850, 2250, 10, 18158, ...
Sequence length: 77

Pieces:
▁Programm | ing | ▁language | ▁paradigm | s | ▁define | ...

Round-trip matches input: True
```

## Code Structure

### Main Workflow
```csharp
// 1. Load model from disk
using var processor = new SentencePieceProcessor();
processor.Load(modelPath);

// 2. Encode: text → token IDs
var tokenIds = processor.EncodeIds("Hello world");  // [5, 104, ...]

// 3. Decode: token IDs → text
var text = processor.DecodeIds(tokenIds);  // "Hello world"

// 4. Encode: text → piece strings (optional)
var pieces = processor.EncodePieces("Hello world");  // ["▁Hello", "▁world"]

// 5. Encode with special tokens (seq2seq)
var options = new EncodeOptions { AddEos = true };
var idsWithEos = processor.EncodeIds(prompt, options);  // Appends EOS (ID 1)

// 6. Decode with special tokens
var decodedWithEos = processor.DecodeIds(idsWithEos);  // Reconstructed text
```

### Encoding Pipeline
1. **Text preprocessing**: Unicode normalization (NFC)
2. **Segmentation**: Split on whitespace and punctuation boundaries
3. **Subword merging**: Apply learned BPE rules to minimize token count
4. **Special token injection** (if requested): Add BOS (beginning) / EOS (end) markers

### Decoding Pipeline
1. **Token → piece mapping**: Look up each token ID in vocabulary
2. **Piece concatenation**: Join consecutive pieces
3. **Whitespace reconstruction**: Replace `▁` (word marker) with spaces
4. **Unicode reconstruction**: Rebuild full characters from encoded segments

### EncodeOptions
- **AddBos**: Prepend beginning-of-sequence token (T5 doesn't use; some models do)
- **AddEos**: Append end-of-sequence token (ID 1 for T5; marks sequence end for decoder)
- **EnableSampling**: Use probabilistic sampling instead of greedy encoding (experimental)
- **Alpha**: Smoothing parameter for sampling (higher = more random)
- **Reverse**: Reverse token sequence (useful for bidirectional models)

## Key Observations

### Multilingual Support
The `t5-small` model was trained on mC4 (multilingual Common Crawl), so it tokenizes:
- **Latin scripts** (English, Spanish, French) with 1–2 characters per token on average
- **Non-Latin scripts** (Indic, CJK, Arabic) with more tokens per word due to script granularity
- **Emoji and special chars** with individual token assignment if in vocab; `<unk>` otherwise

### Round-Trip Consistency
Most texts recover exactly after decode, **except**:
- Out-of-vocabulary (OOV) characters → `<unk>` (ID 2) → `<unk>` string on decode (not original char)
- Example: Hindi text with rare diacritics may see `<unk>` inserted, breaking round-trip

### Encoding Modes
- **Without options**: Greedy decoding; no special tokens injected
- **With AddEos=true**: Appends EOS token (ID 1) for seq2seq decoder input
- **With AddBos=true**: Attempts to prepend BOS, but T5 BOS ID is -1 (undefined in vocab)

## Troubleshooting

| Issue | Cause | Solution |
|-------|-------|----------|
| Model file not found | Path resolution error | Ensure running from `examples/SentencePeice/T5SmallConsole` directory |
| `System.AccessViolationException` | Interop boundary issue | Avoid calling `EncodePieces` with `EncodeOptions`; use `EncodeIds` instead |
| Decode fails with "piece id is out of range" | Invalid token IDs (e.g., BOS = -1) | Ensure all token IDs passed to `DecodeIds()` are valid vocab IDs (0–31999); avoid `AddBos=true` for T5 since BOS is -1 |
| Non-Latin characters show `<unk>` | Text outside training distribution | Expected behavior; rare scripts fall back to OOV token |
| Sequence length mismatch | Unicode normalization difference | SentencePiece uses NFC normalization internally |

## References

- **SentencePiece Paper**: https://arxiv.org/abs/1808.06226
- **Google's SentencePiece Repo**: https://github.com/google/sentencepiece
- **T5 Model Card**: https://huggingface.co/google-t5/t5-small

