namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests.Integration.Generation;

using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests;
using ErgoX.VecraX.ML.NLP.Tokenizers.Tests;
using Xunit;

[Trait(TestCategories.Category, TestCategories.Integration)]
[Trait(TestCategories.Filter, TestCategories.Integration)]
public sealed class Gemma22bItGenerationTests
{
    private const string ModelFolder = "gemma-2-2b-it";

    [Fact]
    public void UserInitiatedComplianceReview()
    {
        GenerationTestUtilities.AssertChatTemplateCase(ModelFolder, "User initiated compliance review");
    }

    [Fact]
    public void AssistantProvidesDirectStatusUpdate()
    {
        GenerationTestUtilities.AssertChatTemplateCase(ModelFolder, "Assistant provides direct status update");
    }
}
