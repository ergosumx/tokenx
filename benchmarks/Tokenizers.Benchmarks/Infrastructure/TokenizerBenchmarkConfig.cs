namespace Tokenizers.Benchmarks.Infrastructure;

using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;

internal sealed class TokenizerBenchmarkConfig : ManualConfig
{
    public TokenizerBenchmarkConfig()
    {
        AddColumn(TargetMethodColumn.Method, StatisticColumn.Min, StatisticColumn.Mean, StatisticColumn.Max, StatisticColumn.StdDev, BaselineColumn.Default);
        AddDiagnoser(MemoryDiagnoser.Default);
        AddJob(Job.ShortRun
            .WithId("ExtendedShortRun")
            .WithWarmupCount(5)
            .WithIterationCount(20)
            .WithMinIterationCount(10)
            .WithLaunchCount(1));
    }
}
