using System;
using System.Net.Http;

namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Options;

public sealed class PretrainedTokenizerOptions
{
    public string Revision { get; set; } = "main";

    public string? AuthToken { get; set; }

    public HttpClient? HttpClient { get; set; }

    internal void Validate()
    {
        if (string.IsNullOrWhiteSpace(Revision))
        {
            throw new InvalidOperationException("Revision cannot be null or whitespace.");
        }
    }
}
