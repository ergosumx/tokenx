namespace ErgoX.VecraX.ML.NLP.Tokenizers.Parity;

using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace;

public static class ParityHashUtilities
{
    private const int NullSentinel = unchecked((int)0x80000000);
    private static readonly Encoding Utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    public static EncodingSummary CreateSummary(EncodingResult encoding)
    {
        ArgumentNullException.ThrowIfNull(encoding);

        return new EncodingSummary
        {
            Length = encoding.Length,
            IdsHash = HashInt32Sequence(encoding.Ids),
            TokensHash = HashStringSequence(encoding.Tokens),
            TypeIdsHash = HashUInt32Sequence(encoding.TypeIds),
            AttentionMaskHash = HashUInt32Sequence(encoding.AttentionMask),
            SpecialTokensMaskHash = HashUInt32Sequence(encoding.SpecialTokensMask),
            OffsetsHash = HashOffsets(encoding.Offsets),
            WordIdsHash = HashOptionalInt32Sequence(encoding.WordIds),
            SequenceIdsHash = HashOptionalInt32Sequence(encoding.SequenceIds),
            Overflowing = encoding.Overflowing.Count == 0
                ? Array.Empty<EncodingSummary>()
                : encoding.Overflowing.Select(CreateSummary).ToArray()
        };
    }

    public static string HashString(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var byteCount = Utf8.GetByteCount(value);
        var buffer = new byte[4 + byteCount];
        BinaryPrimitives.WriteUInt32LittleEndian(buffer.AsSpan(0, 4), (uint)byteCount);
        Utf8.GetBytes(value.AsSpan(), buffer.AsSpan(4, byteCount));
        return ToHex(SHA256.HashData(buffer));
    }

    public static string HashStringSequence(IEnumerable<string> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        var writer = new ArrayBufferWriter<byte>();
        foreach (var value in values)
        {
            ArgumentNullException.ThrowIfNull(value);

            var byteCount = Utf8.GetByteCount(value);
            var span = writer.GetSpan(4 + byteCount);
            BinaryPrimitives.WriteUInt32LittleEndian(span[..4], (uint)byteCount);
            Utf8.GetBytes(value.AsSpan(), span.Slice(4, byteCount));
            writer.Advance(4 + byteCount);
        }

        return ToHex(SHA256.HashData(writer.WrittenSpan));
    }

    public static string HashInt32Sequence(IReadOnlyList<int> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        var writer = new ArrayBufferWriter<byte>(values.Count * 4);
        foreach (var value in values)
        {
            var span = writer.GetSpan(4);
            BinaryPrimitives.WriteInt32LittleEndian(span, value);
            writer.Advance(4);
        }

        return ToHex(SHA256.HashData(writer.WrittenSpan));
    }

    public static string HashUInt32Sequence(IReadOnlyList<uint> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        var writer = new ArrayBufferWriter<byte>(values.Count * 4);
        foreach (var value in values)
        {
            var span = writer.GetSpan(4);
            BinaryPrimitives.WriteUInt32LittleEndian(span, value);
            writer.Advance(4);
        }

        return ToHex(SHA256.HashData(writer.WrittenSpan));
    }

    public static string HashOffsets(IReadOnlyList<(int Start, int End)> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        var writer = new ArrayBufferWriter<byte>(values.Count * 8);
        foreach (var (start, end) in values)
        {
            var span = writer.GetSpan(8);
            BinaryPrimitives.WriteInt32LittleEndian(span[..4], start);
            BinaryPrimitives.WriteInt32LittleEndian(span[4..8], end);
            writer.Advance(8);
        }

        return ToHex(SHA256.HashData(writer.WrittenSpan));
    }

    public static string HashOptionalInt32Sequence(IReadOnlyList<int?> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        var writer = new ArrayBufferWriter<byte>(values.Count * 4);
        foreach (var value in values)
        {
            var span = writer.GetSpan(4);
            BinaryPrimitives.WriteInt32LittleEndian(span, value ?? NullSentinel);
            writer.Advance(4);
        }

        return ToHex(SHA256.HashData(writer.WrittenSpan));
    }

    private static string ToHex(ReadOnlySpan<byte> buffer)
        => Convert.ToHexString(buffer).ToLowerInvariant();
}
