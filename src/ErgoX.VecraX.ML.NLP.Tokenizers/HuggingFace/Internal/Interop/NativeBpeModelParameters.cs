namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Internal.Interop;

internal struct NativeBpeModelParameters
{
    internal NativeBpeModelParameters(string vocabPath, string mergesPath)
    {
        VocabPath = vocabPath;
        MergesPath = mergesPath;
        Dropout = 0f;
        HasDropout = false;
        UnknownToken = null;
        ContinuingSubwordPrefix = null;
        EndOfWordSuffix = null;
        FuseUnknown = false;
        EnableByteFallback = false;
    }

    internal string VocabPath { get; }

    internal string MergesPath { get; }

    internal float Dropout { get; set; }

    internal bool HasDropout { get; set; }

    internal string? UnknownToken { get; set; }

    internal string? ContinuingSubwordPrefix { get; set; }

    internal string? EndOfWordSuffix { get; set; }

    internal bool FuseUnknown { get; set; }

    internal bool EnableByteFallback { get; set; }
}
