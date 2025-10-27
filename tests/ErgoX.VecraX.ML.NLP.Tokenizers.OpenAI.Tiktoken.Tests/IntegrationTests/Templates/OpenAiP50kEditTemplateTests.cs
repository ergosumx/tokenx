namespace ErgoX.VecraX.ML.NLP.Tokenizers.Tests.IntegrationTests.Tiktoken.Templates;

using ErgoX.VecraX.ML.NLP.Tokenizers.Tests;
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
