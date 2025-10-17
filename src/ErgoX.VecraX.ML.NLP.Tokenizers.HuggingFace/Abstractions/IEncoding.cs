using System.Collections.Generic;

namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Abstractions;

/// <summary>
/// Defines the contract for the result of a tokenization operation.
/// </summary>
public interface IEncoding
{
    /// <summary>
    /// Gets the token IDs produced by the tokenizer.
    /// </summary>
    IReadOnlyList<int> Ids { get; }

    /// <summary>
    /// Gets the string tokens associated with each ID.
    /// </summary>
    IReadOnlyList<string> Tokens { get; }

    /// <summary>
    /// Gets the character offsets (start, end) for each token in the original input.
    /// </summary>
    IReadOnlyList<(int Start, int End)> Offsets { get; }

    /// <summary>
    /// Gets the type IDs for each token (used for sequence pair classification tasks).
    /// Typically 0 for first sequence, 1 for second sequence.
    /// </summary>
    IReadOnlyList<uint> TypeIds { get; }

    /// <summary>
    /// Gets the attention mask for each token (1 for real tokens, 0 for padding).
    /// </summary>
    IReadOnlyList<uint> AttentionMask { get; }

    /// <summary>
    /// Gets a mask indicating which tokens are special tokens (1) vs regular tokens (0).
    /// </summary>
    IReadOnlyList<uint> SpecialTokensMask { get; }

    /// <summary>
    /// Gets the word IDs for each token, allowing you to map tokens back to words in the input.
    /// Null values indicate special tokens or tokens not associated with a word.
    /// </summary>
    IReadOnlyList<uint?> WordIds { get; }

    /// <summary>
    /// Gets the sequence IDs for each token (0 for first sequence, 1 for second sequence in pairs).
    /// Null values indicate special tokens not belonging to any sequence.
    /// </summary>
    IReadOnlyList<uint?> SequenceIds { get; }

    /// <summary>
    /// Gets the number of tokens in this encoding.
    /// </summary>
    int Length { get; }

    /// <summary>
    /// Gets any overflow encodings that were truncated from the main encoding.
    /// </summary>
    IReadOnlyList<IEncoding>? Overflowing { get; }
}
