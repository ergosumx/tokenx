namespace ErgoX.VecraX.ML.NLP.Tokenizers.Google.SentencePiece.Tests.IntegrationTests.Templates;

using ErgoX.VecraX.ML.NLP.Tokenizers.Tests;

public sealed class SentencePieceOpenChatTemplateTests : SentencePieceTestBase, IClassFixture<SentencePieceModelFixture>
{
    private readonly SentencePieceModelFixture fixture;

    public SentencePieceOpenChatTemplateTests(SentencePieceModelFixture fixture)
    {
        this.fixture = fixture;
    }

    [Theory]
    [MemberData(nameof(SentencePieceTemplateTestUtilities.GetTemplateFileNames), MemberType = typeof(SentencePieceTemplateTestUtilities))]
    public void TokenizationMatchesPythonReference(string templateFileName)
    {
        SentencePieceTemplateTestUtilities.AssertTemplateCase(fixture.LlamaModel, "openchat-3.5-1210", templateFileName);
    }
}
