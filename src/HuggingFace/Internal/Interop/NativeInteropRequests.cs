namespace ErgoX.TokenX.HuggingFace.Internal.Interop;

using System;

internal readonly unsafe struct NativeDecodeBatchRequest
{
    public NativeDecodeBatchRequest(IntPtr handle, uint* tokens, nuint totalLength, nuint* lengths, nuint count, bool skipSpecialTokens, IntPtr* output)
    {
        Handle = handle;
        Tokens = tokens;
        TotalLength = totalLength;
        Lengths = lengths;
        Count = count;
        SkipSpecialTokens = skipSpecialTokens;
        Output = output;
    }

    public IntPtr Handle { get; }

    public uint* Tokens { get; }

    public nuint TotalLength { get; }

    public nuint* Lengths { get; }

    public nuint Count { get; }

    public bool SkipSpecialTokens { get; }

    public IntPtr* Output { get; }
}

internal readonly struct NativePaddingRequest
{
    public NativePaddingRequest(int direction, uint padId, uint padTypeId, string? padToken, int length, int padToMultipleOf)
    {
        Direction = direction;
        PadId = padId;
        PadTypeId = padTypeId;
        PadToken = padToken;
        Length = length;
        PadToMultipleOf = padToMultipleOf;
    }

    public int Direction { get; }

    public uint PadId { get; }

    public uint PadTypeId { get; }

    public string? PadToken { get; }

    public int Length { get; }

    public int PadToMultipleOf { get; }
}

