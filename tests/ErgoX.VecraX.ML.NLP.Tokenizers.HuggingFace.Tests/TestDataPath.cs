namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests;

using System;
using System.IO;

internal static class TestDataPath
{
    private const string SolutionFileName = "TokenX.HF.sln";

    public static string GetModelRoot(string modelFolder)
    {
        if (string.IsNullOrWhiteSpace(modelFolder))
        {
            throw new ArgumentException("Model folder must be provided.", nameof(modelFolder));
        }

        var root = GetBenchmarksDataRoot();
        return Path.Combine(root, modelFolder);
    }

    public static string GetModelTokenizerPath(string modelFolder)
    {
        var root = GetModelRoot(modelFolder);
        return Path.Combine(root, "tokenizer.json");
    }

    public static string GetBenchmarksDataRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var solutionCandidate = Path.Combine(directory.FullName, SolutionFileName);
            if (File.Exists(solutionCandidate))
            {
                return Path.Combine(directory.FullName, "tests", "_TestData");
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Unable to locate repository root from test context.");
    }
}
