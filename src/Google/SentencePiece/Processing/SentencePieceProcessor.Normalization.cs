namespace ErgoX.VecraX.ML.NLP.Tokenizers.Google.SentencePiece.Processing;

using System;
using System.Collections.Generic;
using ErgoX.VecraX.ML.NLP.Tokenizers.Google.SentencePiece.Internal.Interop;
using ErgoX.VecraX.ML.NLP.Tokenizers.Google.SentencePiece.Models;

/// <summary>
/// Partial class containing text normalization methods for the <see cref="SentencePieceProcessor"/>.
/// </summary>
public sealed partial class SentencePieceProcessor
{
    public string Normalize(string input)
    {
        ThrowIfDisposed();
        using var text = new InteropUtilities.NativeUtf8(input);
        var status = NativeMethods.spc_sentencepiece_processor_normalize(handle, text.View, out var normalized);
        InteropUtilities.EnsureSuccess(status);
        return InteropUtilities.BytesToStringAndDestroy(ref normalized);
    }

    public NormalizedText NormalizeWithOffsets(string input)
    {
        ThrowIfDisposed();
        using var text = new InteropUtilities.NativeUtf8(input);
        var status = NativeMethods.spc_sentencepiece_processor_normalize_with_offsets(handle, text.View, out var normalized);
        InteropUtilities.EnsureSuccess(status);
        var result = InteropUtilities.NormalizedResultToManagedAndDestroy(ref normalized);
        return new NormalizedText(result.Text, result.Offsets);
    }

    public float CalculateEntropy(string input, float alpha = 1.0f)
    {
        ThrowIfDisposed();
        using var text = new InteropUtilities.NativeUtf8(input);
        var status = NativeMethods.spc_sentencepiece_processor_calculate_entropy(handle, text.View, alpha, out var value);
        InteropUtilities.EnsureSuccess(status);
        return value;
    }

    public IReadOnlyList<float> CalculateEntropyBatch(IEnumerable<string> inputs, float alpha = 1.0f, int numThreads = 0)
    {
        ThrowIfDisposed();
        using var nativeInputs = new InteropUtilities.NativeUtf8Array(inputs ?? Array.Empty<string>());
        var status = NativeMethods.spc_sentencepiece_processor_calculate_entropy_batch(handle, nativeInputs.Pointer, nativeInputs.Length, alpha, numThreads, out var array);
        InteropUtilities.EnsureSuccess(status);
        return InteropUtilities.FloatArrayToManagedAndDestroy(ref array);
    }

    public void OverrideNormalizerSpec(IEnumerable<KeyValuePair<string, string>> replacements)
    {
        ThrowIfDisposed();
        using var entries = new InteropUtilities.NativeMapEntries(replacements ?? Array.Empty<KeyValuePair<string, string>>());
        var status = NativeMethods.spc_sentencepiece_processor_override_normalizer_spec(handle, entries.Pointer, entries.Length);
        InteropUtilities.EnsureSuccess(status);
    }
}
