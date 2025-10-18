using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Running;
using Tokenizers.Benchmarks.Reports;

var processed = ArgumentPreprocessor.Process(args);
if (processed.ShouldGenerateReport)
{
	BenchmarkReportGenerator.Generate(processed.ReportOutputPath);
	return;
}

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(processed.RemainingArgs);

internal static class ArgumentPreprocessor
{
	private const string GenerateReportSwitch = "--generate-report";
	private const string OutputSwitch = "--output";

	internal static (bool ShouldGenerateReport, string? ReportOutputPath, string[] RemainingArgs) Process(string[] originalArgs)
	{
		var remaining = new List<string>(originalArgs.Length);
		var queue = new Queue<string>(originalArgs);
		var shouldGenerate = false;
		string? outputPath = null;

		while (queue.Count > 0)
		{
			var current = queue.Dequeue();
			if (string.Equals(current, GenerateReportSwitch, StringComparison.OrdinalIgnoreCase))
			{
				shouldGenerate = true;
				continue;
			}

			if (string.Equals(current, OutputSwitch, StringComparison.OrdinalIgnoreCase) && queue.Count > 0)
			{
				outputPath = queue.Dequeue();
				continue;
			}

			remaining.Add(current);
		}

		return (shouldGenerate, outputPath, remaining.ToArray());
	}
}
