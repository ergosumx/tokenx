using System;

namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Abstractions;

/// <summary>
/// Defines the contract for a tokenizer model that handles the core tokenization algorithm.
/// </summary>
/// <remarks>
/// Models include BPE (Byte-Pair Encoding), WordPiece, Unigram, and WordLevel.
/// Each model implements a specific algorithm for breaking text into tokens.
/// </remarks>
public interface IModel : IDisposable
{
    /// <summary>
    /// Gets the native handle to the underlying model implementation.
    /// </summary>
    IntPtr Handle { get; }
}
