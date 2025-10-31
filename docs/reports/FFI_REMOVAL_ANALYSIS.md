# FFI Binding Removal Analysis for AutoTokenizer
**Generated**: 2025-10-31  
**Purpose**: Identify unused FFI bindings and code that can be removed while preserving AutoTokenizer + Chat Template functionality

---

## Executive Summary

**Current State**: The FFI bridge exposes 47 distinct native functions across 11 modules. 

**Critical Insight**: AutoTokenizer loads **complete tokenizers from tokenizer.json files**. The model (BPE/WordPiece/Unigram) is already embedded in the JSON. The `.NET model/decoder classes are for MANUAL model construction/replacement` - NOT used by AutoTokenizer.

**Recommendation**: **Remove 16 FFI functions (34% reduction)** and their associated Rust/C# code:
- ❌ 2 generation planning functions (not used by AutoTokenizer)
- ❌ 8 model manipulation functions (not used by AutoTokenizer - models come from JSON)
- ❌ 6 decoder manipulation functions (not used by AutoTokenizer - decoders come from JSON)

**Impact**: **Safe removal** - AutoTokenizer only needs: lifecycle, encoding, decoding, config, padding, truncation, and chat templates. Manual model/decoder manipulation APIs are developer utilities not required for AutoTokenizer workflows.

---

## Analysis Methodology

1. **FFI Function Inventory**: Catalogued all `pub extern "C" fn` exports from Rust bridge
2. **P/Invoke Mapping**: Cross-referenced all `LibraryImport` declarations in NativeMethods.cs
3. **Usage Tracing**: Analyzed AutoTokenizer → Tokenizer → NativeInterop call chains
4. **Dependency Analysis**: Identified which functions are required for basic tokenization vs. generation

---

## FFI Functions Inventory (by Module)

### ✅ **Lifecycle Module** (lifecycle.rs) - **KEEP ALL**
All functions are essential for tokenizer creation and lifetime management:
- ✅ `tokenizers_create` - Used by `Tokenizer.FromFile()`, `FromBuffer()`
- ✅ `tokenizers_from_pretrained` - Used by `AutoTokenizer.Load()`, `Tokenizer.FromPretrained()`
- ✅ `tokenizers_free` - Used by `NativeTokenizerHandle.ReleaseHandle()`
- ✅ `tokenizers_free_string` - Used throughout to free native strings

**Justification**: Core lifecycle operations - cannot remove without breaking tokenizer instantiation.

---

### ✅ **Encoding Module** (encoding.rs) - **KEEP ALL**
All functions are used by `Tokenizer.Encode()` and related operations:
- ✅ `tokenizers_encode` - Primary encoding function
- ✅ `tokenizers_encoding_free` - Memory management for encodings
- ✅ `tokenizers_encoding_get_ids` - Extract token IDs
- ✅ `tokenizers_encoding_get_tokens` - Extract token strings
- ✅ `tokenizers_encoding_get_offsets` - Extract character offsets
- ✅ `tokenizers_encoding_get_type_ids` - Extract segment IDs
- ✅ `tokenizers_encoding_get_attention_mask` - Extract attention masks
- ✅ `tokenizers_encoding_get_special_tokens_mask` - Extract special token masks
- ✅ `tokenizers_encoding_get_word_ids` - Extract word alignment
- ✅ `tokenizers_encoding_get_sequence_ids` - Extract sequence IDs
- ✅ `tokenizers_encoding_copy_numeric` - Batch copy for encoding metadata
- ✅ `tokenizers_encoding_get_overflowing_count` - Truncation overflow handling
- ✅ `tokenizers_encoding_get_overflowing` - Retrieve overflowing encodings

**Justification**: Core tokenization functionality - all used by `EncodingResult` marshaling in `Tokenizer.cs`.

---

### ✅ **Decode Module** (decode.rs) - **KEEP ALL**
Both functions are used by `Tokenizer.Decode()` and `Tokenizer.DecodeBatch()`:
- ✅ `tokenizers_decode` - Single sequence decoding
- ✅ `tokenizers_decode_batch_flat` - Batch decoding

**Justification**: Essential for token-to-text conversion.

---

