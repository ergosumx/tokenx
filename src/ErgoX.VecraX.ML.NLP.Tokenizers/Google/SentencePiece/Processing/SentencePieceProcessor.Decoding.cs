namespace ErgoX.VecraX.ML.NLP.Tokenizers.Google.SentencePiece.Processing;

using System;
using System.Collections.Generic;
using ErgoX.VecraX.ML.NLP.Tokenizers.Google.SentencePiece.Internal.Interop;

/// <summary>
/// Partial class containing decoding methods for the <see cref="SentencePieceProcessor"/>.
/// </summary>
public sealed partial class SentencePieceProcessor
{
    public string DecodeIds(IEnumerable<int> ids)
    {
        ThrowIfDisposed();
        using var buffer = new InteropUtilities.NativeInt32Buffer(ids ?? Array.Empty<int>());
        var status = NativeMethods.spc_sentencepiece_processor_decode_ids(handle, buffer.Pointer, buffer.Length, out var text);
        InteropUtilities.EnsureSuccess(status);
        return InteropUtilities.BytesToStringAndDestroy(ref text);
    }

    public byte[] DecodeIdsAsBytes(IEnumerable<int> ids)
    {
        ThrowIfDisposed();
        using var buffer = new InteropUtilities.NativeInt32Buffer(ids ?? Array.Empty<int>());
        var status = NativeMethods.spc_sentencepiece_processor_decode_ids_as_bytes(handle, buffer.Pointer, buffer.Length, out var bytes);
        InteropUtilities.EnsureSuccess(status);
        return InteropUtilities.BytesToArrayAndDestroy(ref bytes);
    }

    public string DecodePieces(IEnumerable<string> pieces)
    {
        ThrowIfDisposed();
        using var nativePieces = new InteropUtilities.NativeUtf8Array(pieces ?? Array.Empty<string>());
        var status = NativeMethods.spc_sentencepiece_processor_decode_pieces(handle, nativePieces.Pointer, nativePieces.Length, out var text);
        InteropUtilities.EnsureSuccess(status);
        return InteropUtilities.BytesToStringAndDestroy(ref text);
    }

    public byte[] DecodeIdsAsSerializedProto(IEnumerable<int> ids)
    {
        ThrowIfDisposed();
        using var buffer = new InteropUtilities.NativeInt32Buffer(ids ?? Array.Empty<int>());
        var status = NativeMethods.spc_sentencepiece_processor_decode_ids_as_serialized_proto(handle, buffer.Pointer, buffer.Length, out var proto);
        InteropUtilities.EnsureSuccess(status);
        return InteropUtilities.BytesToArrayAndDestroy(ref proto);
    }

    public byte[] DecodePiecesAsSerializedProto(IEnumerable<string> pieces)
    {
        ThrowIfDisposed();
        using var nativePieces = new InteropUtilities.NativeUtf8Array(pieces ?? Array.Empty<string>());
        var status = NativeMethods.spc_sentencepiece_processor_decode_pieces_as_serialized_proto(handle, nativePieces.Pointer, nativePieces.Length, out var proto);
        InteropUtilities.EnsureSuccess(status);
        return InteropUtilities.BytesToArrayAndDestroy(ref proto);
    }

    public IReadOnlyList<string> DecodeIdsBatch(IEnumerable<IReadOnlyList<int>> inputs, int numThreads = 0)
    {
        ThrowIfDisposed();
        using var spans = new InteropUtilities.NativeIntSpanArray(inputs ?? Array.Empty<IReadOnlyList<int>>());
        var status = NativeMethods.spc_sentencepiece_processor_decode_ids_batch(handle, spans.Pointer, spans.Length, numThreads, out var list);
        InteropUtilities.EnsureSuccess(status);
        return InteropUtilities.BytesArrayToManagedAndDestroy(ref list);
    }

    public IReadOnlyList<byte[]> DecodeIdsAsBytesBatch(IEnumerable<IReadOnlyList<int>> inputs, int numThreads = 0)
    {
        ThrowIfDisposed();
        using var spans = new InteropUtilities.NativeIntSpanArray(inputs ?? Array.Empty<IReadOnlyList<int>>());
        var status = NativeMethods.spc_sentencepiece_processor_decode_ids_as_bytes_batch(handle, spans.Pointer, spans.Length, numThreads, out var list);
        InteropUtilities.EnsureSuccess(status);
        return BytesArrayToByteArraysAndDestroy(ref list);
    }

    public IReadOnlyList<byte[]> DecodeIdsAsSerializedProtoBatch(IEnumerable<IReadOnlyList<int>> inputs, int numThreads = 0)
    {
        ThrowIfDisposed();
        using var spans = new InteropUtilities.NativeIntSpanArray(inputs ?? Array.Empty<IReadOnlyList<int>>());
        var status = NativeMethods.spc_sentencepiece_processor_decode_ids_as_serialized_proto_batch(handle, spans.Pointer, spans.Length, numThreads, out var list);
        InteropUtilities.EnsureSuccess(status);
        return BytesArrayToByteArraysAndDestroy(ref list);
    }

    public IReadOnlyList<string> DecodePiecesBatch(IEnumerable<IReadOnlyList<string>> inputs, int numThreads = 0)
    {
        ThrowIfDisposed();
        using var spans = new InteropUtilities.NativeStringViewSpanArray(inputs ?? Array.Empty<IReadOnlyList<string>>());
        var status = NativeMethods.spc_sentencepiece_processor_decode_pieces_batch(handle, spans.Pointer, spans.Length, numThreads, out var list);
        InteropUtilities.EnsureSuccess(status);
        return InteropUtilities.BytesArrayToManagedAndDestroy(ref list);
    }

    public IReadOnlyList<byte[]> DecodePiecesAsSerializedProtoBatch(IEnumerable<IReadOnlyList<string>> inputs, int numThreads = 0)
    {
        ThrowIfDisposed();
        using var spans = new InteropUtilities.NativeStringViewSpanArray(inputs ?? Array.Empty<IReadOnlyList<string>>());
        var status = NativeMethods.spc_sentencepiece_processor_decode_pieces_as_serialized_proto_batch(handle, spans.Pointer, spans.Length, numThreads, out var list);
        InteropUtilities.EnsureSuccess(status);
        return BytesArrayToByteArraysAndDestroy(ref list);
    }
}
