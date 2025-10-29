namespace ErgoX.TokenX.SentencePiece.Processing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ErgoX.TokenX.SentencePiece.Internal.Interop;
    using ErgoX.TokenX.SentencePiece.Models;
    using ErgoX.TokenX.SentencePiece.Options;

    /// <summary>
    /// Partial class containing convenience properties and overloads for the <see cref="SentencePieceProcessor"/>.
    /// </summary>
    public sealed partial class SentencePieceProcessor
    {
    public IReadOnlyList<int> PieceToIds(IEnumerable<string> pieces)
    {
        if (pieces is null)
        {
            throw new ArgumentNullException(nameof(pieces));
        }

        ThrowIfDisposed();
        var list = pieces as IList<string> ?? pieces.ToList();
        if (list.Count == 0)
        {
            return Array.Empty<int>();
        }

        var result = new int[list.Count];
        for (int i = 0; i < list.Count; ++i)
        {
            using var value = new InteropUtilities.NativeUtf8(list[i]);
            result[i] = NativeMethods.spc_sentencepiece_processor_piece_to_id(handle, value.View);
        }

        return result;
    }

    public IReadOnlyList<string> IdToPieces(IEnumerable<int> ids)
    {
        if (ids is null)
        {
            throw new ArgumentNullException(nameof(ids));
        }

        ThrowIfDisposed();
        var list = ids as IList<int> ?? ids.ToList();
        if (list.Count == 0)
        {
            return Array.Empty<string>();
        }

        var result = new string[list.Count];
        for (int i = 0; i < list.Count; ++i)
        {
            result[i] = IdToPiece(list[i]);
        }

        return result;
    }

    public IReadOnlyList<int[]> EncodeIds(IEnumerable<string> inputs, EncodeOptions? options = null)
    {
        if (inputs is null)
        {
            throw new ArgumentNullException(nameof(inputs));
        }

        return EncodeIdsBatch(inputs, numThreads: 0, options);
    }

    public IReadOnlyList<IReadOnlyList<string>> EncodePieces(IEnumerable<string> inputs, EncodeOptions? options = null)
    {
        if (inputs is null)
        {
            throw new ArgumentNullException(nameof(inputs));
        }

        return EncodePiecesBatch(inputs, numThreads: 0, options);
    }

    public IReadOnlyList<byte[]> EncodeSerializedProto(IEnumerable<string> inputs, EncodeOptions? options = null)
    {
        if (inputs is null)
        {
            throw new ArgumentNullException(nameof(inputs));
        }

        return EncodeSerializedProtoBatch(inputs, numThreads: 0, options);
    }

    public IReadOnlyList<string> DecodeIds(IEnumerable<IReadOnlyList<int>> inputs, int numThreads = 0)
    {
        if (inputs is null)
        {
            throw new ArgumentNullException(nameof(inputs));
        }

        return DecodeIdsBatch(inputs, numThreads);
    }

    public IReadOnlyList<string> DecodePieces(IEnumerable<IReadOnlyList<string>> inputs, int numThreads = 0)
    {
        if (inputs is null)
        {
            throw new ArgumentNullException(nameof(inputs));
        }

        return DecodePiecesBatch(inputs, numThreads);
    }

    public IReadOnlyList<byte[]> DecodeIdsAsBytes(IEnumerable<IReadOnlyList<int>> inputs, int numThreads = 0)
    {
        if (inputs is null)
        {
            throw new ArgumentNullException(nameof(inputs));
        }

        return DecodeIdsAsBytesBatch(inputs, numThreads);
    }

    public IReadOnlyList<byte[]> DecodeIdsAsSerializedProto(IEnumerable<IReadOnlyList<int>> inputs, int numThreads = 0)
    {
        if (inputs is null)
        {
            throw new ArgumentNullException(nameof(inputs));
        }

        return DecodeIdsAsSerializedProtoBatch(inputs, numThreads);
    }

    public IReadOnlyList<byte[]> DecodePiecesAsSerializedProto(IEnumerable<IReadOnlyList<string>> inputs, int numThreads = 0)
    {
        if (inputs is null)
        {
            throw new ArgumentNullException(nameof(inputs));
        }

        return DecodePiecesAsSerializedProtoBatch(inputs, numThreads);
    }

    public IReadOnlyList<int> SampleEncodeIds(string input, int nbestSize, float alpha, EncodeOptions? options = null)
    {
        return EncodeIds(input, PrepareSamplingOptions(options, nbestSize, alpha));
    }

    public IReadOnlyList<int[]> SampleEncodeIds(IEnumerable<string> inputs, int numThreads = 0, int nbestSize = -1, float alpha = 0.1f, EncodeOptions? options = null)
    {
        if (inputs is null)
        {
            throw new ArgumentNullException(nameof(inputs));
        }

        return EncodeIdsBatch(inputs, numThreads, PrepareSamplingOptions(options, nbestSize, alpha));
    }

    public IReadOnlyList<string> SampleEncodePieces(string input, int nbestSize, float alpha, EncodeOptions? options = null)
    {
        return EncodePieces(input, PrepareSamplingOptions(options, nbestSize, alpha));
    }

    public IReadOnlyList<IReadOnlyList<string>> SampleEncodePieces(IEnumerable<string> inputs, int numThreads = 0, int nbestSize = -1, float alpha = 0.1f, EncodeOptions? options = null)
    {
        if (inputs is null)
        {
            throw new ArgumentNullException(nameof(inputs));
        }

        return EncodePiecesBatch(inputs, numThreads, PrepareSamplingOptions(options, nbestSize, alpha));
    }

    public byte[] SampleEncodeSerializedProto(string input, int nbestSize, float alpha, EncodeOptions? options = null)
    {
        return EncodeSerializedProto(input, PrepareSamplingOptions(options, nbestSize, alpha));
    }

    public IReadOnlyList<byte[]> SampleEncodeSerializedProto(IEnumerable<string> inputs, int numThreads = 0, int nbestSize = -1, float alpha = 0.1f, EncodeOptions? options = null)
    {
        if (inputs is null)
        {
            throw new ArgumentNullException(nameof(inputs));
        }

        return EncodeSerializedProtoBatch(inputs, numThreads, PrepareSamplingOptions(options, nbestSize, alpha));
    }

    public IReadOnlyList<IReadOnlyList<int[]>> NBestEncodeIds(IEnumerable<string> inputs, int nbestSize, EncodeOptions? options = null)
    {
        if (inputs is null)
        {
            throw new ArgumentNullException(nameof(inputs));
        }

        var list = inputs as IList<string> ?? inputs.ToList();
        if (list.Count == 0)
        {
            return Array.Empty<IReadOnlyList<int[]>>();
        }

        var results = new List<IReadOnlyList<int[]>>(list.Count);
        foreach (var input in list)
        {
            results.Add(NBestEncodeIds(input, nbestSize, options));
        }

        return results;
    }

    public IReadOnlyList<IReadOnlyList<IReadOnlyList<string>>> NBestEncodePieces(IEnumerable<string> inputs, int nbestSize, EncodeOptions? options = null)
    {
        if (inputs is null)
        {
            throw new ArgumentNullException(nameof(inputs));
        }

        var list = inputs as IList<string> ?? inputs.ToList();
        if (list.Count == 0)
        {
            return Array.Empty<IReadOnlyList<IReadOnlyList<string>>>();
        }

        var results = new List<IReadOnlyList<IReadOnlyList<string>>>(list.Count);
        foreach (var input in list)
        {
            results.Add(NBestEncodePieces(input, nbestSize, options));
        }

        return results;
    }

    public IReadOnlyList<byte[]> NBestEncodeSerializedProto(IEnumerable<string> inputs, int nbestSize, EncodeOptions? options = null)
    {
        if (inputs is null)
        {
            throw new ArgumentNullException(nameof(inputs));
        }

        var list = inputs as IList<string> ?? inputs.ToList();
        if (list.Count == 0)
        {
            return Array.Empty<byte[]>();
        }

        var results = new byte[list.Count][];
        for (int i = 0; i < list.Count; ++i)
        {
            results[i] = NBestEncodeSerializedProto(list[i], nbestSize, options);
        }

        return results;
    }

    public IReadOnlyList<IReadOnlyList<ScoredIdSequence>> SampleEncodeAndScoreIds(IEnumerable<string> inputs, SampleEncodeAndScoreOptions? options = null)
    {
        if (inputs is null)
        {
            throw new ArgumentNullException(nameof(inputs));
        }

        var list = inputs as IList<string> ?? inputs.ToList();
        if (list.Count == 0)
        {
            return Array.Empty<IReadOnlyList<ScoredIdSequence>>();
        }

        var results = new List<IReadOnlyList<ScoredIdSequence>>(list.Count);
        foreach (var input in list)
        {
            results.Add(SampleEncodeAndScoreIds(input, options));
        }

        return results;
    }

    public IReadOnlyList<IReadOnlyList<ScoredPieceSequence>> SampleEncodeAndScorePieces(IEnumerable<string> inputs, SampleEncodeAndScoreOptions? options = null)
    {
        if (inputs is null)
        {
            throw new ArgumentNullException(nameof(inputs));
        }

        var list = inputs as IList<string> ?? inputs.ToList();
        if (list.Count == 0)
        {
            return Array.Empty<IReadOnlyList<ScoredPieceSequence>>();
        }

        var results = new List<IReadOnlyList<ScoredPieceSequence>>(list.Count);
        foreach (var input in list)
        {
            results.Add(SampleEncodeAndScorePieces(input, options));
        }

        return results;
    }

    public IReadOnlyList<byte[]> SampleEncodeAndScoreSerializedProto(IEnumerable<string> inputs, SampleEncodeAndScoreOptions? options = null)
    {
        if (inputs is null)
        {
            throw new ArgumentNullException(nameof(inputs));
        }

        var list = inputs as IList<string> ?? inputs.ToList();
        if (list.Count == 0)
        {
            return Array.Empty<byte[]>();
        }

        var results = new byte[list.Count][];
        for (int i = 0; i < list.Count; ++i)
        {
            results[i] = SampleEncodeAndScoreSerializedProto(list[i], options);
        }

        return results;
    }

    public IReadOnlyList<float> CalculateEntropy(IEnumerable<string> inputs, float alpha = 1.0f, int numThreads = 0)
    {
        if (inputs is null)
        {
            throw new ArgumentNullException(nameof(inputs));
        }

        return CalculateEntropyBatch(inputs, alpha, numThreads);
    }

    public IReadOnlyList<string> Normalize(IEnumerable<string> inputs)
    {
        if (inputs is null)
        {
            throw new ArgumentNullException(nameof(inputs));
        }

        var list = inputs as IList<string> ?? inputs.ToList();
        if (list.Count == 0)
        {
            return Array.Empty<string>();
        }

        var results = new string[list.Count];
        for (int i = 0; i < list.Count; ++i)
        {
            results[i] = Normalize(list[i]);
        }

        return results;
    }

    public IReadOnlyList<NormalizedText> NormalizeWithOffsets(IEnumerable<string> inputs)
    {
        if (inputs is null)
        {
            throw new ArgumentNullException(nameof(inputs));
        }

        var list = inputs as IList<string> ?? inputs.ToList();
        if (list.Count == 0)
        {
            return Array.Empty<NormalizedText>();
        }

        var results = new NormalizedText[list.Count];
        for (int i = 0; i < list.Count; ++i)
        {
            results[i] = NormalizeWithOffsets(list[i]);
        }

        return results;
    }

    public IReadOnlyList<string> Tokenize(string input, EncodeOptions? options = null)
    {
        return EncodePieces(input, options);
    }

    public string Detokenize(IEnumerable<string> pieces)
    {
        return DecodePieces(pieces);
    }

    private static EncodeOptions PrepareSamplingOptions(EncodeOptions? options, int nbestSize, float alpha)
    {
        var clone = CloneOptions(options);
        clone.EnableSampling = true;
        clone.NBestSize = nbestSize;
        clone.Alpha = alpha;
        return clone;
    }

    private static EncodeOptions CloneOptions(EncodeOptions? options)
    {
        if (options is null)
        {
            return new EncodeOptions();
        }

        return new EncodeOptions
        {
            AddBos = options.AddBos,
            AddEos = options.AddEos,
            Reverse = options.Reverse,
            EmitUnknownPiece = options.EmitUnknownPiece,
            EnableSampling = options.EnableSampling,
            NBestSize = options.NBestSize,
            Alpha = options.Alpha,
        };
    }
    }
}