### ✅ **Config Module** (config.rs) - **KEEP ALL**
All functions are used by `Tokenizer` and `AutoTokenizer`:
- ✅ `tokenizers_token_to_id` - Used by `Tokenizer.TokenToId()`
- ✅ `tokenizers_id_to_token` - Used by `Tokenizer.IdToToken()`
- ✅ `tokenizers_get_config` - Used by `Tokenizer.ToJson()`, `Tokenizer.Save()`
- ✅ `tokenizers_get_padding` - Used by `Tokenizer.GetPadding()`
- ✅ `tokenizers_get_truncation` - Used by `Tokenizer.GetTruncation()`

**Justification**: All actively used for vocabulary lookups and config introspection.

---

### ✅ **Padding Module** (padding.rs) - **KEEP ALL**
Both functions are used by `Tokenizer`:
- ✅ `tokenizers_enable_padding` - Used by `Tokenizer.EnablePadding()`
- ✅ `tokenizers_disable_padding` - Used by `Tokenizer.DisablePadding()`

**Justification**: Padding is a core tokenization feature used by AutoTokenizer and manual configuration.

---

### ✅ **Truncation Module** (truncation.rs) - **KEEP ALL**
Both functions are used by `Tokenizer`:
- ✅ `tokenizers_enable_truncation` - Used by `Tokenizer.EnableTruncation()`
- ✅ `tokenizers_disable_truncation` - Used by `Tokenizer.DisableTruncation()`

**Justification**: Truncation is a core tokenization feature used by AutoTokenizer and manual configuration.

---

### ✅ **Chat Module** (chat.rs) - **KEEP**
Single function used by AutoTokenizer:
- ✅ `tokenizers_apply_chat_template` - Used by `AutoTokenizer.ApplyChatTemplate()`

**Justification**: Chat template rendering is a key AutoTokenizer feature explicitly mentioned as required.

---

### ❌ **Models Module** (models.rs) - **REMOVE ALL 8 FUNCTIONS**
These functions are used by `TokenizerModel`, `BpeModel`, `WordPieceModel`, `UnigramModel` classes for **manual model construction**:
- ❌ `tokenizers_model_from_json` - Create model from JSON config
- ❌ `tokenizers_model_free` - Free model handle
- ❌ `tokenizers_model_get_type` - Query model type
- ❌ `tokenizers_model_to_json` - Serialize model to JSON
- ❌ `tokenizers_model_bpe_from_files` - Create BPE model from vocab/merges
- ❌ `tokenizers_model_wordpiece_from_file` - Create WordPiece model
- ❌ `tokenizers_model_unigram_from_file` - Create Unigram model
- ❌ `tokenizers_tokenizer_set_model` - Replace tokenizer's model

**Justification for Removal**:
- **AutoTokenizer loads complete tokenizers from `tokenizer.json`** via `Tokenizer.FromFile()`
- The `tokenizer.json` file **already contains the model** (BPE/WordPiece/Unigram configuration)
- The Rust FFI `tokenizers_create` function deserializes the **complete tokenizer including the model**
- `.NET model classes (BpeModel, WordPieceModel, UnigramModel) are for MANUAL construction` from separate vocab/merge files
- AutoTokenizer **NEVER calls `SetModel()`** - it uses pre-configured tokenizers from HuggingFace Hub
- These are **developer utilities for custom tokenizer construction**, not AutoTokenizer requirements
- **Only used in tests**: `ModelDecoderBindingIntegrationTests` tests manual model swapping (not AutoTokenizer workflows)

---

### ❌ **Decoders Module** (decoders.rs) - **REMOVE ALL 6 FUNCTIONS**
These functions are used by `TokenizerDecoder` class for **manual decoder construction/replacement**:
- ❌ `tokenizers_decoder_from_json` - Create decoder from JSON
- ❌ `tokenizers_decoder_free` - Free decoder handle
- ❌ `tokenizers_decoder_get_type` - Query decoder type
- ❌ `tokenizers_decoder_to_json` - Serialize decoder to JSON
- ❌ `tokenizers_tokenizer_set_decoder` - Set tokenizer's decoder
- ❌ `tokenizers_tokenizer_clear_decoder` - Clear tokenizer's decoder

**Justification for Removal**:
- **AutoTokenizer loads complete tokenizers from `tokenizer.json`** which includes the decoder configuration
- The Rust FFI `tokenizers_create` function deserializes the **complete tokenizer including the decoder**
- `.NET TokenizerDecoder class is for MANUAL decoder manipulation` (e.g., swapping ByteLevel decoder for WordPiece decoder)
- AutoTokenizer **NEVER calls `SetDecoder()` or `ClearDecoder()`** - it uses the decoder from the tokenizer.json
- These are **developer utilities for custom tokenizer construction**, not AutoTokenizer requirements
- **Only used in tests**: `ModelDecoderBindingIntegrationTests` tests manual decoder swapping (not AutoTokenizer workflows)

