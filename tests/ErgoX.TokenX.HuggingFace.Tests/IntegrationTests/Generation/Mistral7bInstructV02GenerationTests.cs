namespace ErgoX.TokenX.HuggingFace.Tests.Integration.Generation;

using ErgoX.TokenX.HuggingFace.Tests;
using ErgoX.TokenX.Tests;
using Xunit;

[Trait(TestCategories.Category, TestCategories.Integration)]
[Trait(TestCategories.Filter, TestCategories.Integration)]
public sealed class Mistral7bInstructV02GenerationTests
{
    private const string ModelFolder = "mistral-7b-instruct-v0_2";

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

