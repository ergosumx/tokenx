namespace Tokenizers.Benchmarks.Assets;

using System;
using System.Collections.Generic;
using System.Linq;

public enum SequenceLength
{
    Tiny,
    Short,
    Medium,
    Long,
    ExtraLong,
    Massive
}

public sealed record BenchmarkCase(SequenceLength Length, string Description, string Text, IReadOnlyList<string> Batch)
{
    public override string ToString() => Description;
}

internal static class TextCorpus
{
    private static readonly string BaseSentence =
        "In a distant research lab, engineers tuned transformers while monitoring streaming telemetry from multilingual datasets.";

    private static readonly string[] SupplementarySentences =
    {
        "The models adapted to sudden domain shifts as analysts injected noisy transcripts into the queue.",
        "Meanwhile, the observability stack captured latency spikes and surfaced alerts to the on-call rotation.",
        "A compliance auditor reviewed masked payloads to ensure privacy constraints remained intact.",
        "After midnight, an A/B experiment introduced synthetic dialogue to probe robustness against adversarial prompts.",
        "Statisticians summarized the nightly run, documenting throughput, token distributions, and error trends."
    };

    public static IReadOnlyList<BenchmarkCase> GetCases()
    {
        return new[]
        {
            CreateCase(SequenceLength.Tiny, "Tiny (≈16 tokens)", 1, 2, batchSize: 16),
            CreateCase(SequenceLength.Short, "Short (≈32 tokens)", 1, 4, batchSize: 32),
            CreateCase(SequenceLength.Medium, "Medium (≈128 tokens)", 4, 6, batchSize: 32),
            CreateCase(SequenceLength.Long, "Long (≈512 tokens)", 16, 8, batchSize: 32),
            CreateCase(SequenceLength.ExtraLong, "Extra Long (≈1024 tokens)", 32, 12, batchSize: 32),
            CreateCase(SequenceLength.Massive, "Massive (≈2048 tokens)", 64, 16, batchSize: 16)
        };
    }

    private static BenchmarkCase CreateCase(SequenceLength length, string description, int baseRepeats, int shuffleSeed, int batchSize)
    {
        var random = new Random(shuffleSeed);
        var segments = new List<string>(baseRepeats * (SupplementarySentences.Length + 1));
        for (var i = 0; i < baseRepeats; i++)
        {
            segments.Add(BaseSentence);
            segments.AddRange(SupplementarySentences.OrderBy(_ => random.Next()));
        }

        var text = string.Join(' ', segments);
        var batch = Enumerable.Range(0, batchSize)
            .Select(index => text + $" [sample:{index:D2}]")
            .ToArray();

        return new BenchmarkCase(length, description, text, batch);
    }
}
