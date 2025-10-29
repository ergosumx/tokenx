namespace Examples.HuggingFace.E5SmallV2;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using ErgoX.TokenX.HuggingFace;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

/// <summary>
/// E5-small-v2 example: Query-document embeddings with task-specific prompts.
/// 
/// Key feature: Uses "query:" prefix injection to signal task context
/// - "query: " for search queries
/// - "passage: " for documents to be retrieved (not used here, but in real retrieval)
/// 
/// Pipeline:
/// 1. Load tokenizer (BPE with special tokens)
/// 2. Load ONNX model (quantized, task-aware)
/// 3. For each sample: prepend "query: " prefix before tokenization
/// 4. Run inference, extract embedding
/// 
/// Difference from AllMiniLmL6V2:
/// - All-MiniLM: Generic similarity (embedding norm ~6-7)
/// - E5: Query-optimized embeddings (embedding norm ~5.8-5.9)
/// - E5 improves retrieval relevance with prefix awareness
/// </summary>
internal static class Program
{
    private const string ModelId = "e5-small-v2";

    private static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        var modelDirectory = ResolveModelDirectory(ModelId);
        var samples = LoadSamples();

        using var tokenizer = AutoTokenizer.Load(modelDirectory, new AutoTokenizerLoadOptions
        {
            ApplyTokenizerDefaults = true,
            LoadGenerationConfig = true
        });

        using var session = CreateSession(modelDirectory);

        Console.WriteLine($"Loaded '{ModelId}' tokenizer and ONNX model from: {modelDirectory}");
        Console.WriteLine();

