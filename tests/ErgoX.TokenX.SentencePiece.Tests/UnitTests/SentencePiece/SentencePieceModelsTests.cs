namespace ErgoX.TokenX.SentencePiece.Tests.Unit;

using System.Collections.Generic;
using ErgoX.TokenX.SentencePiece.Models;
using ErgoX.TokenX.HuggingFace.Tests;
using Xunit;

[Trait(TestCategories.Category, TestCategories.Unit)]
[Trait(TestCategories.Filter, TestCategories.Unit)]
public sealed class SentencePieceModelsTests
{
    [Fact]
    public void NormalizedText_PreservesMembers()
    {
        var normalized = new NormalizedText("text", new[] { 0, 1, 2 });

        Assert.Equal("text", normalized.Text);
        Assert.Equal(new[] { 0, 1, 2 }, normalized.Offsets);
    }

    [Fact]
    public void ScoredIdSequence_EqualityReflectsMembers()
    {
        IReadOnlyList<int> ids = new[] { 1, 2, 3 };
        var left = new ScoredIdSequence(ids, 1.5f);
        var right = new ScoredIdSequence(ids, 1.5f);

        Assert.Equal(left, right);
        Assert.Equal(left.GetHashCode(), right.GetHashCode());
    }

    [Fact]
    public void ScoredPieceSequence_EqualityReflectsMembers()
    {
        IReadOnlyList<string> pieces = new[] { "▁Hello", "world" };
        var left = new ScoredPieceSequence(pieces, 0.75f);
        var right = new ScoredPieceSequence(pieces, 0.75f);

        Assert.Equal(left, right);
        Assert.Equal(left.GetHashCode(), right.GetHashCode());
    }
}

