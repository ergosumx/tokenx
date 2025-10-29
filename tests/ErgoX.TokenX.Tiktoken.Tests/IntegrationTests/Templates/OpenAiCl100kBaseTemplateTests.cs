namespace ErgoX.TokenX.Tests.IntegrationTests.Tiktoken.Templates;

using ErgoX.TokenX.Tests;
using Xunit;

public sealed class OpenAiCl100kBaseTemplateTests : TiktokenTestBase
{
    private const string EncodingFolder = "openai-cl100k_base";

    [Theory]
    [MemberData(nameof(TiktokenTemplateTestUtilities.GetTemplateFileNames), MemberType = typeof(TiktokenTemplateTestUtilities))]
    public void TokenizationMatchesPythonReference(string templateFileName)
    {
        TiktokenTemplateTestUtilities.AssertTemplateCase(EncodingFolder, templateFileName);
    }
}

