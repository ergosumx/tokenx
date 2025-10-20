namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Internal.Interop;

using System;
using System.Runtime.InteropServices;

/// <summary>
/// P/Invoke declarations that mirror the reduced native tokenizer bindings.
/// </summary>
internal static partial class NativeMethods
{
    private const string LibraryName = "tokenx_bridge";

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_get_last_error")]
    internal static partial IntPtr TokenizerGetLastError();

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_create", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial IntPtr TokenizerCreateFromJson(string json, out int status);

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_from_pretrained", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial IntPtr TokenizerCreateFromPretrained(string identifier, string? revision, string? authToken, out int status);

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_free")]
    internal static partial void TokenizerFree(IntPtr handle);

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_encode", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial IntPtr TokenizerEncode(
        IntPtr handle,
        string sequence,
        string? pair,
        [MarshalAs(UnmanagedType.Bool)] bool addSpecialTokens,
        out nuint encodingLength,
        out int status);

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_encoding_free")]
    internal static partial void EncodingFree(IntPtr encoding);

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_encoding_get_ids")]
    internal static partial void EncodingGetIds(IntPtr encoding, [Out] uint[] buffer, nuint length);

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_encoding_get_tokens")]
    internal static partial void EncodingGetTokens(IntPtr encoding, IntPtr[] buffer, nuint length);

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_encoding_get_offsets")]
    internal static partial void EncodingGetOffsets(IntPtr encoding, [Out] uint[] buffer, nuint length);

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_encoding_get_type_ids")]
    internal static partial void EncodingGetTypeIds(IntPtr encoding, [Out] uint[] buffer, nuint length);

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_encoding_get_attention_mask")]
    internal static partial void EncodingGetAttentionMask(IntPtr encoding, [Out] uint[] buffer, nuint length);

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_encoding_get_special_tokens_mask")]
    internal static partial void EncodingGetSpecialTokensMask(IntPtr encoding, [Out] uint[] buffer, nuint length);

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_encoding_get_word_ids")]
    internal static partial void EncodingGetWordIds(IntPtr encoding, [Out] int[] buffer, nuint length);

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_encoding_get_sequence_ids")]
    internal static partial void EncodingGetSequenceIds(IntPtr encoding, [Out] int[] buffer, nuint length);

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_encoding_copy_numeric")]
    internal static partial int EncodingCopyNumeric(
        IntPtr encoding,
        ref EncodingNumericDest destination,
        nuint destinationLength,
        out int status);

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_encoding_get_overflowing_count")]
    internal static partial nuint EncodingGetOverflowingCount(IntPtr encoding);

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_encoding_get_overflowing")]
    internal static partial IntPtr EncodingGetOverflowing(
        IntPtr encoding,
        nuint index,
        out nuint encodingLength,
        out int status);

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_token_to_id", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int TokenToId(IntPtr handle, string token, out int status);

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_id_to_token")]
    internal static partial IntPtr IdToToken(IntPtr handle, uint id, out int status);

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_model_from_json", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial IntPtr TokenizersModelFromJson(string json, out int status);

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_model_free")]
    internal static partial void TokenizersModelFree(IntPtr model);

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_model_get_type")]
    internal static partial IntPtr TokenizersModelGetType(IntPtr model, out int status);

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_model_to_json")]
    internal static partial IntPtr TokenizersModelToJson(IntPtr model, [MarshalAs(UnmanagedType.Bool)] bool pretty, out int status);

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_model_bpe_from_files", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial IntPtr TokenizersModelBpeFromFiles(
        string vocabPath,
        string mergesPath,
        float dropout,
        [MarshalAs(UnmanagedType.Bool)] bool hasDropout,
        string? unkToken,
        string? continuingSubwordPrefix,
        string? endOfWordSuffix,
        [MarshalAs(UnmanagedType.Bool)] bool fuseUnknown,
        [MarshalAs(UnmanagedType.Bool)] bool enableByteFallback,
        out int status);

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_model_wordpiece_from_file", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial IntPtr TokenizersModelWordPieceFromFile(
        string vocabPath,
        string unkToken,
        string? continuingSubwordPrefix,
        nuint maxInputCharsPerWord,
        [MarshalAs(UnmanagedType.Bool)] bool hasMaxInputCharsPerWord,
        out int status);

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_model_unigram_from_file", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial IntPtr TokenizersModelUnigramFromFile(string modelPath, out int status);

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_tokenizer_set_model")]
    internal static partial int TokenizersTokenizerSetModel(IntPtr tokenizer, IntPtr model, out int status);

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

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_get_config")]
    internal static partial IntPtr TokenizerGetConfig(IntPtr handle, [MarshalAs(UnmanagedType.Bool)] bool pretty, out int status);

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_decode")]
    internal static partial IntPtr TokenizerDecode(
        IntPtr handle,
        uint[] ids,
        nuint length,
        [MarshalAs(UnmanagedType.Bool)] bool skipSpecialTokens,
        out int status);

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_decode_batch_flat")]
    internal static unsafe partial int TokenizerDecodeBatchFlat(
        IntPtr handle,
        uint* tokens,
        nuint totalLength,
        nuint* lengths,
        nuint count,
        [MarshalAs(UnmanagedType.Bool)] bool skipSpecialTokens,
        IntPtr* output,
        out int status);

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_apply_chat_template", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial IntPtr TokenizerApplyChatTemplate(
        IntPtr handle,
        string template,
        string messagesJson,
        string? variablesJson,
        [MarshalAs(UnmanagedType.Bool)] bool addGenerationPrompt,
        out int status);

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_normalize_generation_config", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial IntPtr TokenizersNormalizeGenerationConfig(string source, out int status);

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_plan_logits_processors", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial IntPtr TokenizersPlanLogitsProcessors(string source, out int status);

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_plan_stopping_criteria", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial IntPtr TokenizersPlanStoppingCriteria(string source, out int status);

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_free_string")]
    internal static partial void FreeString(IntPtr str);

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_enable_padding", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int TokenizerEnablePadding(
        IntPtr handle,
        int direction,
        uint padId,
        uint padTypeId,
        string? padToken,
        int length,
        int padToMultipleOf,
        out int status);

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_disable_padding")]
    internal static partial int TokenizerDisablePadding(IntPtr handle, out int status);

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_get_padding")]
    internal static partial IntPtr TokenizerGetPadding(IntPtr handle, out int status);

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_enable_truncation")]
    internal static partial int TokenizerEnableTruncation(
        IntPtr handle,
        nuint maxLength,
        nuint stride,
        int strategy,
        int direction,
        out int status);

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_disable_truncation")]
    internal static partial int TokenizerDisableTruncation(IntPtr handle, out int status);

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_get_truncation")]
    internal static partial IntPtr TokenizerGetTruncation(IntPtr handle, out int status);

    [StructLayout(LayoutKind.Sequential)]
    internal struct EncodingOffsetNative
    {
        public uint Start;
        public uint End;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct EncodingNumericDest
    {
        public IntPtr Ids;
        public IntPtr TypeIds;
        public IntPtr AttentionMask;
        public IntPtr SpecialTokensMask;
        public IntPtr Offsets;
        public IntPtr WordIds;
        public IntPtr SequenceIds;
    }

    internal static string? GetLastErrorMessage()
    {
        var ptr = TokenizerGetLastError();
        return ptr == IntPtr.Zero ? null : Marshal.PtrToStringUTF8(ptr);
    }
}
