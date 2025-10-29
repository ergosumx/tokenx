namespace ErgoX.TokenX.HuggingFace;

using System;
using ErgoX.TokenX.HuggingFace.Internal;
using ErgoX.TokenX.HuggingFace.Internal.Interop;
using ErgoX.TokenX.HuggingFace.Options;

/// <summary>
/// Represents a SentencePiece-compatible Unigram model.
/// </summary>
public sealed class UnigramModel : TokenizerModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnigramModel"/> class using default options.
    /// </summary>
    /// <param name="modelPath">Path to the SentencePiece model file.</param>
    public UnigramModel(string modelPath)
        : this(modelPath, UnigramModelOptions.Default)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnigramModel"/> class.
    /// </summary>
    /// <param name="modelPath">Path to the SentencePiece model file.</param>
    /// <param name="options">The model configuration options.</param>
    public UnigramModel(string modelPath, UnigramModelOptions? options)
        : base(CreateHandle(modelPath, options, out var interop), interop)
    {
    }

    private static NativeModelHandle CreateHandle(string modelPath, UnigramModelOptions? options, out INativeInterop interop)
    {
        interop = NativeInteropProvider.Current;
        ArgumentNullException.ThrowIfNull(interop);

        var resolvedOptions = options ?? UnigramModelOptions.Default;
        return NativeModelHandle.CreateUnigram(modelPath, resolvedOptions, interop);
    }
}

