namespace ErgoX.VecraX.ML.NLP.Tokenizers.Google.SentencePiece.Processing;

using System;
using System.Collections.Generic;
using System.Linq;
using ErgoX.VecraX.ML.NLP.Tokenizers.Google.SentencePiece.Internal.Interop;
using ErgoX.VecraX.ML.NLP.Tokenizers.Google.SentencePiece.Models;
using ErgoX.VecraX.ML.NLP.Tokenizers.Google.SentencePiece.Options;

/// <summary>
/// Partial class containing N-best and sampling-based encoding methods for the <see cref="SentencePieceProcessor"/>.
/// </summary>
public sealed partial class SentencePieceProcessor
{
    public IReadOnlyList<int[]> NBestEncodeIds(string input, int nbestSize, EncodeOptions? options = null)
    {
        ThrowIfDisposed();
        using var text = new InteropUtilities.NativeUtf8(input);
        return InvokeWithEncodeOptions(options, pointer =>
        {
            var status = NativeMethods.spc_sentencepiece_processor_nbest_encode_ids(handle, text.View, nbestSize, pointer, out var list);
            InteropUtilities.EnsureSuccess(status);
            return InteropUtilities.IntArrayListToManagedAndDestroy(ref list);
        });
    }

    public IReadOnlyList<IReadOnlyList<string>> NBestEncodePieces(string input, int nbestSize, EncodeOptions? options = null)
    {
        ThrowIfDisposed();
        using var text = new InteropUtilities.NativeUtf8(input);
        return InvokeWithEncodeOptions(options, pointer =>
        {
            var status = NativeMethods.spc_sentencepiece_processor_nbest_encode_pieces(handle, text.View, nbestSize, pointer, out var list);
            InteropUtilities.EnsureSuccess(status);
            return InteropUtilities.BytesArrayListToManagedAndDestroy(ref list);
        });
    }

    public byte[] NBestEncodeSerializedProto(string input, int nbestSize, EncodeOptions? options = null)
    {
        ThrowIfDisposed();
        using var text = new InteropUtilities.NativeUtf8(input);
        return InvokeWithEncodeOptions(options, pointer =>
        {
            var status = NativeMethods.spc_sentencepiece_processor_nbest_encode_serialized_proto(handle, text.View, nbestSize, pointer, out var proto);
            InteropUtilities.EnsureSuccess(status);
            return InteropUtilities.BytesToArrayAndDestroy(ref proto);
        });
    }

    public IReadOnlyList<ScoredIdSequence> SampleEncodeAndScoreIds(string input, SampleEncodeAndScoreOptions? options = null)
    {
        ThrowIfDisposed();
        using var text = new InteropUtilities.NativeUtf8(input);
        return InvokeWithSampleOptions(options, pointer =>
        {
            var status = NativeMethods.spc_sentencepiece_processor_sample_encode_and_score_ids(handle, text.View, pointer, out var list);
            InteropUtilities.EnsureSuccess(status);
            var items = InteropUtilities.ScoredIntArrayListToManagedAndDestroy(ref list);
            return items.Select(entry => new ScoredIdSequence(entry.Ids, entry.Score)).ToArray();
        });
    }

    public IReadOnlyList<ScoredPieceSequence> SampleEncodeAndScorePieces(string input, SampleEncodeAndScoreOptions? options = null)
    {
        ThrowIfDisposed();
        using var text = new InteropUtilities.NativeUtf8(input);
        return InvokeWithSampleOptions(options, pointer =>
        {
            var status = NativeMethods.spc_sentencepiece_processor_sample_encode_and_score_pieces(handle, text.View, pointer, out var list);
            InteropUtilities.EnsureSuccess(status);
            var items = InteropUtilities.ScoredBytesArrayListToManagedAndDestroy(ref list);
            return items.Select(entry => new ScoredPieceSequence(entry.Pieces, entry.Score)).ToArray();
        });
    }

    public byte[] SampleEncodeAndScoreSerializedProto(string input, SampleEncodeAndScoreOptions? options = null)
    {
        ThrowIfDisposed();
        using var text = new InteropUtilities.NativeUtf8(input);
        return InvokeWithSampleOptions(options, pointer =>
        {
            var status = NativeMethods.spc_sentencepiece_processor_sample_encode_and_score_serialized_proto(handle, text.View, pointer, out var proto);
            InteropUtilities.EnsureSuccess(status);
            return InteropUtilities.BytesToArrayAndDestroy(ref proto);
        });
    }
}
