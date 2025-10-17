using System.Runtime.InteropServices;

namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Internal.Interop;

internal static partial class NativeMethods
{
    private const string LibraryName = "tokenizers";

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_create", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial IntPtr TokenizerCreateFromJson(string json, out int status);

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

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_free_string")]
    internal static partial void FreeString(IntPtr str);

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_get_config")]
    internal static partial IntPtr TokenizerGetConfig(IntPtr handle, [MarshalAs(UnmanagedType.Bool)] bool pretty, out int status);

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_decode")]
    internal static partial IntPtr TokenizerDecode(
        IntPtr handle,
        uint[] ids,
        nuint length,
        [MarshalAs(UnmanagedType.Bool)] bool skipSpecialTokens,
        out int status);

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_get_last_error")]
    internal static partial IntPtr TokenizerGetLastError();

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_get_vocab")]
    internal static partial IntPtr TokenizerGetVocab(IntPtr handle, [MarshalAs(UnmanagedType.Bool)] bool withAdded, out int status);

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

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_add_tokens", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int TokenizerAddTokens(IntPtr handle, string tokensJson, out int status);

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_add_special_tokens", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int TokenizerAddSpecialTokens(IntPtr handle, string tokensJson, out int status);

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_get_added_tokens_decoder")]
    internal static partial IntPtr TokenizerGetAddedTokensDecoder(IntPtr handle, out int status);

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_set_encode_special_tokens")]
    internal static partial int TokenizerSetEncodeSpecialTokens(
        IntPtr handle,
        [MarshalAs(UnmanagedType.Bool)] bool value,
        out int status);

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_get_encode_special_tokens")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool TokenizerGetEncodeSpecialTokens(IntPtr handle, out int status);

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_num_special_tokens_to_add")]
    internal static partial int TokenizerNumSpecialTokensToAdd(
        IntPtr handle,
        [MarshalAs(UnmanagedType.Bool)] bool isPair,
        out int status);

    // Model-related methods

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

    #region Normalizers

    // Lowercase Normalizer
    /// <summary>
    /// Creates a new Lowercase normalizer.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_lowercase_new")]
    internal static partial IntPtr LowercaseNew(out int status);

