namespace Examples.HuggingFace.MultilingualE5Small;

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
/// Multilingual E5 example: Cross-lingual query-document embeddings (100+ languages).
/// 
/// Key differences from E5SmallV2:
/// - Uses XLM-RoBERTa tokenizer (not BERT) for multilingual support
/// - Unified embedding space: semantically similar text in different languages cluster together
/// - Special tokens: &lt;s&gt; (ID: 0), &lt;/s&gt;, &lt;unk&gt;, &lt;pad&gt; (not BERT's [CLS]/[SEP])
/// - Embedding norm: ~3.7-3.8 (lower than monolingual, more distributed across languages)
/// 
/// Pipeline:
/// 1. Load multilingual tokenizer (250K vocab, all languages)
/// 2. Load ONNX model (trained on parallel corpora for alignment)
/// 3. For each sample (any language): prepend "query:" prefix
/// 4. Tokenize and run inference
/// 5. Embeddings are comparable across languages
/// 
/// Use: Global search, multilingual FAQ, cross-lingual deduplication
/// </summary>
internal static class Program
{
    private const string ModelId = "multilingual-e5-small";

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
            // "query:" prefix works across all languages (same token ID in multilingual vocab)
            var prompt = $"query: {sample.Text}";
            Console.WriteLine($"Embedding multilingual query '{sample.Id}':");
            Console.WriteLine(prompt);
            Console.WriteLine();

            // XLM-RoBERTa tokenization:
            // - First token: 0 (BOS, different from BERT's 101)
            // - Prefix tokens: 41 (query), 1294 (colon)
            // - Content tokens: Language-specific subwords
            // Resulting embeddings are in shared cross-lingual space
            var encoding = tokenizer.Tokenizer.Encode(prompt);
            var embedding = ComputeEmbedding(session, encoding);

            Console.WriteLine("Token IDs:");
            Console.WriteLine(string.Join(", ", encoding.Ids));
            Console.WriteLine();

            var preview = string.Join(", ", embedding.Take(6).Select(value => value.ToString("F4")));
            Console.WriteLine($"Embedding preview: {preview}");

            // L2 norm ~3.7-3.8 (lower than monolingual) due to multilingual training
            // Lower norm = more distributed embedding space to cover 100+ languages
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
            var id = root.TryGetProperty("id", out var idNode)
                ? idNode.GetString() ?? Path.GetFileNameWithoutExtension(filePath)
                : Path.GetFileNameWithoutExtension(filePath);
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

