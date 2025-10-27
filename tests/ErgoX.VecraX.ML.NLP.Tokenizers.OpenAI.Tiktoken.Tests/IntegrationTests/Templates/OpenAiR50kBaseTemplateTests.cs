namespace ErgoX.VecraX.ML.NLP.Tokenizers.Tests.IntegrationTests.Tiktoken.Templates;

using ErgoX.VecraX.ML.NLP.Tokenizers.Tests;
using Xunit;

public sealed class OpenAiR50kBaseTemplateTests : TiktokenTestBase
{
    private const string EncodingFolder = "openai-r50k_base";

    [Theory]
    [MemberData(nameof(TiktokenTemplateTestUtilities.GetTemplateFileNames), MemberType = typeof(TiktokenTemplateTestUtilities))]
    public void TokenizationMatchesPythonReference(string templateFileName)
    {
        TiktokenTemplateTestUtilities.AssertTemplateCase(EncodingFolder, templateFileName);
    }
}