---

### ❌ **Generation Module** (generation.rs) - **REMOVE 3 FUNCTIONS**
Only ONE function is actively used:
- ✅ `tokenizers_normalize_generation_config` - **KEEP** - Used by `AutoTokenizer.LoadAsync()` to normalize generation_config.json
- ❌ `tokenizers_plan_logits_processors` - **REMOVE** - Used by `LogitsBindingPlanner` (utility for generation planning)
- ❌ `tokenizers_plan_stopping_criteria` - **REMOVE** - Used by `StoppingCriterionPlanner` (utility for generation planning)

**Justification for Removal**:
- `PlanLogitsProcessors` and `PlanStoppingCriteria` are **NOT** used by AutoTokenizer's core functionality
- These functions are called by:
  - `LogitsBindingPlanner.Plan()` (src/HuggingFace/Core/Generation/LogitsBindingPlanner.cs)
  - `StoppingCriterionPlanner.Plan()` (src/HuggingFace/Core/Generation/StoppingCriterionPlanner.cs)
- These planners appear to be utilities for **text generation** workflows, NOT tokenization
- AutoTokenizer only uses `NormalizeGenerationConfig` to parse `generation_config.json` for metadata
- The planning functions are **generation runtime utilities**, not tokenization infrastructure

---

### ✅ **Error Module** (error.rs) - **KEEP**
Single function used throughout:
- ✅ `tokenizers_get_last_error` - Used by `NativeMethods.GetLastErrorMessage()` for error reporting

**Justification**: Critical for error handling across all FFI calls.

---

## Summary Statistics

| **Category**                     | **Total Functions** | **Used by AutoTokenizer** | **Can Remove** |
|----------------------------------|---------------------|---------------------------|----------------|
| Lifecycle (create/free)          | 4                   | 4 ✅                       | 0              |
| Encoding (encode/get_ids/etc)    | 13                  | 13 ✅                      | 0              |
| Decoding (decode/batch)          | 2                   | 2 ✅                       | 0              |
| Config (token_to_id/get_config)  | 5                   | 5 ✅                       | 0              |
| Padding (enable/disable)         | 2                   | 2 ✅                       | 0              |
| Truncation (enable/disable)      | 2                   | 2 ✅                       | 0              |
| Chat (apply_chat_template)       | 1                   | 1 ✅                       | 0              |
| Models (BPE/WordPiece/Unigram)   | 8                   | 0 🚫                       | **8 ❌**        |
| Decoders (set/clear)             | 6                   | 0 🚫                       | **6 ❌**        |
| Generation (normalize/plan)      | 3                   | 1 ✅                       | **2 ❌**        |
| Error Handling                   | 1                   | 1 ✅                       | 0              |
| **TOTAL**                        | **47**              | **31**                    | **16 (34%)**   |

### Key Insight: tokenizer.json Contains Everything

**AutoTokenizer workflow**:
```csharp
// AutoTokenizer loads a COMPLETE tokenizer from tokenizer.json
var tokenizer = AutoTokenizer.Load("path/to/model");  // Calls Tokenizer.FromFile("tokenizer.json")

// tokenizer.json contains:
// - Model (BPE/WordPiece/Unigram) with vocabulary and merges
// - Decoder (ByteLevel/WordPiece/Metaspace/etc.)
// - Normalizer, Pre-tokenizer, Post-processor
// - Special tokens, padding/truncation defaults

// NO manual model/decoder construction needed!
```

**Manual construction workflow** (NOT used by AutoTokenizer):
```csharp
// This is what the model/decoder classes are for:
var model = new BpeModel("vocab.json", "merges.txt");  // Manual construction from separate files
tokenizer.SetModel(model);  // Replace model in existing tokenizer

var decoder = TokenizerDecoder.FromJson("{...}");  // Manual decoder construction
tokenizer.SetDecoder(decoder);  // Replace decoder in existing tokenizer
```

**Conclusion**: AutoTokenizer never constructs models/decoders manually - it loads complete pre-configured tokenizers from HuggingFace.

---

## Recommended Removal Plan

### Phase 1: Remove Model Manipulation Module (8 functions)

