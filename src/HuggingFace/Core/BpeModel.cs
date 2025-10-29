namespace ErgoX.TokenX.HuggingFace;

using System;
using ErgoX.TokenX.HuggingFace.Internal;
using ErgoX.TokenX.HuggingFace.Internal.Interop;
using ErgoX.TokenX.HuggingFace.Options;

/// <summary>
/// Represents a Byte-Pair Encoding (BPE) model that can be attached to a tokenizer.
/// </summary>
public sealed class BpeModel : TokenizerModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BpeModel"/> class using default options.
    /// </summary>
    /// <param name="vocabPath">Path to the vocabulary JSON file.</param>
    /// <param name="mergesPath">Path to the merges.txt file.</param>
    public BpeModel(string vocabPath, string mergesPath)
        : this(vocabPath, mergesPath, BpeModelOptions.Default)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BpeModel"/> class.
    /// </summary>
    /// <param name="vocabPath">Path to the vocabulary JSON file.</param>
    /// <param name="mergesPath">Path to the merges.txt file.</param>
    /// <param name="options">The model configuration options.</param>
    public BpeModel(string vocabPath, string mergesPath, BpeModelOptions? options)
        : base(CreateHandle(vocabPath, mergesPath, options, out var interop), interop)
    {
    }

    private static NativeModelHandle CreateHandle(string vocabPath, string mergesPath, BpeModelOptions? options, out INativeInterop interop)
    {
        interop = NativeInteropProvider.Current;
        ArgumentNullException.ThrowIfNull(interop);

        var resolvedOptions = options ?? BpeModelOptions.Default;
        return NativeModelHandle.CreateBpe(vocabPath, mergesPath, resolvedOptions, interop);
    }
}

