using System;

namespace ErgoX.TokenX.HuggingFace;

/// <summary>
/// Represents a token that is added to a tokenizer's vocabulary beyond the base model.
/// </summary>
/// <remarks>
/// Added tokens allow extending a tokenizer with custom tokens, special tokens, or domain-specific vocabulary.
/// Properties control how the token is matched and normalized during tokenization.
/// </remarks>
public sealed class AddedToken
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddedToken"/> class.
    /// </summary>
    /// <param name="content">The token string (e.g., "[DOMAIN]", "##suffix").</param>
    /// <param name="isSpecial">If <c>true</c>, the token is treated as special and not normalized.</param>
    /// <param name="singleWord">If <c>true</c>, the token only matches as a complete word, not as a subword.</param>
    /// <param name="leftStrip">If <c>true</c>, leading whitespace is stripped when matching this token.</param>
    /// <param name="rightStrip">If <c>true</c>, trailing whitespace is stripped when matching this token.</param>
    /// <param name="normalized">If <c>true</c>, the token content is normalized; if <c>null</c>, defaults based on <paramref name="isSpecial"/>.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="content"/> is null, empty, or whitespace-only.</exception>
    public AddedToken(
        string content,
        bool isSpecial = false,
        bool singleWord = false,
        bool leftStrip = false,
        bool rightStrip = false,
        bool? normalized = null)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Token content must be provided.", nameof(content));
        }

        Content = content;
        IsSpecial = isSpecial;
        SingleWord = singleWord;
        LeftStrip = leftStrip;
        RightStrip = rightStrip;
        Normalized = normalized ?? !isSpecial;
    }

    /// <summary>
    /// Gets the token string content.
    /// </summary>
    public string Content { get; }

    /// <summary>
    /// Gets a value indicating whether this token is treated as special (not normalized during tokenization).
    /// </summary>
    public bool IsSpecial { get; }

    /// <summary>
    /// Gets a value indicating whether this token only matches as a complete word, not as a subword.
    /// </summary>
    public bool SingleWord { get; }

    /// <summary>
    /// Gets a value indicating whether leading whitespace should be stripped when matching this token.
    /// </summary>
    public bool LeftStrip { get; }

    /// <summary>
    /// Gets a value indicating whether trailing whitespace should be stripped when matching this token.
    /// </summary>
    public bool RightStrip { get; }

    /// <summary>
    /// Gets a value indicating whether the token content should be normalized during matching.
    /// </summary>
    public bool Normalized { get; }

    /// <summary>
    /// Creates a new <see cref="AddedToken"/> with the <see cref="IsSpecial"/> property modified.
    /// </summary>
    /// <param name="value">The new value for <see cref="IsSpecial"/>.</param>
    /// <returns>A new <see cref="AddedToken"/> instance with the updated property.</returns>
    public AddedToken WithSpecial(bool value)
        => new(Content, value, SingleWord, LeftStrip, RightStrip, Normalized);
}

