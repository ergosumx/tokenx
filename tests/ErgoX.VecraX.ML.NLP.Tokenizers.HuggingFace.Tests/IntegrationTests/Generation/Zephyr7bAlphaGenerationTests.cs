namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests.Integration.Generation;

using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests;
using ErgoX.VecraX.ML.NLP.Tokenizers.Tests;
using Xunit;

[Trait(TestCategories.Category, TestCategories.Integration)]
[Trait(TestCategories.Filter, TestCategories.Integration)]
public sealed class Zephyr7bAlphaGenerationTests
{
    private const string ModelFolder = "zephyr-7b-alpha";

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
