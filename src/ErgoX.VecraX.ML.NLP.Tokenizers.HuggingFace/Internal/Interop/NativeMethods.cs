using System.Runtime.InteropServices;

namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Internal.Interop;

/// <summary>
/// P/Invoke declarations for the native tokenizers library.
/// This file contains ONLY functions that are actually exported from the Rust C bindings.
/// Last validated: 2025-10-17
/// </summary>
internal static partial class NativeMethods
{
    private const string LibraryName = "tokenizers";

    #region Core Tokenizer Functions (from lib.rs)

    /// <summary>
    /// Gets the last error message from the native library.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_get_last_error")]
    internal static partial IntPtr TokenizerGetLastError();

    /// <summary>
    /// Creates a tokenizer from JSON configuration.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_create", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial IntPtr TokenizerCreateFromJson(string json, out int status);

    /// <summary>
    /// Frees a tokenizer handle.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_free")]
    internal static partial void TokenizerFree(IntPtr handle);

    /// <summary>
    /// Encodes a text sequence into tokens.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_encode", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial IntPtr TokenizerEncode(
        IntPtr handle,
        string sequence,
        string? pair,
        [MarshalAs(UnmanagedType.Bool)] bool addSpecialTokens,
        out nuint encodingLength,
        out int status);

    /// <summary>
    /// Frees an encoding handle.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_encoding_free")]
    internal static partial void EncodingFree(IntPtr encoding);

    /// <summary>
    /// Gets the token IDs from an encoding.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_encoding_get_ids")]
    internal static partial void EncodingGetIds(IntPtr encoding, [Out] uint[] buffer, nuint length);

    /// <summary>
    /// Gets the token strings from an encoding.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_encoding_get_tokens")]
    internal static partial void EncodingGetTokens(IntPtr encoding, IntPtr[] buffer, nuint length);

    /// <summary>
    /// Gets the character offsets for each token.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_encoding_get_offsets")]
    internal static partial void EncodingGetOffsets(IntPtr encoding, [Out] uint[] buffer, nuint length);

    /// <summary>
    /// Gets the type IDs from an encoding.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_encoding_get_type_ids")]
    internal static partial void EncodingGetTypeIds(IntPtr encoding, [Out] uint[] buffer, nuint length);

    /// <summary>
    /// Gets the attention mask from an encoding.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_encoding_get_attention_mask")]
    internal static partial void EncodingGetAttentionMask(IntPtr encoding, [Out] uint[] buffer, nuint length);

    /// <summary>
    /// Gets the special tokens mask from an encoding.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_encoding_get_special_tokens_mask")]
    internal static partial void EncodingGetSpecialTokensMask(IntPtr encoding, [Out] uint[] buffer, nuint length);

    /// <summary>
    /// Gets the word IDs from an encoding.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_encoding_get_word_ids")]
    internal static partial void EncodingGetWordIds(IntPtr encoding, [Out] int[] buffer, nuint length);

    /// <summary>
    /// Gets the sequence IDs from an encoding.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_encoding_get_sequence_ids")]
    internal static partial void EncodingGetSequenceIds(IntPtr encoding, [Out] int[] buffer, nuint length);

    /// <summary>
    /// Gets the count of overflowing encodings.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_encoding_get_overflowing_count")]
    internal static partial nuint EncodingGetOverflowingCount(IntPtr encoding);

    /// <summary>
    /// Gets an overflowing encoding by index.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_encoding_get_overflowing")]
    internal static partial IntPtr EncodingGetOverflowing(
        IntPtr encoding,
        nuint index,
        out nuint encodingLength,
        out int status);

    /// <summary>
    /// Converts a token string to its ID.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_token_to_id", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int TokenToId(IntPtr handle, string token, out int status);

    /// <summary>
    /// Converts a token ID to its string.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_id_to_token")]
    internal static partial IntPtr IdToToken(IntPtr handle, uint id, out int status);

    /// <summary>
    /// Gets the tokenizer configuration as JSON.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_get_config")]
    internal static partial IntPtr TokenizerGetConfig(IntPtr handle, [MarshalAs(UnmanagedType.Bool)] bool pretty, out int status);

    /// <summary>
    /// Decodes token IDs back to text.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_decode")]
    internal static partial IntPtr TokenizerDecode(
        IntPtr handle,
        uint[] ids,
        nuint length,
        [MarshalAs(UnmanagedType.Bool)] bool skipSpecialTokens,
        out int status);

    /// <summary>
    /// Frees a string allocated by the native library.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_free_string")]
    internal static partial void FreeString(IntPtr str);

    /// <summary>
    /// Gets the vocabulary as JSON.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_get_vocab")]
    internal static partial IntPtr TokenizerGetVocab(IntPtr handle, [MarshalAs(UnmanagedType.Bool)] bool withAdded, out int status);

    /// <summary>
    /// Adds tokens to the tokenizer.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_add_tokens", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int TokenizerAddTokens(IntPtr handle, string tokensJson, out int status);

    /// <summary>
    /// Adds special tokens to the tokenizer.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_add_special_tokens", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int TokenizerAddSpecialTokens(IntPtr handle, string tokensJson, out int status);

    /// <summary>
    /// Gets the added tokens decoder as JSON.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_get_added_tokens_decoder")]
    internal static partial IntPtr TokenizerGetAddedTokensDecoder(IntPtr handle, out int status);

    /// <summary>
    /// Sets whether to encode special tokens.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_set_encode_special_tokens")]
    internal static partial int TokenizerSetEncodeSpecialTokens(
        IntPtr handle,
        [MarshalAs(UnmanagedType.Bool)] bool value,
        out int status);

    /// <summary>
    /// Gets whether special tokens are encoded.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_get_encode_special_tokens")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool TokenizerGetEncodeSpecialTokens(IntPtr handle, out int status);

    /// <summary>
    /// Gets the number of special tokens that will be added.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_num_special_tokens_to_add")]
    internal static partial int TokenizerNumSpecialTokensToAdd(
        IntPtr handle,
        [MarshalAs(UnmanagedType.Bool)] bool isPair,
        out int status);

    /// <summary>
    /// Enables padding for the tokenizer.
    /// </summary>
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

    /// <summary>
    /// Disables padding for the tokenizer.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_disable_padding")]
    internal static partial int TokenizerDisablePadding(IntPtr handle, out int status);

    /// <summary>
    /// Gets the padding configuration as JSON.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_get_padding")]
    internal static partial IntPtr TokenizerGetPadding(IntPtr handle, out int status);

    /// <summary>
    /// Enables truncation for the tokenizer.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_enable_truncation")]
    internal static partial int TokenizerEnableTruncation(
        IntPtr handle,
        nuint maxLength,
        nuint stride,
        int strategy,
        int direction,
        out int status);

    /// <summary>
    /// Disables truncation for the tokenizer.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_disable_truncation")]
    internal static partial int TokenizerDisableTruncation(IntPtr handle, out int status);

    /// <summary>
    /// Gets the truncation configuration as JSON.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_get_truncation")]
    internal static partial IntPtr TokenizerGetTruncation(IntPtr handle, out int status);

    #endregion

    #region Encoding Manipulation Functions (from encoding/methods.rs)

    /// <summary>
    /// Merges two encodings together.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_encoding_merge")]
    internal static partial IntPtr EncodingMerge(
        IntPtr encoding,
        IntPtr pairEncoding,
        [MarshalAs(UnmanagedType.Bool)] bool growingOffsets,
        out nuint length,
        out int status);

    /// <summary>
    /// Pads an encoding to a target length.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_encoding_pad")]
    internal static partial IntPtr EncodingPad(
        IntPtr encoding,
        nuint targetLength,
        uint padId,
        uint padTypeId,
        IntPtr padToken,
        int direction,
        out nuint length,
        out int status);

    /// <summary>
    /// Truncates an encoding to a maximum length.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_encoding_truncate")]
    internal static partial IntPtr EncodingTruncate(
        IntPtr encoding,
        nuint maxLength,
        nuint stride,
        int direction,
        out nuint length,
        out int status);

    /// <summary>
    /// Sets the sequence ID for an encoding.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_encoding_set_sequence_id")]
    internal static partial int EncodingSetSequenceId(IntPtr encoding, nuint sequenceId, out int status);

    /// <summary>
    /// Gets the token range for a given word index.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_encoding_word_to_tokens")]
    internal static partial int EncodingWordToTokens(
        IntPtr encoding,
        uint wordIndex,
        nuint sequenceIndex,
        out int status);

    /// <summary>
    /// Gets the character range for a given word index.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_encoding_word_to_chars")]
    internal static partial int EncodingWordToChars(
        IntPtr encoding,
        uint wordIndex,
        nuint sequenceIndex,
        out int status);

    /// <summary>
    /// Gets the sequence index for a given token index.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_encoding_token_to_sequence")]
    internal static partial int EncodingTokenToSequence(
        IntPtr encoding,
        nuint tokenIndex,
        out int status);

    /// <summary>
    /// Gets the character range for a given token index.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_encoding_token_to_chars")]
    internal static partial int EncodingTokenToChars(
        IntPtr encoding,
        nuint tokenIndex,
        out int status);

    /// <summary>
    /// Gets the word index for a given token index.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_encoding_token_to_word")]
    internal static partial int EncodingTokenToWord(
        IntPtr encoding,
        nuint tokenIndex,
        out int status);

    /// <summary>
    /// Gets the token index for a given character position.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_encoding_char_to_token")]
    internal static partial int EncodingCharToToken(
        IntPtr encoding,
        nuint charPos,
        nuint sequenceIndex,
        out int status);

    /// <summary>
    /// Gets the word index for a given character position.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_encoding_char_to_word")]
    internal static partial int EncodingCharToWord(
        IntPtr encoding,
        nuint charPos,
        nuint sequenceIndex,
        out int status);

    #endregion

    #region Model Functions

    // BPE Model
    /// <summary>
    /// Creates a BPE model from vocabulary and merges strings.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_bpe_create", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial IntPtr BpeCreate(
        string vocabJson,
        string mergesStr,
        nuint cacheCapacity,
        float dropout,
        string? unknownToken,
        string? continuingSubwordPrefix,
        string? endOfWordSuffix,
        [MarshalAs(UnmanagedType.Bool)] bool fuseUnknown,
        [MarshalAs(UnmanagedType.Bool)] bool byteFallback,
        out int status);

    /// <summary>
    /// Creates a BPE model from vocabulary and merges files.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_bpe_from_file", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial IntPtr BpeFromFile(
        string vocabPath,
        string mergesPath,
        nuint cacheCapacity,
        float dropout,
        string? unknownToken,
        string? continuingSubwordPrefix,
        string? endOfWordSuffix,
        [MarshalAs(UnmanagedType.Bool)] bool fuseUnknown,
        out int status);

    /// <summary>
    /// Frees a BPE model handle.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_bpe_free")]
    internal static partial void BpeFree(IntPtr model);

    // WordPiece Model
    /// <summary>
    /// Creates a WordPiece model from a vocabulary file.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_wordpiece_from_file", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial IntPtr WordPieceFromFile(
        string vocabPath,
        string? unkToken,
        nuint maxInputCharsPerWord,
        string? continuingSubwordPrefix,
        out int status);

    /// <summary>
    /// Frees a WordPiece model handle.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_wordpiece_free")]
    internal static partial void WordPieceFree(IntPtr model);

    // WordLevel Model
    /// <summary>
    /// Creates a WordLevel model from a vocabulary JSON file.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_wordlevel_from_file", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial IntPtr WordLevelFromFile(
        string vocabPath,
        string? unkToken,
        out int status);

    /// <summary>
    /// Frees a WordLevel model handle.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_wordlevel_free")]
    internal static partial void WordLevelFree(IntPtr model);

    // Unigram Model
    /// <summary>
    /// Creates a Unigram model from a vocabulary with scores.
    /// </summary>
    [DllImport(LibraryName, EntryPoint = "tokenizers_unigram_new")]
    internal static extern IntPtr UnigramNew(
        Models.UnigramModel.VocabItem[] vocab,
        nuint vocabLen,
        IntPtr unkId,
        [MarshalAs(UnmanagedType.Bool)] bool byteFallback,
        out int status);

    /// <summary>
    /// Frees a Unigram model handle.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_unigram_free")]
    internal static partial void UnigramFree(IntPtr model);

    #endregion

    #region Decoder Functions

    // BPE Decoder
    /// <summary>
    /// Creates a new BPE decoder.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_bpe_decoder_new", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial IntPtr BpeDecoderNew(string suffix, out int status);

    /// <summary>
    /// Frees a BPE decoder handle.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_bpe_decoder_free")]
    internal static partial void BpeDecoderFree(IntPtr decoder);

    // ByteLevel Decoder
    /// <summary>
    /// Creates a new ByteLevel decoder.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_bytelevel_decoder_new")]
    internal static partial IntPtr ByteLevelDecoderNew(out int status);

    /// <summary>
    /// Frees a ByteLevel decoder handle.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_bytelevel_decoder_free")]
    internal static partial void ByteLevelDecoderFree(IntPtr decoder);

    // ByteFallback Decoder
    /// <summary>
    /// Creates a new ByteFallback decoder.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_bytefallback_decoder_new")]
    internal static partial IntPtr ByteFallbackDecoderNew(out int status);

    /// <summary>
    /// Frees a ByteFallback decoder handle.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_bytefallback_decoder_free")]
    internal static partial void ByteFallbackDecoderFree(IntPtr decoder);

    // CTC Decoder
    /// <summary>
    /// Creates a new CTC decoder.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_ctc_decoder_new", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial IntPtr CtcDecoderNew(
        string padToken,
        string wordDelimiterToken,
        [MarshalAs(UnmanagedType.Bool)] bool cleanup,
        out int status);

    /// <summary>
    /// Frees a CTC decoder handle.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_ctc_decoder_free")]
    internal static partial void CtcDecoderFree(IntPtr decoder);

    // Fuse Decoder
    /// <summary>
    /// Creates a new Fuse decoder.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_fuse_decoder_new")]
    internal static partial IntPtr FuseDecoderNew(out int status);

    /// <summary>
    /// Frees a Fuse decoder handle.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_fuse_decoder_free")]
    internal static partial void FuseDecoderFree(IntPtr decoder);

    // Metaspace Decoder
    /// <summary>
    /// Creates a new Metaspace decoder.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_metaspace_decoder_new", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial IntPtr MetaspaceDecoderNew(
        string replacement,
        byte prependScheme,
        [MarshalAs(UnmanagedType.Bool)] bool split,
        out int status);

    /// <summary>
    /// Frees a Metaspace decoder handle.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_metaspace_decoder_free")]
    internal static partial void MetaspaceDecoderFree(IntPtr decoder);

    // Replace Decoder
    /// <summary>
    /// Creates a new Replace decoder.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_replace_decoder_new", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial IntPtr ReplaceDecoderNew(
        string pattern,
        string content,
        out int status);

    /// <summary>
    /// Frees a Replace decoder handle.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_replace_decoder_free")]
    internal static partial void ReplaceDecoderFree(IntPtr decoder);

    // Strip Decoder
    /// <summary>
    /// Creates a new Strip decoder.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_strip_decoder_new", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial IntPtr StripDecoderNew(
        string content,
        nuint left,
        nuint right,
        out int status);

    /// <summary>
    /// Frees a Strip decoder handle.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_strip_decoder_free")]
    internal static partial void StripDecoderFree(IntPtr decoder);

    // WordPiece Decoder
    /// <summary>
    /// Creates a new WordPiece decoder.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_wordpiece_decoder_new", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial IntPtr WordPieceDecoderNew(
        string prefix,
        [MarshalAs(UnmanagedType.Bool)] bool cleanup,
        out int status);

    /// <summary>
    /// Frees a WordPiece decoder handle.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_wordpiece_decoder_free")]
    internal static partial void WordPieceDecoderFree(IntPtr decoder);

    #endregion

    #region Helper Methods

    /// <summary>
    /// Gets the last error message from the native library.
    /// </summary>
    internal static string GetLastError()
    {
        var ptr = TokenizerGetLastError();
        return ptr == IntPtr.Zero ? "Unknown error" : Marshal.PtrToStringUTF8(ptr) ?? "Unknown error";
    }

    /// <summary>
    /// Gets the last error message from the native library, or null if no error.
    /// </summary>
    internal static string? GetLastErrorMessage()
    {
        var ptr = TokenizerGetLastError();
        return ptr == IntPtr.Zero ? null : Marshal.PtrToStringUTF8(ptr);
    }

    #endregion
}