#### 1.1 Rust FFI Changes

**File**: `.ext/hf_bridge/src/ffi/models.rs`
- ❌ **DELETE ENTIRE FILE** (all 8 model FFI functions)

**File**: `.ext/hf_bridge/src/ffi/mod.rs`
```rust
// ❌ DELETE this module declaration
pub mod models;
```

**File**: `.ext/hf_bridge/src/lib.rs`
- No changes needed (models module not exported at top level)

---

#### 1.2 .NET P/Invoke Changes

**File**: `src/HuggingFace/Internal/Interop/NativeMethods.cs`

Remove these 8 declarations:
```csharp
// ❌ DELETE - Lines ~98-122
[LibraryImport(LibraryName, EntryPoint = "tokenizers_model_from_json", StringMarshalling = StringMarshalling.Utf8)]
internal static partial IntPtr TokenizersModelFromJson(string json, out int status);

[LibraryImport(LibraryName, EntryPoint = "tokenizers_model_free")]
internal static partial void TokenizersModelFree(IntPtr model);

[LibraryImport(LibraryName, EntryPoint = "tokenizers_model_get_type")]
internal static partial IntPtr TokenizersModelGetType(IntPtr model, out int status);

[LibraryImport(LibraryName, EntryPoint = "tokenizers_model_to_json")]
internal static partial IntPtr TokenizersModelToJson(IntPtr model, [MarshalAs(UnmanagedType.Bool)] bool pretty, out int status);

[LibraryImport(LibraryName, EntryPoint = "tokenizers_model_bpe_from_files", StringMarshalling = StringMarshalling.Utf8)]
internal static partial IntPtr TokenizersModelBpeFromFiles(...);

[LibraryImport(LibraryName, EntryPoint = "tokenizers_model_wordpiece_from_file", StringMarshalling = StringMarshalling.Utf8)]
internal static partial IntPtr TokenizersModelWordPieceFromFile(...);

[LibraryImport(LibraryName, EntryPoint = "tokenizers_model_unigram_from_file", StringMarshalling = StringMarshalling.Utf8)]
internal static partial IntPtr TokenizersModelUnigramFromFile(string modelPath, out int status);

[LibraryImport(LibraryName, EntryPoint = "tokenizers_tokenizer_set_model")]
internal static partial int TokenizersTokenizerSetModel(IntPtr tokenizer, IntPtr model, out int status);
```

**File**: `src/HuggingFace/Internal/Interop/INativeInterop.cs`

Remove these 8 interface methods:
```csharp
// ❌ DELETE
IntPtr TokenizersModelFromJson(string json, out int status);
void TokenizersModelFree(IntPtr model);
IntPtr TokenizersModelGetType(IntPtr model, out int status);
IntPtr TokenizersModelToJson(IntPtr model, bool pretty, out int status);
IntPtr TokenizersModelBpeFromFiles(in NativeBpeModelParameters parameters, out int status);
IntPtr TokenizersModelWordPieceFromFile(...);
IntPtr TokenizersModelUnigramFromFile(string modelPath, out int status);
int TokenizersTokenizerSetModel(IntPtr tokenizer, IntPtr model, out int status);
```

**File**: `src/HuggingFace/Internal/Interop/NativeMethodsBridge.cs`

Remove these 8 implementations (lines ~79-111)

---

#### 1.3 Remove .NET Model Classes and Infrastructure

**Delete entire files**:
- ❌ `src/HuggingFace/Core/BpeModel.cs`
- ❌ `src/HuggingFace/Core/WordPieceModel.cs`
- ❌ `src/HuggingFace/Core/UnigramModel.cs`
- ❌ `src/HuggingFace/Core/TokenizerModel.cs`
- ❌ `src/HuggingFace/Options/BpeModelOptions.cs`
- ❌ `src/HuggingFace/Options/WordPieceModelOptions.cs`
- ❌ `src/HuggingFace/Options/UnigramModelOptions.cs`
- ❌ `src/HuggingFace/Internal/NativeModelHandle.cs`
- ❌ `src/HuggingFace/Internal/Interop/NativeBpeModelParameters.cs`
- ❌ `src/HuggingFace/Abstractions/IModel.cs`

**File**: `src/HuggingFace/Core/Tokenizer.cs`

Remove the `SetModel()` method:
```csharp
// ❌ DELETE - Lines 506-533
public void SetModel(IModel model) { ... }
```

