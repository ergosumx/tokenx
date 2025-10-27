namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests.Integration.Generation;

using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests;
using ErgoX.VecraX.ML.NLP.Tokenizers.Tests;
using Xunit;

[Trait(TestCategories.Category, TestCategories.Integration)]
[Trait(TestCategories.Filter, TestCategories.Integration)]
public sealed class Mixtral8x7bInstructV01GenerationTests
{
    private const string ModelFolder = "mixtral-8x7b-instruct-v0_1";

    [Fact]
    public void SystemPrimingWithFollowUpRequest()
    {
        GenerationTestUtilities.AssertChatTemplateCase(ModelFolder, "System priming with follow-up request");
    }

    [Fact]
    public void AssistantFinalizesRemediationGuidance()
    {
        GenerationTestUtilities.AssertChatTemplateCase(ModelFolder, "Assistant finalizes remediation guidance");
    }
}
