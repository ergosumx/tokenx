namespace ErgoX.TokenX.HuggingFace.Tests;

using System;
using System.IO;

internal static class TestDataPath
{
    private const string SolutionFileName = "TokenX.sln";

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

    public static string GetModelValidationManifestPath(string modelFolder)
    {
        var root = GetModelRoot(modelFolder);
        return Path.Combine(root, "tokenx-tests-validation.json");
    }

    public static string GetBenchmarksDataRoot()
    {
        return Path.Combine(GetRepositoryRoot(), "tests", "_huggingface");
    }

    public static string GetTokenizationTemplatesRoot()
    {
        return Path.Combine(GetRepositoryRoot(), "tests", "__templates");
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