---

### Phase 2: Remove Decoder Manipulation Module (6 functions)

#### 2.1 Rust FFI Changes

**File**: `.ext/hf_bridge/src/ffi/decoders.rs`
- ❌ **DELETE ENTIRE FILE** (all 6 decoder FFI functions)

**File**: `.ext/hf_bridge/src/ffi/mod.rs`
```rust
// ❌ DELETE this module declaration
pub mod decoders;
```

---

#### 2.2 .NET P/Invoke Changes

**File**: `src/HuggingFace/Internal/Interop/NativeMethods.cs`

Remove these 6 declarations:
```csharp
// ❌ DELETE - Lines ~125-145
[LibraryImport(LibraryName, EntryPoint = "tokenizers_decoder_from_json", StringMarshalling = StringMarshalling.Utf8)]
internal static partial IntPtr TokenizersDecoderFromJson(string json, out int status);

[LibraryImport(LibraryName, EntryPoint = "tokenizers_decoder_free")]
internal static partial void TokenizersDecoderFree(IntPtr decoder);

[LibraryImport(LibraryName, EntryPoint = "tokenizers_decoder_get_type")]
internal static partial IntPtr TokenizersDecoderGetType(IntPtr decoder, out int status);

[LibraryImport(LibraryName, EntryPoint = "tokenizers_decoder_to_json")]
internal static partial IntPtr TokenizersDecoderToJson(IntPtr decoder, [MarshalAs(UnmanagedType.Bool)] bool pretty, out int status);

[LibraryImport(LibraryName, EntryPoint = "tokenizers_tokenizer_set_decoder")]
internal static partial int TokenizersTokenizerSetDecoder(IntPtr tokenizer, IntPtr decoder, out int status);

[LibraryImport(LibraryName, EntryPoint = "tokenizers_tokenizer_clear_decoder")]
internal static partial int TokenizersTokenizerClearDecoder(IntPtr tokenizer, out int status);
```

**File**: `src/HuggingFace/Internal/Interop/INativeInterop.cs`

Remove these 6 interface methods

**File**: `src/HuggingFace/Internal/Interop/NativeMethodsBridge.cs`

Remove these 6 implementations (lines ~113-133)

---

#### 2.3 Remove .NET Decoder Classes and Infrastructure

**Delete entire files**:
- ❌ `src/HuggingFace/Core/TokenizerDecoder.cs`
- ❌ `src/HuggingFace/Internal/NativeDecoderHandle.cs`
- ❌ `src/HuggingFace/Abstractions/IDecoder.cs`

**File**: `src/HuggingFace/Core/Tokenizer.cs`

Remove these methods:
```csharp
// ❌ DELETE - Lines 535-557
public void SetDecoder(IDecoder decoder) { ... }

// ❌ DELETE - Lines 559-571
public void ClearDecoder() { ... }
```

---

### Phase 3: Remove Generation Planning FFI Functions (2 functions)

#### 3.1 Rust FFI Changes
**File**: `.ext/hf_bridge/src/ffi/generation.rs`

Remove these two functions:
```rust
// ❌ DELETE
#[no_mangle]
pub unsafe extern "C" fn tokenizers_plan_logits_processors(
    source: *const c_char,
    status: *mut c_int,
) -> *mut c_char { ... }

// ❌ DELETE
#[no_mangle]
pub unsafe extern "C" fn tokenizers_plan_stopping_criteria(
    source: *const c_char,
    status: *mut c_int,
) -> *mut c_char { ... }
```

Keep this function (used by AutoTokenizer):
```rust
// ✅ KEEP - Used by AutoTokenizer.LoadAsync()
#[no_mangle]
pub unsafe extern "C" fn tokenizers_normalize_generation_config(
    source: *const c_char,
    status: *mut c_int,
) -> *mut c_char { ... }
```

**File**: `.ext/hf_bridge/src/lib.rs`
- No changes needed (generation module stays, just with fewer functions)

---

#### 3.2 .NET P/Invoke Changes

**File**: `src/HuggingFace/Internal/Interop/NativeMethods.cs`

Remove these 2 declarations:
```csharp
// ❌ DELETE
[LibraryImport(LibraryName, EntryPoint = "tokenizers_plan_logits_processors", StringMarshalling = StringMarshalling.Utf8)]
internal static partial IntPtr TokenizersPlanLogitsProcessors(string source, out int status);

// ❌ DELETE
[LibraryImport(LibraryName, EntryPoint = "tokenizers_plan_stopping_criteria", StringMarshalling = StringMarshalling.Utf8)]
internal static partial IntPtr TokenizersPlanStoppingCriteria(string source, out int status);
```

