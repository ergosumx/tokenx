namespace ErgoX.VecraX.ML.NLP.Tokenizers.Tests;

using Xunit;

[Trait(TestTraits.Component, TestTraits.Components.Common)]
public abstract class CommonTestBase
{
}

[Trait(TestTraits.Component, TestTraits.Components.HuggingFace)]
public abstract class HuggingFaceTestBase
{
}

[Trait(TestTraits.Component, TestTraits.Components.SentencePiece)]
public abstract class SentencePieceTestBase
{
}

[Trait(TestTraits.Component, TestTraits.Components.Tiktoken)]
public abstract class TiktokenTestBase
{
}
