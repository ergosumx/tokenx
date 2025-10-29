namespace ErgoX.TokenX.Tests.IntegrationTests.Tiktoken.Templates;

using ErgoX.TokenX.Tests;
using Xunit;

public sealed class OpenAiO200kBaseTemplateTests : TiktokenTestBase
{
    private const string EncodingFolder = "openai-o200k_base";

    [Theory]
    [MemberData(nameof(TiktokenTemplateTestUtilities.GetTemplateFileNames), MemberType = typeof(TiktokenTemplateTestUtilities))]
    public void TokenizationMatchesPythonReference(string templateFileName)
    {
        TiktokenTemplateTestUtilities.AssertTemplateCase(EncodingFolder, templateFileName);
    }
}

