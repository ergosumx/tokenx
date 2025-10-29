namespace ErgoX.TokenX.HuggingFace.Tests.Integration.Generation;

using ErgoX.TokenX.HuggingFace.Tests;
using ErgoX.TokenX.Tests;
using Xunit;

[Trait(TestCategories.Category, TestCategories.Integration)]
[Trait(TestCategories.Filter, TestCategories.Integration)]
public sealed class Qwen1p57bChatGenerationTests
{
    private const string ModelFolder = "qwen1p5-7b-chat";

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

