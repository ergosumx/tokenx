using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace;

/// <summary>
/// Represents the output of a tokenization operation.
/// </summary>
public sealed partial class EncodingResult
{
    /// <summary>
    /// Gets the token IDs produced by the tokenizer.
    /// </summary>
    public IReadOnlyList<int> Ids { get; }

    /// <summary>
    /// Gets the string tokens associated with each ID.
    /// </summary>
    public IReadOnlyList<string> Tokens { get; }

    /// <summary>
    /// Gets the character offsets (start, end) for each token in the original input.
    /// </summary>
    public IReadOnlyList<(int Start, int End)> Offsets { get; }

    /// <summary>
    /// Gets the type IDs for each token (used for sequence pair classification tasks).
    /// Typically 0 for first sequence, 1 for second sequence.
    /// </summary>
    public IReadOnlyList<uint> TypeIds { get; }

    /// <summary>
    /// Gets the attention mask for each token.
    /// Values are typically 1 for real tokens and 0 for padding tokens.
    /// </summary>
    public IReadOnlyList<uint> AttentionMask { get; }

    /// <summary>
    /// Gets the special tokens mask identifying which tokens are special tokens.
    /// Values are 1 for special tokens (like [CLS], [SEP]) and 0 for regular tokens.
    /// </summary>
    public IReadOnlyList<uint> SpecialTokensMask { get; }

    /// <summary>
    /// Gets the word indices for each token, indicating which word in the input each token belongs to.
    /// Null values indicate tokens that don't belong to any word (e.g., special tokens).
    /// </summary>
    public IReadOnlyList<int?> WordIds { get; }

    /// <summary>
    /// Gets the sequence indices for each token when encoding sequence pairs.
    /// Null values indicate tokens that don't belong to any sequence.
    /// </summary>
    public IReadOnlyList<int?> SequenceIds { get; }

    /// <summary>
    /// Gets the list of overflowing encodings generated when the input was truncated.
    /// Empty if no truncation occurred.
    /// </summary>
    public IReadOnlyList<EncodingResult> Overflowing { get; }

    /// <summary>
    /// Gets the number of tokens in this encoding.
    /// </summary>
    public int Length => Ids.Count;

    /// <summary>
    /// Initializes a new instance of the <see cref="EncodingResult"/> class with basic properties (legacy constructor).
    /// </summary>
    /// <param name="ids">The token IDs.</param>
    /// <param name="tokens">The string tokens.</param>
    /// <param name="offsets">The character offsets.</param>
    public EncodingResult(IReadOnlyList<int> ids, IReadOnlyList<string> tokens, IReadOnlyList<(int Start, int End)> offsets)
    {
        Ids = new ReadOnlyCollection<int>(ids.ToArray());
        Tokens = new ReadOnlyCollection<string>(tokens.ToArray());
        Offsets = new ReadOnlyCollection<(int Start, int End)>(offsets.ToArray());
        TypeIds = new ReadOnlyCollection<uint>(Array.Empty<uint>());
        AttentionMask = new ReadOnlyCollection<uint>(Array.Empty<uint>());
        SpecialTokensMask = new ReadOnlyCollection<uint>(Array.Empty<uint>());
        WordIds = new ReadOnlyCollection<int?>(Array.Empty<int?>());
        SequenceIds = new ReadOnlyCollection<int?>(Array.Empty<int?>());
        Overflowing = new ReadOnlyCollection<EncodingResult>(Array.Empty<EncodingResult>());
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EncodingResult"/> class with all properties.
    /// </summary>
    internal EncodingResult(
        IReadOnlyList<int> ids,
        IReadOnlyList<string> tokens,
        IReadOnlyList<(int Start, int End)> offsets,
        IReadOnlyList<uint> typeIds,
        IReadOnlyList<uint> attentionMask,
        IReadOnlyList<uint> specialTokensMask,
        IReadOnlyList<int?> wordIds,
        IReadOnlyList<int?> sequenceIds,
        IReadOnlyList<EncodingResult> overflowing)
    {
        Ids = new ReadOnlyCollection<int>(ids.ToArray());
        Tokens = new ReadOnlyCollection<string>(tokens.ToArray());
        Offsets = new ReadOnlyCollection<(int Start, int End)>(offsets.ToArray());
        TypeIds = new ReadOnlyCollection<uint>(typeIds.ToArray());
        AttentionMask = new ReadOnlyCollection<uint>(attentionMask.ToArray());
        SpecialTokensMask = new ReadOnlyCollection<uint>(specialTokensMask.ToArray());
        WordIds = new ReadOnlyCollection<int?>(wordIds.ToArray());
        SequenceIds = new ReadOnlyCollection<int?>(sequenceIds.ToArray());
        Overflowing = new ReadOnlyCollection<EncodingResult>(overflowing.ToArray());
    }

    public EncodingResult WithPadding(int targetLength, int padId, string padToken)
    {
        if (Length >= targetLength)
        {
            return this;
        }

        var paddedIds = new List<int>(Ids);
        var paddedTokens = new List<string>(Tokens);
        var paddedOffsets = new List<(int Start, int End)>(Offsets);

        while (paddedIds.Count < targetLength)
        {
            paddedIds.Add(padId);
            paddedTokens.Add(padToken);
            paddedOffsets.Add((0, 0));
        }

        return new EncodingResult(paddedIds, paddedTokens, paddedOffsets);
    }

    public EncodingResult WithLeftPadding(int targetLength, int padId, string padToken)
    {
        if (Length >= targetLength)
        {
            return this;
        }

        var padCount = targetLength - Length;
        var paddedIds = new List<int>(targetLength);
        var paddedTokens = new List<string>(targetLength);
        var paddedOffsets = new List<(int Start, int End)>(targetLength);

        for (var i = 0; i < padCount; i++)
        {
            paddedIds.Add(padId);
            paddedTokens.Add(padToken);
            paddedOffsets.Add((0, 0));
        }

        paddedIds.AddRange(Ids);
        paddedTokens.AddRange(Tokens);
        paddedOffsets.AddRange(Offsets);

        return new EncodingResult(paddedIds, paddedTokens, paddedOffsets);
    }
}
