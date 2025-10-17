# ErgoX.VecraX.ML.NLP.Tokenizers - Project Structure

**Created**: October 17, 2025  
**Type**: Consumer-focused HuggingFace Tokenizers Library for .NET

---

## Directory Structure

```
ErgoX.VecraX.ML.NLP.Tokenizers/
├── src/
│   └── ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace/
│       ├── ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.sln
│       ├── ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.csproj
│       ├── Abstractions/          (NEW - Clean interfaces)
│       ├── Core/                  (Tokenizer, Encoding core)
│       ├── Models/                (BPE, WordPiece, Unigram, WordLevel)
│       ├── Decoders/              (All 9 decoders)
│       ├── Encoding/              (Encoding manipulation)
│       ├── Options/               (Padding, Truncation, Config)
│       └── Internal/
│           └── Interop/           (P/Invoke to Rust)
├── tests/
│   └── ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests/
│       ├── Core/                  (Tokenizer, Encoding tests)
│       ├── Models/                (Model loader tests)
│       ├── Decoders/              (Decoder tests)
│       └── Encoding/              (Encoding manipulation tests)
├── ext/
│   └── tokenizers/
│       └── bindings/              (Consumer-only Rust FFI bindings)
│           ├── tokenizer.rs
│           ├── models/
│           ├── decoders/
│           └── encoding/
└── internal/                      (Project documentation - this folder)
    ├── STRUCTURE.md               (This file)
    ├── MIGRATION.md               (Migration tracking)
    └── DECISIONS.md               (Architecture decisions)
```

---

## Library Scope: Consumer-Focused

### ✅ What's Included

**Purpose**: Load and use pre-trained tokenizers from HuggingFace Hub

1. **Core Tokenizer API**
   - Load from files (`Tokenizer.FromFile()`)
   - Load from JSON string (`Tokenizer.FromString()`)
   - Load from bytes (`Tokenizer.FromBytes()`)
   - Encode text → tokens
   - Decode tokens → text
   - Batch operations

2. **Model Loaders (4 types)**
   - BPE (Byte-Pair Encoding)
   - WordPiece (BERT-style)
   - Unigram (SentencePiece)
   - WordLevel (simple word-to-id mapping)

3. **Decoders (9 types)**
   - BPE, ByteLevel, WordPiece, Metaspace
   - CTC (for ASR/STT)
   - Fuse, Strip, Replace
   - ByteFallback

4. **Encoding Features**
   - Token IDs, tokens, offsets
   - Type IDs, attention mask
   - Special tokens mask, word IDs
   - Character/token/word position mapping
   - Padding, truncation

5. **Abstractions (NEW)**
   - `ITokenizer` - Clean tokenizer interface
   - `IEncoding` - Encoding result interface
   - `IModel` - Model loader interface
   - `IDecoder` - Decoder interface

### ❌ What's NOT Included

**These are in separate Advanced library (future)**:

1. **Normalizers** (12 types)
   - Lowercase, Unicode normalization, Strip, Replace
   - BERT, NMT, Prepend normalizers
   - Sequence normalizer

2. **PreTokenizers** (11 types)
   - BERT, ByteLevel, Whitespace, Split
   - Punctuation, Digits, Metaspace
   - Sequence pre-tokenizer

3. **PostProcessors** (4 types)
   - BERT, RoBERTa, ByteLevel, Template

4. **Training API**
   - Not in scope (use Python for training)

---

## Namespace Convention

```csharp
// Consumer library namespace
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Models;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Decoders;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Abstractions;
```

---

## Target Users

**Primary (95%)**: Developers consuming pre-trained models
- Load BERT, GPT-2, RoBERTa, T5 tokenizers
- Encode text for model input
- Decode model output
- Simple, clean API

**Not Target (5%)**: Advanced users needing custom pipelines
- These users will use separate Advanced library (future)
- Or use Python for training/customization

---

## Key Design Decisions

1. **Consumer-First**: Optimized for loading pre-trained models
2. **Clean Abstractions**: New interfaces for better API surface
3. **Improved Structure**: Logical folder organization
4. **FFI Separation**: Only consumer-facing Rust bindings
5. **No Training**: Training belongs in Python ecosystem
6. **Decoders Included**: Valuable for debugging and ASR/STT

---

## File Migration Status

### Phase 1: Project Structure ✅ COMPLETE
- [x] Solution created
- [x] Consumer library project created
- [x] Test project created
- [x] Projects added to solution
- [x] Folder structure created

### Phase 2: Rust FFI Bindings (In Progress)
- [ ] Copy tokenizer.rs
- [ ] Copy models/ folder
- [ ] Copy decoders/ folder
- [ ] Copy encoding/ folder
- [ ] Exclude: normalizers/, pre_tokenizers/, post_processors/

### Phase 3: C# Core Classes (Pending)
- [ ] Core/Tokenizer.cs
- [ ] Core/Encoding.cs
- [ ] Core/EncodingResult.cs
- [ ] Core/AddedToken.cs
- [ ] Options/ (4 files)

### Phase 4: C# Models (Pending)
- [ ] Models/BpeModel.cs
- [ ] Models/WordPieceModel.cs
- [ ] Models/UnigramModel.cs
- [ ] Models/WordLevelModel.cs

### Phase 5: C# Decoders (Pending)
- [ ] Decoders/ (9 decoder files)

### Phase 6: C# Infrastructure (Pending)
- [ ] Internal/Interop/NativeMethods.cs (consumer methods only)
- [ ] Internal/NativeTokenizerHandle.cs
- [ ] runtimes/ (native DLLs)

### Phase 7: Abstractions (Pending - NEW)
- [ ] Abstractions/ITokenizer.cs
- [ ] Abstractions/IEncoding.cs
- [ ] Abstractions/IModel.cs
- [ ] Abstractions/IDecoder.cs

### Phase 8: Tests (Pending)
- [ ] Core/ tests (~65 tests)
- [ ] Models/ tests (~50 tests)
- [ ] Decoders/ tests (~63 tests)
- [ ] Encoding/ tests (~50 tests)

---

## Build & Test Status

### Expected Metrics
- **Test Count**: ~180 tests
- **Pass Rate**: 100%
- **Build Time**: < 30 seconds
- **Coverage**: Core features 100%

### Current Status
- **Build**: Not yet attempted
- **Tests**: Not yet run
- **Status**: Phase 1 complete, ready for file migration

---

## Next Steps

1. ✅ Create project structure (COMPLETE)
2. 🔄 Move Rust FFI consumer bindings (IN PROGRESS)
3. ⏭️ Move C# consumer classes
4. ⏭️ Move consumer tests
5. ⏭️ Build and validate
6. ⏭️ Create abstractions
7. ⏭️ Update documentation

---

**Last Updated**: October 17, 2025 - Phase 1 Complete
