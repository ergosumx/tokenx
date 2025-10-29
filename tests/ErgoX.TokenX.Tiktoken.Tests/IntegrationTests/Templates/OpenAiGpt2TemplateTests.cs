namespace ErgoX.TokenX.Tests.IntegrationTests.Tiktoken.Templates;

using ErgoX.TokenX.Tests;
using Xunit;

public sealed class OpenAiGpt2TemplateTests : TiktokenTestBase
{
    private const string EncodingFolder = "openai-gpt2";

    [Theory]
    [MemberData(nameof(TiktokenTemplateTestUtilities.GetTemplateFileNames), MemberType = typeof(TiktokenTemplateTestUtilities))]
    public void TokenizationMatchesPythonReference(string templateFileName)
    {
        TiktokenTemplateTestUtilities.AssertTemplateCase(EncodingFolder, templateFileName);
    }
}

