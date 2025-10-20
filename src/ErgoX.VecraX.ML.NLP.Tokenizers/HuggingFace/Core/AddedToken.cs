using System;

namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace;

public sealed class AddedToken
{
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

    public string Content { get; }

    public bool IsSpecial { get; }

    public bool SingleWord { get; }

    public bool LeftStrip { get; }

    public bool RightStrip { get; }

    public bool Normalized { get; }

    public AddedToken WithSpecial(bool value)
        => new(Content, value, SingleWord, LeftStrip, RightStrip, Normalized);
}
