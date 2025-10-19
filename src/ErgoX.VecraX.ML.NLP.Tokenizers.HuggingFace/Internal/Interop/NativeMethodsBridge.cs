namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Internal.Interop;

using System;

internal sealed class NativeMethodsBridge : INativeInterop
{
    public static NativeMethodsBridge Instance { get; } = new();

    private NativeMethodsBridge()
    {
    }

    public IntPtr TokenizerCreateFromJson(string json, out int status)
        => NativeMethods.TokenizerCreateFromJson(json, out status);

    public IntPtr TokenizerCreateFromPretrained(string identifier, string? revision, string? authToken, out int status)
        => NativeMethods.TokenizerCreateFromPretrained(identifier, revision, authToken, out status);

    public void TokenizerFree(IntPtr handle)
        => NativeMethods.TokenizerFree(handle);

    public IntPtr TokenizerEncode(IntPtr handle, string sequence, string? pair, bool addSpecialTokens, out nuint encodingLength, out int status)
        => NativeMethods.TokenizerEncode(handle, sequence, pair, addSpecialTokens, out encodingLength, out status);

    public void EncodingFree(IntPtr encoding)
        => NativeMethods.EncodingFree(encoding);

    public void EncodingGetIds(IntPtr encoding, uint[] buffer, nuint length)
        => NativeMethods.EncodingGetIds(encoding, buffer, length);

    public void EncodingGetTokens(IntPtr encoding, IntPtr[] buffer, nuint length)
        => NativeMethods.EncodingGetTokens(encoding, buffer, length);

    public void EncodingGetOffsets(IntPtr encoding, uint[] buffer, nuint length)
        => NativeMethods.EncodingGetOffsets(encoding, buffer, length);

    public void EncodingGetTypeIds(IntPtr encoding, uint[] buffer, nuint length)
        => NativeMethods.EncodingGetTypeIds(encoding, buffer, length);

    public void EncodingGetAttentionMask(IntPtr encoding, uint[] buffer, nuint length)
        => NativeMethods.EncodingGetAttentionMask(encoding, buffer, length);

    public void EncodingGetSpecialTokensMask(IntPtr encoding, uint[] buffer, nuint length)
        => NativeMethods.EncodingGetSpecialTokensMask(encoding, buffer, length);

    public void EncodingGetWordIds(IntPtr encoding, int[] buffer, nuint length)
        => NativeMethods.EncodingGetWordIds(encoding, buffer, length);

    public void EncodingGetSequenceIds(IntPtr encoding, int[] buffer, nuint length)
        => NativeMethods.EncodingGetSequenceIds(encoding, buffer, length);

    public int EncodingCopyNumeric(IntPtr encoding, ref NativeMethods.EncodingNumericDest destination, nuint destinationLength, out int status)
        => NativeMethods.EncodingCopyNumeric(encoding, ref destination, destinationLength, out status);

    public nuint EncodingGetOverflowingCount(IntPtr encoding)
        => NativeMethods.EncodingGetOverflowingCount(encoding);

    public IntPtr EncodingGetOverflowing(IntPtr encoding, nuint index, out nuint encodingLength, out int status)
        => NativeMethods.EncodingGetOverflowing(encoding, index, out encodingLength, out status);

    public int TokenToId(IntPtr handle, string token, out int status)
        => NativeMethods.TokenToId(handle, token, out status);

    public IntPtr IdToToken(IntPtr handle, uint id, out int status)
        => NativeMethods.IdToToken(handle, id, out status);

    public IntPtr TokenizerGetConfig(IntPtr handle, bool pretty, out int status)
        => NativeMethods.TokenizerGetConfig(handle, pretty, out status);

    public IntPtr TokenizerDecode(IntPtr handle, uint[] ids, nuint length, bool skipSpecialTokens, out int status)
        => NativeMethods.TokenizerDecode(handle, ids, length, skipSpecialTokens, out status);

    public unsafe int TokenizerDecodeBatchFlat(in NativeDecodeBatchRequest request, out int status)
        => NativeMethods.TokenizerDecodeBatchFlat(
            request.Handle,
            request.Tokens,
            request.TotalLength,
            request.Lengths,
            request.Count,
            request.SkipSpecialTokens,
            request.Output,
            out status);

    public IntPtr TokenizerApplyChatTemplate(IntPtr handle, string template, string messagesJson, string? variablesJson, bool addGenerationPrompt, out int status)
        => NativeMethods.TokenizerApplyChatTemplate(handle, template, messagesJson, variablesJson, addGenerationPrompt, out status);

    public IntPtr TokenizersNormalizeGenerationConfig(string source, out int status)
        => NativeMethods.TokenizersNormalizeGenerationConfig(source, out status);

    public IntPtr TokenizersPlanLogitsProcessors(string source, out int status)
        => NativeMethods.TokenizersPlanLogitsProcessors(source, out status);

    public IntPtr TokenizersPlanStoppingCriteria(string source, out int status)
        => NativeMethods.TokenizersPlanStoppingCriteria(source, out status);

    public void FreeString(IntPtr str)
        => NativeMethods.FreeString(str);

    public int TokenizerEnablePadding(IntPtr handle, in NativePaddingRequest request, out int status)
        => NativeMethods.TokenizerEnablePadding(handle, request.Direction, request.PadId, request.PadTypeId, request.PadToken, request.Length, request.PadToMultipleOf, out status);

    public int TokenizerDisablePadding(IntPtr handle, out int status)
        => NativeMethods.TokenizerDisablePadding(handle, out status);

    public IntPtr TokenizerGetPadding(IntPtr handle, out int status)
        => NativeMethods.TokenizerGetPadding(handle, out status);

    public int TokenizerEnableTruncation(IntPtr handle, nuint maxLength, nuint stride, int strategy, int direction, out int status)
        => NativeMethods.TokenizerEnableTruncation(handle, maxLength, stride, strategy, direction, out status);

    public int TokenizerDisableTruncation(IntPtr handle, out int status)
        => NativeMethods.TokenizerDisableTruncation(handle, out status);

    public IntPtr TokenizerGetTruncation(IntPtr handle, out int status)
        => NativeMethods.TokenizerGetTruncation(handle, out status);

    public string? GetLastErrorMessage()
        => NativeMethods.GetLastErrorMessage();
}
