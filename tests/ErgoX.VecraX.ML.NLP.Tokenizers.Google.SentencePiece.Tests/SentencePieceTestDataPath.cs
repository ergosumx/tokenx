namespace ErgoX.VecraX.ML.NLP.Tokenizers.Google.SentencePiece.Tests;

using System;
using System.IO;

internal static class SentencePieceTestDataPath
{
    private const string SolutionFileName = "TokenX.sln";

    public static string GetSentencePieceRoot()
    {
        return Path.Combine(GetRepositoryRoot(), "tests", "_sentencepeice");
    }

    public static string GetTokenizationTemplatesRoot()
    {
        return Path.Combine(GetRepositoryRoot(), "tests", "__templates");
    }

    public static string GetManifestPath(string modelId)
    {
        if (string.IsNullOrWhiteSpace(modelId))
        {
            throw new ArgumentException("Model identifier must be provided.", nameof(modelId));
        }

        return Path.Combine(GetSentencePieceRoot(), modelId, "tokenx-tests-validation.json");
    }

    public static string GetModelPath(string modelId, string fileName)
    {
        if (string.IsNullOrWhiteSpace(modelId))
        {
            throw new ArgumentException("Model identifier must be provided.", nameof(modelId));
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("Model file name must be provided.", nameof(fileName));
        }

        return Path.Combine(GetSentencePieceRoot(), modelId, fileName);
    }

    private static string GetRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var solutionCandidate = Path.Combine(directory.FullName, SolutionFileName);
            if (File.Exists(solutionCandidate))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Unable to locate repository root from test context.");
    }
}
