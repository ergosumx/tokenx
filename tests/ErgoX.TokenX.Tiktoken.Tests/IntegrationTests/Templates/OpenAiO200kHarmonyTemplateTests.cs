namespace ErgoX.TokenX.Tests.IntegrationTests.Tiktoken.Templates;

using ErgoX.TokenX.Tests;
using Xunit;

public sealed class OpenAiO200kHarmonyTemplateTests : TiktokenTestBase
{
    private const string EncodingFolder = "openai-o200k_harmony";

    [Theory]
    [MemberData(nameof(TiktokenTemplateTestUtilities.GetTemplateFileNames), MemberType = typeof(TiktokenTemplateTestUtilities))]
    public void TokenizationMatchesPythonReference(string templateFileName)
    {
        TiktokenTemplateTestUtilities.AssertTemplateCase(EncodingFolder, templateFileName);
    }
}