    /// <summary>
    /// Normalizes a string using the Lowercase normalizer.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_lowercase_normalize_str", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial nuint LowercaseNormalizeStr(
        IntPtr normalizer,
        string input,
        IntPtr output,
        nuint outputLen,
        out int status);

    /// <summary>
    /// Frees a Lowercase normalizer handle.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_lowercase_free")]
    internal static partial void LowercaseFree(IntPtr normalizer);

    // ===== Sequence Normalizer =====

    /// <summary>
    /// Creates a new Sequence normalizer from an array of normalizers.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_sequence_normalizer_new")]
    internal static unsafe partial IntPtr SequenceNormalizerNew(
        nuint* normalizer_handles,
        int* normalizer_types,
        nuint count,
        out int status);

    /// <summary>
    /// Normalizes a string using the Sequence normalizer.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_sequence_normalizer_normalize", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial nuint SequenceNormalizerNormalizeStr(
        IntPtr normalizer,
        string input,
        IntPtr output,
        nuint outputLen,
        out int status);

    /// <summary>
    /// Frees a Sequence normalizer handle.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_sequence_normalizer_free")]
    internal static partial void SequenceNormalizerFree(IntPtr normalizer);

    // NFD Normalizer
    /// <summary>
    /// Creates a new NFD (Canonical Decomposition) normalizer.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_nfd_new")]
    internal static partial IntPtr NfdNew(out int status);

    /// <summary>
    /// Normalizes a string using the NFD normalizer.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_nfd_normalize_str", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial nuint NfdNormalizeStr(
        IntPtr normalizer,
        string input,
        IntPtr output,
        nuint outputLen,
        out int status);

    /// <summary>
    /// Frees an NFD normalizer handle.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_nfd_free")]
    internal static partial void NfdFree(IntPtr normalizer);

    // NFC Normalizer
    /// <summary>
    /// Creates a new NFC (Canonical Decomposition + Composition) normalizer.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_nfc_new")]
    internal static partial IntPtr NfcNew(out int status);

    /// <summary>
    /// Normalizes a string using the NFC normalizer.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_nfc_normalize_str", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial nuint NfcNormalizeStr(
        IntPtr normalizer,
        string input,
        IntPtr output,
        nuint outputLen,
        out int status);

    /// <summary>
    /// Frees an NFC normalizer handle.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_nfc_free")]
    internal static partial void NfcFree(IntPtr normalizer);

    // NFKD Normalizer
    /// <summary>
    /// Creates a new NFKD (Compatibility Decomposition) normalizer.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_nfkd_new")]
    internal static partial IntPtr NfkdNew(out int status);

    /// <summary>
    /// Normalizes a string using the NFKD normalizer.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_nfkd_normalize_str", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial nuint NfkdNormalizeStr(
        IntPtr normalizer,
        string input,
        IntPtr output,
        nuint outputLen,
        out int status);

    /// <summary>
    /// Frees an NFKD normalizer handle.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_nfkd_free")]
    internal static partial void NfkdFree(IntPtr normalizer);

    // NFKC Normalizer
    /// <summary>
    /// Creates a new NFKC (Compatibility Decomposition + Composition) normalizer.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_nfkc_new")]
    internal static partial IntPtr NfkcNew(out int status);

    /// <summary>
    /// Normalizes a string using the NFKC normalizer.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_nfkc_normalize_str", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial nuint NfkcNormalizeStr(
        IntPtr normalizer,
        string input,
        IntPtr output,
        nuint outputLen,
        out int status);

    /// <summary>
    /// Frees an NFKC normalizer handle.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_nfkc_free")]
    internal static partial void NfkcFree(IntPtr normalizer);

    // BERT Normalizer
    /// <summary>
    /// Creates a new BERT normalizer.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_bert_normalizer_new")]
    internal static partial IntPtr BertNormalizerNew(
        [MarshalAs(UnmanagedType.Bool)] bool cleanText,
        [MarshalAs(UnmanagedType.Bool)] bool handleChineseChars,
        IntPtr stripAccents,
        [MarshalAs(UnmanagedType.Bool)] bool lowercase,
        out int status);

    /// <summary>
    /// Normalizes a string using the BERT normalizer.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_bert_normalizer_normalize_str", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial nuint BertNormalizerNormalizeStr(
        IntPtr normalizer,
        string input,
        IntPtr output,
        nuint outputLen,
        out int status);

    /// <summary>
    /// Frees a BERT normalizer handle.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_bert_normalizer_free")]
    internal static partial void BertNormalizerFree(IntPtr normalizer);

    // Strip Normalizer
    /// <summary>
    /// Creates a new Strip normalizer.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_strip_normalizer_new")]
    internal static partial IntPtr StripNew(
        [MarshalAs(UnmanagedType.Bool)] bool left,
        [MarshalAs(UnmanagedType.Bool)] bool right,
        out int status);

    /// <summary>
    /// Normalizes a string using the Strip normalizer.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_strip_normalizer_normalize_str", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial nuint StripNormalizeStr(
        IntPtr normalizer,
        string input,
        IntPtr output,
        nuint outputLen,
        out int status);

    /// <summary>
    /// Frees a Strip normalizer handle.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_strip_normalizer_free")]
    internal static partial void StripFree(IntPtr normalizer);

    // Replace Normalizer
    /// <summary>
    /// Creates a new Replace normalizer.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_replace_normalizer_new", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial IntPtr ReplaceNew(
        string pattern,
        string content,
        out int status);

    /// <summary>
    /// Normalizes a string using the Replace normalizer.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_replace_normalizer_normalize_str", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial nuint ReplaceNormalizeStr(
        IntPtr normalizer,
        string input,
        IntPtr output,
        nuint outputLen,
        out int status);

    /// <summary>
    /// Frees a Replace normalizer handle.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_replace_normalizer_free")]
    internal static partial void ReplaceFree(IntPtr normalizer);

    // Prepend Normalizer
    /// <summary>
    /// Creates a new Prepend normalizer.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_prepend_normalizer_new", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial IntPtr PrependNew(
        string prepend,
        out int status);

    /// <summary>
    /// Normalizes a string using the Prepend normalizer.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_prepend_normalizer_normalize_str", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial nuint PrependNormalizeStr(
        IntPtr normalizer,
        string input,
        IntPtr output,
        nuint outputLen,
        out int status);

    /// <summary>
    /// Frees a Prepend normalizer handle.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_prepend_normalizer_free")]
    internal static partial void PrependFree(IntPtr normalizer);

    // StripAccents Normalizer
    /// <summary>
    /// Creates a new StripAccents normalizer.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_strip_accents_new")]
    internal static partial IntPtr StripAccentsNew(out int status);

    /// <summary>
    /// Normalizes a string using the StripAccents normalizer.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_strip_accents_normalize_str", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial nuint StripAccentsNormalizeStr(
        IntPtr normalizer,
        string input,
        IntPtr output,
        nuint outputLen,
        out int status);

    /// <summary>
    /// Frees a StripAccents normalizer handle.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_strip_accents_free")]
    internal static partial void StripAccentsFree(IntPtr normalizer);

    // ByteLevel Normalizer
    /// <summary>
    /// Creates a new ByteLevel normalizer.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_byte_level_normalizer_new")]
    internal static partial IntPtr ByteLevelNormalizerNew(out int status);

    /// <summary>
    /// Normalizes a string using the ByteLevel normalizer.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_byte_level_normalizer_normalize_str", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial nuint ByteLevelNormalizerNormalizeStr(
        IntPtr normalizer,
        string input,
        IntPtr output,
        nuint outputLen,
        out int status);

    /// <summary>
    /// Frees a ByteLevel normalizer handle.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_byte_level_normalizer_free")]
    internal static partial void ByteLevelNormalizerFree(IntPtr normalizer);

    // Nmt Normalizer
    /// <summary>
    /// Creates a new Nmt normalizer.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_nmt_normalizer_new")]
    internal static partial IntPtr NmtNormalizerNew(out int status);

    /// <summary>
    /// Normalizes a string using the Nmt normalizer.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_nmt_normalizer_normalize_str", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial nuint NmtNormalizerNormalizeStr(
        IntPtr normalizer,
        string input,
        IntPtr output,
        nuint outputLen,
        out int status);

    /// <summary>
    /// Frees an Nmt normalizer handle.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_nmt_normalizer_free")]
    internal static partial void NmtNormalizerFree(IntPtr normalizer);

    #endregion

    #region Pre-tokenizers

    // Whitespace PreTokenizer
    /// <summary>
    /// Creates a new Whitespace pre-tokenizer.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_whitespace_new")]
    internal static partial IntPtr WhitespaceNew(out int status);

    /// <summary>
    /// Pre-tokenizes a string using the Whitespace pre-tokenizer.
    /// Returns JSON array of segments.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_whitespace_pre_tokenize_str", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial nuint WhitespacePreTokenizeStr(
        IntPtr preTokenizer,
        string input,
        IntPtr output,
        nuint outputLen,
        out int status);

    /// <summary>
    /// Frees a Whitespace pre-tokenizer handle.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_whitespace_free")]
    internal static partial void WhitespaceFree(IntPtr preTokenizer);

    // WhitespaceSplit PreTokenizer
    /// <summary>
    /// Creates a new WhitespaceSplit pre-tokenizer.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_whitespace_split_new")]
    internal static partial IntPtr WhitespaceSplitNew(out int status);

    /// <summary>
    /// Pre-tokenizes a string using the WhitespaceSplit pre-tokenizer.
    /// Returns JSON array of segments.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_whitespace_split_pre_tokenize_str", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial nuint WhitespaceSplitPreTokenizeStr(
        IntPtr preTokenizer,
        string input,
        IntPtr output,
        nuint outputLen,
        out int status);

    /// <summary>
    /// Frees a WhitespaceSplit pre-tokenizer handle.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_whitespace_split_free")]
    internal static partial void WhitespaceSplitFree(IntPtr preTokenizer);

    // Sequence PreTokenizer
    /// <summary>
    /// Creates a new Sequence pre-tokenizer from an array of pre-tokenizers.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_sequence_pre_tokenizer_new")]
    internal static unsafe partial IntPtr SequencePreTokenizerNew(
        nuint* preTokenizerHandles,
        int* preTokenizerTypes,
        nuint count,
        out int status);

    /// <summary>
    /// Pre-tokenizes a string using the Sequence pre-tokenizer.
    /// Returns JSON array of segments.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_sequence_pre_tokenizer_pre_tokenize", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial nuint SequencePreTokenizerPreTokenize(
        IntPtr preTokenizer,
        string input,
        IntPtr output,
        nuint outputLen,
        out int status);

    /// <summary>
    /// Frees a Sequence pre-tokenizer handle.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_sequence_pre_tokenizer_free")]
    internal static partial void SequencePreTokenizerFree(IntPtr preTokenizer);

    // BertPreTokenizer
    /// <summary>
    /// Creates a new BertPreTokenizer.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_bert_pre_tokenizer_new")]
    internal static partial IntPtr BertPreTokenizerNew(out int status);

    /// <summary>
    /// Pre-tokenizes a string using the BertPreTokenizer.
    /// Returns JSON array of segments.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_bert_pre_tokenizer_pre_tokenize_str", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial nuint BertPreTokenizerPreTokenizeStr(
        IntPtr preTokenizer,
        string input,
        IntPtr output,
        nuint outputLen,
        out int status);

    /// <summary>
    /// Frees a BertPreTokenizer handle.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_bert_pre_tokenizer_free")]
    internal static partial void BertPreTokenizerFree(IntPtr preTokenizer);

    // ByteLevel PreTokenizer
    /// <summary>
    /// Creates a new ByteLevel pre-tokenizer.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_byte_level_pre_tokenizer_new")]
    internal static partial IntPtr ByteLevelPreTokenizerNew(
        [MarshalAs(UnmanagedType.I1)] bool addPrefixSpace,
        [MarshalAs(UnmanagedType.I1)] bool useRegex,
        out int status);

    /// <summary>
    /// Pre-tokenizes a string using the ByteLevel pre-tokenizer.
    /// Returns JSON array of segments.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_byte_level_pre_tokenizer_pre_tokenize_str", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial nuint ByteLevelPreTokenizerPreTokenizeStr(
        IntPtr preTokenizer,
        string input,
        IntPtr output,
        nuint outputLen,
        out int status);

    /// <summary>
    /// Returns the alphabet used by the ByteLevel pre-tokenizer as JSON array.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_byte_level_pre_tokenizer_alphabet")]
    internal static partial nuint ByteLevelPreTokenizerAlphabet(
        IntPtr output,
        nuint outputLen,
        out int status);

    /// <summary>
    /// Frees a ByteLevel pre-tokenizer handle.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_byte_level_pre_tokenizer_free")]
    internal static partial void ByteLevelPreTokenizerFree(IntPtr preTokenizer);

    // Digits PreTokenizer
    /// <summary>
    /// Creates a new Digits pre-tokenizer.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_digits_new")]
    internal static partial IntPtr DigitsNew(
        [MarshalAs(UnmanagedType.I1)] bool individualDigits,
        out int status);

    /// <summary>
    /// Pre-tokenizes a string using the Digits pre-tokenizer.
    /// Returns JSON array of segments.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_digits_pre_tokenize_str", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial nuint DigitsPreTokenizeStr(
        IntPtr preTokenizer,
        string input,
        IntPtr output,
        nuint outputLen,
        out int status);

    /// <summary>
    /// Frees a Digits pre-tokenizer handle.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_digits_free")]
    internal static partial void DigitsFree(IntPtr preTokenizer);

    // CharDelimiterSplit PreTokenizer
    /// <summary>
    /// Creates a new CharDelimiterSplit pre-tokenizer.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_char_delimiter_split_new", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial IntPtr CharDelimiterSplitNew(
        string delimiter,
        out int status);

    /// <summary>
    /// Pre-tokenizes a string using the CharDelimiterSplit pre-tokenizer.
    /// Returns JSON array of segments.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_char_delimiter_split_pre_tokenize_str", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial nuint CharDelimiterSplitPreTokenizeStr(
        IntPtr preTokenizer,
        string input,
        IntPtr output,
        nuint outputLen,
        out int status);

    /// <summary>
    /// Frees a CharDelimiterSplit pre-tokenizer handle.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_char_delimiter_split_free")]
    internal static partial void CharDelimiterSplitFree(IntPtr preTokenizer);

    // Punctuation PreTokenizer
    /// <summary>
    /// Creates a new Punctuation pre-tokenizer.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_punctuation_new", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial IntPtr PunctuationNew(
        string behavior,
        out int status);

    /// <summary>
    /// Pre-tokenizes a string using the Punctuation pre-tokenizer.
    /// Returns JSON array of segments.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_punctuation_pre_tokenize_str", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial nuint PunctuationPreTokenizeStr(
        IntPtr preTokenizer,
        string input,
        IntPtr output,
        nuint outputLen,
        out int status);

    /// <summary>
    /// Frees a Punctuation pre-tokenizer handle.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_punctuation_free")]
    internal static partial void PunctuationFree(IntPtr preTokenizer);

    // UnicodeScripts PreTokenizer
    /// <summary>
    /// Creates a new UnicodeScripts pre-tokenizer.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_unicode_scripts_new")]
    internal static partial IntPtr UnicodeScriptsNew(out int status);

    /// <summary>
    /// Pre-tokenizes a string using the UnicodeScripts pre-tokenizer.
    /// Returns JSON array of segments.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_unicode_scripts_pre_tokenize_str", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial nuint UnicodeScriptsPreTokenizeStr(
        IntPtr preTokenizer,
        string input,
        IntPtr output,
        nuint outputLen,
        out int status);

    /// <summary>
    /// Frees a UnicodeScripts pre-tokenizer handle.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_unicode_scripts_free")]
    internal static partial void UnicodeScriptsFree(IntPtr preTokenizer);

    // Split PreTokenizer
    /// <summary>
    /// Creates a new Split pre-tokenizer with a string pattern.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_split_new", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial IntPtr SplitNew(
        string pattern,
        string behavior,
        [MarshalAs(UnmanagedType.I1)] bool invert,
        out int status);

    /// <summary>
    /// Creates a new Split pre-tokenizer with a regex pattern.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_split_new_regex", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial IntPtr SplitNewRegex(
        string pattern,
        string behavior,
        [MarshalAs(UnmanagedType.I1)] bool invert,
        out int status);

    /// <summary>
    /// Pre-tokenizes a string using the Split pre-tokenizer.
    /// Returns JSON array of segments.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_split_pre_tokenize_str", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial nuint SplitPreTokenizeStr(
        IntPtr preTokenizer,
        string input,
        IntPtr output,
        nuint outputLen,
        out int status);

    /// <summary>
    /// Frees a Split pre-tokenizer handle.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_split_free")]
    internal static partial void SplitFree(IntPtr preTokenizer);
    // Metaspace PreTokenizer
    /// <summary>
    /// Creates a new Metaspace pre-tokenizer.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_metaspace_new", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial IntPtr MetaspaceNew(
        string replacement,
        string prependScheme,
        [MarshalAs(UnmanagedType.I1)] bool split,
        out int status);

    /// <summary>
    /// Pre-tokenizes a string using the Metaspace pre-tokenizer.
    /// Returns JSON array of segments.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_metaspace_pre_tokenize_str", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial nuint MetaspacePreTokenizeStr(
        IntPtr preTokenizer,
        string input,
        IntPtr output,
        nuint outputLen,
        out int status);

    /// <summary>
    /// Frees a Metaspace pre-tokenizer handle.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_metaspace_free")]
    internal static partial void MetaspaceFree(IntPtr preTokenizer);

    // FixedLength PreTokenizer
    /// <summary>
    /// Creates a new FixedLength pre-tokenizer.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_fixedlength_new")]
    internal static partial IntPtr FixedLengthNew(
        nuint length,
        out int status);

    /// <summary>
    /// Pre-tokenizes a string using the FixedLength pre-tokenizer.
    /// Returns JSON array of segments.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_fixedlength_pre_tokenize_str", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial nuint FixedLengthPreTokenizeStr(
        IntPtr preTokenizer,
        string input,
        IntPtr output,
        nuint outputLen,
        out int status);

    /// <summary>
    /// Frees a FixedLength pre-tokenizer handle.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_fixedlength_free")]
    internal static partial void FixedLengthFree(IntPtr preTokenizer);

    #endregion

    #region Post-Processors

    // BERT Post-Processor
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_bert_postprocessor_new", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial IntPtr BertPostProcessorNew(
        string sep,
        uint sepId,
        string cls,
        uint clsId,
        out int status);

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_bert_postprocessor_free")]
    internal static partial void BertPostProcessorFree(IntPtr processor);

    // RoBERTa Post-Processor
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_roberta_postprocessor_new", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial IntPtr RobertaPostProcessorNew(
        string sep,
        uint sepId,
        string cls,
        uint clsId,
        [MarshalAs(UnmanagedType.Bool)] bool trimOffsets,
        [MarshalAs(UnmanagedType.Bool)] bool addPrefixSpace,
        out int status);

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_roberta_postprocessor_free")]
    internal static partial void RobertaPostProcessorFree(IntPtr processor);

    // ByteLevel Post-Processor
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_bytelevel_postprocessor_new")]
    internal static partial IntPtr ByteLevelPostProcessorNew(
        [MarshalAs(UnmanagedType.Bool)] bool trimOffsets,
        out int status);

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_bytelevel_postprocessor_free")]
    internal static partial void ByteLevelPostProcessorFree(IntPtr processor);

    // Template Post-Processor
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_template_postprocessor_new", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial IntPtr TemplatePostProcessorNew(
        string singleTemplate,
        string? pairTemplate,
        string specialTokensJson,
        out int status);

    [LibraryImport(LibraryName, EntryPoint = "tokenizers_template_postprocessor_free")]
    internal static partial void TemplatePostProcessorFree(IntPtr processor);

    #endregion

    #region Decoders

    // BPE Decoder
    /// <summary>
    /// Creates a new BPE decoder.
    /// </summary>
    [LibraryImport(LibraryName, EntryPoint = "tokenizers_bpe_decoder_new", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial IntPtr BpeDecoderNew(
        string suffix,
        out int status);

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

    #endregion

    /// <summary>
    /// Gets the last error message from the native library.
    /// </summary>
    internal static string GetLastError()
    {
        var ptr = TokenizerGetLastError();
        return ptr == IntPtr.Zero ? "Unknown error" : Marshal.PtrToStringUTF8(ptr) ?? "Unknown error";
    }

    internal static string? GetLastErrorMessage()
    {
        var ptr = TokenizerGetLastError();
        return ptr == IntPtr.Zero ? null : Marshal.PtrToStringUTF8(ptr);
    }
}

