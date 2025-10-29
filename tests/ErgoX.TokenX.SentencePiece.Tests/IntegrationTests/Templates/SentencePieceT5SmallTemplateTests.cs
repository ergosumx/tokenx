namespace ErgoX.VecraX.ML.NLP.Tokenizers.Google.SentencePiece.Tests.IntegrationTests.Templates;

using ErgoX.VecraX.ML.NLP.Tokenizers.Tests;

public sealed class SentencePieceT5SmallTemplateTests : SentencePieceTestBase, IClassFixture<SentencePieceModelFixture>
{
    private readonly SentencePieceModelFixture fixture;

    public SentencePieceT5SmallTemplateTests(SentencePieceModelFixture fixture)
    {
        this.fixture = fixture;
    }

    [Theory]
    [MemberData(nameof(SentencePieceTemplateTestUtilities.GetTemplateFileNames), MemberType = typeof(SentencePieceTemplateTestUtilities))]
    public void TokenizationMatchesPythonReference(string templateFileName)
    {
        SentencePieceTemplateTestUtilities.AssertTemplateCase(fixture.T5SmallModel, "t5-small", templateFileName);
    }
}
