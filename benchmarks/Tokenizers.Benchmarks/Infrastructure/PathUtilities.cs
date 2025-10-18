namespace Tokenizers.Benchmarks.Infrastructure;

using System;
using System.IO;

internal static class PathUtilities
{
    private const string SolutionFileName = "TokenX.HF.sln";

    public static string GetSolutionRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var solutionPath = Path.Combine(directory.FullName, SolutionFileName);
            if (File.Exists(solutionPath))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException($"Unable to locate {SolutionFileName} starting from {AppContext.BaseDirectory}.");
    }

    public static string GetBenchmarkAssetsRoot()
    {
        var root = GetSolutionRoot();
        var assets = Path.Combine(root, "Benchmarks", "data");
        Directory.CreateDirectory(assets);
        return assets;
    }
}
