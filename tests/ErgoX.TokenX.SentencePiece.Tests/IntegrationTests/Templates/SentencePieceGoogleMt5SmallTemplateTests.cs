namespace ErgoX.TokenX.SentencePiece.Tests.IntegrationTests.Templates;

using ErgoX.TokenX.Tests;

public sealed class SentencePieceGoogleMt5SmallTemplateTests : SentencePieceTestBase, IClassFixture<SentencePieceModelFixture>
{
    private readonly SentencePieceModelFixture fixture;

    public SentencePieceGoogleMt5SmallTemplateTests(SentencePieceModelFixture fixture)
    {
        this.fixture = fixture;
    }

    [Theory]
    [MemberData(nameof(SentencePieceTemplateTestUtilities.GetTemplateFileNames), MemberType = typeof(SentencePieceTemplateTestUtilities))]
    public void TokenizationMatchesPythonReference(string templateFileName)
    {
        SentencePieceTemplateTestUtilities.AssertTemplateCase(fixture.Mt5SmallModel, "google-mt5-small", templateFileName);
    }
}

