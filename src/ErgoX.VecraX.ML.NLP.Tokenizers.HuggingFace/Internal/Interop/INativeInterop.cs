namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Internal.Interop;

using System;

internal interface INativeInterop
{
    IntPtr TokenizerCreateFromJson(string json, out int status);

    IntPtr TokenizerCreateFromPretrained(string identifier, string? revision, string? authToken, out int status);

    void TokenizerFree(IntPtr handle);

    IntPtr TokenizerEncode(IntPtr handle, string sequence, string? pair, bool addSpecialTokens, out nuint encodingLength, out int status);

    void EncodingFree(IntPtr encoding);

    void EncodingGetIds(IntPtr encoding, uint[] buffer, nuint length);

    void EncodingGetTokens(IntPtr encoding, IntPtr[] buffer, nuint length);

    void EncodingGetOffsets(IntPtr encoding, uint[] buffer, nuint length);

    void EncodingGetTypeIds(IntPtr encoding, uint[] buffer, nuint length);

    void EncodingGetAttentionMask(IntPtr encoding, uint[] buffer, nuint length);

    void EncodingGetSpecialTokensMask(IntPtr encoding, uint[] buffer, nuint length);

    void EncodingGetWordIds(IntPtr encoding, int[] buffer, nuint length);

    void EncodingGetSequenceIds(IntPtr encoding, int[] buffer, nuint length);

    int EncodingCopyNumeric(IntPtr encoding, ref NativeMethods.EncodingNumericDest destination, nuint destinationLength, out int status);

    nuint EncodingGetOverflowingCount(IntPtr encoding);

    IntPtr EncodingGetOverflowing(IntPtr encoding, nuint index, out nuint encodingLength, out int status);

    int TokenToId(IntPtr handle, string token, out int status);

    IntPtr IdToToken(IntPtr handle, uint id, out int status);

    IntPtr TokenizerGetConfig(IntPtr handle, bool pretty, out int status);

    IntPtr TokenizerDecode(IntPtr handle, uint[] ids, nuint length, bool skipSpecialTokens, out int status);

    unsafe int TokenizerDecodeBatchFlat(in NativeDecodeBatchRequest request, out int status);

    IntPtr TokenizerApplyChatTemplate(IntPtr handle, string template, string messagesJson, string? variablesJson, bool addGenerationPrompt, out int status);

    IntPtr TokenizersNormalizeGenerationConfig(string source, out int status);

    IntPtr TokenizersPlanLogitsProcessors(string source, out int status);

    IntPtr TokenizersPlanStoppingCriteria(string source, out int status);

    void FreeString(IntPtr str);

    int TokenizerEnablePadding(IntPtr handle, in NativePaddingRequest request, out int status);

    int TokenizerDisablePadding(IntPtr handle, out int status);

    IntPtr TokenizerGetPadding(IntPtr handle, out int status);

    int TokenizerEnableTruncation(IntPtr handle, nuint maxLength, nuint stride, int strategy, int direction, out int status);

    int TokenizerDisableTruncation(IntPtr handle, out int status);

    IntPtr TokenizerGetTruncation(IntPtr handle, out int status);

    string? GetLastErrorMessage();
}
