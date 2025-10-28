namespace ErgoX.VecraX.ML.NLP.Tokenizers.Tests;

using System;
using System.IO;

internal static class TestDataPath
{
    private const string SolutionFileName = "TokenX.sln";

    public static string GetEncodingRoot(string encodingFolder)
    {
        if (string.IsNullOrWhiteSpace(encodingFolder))
        {
            throw new ArgumentException("Encoding folder must be provided.", nameof(encodingFolder));
        }

        var root = GetTiktokenDataRoot();
        return Path.Combine(root, encodingFolder);
    }

    public static string GetMergeableRanksPath(string encodingFolder)
    {
        var root = GetEncodingRoot(encodingFolder);
        return Path.Combine(root, "mergeable_ranks.tiktoken");
    }

    public static string GetValidationManifestPath(string encodingFolder)
    {
        var root = GetEncodingRoot(encodingFolder);
        return Path.Combine(root, "tokenx-tests-validation.json");
    }

    public static string GetTokenizationTemplatesRoot()
    {
        return Path.Combine(GetRepositoryRoot(), "tests", "__templates");
    }

    private static string GetTiktokenDataRoot()
    {
        return Path.Combine(GetRepositoryRoot(), "tests", "_tiktoken");
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
