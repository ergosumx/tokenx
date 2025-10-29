namespace ErgoX.TokenX.Tests.IntegrationTests.Tiktoken.Templates;

using ErgoX.TokenX.Tests;
using Xunit;

public sealed class OpenAiP50kEditTemplateTests : TiktokenTestBase
{
    private const string EncodingFolder = "openai-p50k_edit";

    [Theory]
    [MemberData(nameof(TiktokenTemplateTestUtilities.GetTemplateFileNames), MemberType = typeof(TiktokenTemplateTestUtilities))]
    public void TokenizationMatchesPythonReference(string templateFileName)
    {
        TiktokenTemplateTestUtilities.AssertTemplateCase(EncodingFolder, templateFileName);
    }
}