**File**: `src/HuggingFace/Internal/Interop/INativeInterop.cs`

Remove these 2 interface methods:
```csharp
// ❌ DELETE
IntPtr TokenizersPlanLogitsProcessors(string source, out int status);
IntPtr TokenizersPlanStoppingCriteria(string source, out int status);
```

**File**: `src/HuggingFace/Internal/Interop/NativeMethodsBridge.cs`

Remove these 2 implementations (lines ~154-158)

---

#### 3.3 .NET High-Level API Changes

**File**: `src/HuggingFace/Core/Tokenizer.cs`

Remove these internal methods:
```csharp
// ❌ DELETE - Lines 151-179
internal static string PlanLogitsProcessors(string json) { ... }

// ❌ DELETE - Lines 182-210
internal static string PlanStoppingCriteria(string json) { ... }
```

Keep this method (used by AutoTokenizer):
```csharp
// ✅ KEEP - Used by AutoTokenizer.LoadAsync()
internal static string NormalizeGenerationConfig(string json) { ... }
```

---

#### 3.4 Remove Generation Planner Utilities

**Delete entire files**:
- ❌ `src/HuggingFace/Core/Generation/LogitsBindingPlanner.cs`
- ❌ `src/HuggingFace/Core/Generation/StoppingCriterionPlanner.cs`

**Keep these files** (data structures used by AutoTokenizer):
- ✅ `src/HuggingFace/Core/Generation/GenerationConfig.cs`
- ✅ `src/HuggingFace/Core/Generation/GenerationSettings.cs`
- ✅ `src/HuggingFace/Core/Generation/GenerationRequest.cs`
- ✅ `src/HuggingFace/Core/Generation/StreamingGenerationRequest.cs`
- ✅ `src/HuggingFace/Core/Generation/GenerationOptions.cs`
- ✅ `src/HuggingFace/Core/Generation/StreamGenerationOptions.cs`

---

### Phase 4: Remove Tests for Deleted Features

**Delete entire files**:
- ❌ `tests/ErgoX.TokenX.HuggingFace.Tests/IntegrationTests/Core/ModelDecoderBindingIntegrationTests.cs`
  - Tests `SetModel()`, `SetDecoder()`, `ClearDecoder()` - all removed

**Note**: Keep all other tests - AutoTokenizer, encoding, decoding, chat template, padding, truncation tests remain unchanged.

---

### Phase 5: Verify and Test

1. **Build Verification**
   ```powershell
   # Rebuild native library
   cd .ext/hf_bridge
   cargo build --release
   
   # Rebuild .NET project
   cd ../..
   dotnet build
   ```

2. **Test Coverage**
   - Run all existing AutoTokenizer tests
   - Verify encoding/decoding still works
   - Verify chat template rendering works
   - Verify generation config loading works (uses `NormalizeGenerationConfig`)

