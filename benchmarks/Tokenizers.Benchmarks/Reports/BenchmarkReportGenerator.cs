namespace Tokenizers.Benchmarks.Reports;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Tokenizers.Benchmarks.Infrastructure;

internal static class BenchmarkReportGenerator
{
    private const string DefaultReportFileName = "TokenizersBenchmarkReport.md";
    private static readonly Regex MeanExtractor = new("(?<value>[0-9.,]+)\\s*(?<unit>ns|us|ms|s)", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    public static void Generate(string? outputPath)
    {
        var artifactsRoot = ResolveArtifactsRoot();

    var csvFiles = Directory.GetFiles(artifactsRoot, "Tokenizers.Benchmarks.Benchmarks.*-report.csv", SearchOption.TopDirectoryOnly);
        if (csvFiles.Length == 0)
        {
            throw new InvalidOperationException("No benchmark CSV reports were found. Run the benchmarks before generating a report.");
        }

        var entries = csvFiles
            .SelectMany(ParseCsv)
            .Where(entry => entry.Operation is not null)
            .ToList();

        if (entries.Count == 0)
        {
            throw new InvalidOperationException("No entries could be extracted from the benchmark CSV reports.");
        }

        var reportBuilder = new StringBuilder();
        reportBuilder.AppendLine("# Tokenizer Benchmark Comparison");
        reportBuilder.AppendLine();
        reportBuilder.AppendLine($"Generated on {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        reportBuilder.AppendLine();
        reportBuilder.AppendLine("This report compares the HuggingFace and Microsoft.ML tokenizers across multiple workloads. Metrics reflect the latest BenchmarkDotNet run using the extended configuration.");
        reportBuilder.AppendLine();

        foreach (var group in entries
                     .Where(e => e.Operation is not null)
                     .GroupBy(e => new OperationCaseKey(e.Benchmark, e.Operation!, e.Case))
                     .OrderBy(g => g.Key.Benchmark, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(g => g.Key.Operation, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(g => g.Key.Case, StringComparer.OrdinalIgnoreCase))
        {
            AppendGroup(reportBuilder, group.Key.Benchmark, group.Key.Operation, group.Key.Case, group.ToList());
        }

        var destination = ResolveOutputPath(outputPath);
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        File.WriteAllText(destination, reportBuilder.ToString());
    }

    private static void AppendGroup(StringBuilder builder, string benchmark, string operation, string caseDescription, IReadOnlyCollection<BenchmarkEntry> entries)
    {
        builder.AppendLine($"## {benchmark} — {operation} — {caseDescription}");
        builder.AppendLine();
        builder.AppendLine("| Implementation | Mean (ms) | StdDev (ms) | Relative | Graph |");
        builder.AppendLine("| -------------- | --------: | ----------: | -------: | :---- |");

        var maxMean = entries.Max(e => e.MeanMilliseconds);

        foreach (var entry in entries.OrderBy(e => e.MeanMilliseconds))
        {
            var relative = entry.MeanMilliseconds / maxMean;
            var bar = BuildBar(relative);
            builder.AppendLine($"| {entry.Implementation} | {entry.MeanMilliseconds:F3} | {entry.StdDevMilliseconds:F3} | {relative,7:P1} | {bar} |");
        }

        builder.AppendLine();
    }

    private static string BuildBar(double relative)
    {
        const int barWidth = 20;
        var filledSegments = Math.Clamp((int)Math.Round(relative * barWidth), 1, barWidth);
        return new string('█', filledSegments).PadRight(barWidth, '░');
    }

    private static string ResolveOutputPath(string? outputPath)
    {
        if (string.IsNullOrWhiteSpace(outputPath))
        {
            return Path.Combine(PathUtilities.GetBenchmarkAssetsRoot(), "reports", DefaultReportFileName);
        }

        return Path.IsPathRooted(outputPath)
            ? outputPath
            : Path.Combine(PathUtilities.GetSolutionRoot(), outputPath);
    }

    private static string ResolveArtifactsRoot()
    {
        // Try the project-local artifacts folder first to support custom artifact paths used during development.
        var candidates = new[]
        {
            Path.Combine(PathUtilities.GetSolutionRoot(), "benchmarks", "Tokenizers.Benchmarks", "BenchmarkDotNet.Artifacts", "results"),
            Path.Combine(PathUtilities.GetSolutionRoot(), "BenchmarkDotNet.Artifacts", "results"),
        };

        foreach (var candidate in candidates)
        {
            if (Directory.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new InvalidOperationException($"Benchmark results directory not found. Checked:{Environment.NewLine}{string.Join(Environment.NewLine, candidates)}");
    }

    private static IEnumerable<BenchmarkEntry> ParseCsv(string csvPath)
    {
        using var reader = new StreamReader(csvPath);
        string? headerLine = reader.ReadLine();
        if (headerLine is null)
        {
            yield break;
        }

        var headers = headerLine.Split(',');
        var indexes = new CsvIndexes(headers);
        var benchmarkName = GetBenchmarkName(csvPath);

        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var columns = SplitCsvLine(line);
            if (columns.Count != headers.Length)
            {
                continue;
            }

            var method = columns[indexes.Method];
            var operation = GetOperation(method);
            var implementation = GetImplementation(method);
            var mean = ParseDuration(columns[indexes.Mean]);
            var stdDev = ParseDuration(columns[indexes.StdDev]);
            var caseDescription = columns[indexes.Case].Trim('"');

            yield return new BenchmarkEntry(benchmarkName, implementation, operation, caseDescription, mean, stdDev);
        }
    }

    private static IReadOnlyList<string> SplitCsvLine(string line)
    {
        var result = new List<string>();
        var builder = new StringBuilder();
        var insideQuotes = false;

        foreach (var character in line)
        {
            if (character == '"')
            {
                insideQuotes = !insideQuotes;
                continue;
            }

            if (character == ',' && !insideQuotes)
            {
                result.Add(builder.ToString());
                builder.Clear();
                continue;
            }

            builder.Append(character);
        }

        result.Add(builder.ToString());
        return result;
    }

    private static double ParseDuration(string value)
    {
        var normalized = value.Replace('μ', 'u');
        var match = MeanExtractor.Match(normalized);
        if (!match.Success)
        {
            throw new InvalidOperationException($"Unable to parse duration value '{value}'.");
        }

        var numericValue = double.Parse(match.Groups["value"].Value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture);
        var unit = match.Groups["unit"].Value.ToLowerInvariant();

        return unit switch
        {
            "ns" => numericValue / 1_000_000,
            "us" => numericValue / 1_000,
            "ms" => numericValue,
            "s" => numericValue * 1_000,
            _ => throw new InvalidOperationException($"Unsupported duration unit '{unit}' in value '{value}'.")
        };
    }

    private static string? GetOperation(string method)
    {
        var underscoreIndex = method.IndexOf('_');
        return underscoreIndex > 0 ? method[(underscoreIndex + 1)..] : null;
    }

    private static string GetImplementation(string method)
    {
        var underscoreIndex = method.IndexOf('_');
        return underscoreIndex > 0 ? method[..underscoreIndex] : method;
    }

    private sealed record BenchmarkEntry(
        string Benchmark,
        string Implementation,
        string? Operation,
        string Case,
        double MeanMilliseconds,
        double StdDevMilliseconds);

    private sealed record OperationCaseKey(string Benchmark, string Operation, string Case);

    private sealed record CsvIndexes(int Method, int Case, int Mean, int StdDev)
    {
        public CsvIndexes(IReadOnlyList<string> headers)
            : this(
                Array.IndexOf(headers.ToArray(), "Method"),
                ResolveCaseIndex(headers),
                Array.IndexOf(headers.ToArray(), "Mean"),
                Array.IndexOf(headers.ToArray(), "StdDev"))
        {
            if (Method < 0 || Case < 0 || Mean < 0 || StdDev < 0)
            {
                throw new InvalidOperationException("Unexpected CSV schema. Expected columns: Method, Case/Parameters, Mean, StdDev.");
            }
        }

        private static int ResolveCaseIndex(IReadOnlyList<string> headers)
        {
            var array = headers.ToArray();
            var index = Array.IndexOf(array, "Parameters");
            if (index >= 0)
            {
                return index;
            }

            index = Array.IndexOf(array, "Case");
            return index;
        }
    }

    private static string GetBenchmarkName(string csvPath)
    {
        var fileName = Path.GetFileNameWithoutExtension(csvPath);
        if (fileName.EndsWith("-report", StringComparison.OrdinalIgnoreCase))
        {
            fileName = fileName[..^"-report".Length];
        }

        var segments = fileName.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return segments.Length > 0 ? segments[^1] : fileName;
    }
}
