namespace ErgoX.VecraX.ML.NLP.Tokenizers.Google.SentencePiece.Processing;

using System;
using System.Collections.Generic;
using ErgoX.VecraX.ML.NLP.Tokenizers.Google.SentencePiece.Internal.Interop;
using ErgoX.VecraX.ML.NLP.Tokenizers.Google.SentencePiece.Options;

/// <summary>
/// Partial class containing encoding methods for the <see cref="SentencePieceProcessor"/>.
/// </summary>
public sealed partial class SentencePieceProcessor
{
    public int[] EncodeIds(string input, EncodeOptions? options = null)
    {
        ThrowIfDisposed();
        using var text = new InteropUtilities.NativeUtf8(input);
        return InvokeWithEncodeOptions(options, pointer =>
        {
            var status = NativeMethods.spc_sentencepiece_processor_encode_ids(handle, text.View, pointer, out var ids);
            InteropUtilities.EnsureSuccess(status);
            return InteropUtilities.IntArrayToManagedAndDestroy(ref ids);
        });
    }

    public IReadOnlyList<string> EncodePieces(string input, EncodeOptions? options = null)
    {
        ThrowIfDisposed();
        using var text = new InteropUtilities.NativeUtf8(input);
        return InvokeWithEncodeOptions(options, pointer =>
        {
            var status = NativeMethods.spc_sentencepiece_processor_encode_pieces(handle, text.View, pointer, out var pieces);
            InteropUtilities.EnsureSuccess(status);
            return InteropUtilities.BytesArrayToManagedAndDestroy(ref pieces);
        });
    }

    public byte[] EncodeSerializedProto(string input, EncodeOptions? options = null)
    {
        ThrowIfDisposed();
        using var text = new InteropUtilities.NativeUtf8(input);
        return InvokeWithEncodeOptions(options, pointer =>
        {
            var status = NativeMethods.spc_sentencepiece_processor_encode_serialized_proto(handle, text.View, pointer, out var proto);
            InteropUtilities.EnsureSuccess(status);
            return InteropUtilities.BytesToArrayAndDestroy(ref proto);
        });
    }

    public IReadOnlyList<int[]> EncodeIdsBatch(IEnumerable<string> inputs, int numThreads = 0, EncodeOptions? options = null)
    {
        ThrowIfDisposed();
        using var nativeInputs = new InteropUtilities.NativeUtf8Array(inputs ?? Array.Empty<string>());
        return InvokeWithEncodeOptions(options, pointer =>
        {
            var status = NativeMethods.spc_sentencepiece_processor_encode_ids_batch(handle, nativeInputs.Pointer, nativeInputs.Length, numThreads, pointer, out var lists);
            InteropUtilities.EnsureSuccess(status);
            return InteropUtilities.IntArrayListToManagedAndDestroy(ref lists);
        });
    }

    public IReadOnlyList<IReadOnlyList<string>> EncodePiecesBatch(IEnumerable<string> inputs, int numThreads = 0, EncodeOptions? options = null)
    {
        ThrowIfDisposed();
        using var nativeInputs = new InteropUtilities.NativeUtf8Array(inputs ?? Array.Empty<string>());
        return InvokeWithEncodeOptions(options, pointer =>
        {
            var status = NativeMethods.spc_sentencepiece_processor_encode_pieces_batch(handle, nativeInputs.Pointer, nativeInputs.Length, numThreads, pointer, out var lists);
            InteropUtilities.EnsureSuccess(status);
            return InteropUtilities.BytesArrayListToManagedAndDestroy(ref lists);
        });
    }

    public IReadOnlyList<byte[]> EncodeSerializedProtoBatch(IEnumerable<string> inputs, int numThreads = 0, EncodeOptions? options = null)
    {
        ThrowIfDisposed();
        using var nativeInputs = new InteropUtilities.NativeUtf8Array(inputs ?? Array.Empty<string>());
        return InvokeWithEncodeOptions(options, pointer =>
        {
            var status = NativeMethods.spc_sentencepiece_processor_encode_serialized_proto_batch(handle, nativeInputs.Pointer, nativeInputs.Length, numThreads, pointer, out var list);
            InteropUtilities.EnsureSuccess(status);
            return BytesArrayToByteArraysAndDestroy(ref list);
        });
    }
}
