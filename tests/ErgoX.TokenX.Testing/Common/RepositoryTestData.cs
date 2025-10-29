namespace ErgoX.TokenX.Tests;

using System;
using System.IO;

public static class RepositoryTestData
{
    private const string SolutionFileName = "TokenX.sln";

    public static string GetRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var solutionCandidate = Path.Combine(directory.FullName, SolutionFileName);
            if (File.Exists(solutionCandidate))
            {
                return Path.Combine(directory.FullName, "tests", "_testdata_sentencepeice");
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Unable to locate repository root from test context.");
    }

    public static string GetPath(params string[] segments)
    {
        if (segments is null)
        {
            throw new ArgumentNullException(nameof(segments));
        }

        if (segments.Length == 0)
        {
            return GetRoot();
        }

        var pathSegments = new string[segments.Length + 1];
        pathSegments[0] = GetRoot();
        Array.Copy(segments, 0, pathSegments, 1, segments.Length);
        return Path.Combine(pathSegments);
    }
}

