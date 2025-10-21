namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace;

using System;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Internal;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Internal.Interop;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Options;

/// <summary>
/// Represents a WordPiece model that can be attached to a tokenizer.
/// </summary>
public sealed class WordPieceModel : TokenizerModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WordPieceModel"/> class using default options.
    /// </summary>
    /// <param name="vocabPath">Path to the vocabulary file.</param>
    public WordPieceModel(string vocabPath)
        : this(vocabPath, WordPieceModelOptions.Default)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WordPieceModel"/> class.
    /// </summary>
    /// <param name="vocabPath">Path to the vocabulary file.</param>
    /// <param name="options">The model configuration options.</param>
    public WordPieceModel(string vocabPath, WordPieceModelOptions? options)
        : base(CreateHandle(vocabPath, options, out var interop), interop)
    {
    }

    private static NativeModelHandle CreateHandle(string vocabPath, WordPieceModelOptions? options, out INativeInterop interop)
    {
        interop = NativeInteropProvider.Current;
        ArgumentNullException.ThrowIfNull(interop);

        var resolvedOptions = options ?? WordPieceModelOptions.Default;
        return NativeModelHandle.CreateWordPiece(vocabPath, resolvedOptions, interop);
    }
}
