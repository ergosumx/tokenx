namespace ErgoX.VecraX.ML.NLP.Tokenizers.Tiktoken;

using System;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Convenience helpers for constructing <see cref="TiktokenEncoding"/> instances.
/// </summary>
public static class TiktokenEncodingFactory
{
    public static TiktokenEncoding FromTiktokenFile(
        string name,
        string pattern,
        string tiktokenFilePath,
        IReadOnlyDictionary<string, int> specialTokens,
        uint? explicitVocabularySize = null)
    {
        if (!File.Exists(tiktokenFilePath))
        {
            throw new FileNotFoundException($"TikToken vocabulary file not found at '{tiktokenFilePath}'.", tiktokenFilePath);
        }

        using var stream = File.OpenRead(tiktokenFilePath);
        return FromTiktokenStream(name, pattern, stream, specialTokens, explicitVocabularySize);
    }

    public static TiktokenEncoding FromTiktokenStream(
        string name,
        string pattern,
        Stream mergeableRanksStream,
        IReadOnlyDictionary<string, int> specialTokens,
        uint? explicitVocabularySize = null)
    {
        var mergeableRanks = TiktokenBpeLoader.Load(mergeableRanksStream);
        return TiktokenEncoding.Create(name, pattern, mergeableRanks, specialTokens, explicitVocabularySize);
    }
}