        foreach (var sample in samples)
        {
            // E5 requires explicit task prefix: "query:" for search queries
            // Prefix is learned during training to signal task context
            // Token ID 23032 = "query" in BERT vocabulary
            var prompt = $"query: {sample.Text}";
            Console.WriteLine($"Embedding query for sample '{sample.Id}':");
            Console.WriteLine(prompt);

            // Tokenize with prefix: text becomes [101, 23032, 1024, ...content tokens..., 102]
            // 101 = [CLS], 23032 = query token, 1024 = colon punctuation
            var encoding = tokenizer.Tokenizer.Encode(prompt);
            var embedding = ComputeEmbedding(session, encoding);

            Console.WriteLine("Token IDs:");
            Console.WriteLine(string.Join(", ", encoding.Ids));
            Console.WriteLine();

            var preview = string.Join(", ", embedding.Take(6).Select(value => value.ToString("F4")));
            Console.WriteLine($"Embedding preview: {preview}");

            // L2 norm typically 5.8-5.9 for E5 (lower than AllMiniLm due to retrieval specialization)
            var norm = Math.Sqrt(embedding.Select(value => value * value).Sum());
            Console.WriteLine($"Embedding L2 norm: {norm:F4}");
            Console.WriteLine(new string('-', 72));
        }
    }

    private static IReadOnlyList<EmbeddingSample> LoadSamples()
    {
        var relative = Path.Combine("..", "..", "..", "..", "..", ".data", "embeddings");
        var samplesDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, relative));
        if (!Directory.Exists(samplesDirectory))
        {
            throw new DirectoryNotFoundException($"Samples directory '{samplesDirectory}' was not found.");
        }

        var results = new List<EmbeddingSample>();
        foreach (var filePath in Directory.EnumerateFiles(samplesDirectory, "*.json", SearchOption.TopDirectoryOnly))
        {
            var json = File.ReadAllText(filePath, Encoding.UTF8);
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            var id = root.TryGetProperty("id", out var idNode) ? idNode.GetString() ?? Path.GetFileNameWithoutExtension(filePath) : Path.GetFileNameWithoutExtension(filePath);
            var text = root.GetProperty("single").GetProperty("text").GetString();
            if (string.IsNullOrWhiteSpace(text))
            {
                continue;
            }

            results.Add(new EmbeddingSample(id, text));
        }

        return results;
    }

    private static InferenceSession CreateSession(string modelDirectory)
    {
        var modelPath = Path.Combine(modelDirectory, "model_quantized.onnx");
        if (!File.Exists(modelPath))
        {
            throw new FileNotFoundException($"Quantized model not found at '{modelPath}'.", modelPath);
        }

        var options = new SessionOptions
        {
            LogSeverityLevel = OrtLoggingLevel.ORT_LOGGING_LEVEL_WARNING,
            GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL
        };

        try
        {
            return new InferenceSession(modelPath, options);
        }
        finally
        {
            options.Dispose();
        }
    }

    private static float[] ComputeEmbedding(InferenceSession session, EncodingResult encoding)
    {
        var inputIds = encoding.Ids.Select(id => (long)id).ToArray();
        var attentionMask = encoding.AttentionMask.Count == encoding.Length
            ? encoding.AttentionMask.Select(value => (long)value).ToArray()
            : Enumerable.Repeat(1L, encoding.Length).ToArray();
        var tokenTypeIds = encoding.TypeIds.Count == encoding.Length
            ? encoding.TypeIds.Select(value => (long)value).ToArray()
            : new long[encoding.Length];

        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("input_ids", CreateTensor(inputIds))
        };

        if (session.InputMetadata.ContainsKey("attention_mask"))
        {
            inputs.Add(NamedOnnxValue.CreateFromTensor("attention_mask", CreateTensor(attentionMask)));
        }

        if (session.InputMetadata.ContainsKey("token_type_ids"))
        {
            inputs.Add(NamedOnnxValue.CreateFromTensor("token_type_ids", CreateTensor(tokenTypeIds)));
        }

        try
        {
            using var results = session.Run(inputs);
            return ExtractEmbedding(results);
        }
        finally
        {
            foreach (var input in inputs)
            {
                if (input is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }
    }

    private static DenseTensor<long> CreateTensor(long[] values)
    {
        return new DenseTensor<long>(values, new[] { 1, values.Length });
    }

    private static float[] ExtractEmbedding(IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results)
    {
        foreach (var result in results)
        {
            if (result.Value is DenseTensor<float> dense)
            {
                return FlattenDenseTensor(dense);
            }

            if (result.Value is IEnumerable<float> enumerable)
            {
                return enumerable.ToArray();
            }

            if (result.Value is DenseTensor<double> denseDouble)
            {
                return FlattenDenseTensor(denseDouble).Select(value => (float)value).ToArray();
            }
        }

        throw new InvalidOperationException("Unable to locate a floating-point embedding in the model outputs.");
    }

    private static float[] FlattenDenseTensor(DenseTensor<float> tensor)
    {
        return tensor.Rank switch
        {
            1 => tensor.ToArray(),
            2 => ExtractRow(tensor, 0),
            3 => ExtractSlice(tensor, 0, 0),
            _ => throw new InvalidOperationException($"Unsupported embedding tensor rank {tensor.Rank}.")
        };
    }

    private static double[] FlattenDenseTensor(DenseTensor<double> tensor)
    {
        return tensor.Rank switch
        {
            1 => tensor.ToArray(),
            2 => ExtractRow(tensor, 0),
            3 => ExtractSlice(tensor, 0, 0),
            _ => throw new InvalidOperationException($"Unsupported embedding tensor rank {tensor.Rank}.")
        };
    }

    private static float[] ExtractRow(DenseTensor<float> tensor, int row)
    {
        var length = tensor.Dimensions[1];
        var values = new float[length];
        for (var index = 0; index < length; index++)
        {
            values[index] = tensor[row, index];
        }

        return values;
    }

    private static double[] ExtractRow(DenseTensor<double> tensor, int row)
    {
        var length = tensor.Dimensions[1];
        var values = new double[length];
        for (var index = 0; index < length; index++)
        {
            values[index] = tensor[row, index];
        }

        return values;
    }

    private static float[] ExtractSlice(DenseTensor<float> tensor, int batchIndex, int tokenIndex)
    {
        var length = tensor.Dimensions[2];
        var values = new float[length];
        for (var index = 0; index < length; index++)
        {
            values[index] = tensor[batchIndex, tokenIndex, index];
        }

        return values;
    }

    private static double[] ExtractSlice(DenseTensor<double> tensor, int batchIndex, int tokenIndex)
    {
        var length = tensor.Dimensions[2];
        var values = new double[length];
        for (var index = 0; index < length; index++)
        {
            values[index] = tensor[batchIndex, tokenIndex, index];
        }

        return values;
    }

    private static string ResolveModelDirectory(string modelId)
    {
        var relative = Path.Combine("..", "..", "..", "..", "..", ".models", modelId);
        var fullPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, relative));
        if (!Directory.Exists(fullPath))
        {
            throw new DirectoryNotFoundException($"Model directory '{fullPath}' was not found.");
        }

        return fullPath;
    }

    private sealed record EmbeddingSample(string Id, string Text);
}