3. **Integration Verification**
   - Verify `AutoTokenizer.Load()` works
   - Verify `AutoTokenizer.ApplyChatTemplate()` works
   - Verify `AutoTokenizer.Generate()` data structures still work (they don't call removed FFI functions)

---

## Files to Remove (Complete List)

### Rust FFI Files (3 files)
- ❌ `.ext/hf_bridge/src/ffi/models.rs` (ENTIRE FILE - 8 functions)
- ❌ `.ext/hf_bridge/src/ffi/decoders.rs` (ENTIRE FILE - 6 functions)
- ⚠️ `.ext/hf_bridge/src/ffi/generation.rs` (remove 2 functions, keep 1)

### .NET Core Classes (13 files)
- ❌ `src/HuggingFace/Core/BpeModel.cs`
- ❌ `src/HuggingFace/Core/WordPieceModel.cs`
- ❌ `src/HuggingFace/Core/UnigramModel.cs`
- ❌ `src/HuggingFace/Core/TokenizerModel.cs`
- ❌ `src/HuggingFace/Core/TokenizerDecoder.cs`
- ❌ `src/HuggingFace/Core/Generation/LogitsBindingPlanner.cs`
- ❌ `src/HuggingFace/Core/Generation/StoppingCriterionPlanner.cs`

### .NET Options (3 files)
- ❌ `src/HuggingFace/Options/BpeModelOptions.cs`
- ❌ `src/HuggingFace/Options/WordPieceModelOptions.cs`
- ❌ `src/HuggingFace/Options/UnigramModelOptions.cs`

### .NET Internal/Handle Classes (3 files)
- ❌ `src/HuggingFace/Internal/NativeModelHandle.cs`
- ❌ `src/HuggingFace/Internal/NativeDecoderHandle.cs`
- ❌ `src/HuggingFace/Internal/Interop/NativeBpeModelParameters.cs`

### .NET Abstractions (2 files)
- ❌ `src/HuggingFace/Abstractions/IModel.cs`
- ❌ `src/HuggingFace/Abstractions/IDecoder.cs`

### Test Files (1 file)
- ❌ `tests/ErgoX.TokenX.HuggingFace.Tests/IntegrationTests/Core/ModelDecoderBindingIntegrationTests.cs`

### Partial Edits Required (6 files)
- ⚠️ `src/HuggingFace/Internal/Interop/NativeMethods.cs` (remove 16 P/Invoke declarations)
- ⚠️ `src/HuggingFace/Internal/Interop/INativeInterop.cs` (remove 16 interface methods)
- ⚠️ `src/HuggingFace/Internal/Interop/NativeMethodsBridge.cs` (remove 16 implementations)
- ⚠️ `src/HuggingFace/Core/Tokenizer.cs` (remove 4 methods: SetModel, SetDecoder, ClearDecoder, PlanLogitsProcessors, PlanStoppingCriteria)
- ⚠️ `.ext/hf_bridge/src/ffi/mod.rs` (remove 2 module declarations: models, decoders)
- ⚠️ `.ext/hf_bridge/src/ffi/generation.rs` (remove 2 FFI functions)

---

**Total Removal**:
- 22 complete files deleted
- 6 files partially edited
- 16 FFI functions removed (34% reduction)
- Estimated code reduction: ~3,000-4,000 lines

---

## Risk Assessment

| **Risk**                          | **Likelihood** | **Impact** | **Mitigation**                                                                 |
|-----------------------------------|----------------|------------|-------------------------------------------------------------------------------|
| Break AutoTokenizer core          | **Low**        | Critical   | Removed functions are NOT used by AutoTokenizer's core functionality          |
| Break generation metadata loading | **Low**        | High       | `NormalizeGenerationConfig` is retained (only function actually used)         |
| Break external consumers          | **Medium**     | Medium     | If consumers use `LogitsBindingPlanner`/`StoppingCriterionPlanner` directly   |
| Build breaks                      | **Low**        | Medium     | Clear removal plan with verification steps                                    |

**Recommendation**: **Safe to proceed**. The removed functions are generation planning utilities not used by AutoTokenizer.

---

## Alternative: Minimal Impact Removal

If concerned about external consumers using the planner utilities, consider:

**Option A**: Keep .NET planner classes but stub them out to throw `NotSupportedException`
```csharp
public static class LogitsBindingPlanner
{
    public static string Plan(string json)
        => throw new NotSupportedException("Logits processor planning requires full generation library.");
}
```

**Option B**: Keep everything but mark as obsolete with warnings
```csharp
[Obsolete("This functionality is not supported in tokenizer-only mode.", error: false)]
public static string PlanLogitsProcessors(string json) { ... }
```

**Recommendation**: **Proceed with full removal**. If these utilities were used, they belong in a separate generation library, not the tokenization core.

---

## What Cannot Be Removed (31 Essential Functions)

### Core Tokenization Functions - All Required for AutoTokenizer
- **Lifecycle (4 funcs)**: create/free tokenizers and strings
  - `tokenizers_create` - Load tokenizer from JSON (includes model+decoder)
  - `tokenizers_from_pretrained` - Download tokenizer from HuggingFace Hub
  - `tokenizers_free` - Free tokenizer handle
  - `tokenizers_free_string` - Free native strings

- **Encoding (13 funcs)**: encode text into token IDs with all metadata
  - `tokenizers_encode` - Primary encoding function
  - `tokenizers_encoding_free` - Free encoding handle
  - `tokenizers_encoding_get_ids` - Get token IDs
  - `tokenizers_encoding_get_tokens` - Get token strings
  - `tokenizers_encoding_get_offsets` - Get character offsets
  - `tokenizers_encoding_get_type_ids` - Get segment IDs
  - `tokenizers_encoding_get_attention_mask` - Get attention mask
  - `tokenizers_encoding_get_special_tokens_mask` - Get special token mask
  - `tokenizers_encoding_get_word_ids` - Get word alignment
  - `tokenizers_encoding_get_sequence_ids` - Get sequence IDs
  - `tokenizers_encoding_copy_numeric` - Batch copy encoding data
  - `tokenizers_encoding_get_overflowing_count` - Count overflow encodings
  - `tokenizers_encoding_get_overflowing` - Get overflow encoding

- **Decoding (2 funcs)**: decode token IDs back to text
  - `tokenizers_decode` - Decode single sequence
  - `tokenizers_decode_batch_flat` - Decode batch of sequences

- **Padding/Truncation (4 funcs)**: configure text preprocessing
  - `tokenizers_enable_padding` - Enable padding
  - `tokenizers_disable_padding` - Disable padding
  - `tokenizers_enable_truncation` - Enable truncation
  - `tokenizers_disable_truncation` - Disable truncation

- **Config (5 funcs)**: introspect tokenizer state and vocabulary
  - `tokenizers_token_to_id` - Lookup token ID
  - `tokenizers_id_to_token` - Lookup token string
  - `tokenizers_get_config` - Get tokenizer JSON config
  - `tokenizers_get_padding` - Get padding settings
  - `tokenizers_get_truncation` - Get truncation settings

- **Chat (1 func)**: render chat templates
  - `tokenizers_apply_chat_template` - Format messages using Jinja2 templates

- **Generation Config (1 func)**: normalize generation settings
  - `tokenizers_normalize_generation_config` - Parse generation_config.json

- **Error (1 func)**: error reporting
  - `tokenizers_get_last_error` - Get last error message

All 31 functions are actively used by AutoTokenizer for its core workflows.

---

## Conclusion

**Summary**: Out of 47 FFI functions, **31 are required** for AutoTokenizer + Chat Template functionality. **16 functions can be safely removed** (34% reduction):
- 8 model manipulation functions (BPE/WordPiece/Unigram manual construction)
- 6 decoder manipulation functions (decoder manual construction/replacement)
- 2 generation planning functions (logits processors, stopping criteria)

**Recommendation**: **Proceed with removal**. The removed functions are:
1. **Developer utilities for custom tokenizer construction** (models/decoders) - AutoTokenizer loads complete tokenizers from HuggingFace Hub
2. **Generation runtime utilities** (planning) - Not used by AutoTokenizer's tokenization/chat workflows

**Effort Estimate**: 
- Rust changes: 2 hours (delete 2 modules, remove 2 functions)
- .NET changes: 3 hours (delete 22 files, edit 6 files, remove 16 P/Invoke declarations)
- Testing: 3 hours (verify all AutoTokenizer tests pass, delete model/decoder tests)
- Documentation: 1 hour (update README, migration guide)
- **Total**: ~8-10 hours

**Risk**: **Low** - Removed functions are not in any AutoTokenizer code path. Only affects:
- Users who manually construct tokenizers from separate vocab/merges files (not AutoTokenizer workflows)
- Users who manually swap models/decoders (not AutoTokenizer workflows)
- Users who use generation planning utilities (separate concern from tokenization)

**Impact**: 
- ✅ AutoTokenizer: **UNAFFECTED** - All core workflows preserved
- ✅ Chat Templates: **UNAFFECTED** - `apply_chat_template` preserved
- ✅ Encoding/Decoding: **UNAFFECTED** - All encoding/decoding functions preserved
- ✅ Padding/Truncation: **UNAFFECTED** - All preprocessing functions preserved
- ❌ Manual Model Construction: **REMOVED** - Cannot create BpeModel/WordPieceModel/UnigramModel from scratch
- ❌ Manual Decoder Swapping: **REMOVED** - Cannot call SetDecoder()/ClearDecoder()
- ❌ Generation Planning: **REMOVED** - Cannot call PlanLogitsProcessors()/PlanStoppingCriteria()

---

## Next Steps

1. Review this report with stakeholders
2. Decide on removal approach (full removal vs. stubbing vs. obsolete warnings)
3. Create a branch for the removal work
4. Execute Phase 1 changes
5. Execute Phase 2 verification
6. Merge and deploy

---

**Generated by**: GitHub Copilot  
**Date**: 2025-01-XX  
**Reviewed by**: [Pending]
