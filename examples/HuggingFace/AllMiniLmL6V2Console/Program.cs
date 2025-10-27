namespace Examples.HuggingFace.AllMiniLmL6V2;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

/// <summary>
/// AllMiniLmL6V2 example: Generates sentence embeddings using BERT-based model.
/// 
/// Pipeline:
/// 1. Load tokenizer (converts text → token IDs)
/// 2. Load ONNX model (quantized for CPU inference)
/// 3. For each sample text:
///    a. Tokenize text (BERT adds [CLS], [SEP], [PAD])
///    b. Run ONNX inference
///    c. Extract [CLS] token embedding (384-dim vector)
///    d. Compute L2 norm for cosine similarity
/// 
/// Use case: Semantic search, clustering, deduplication
/// Quantization: QINT8 (4× smaller, 1-2% accuracy loss)
/// </summary>
internal static class Program
{
    private const string ModelId = "all-minilm-l6-v2";

    private static void Main()
    {
        // Set output encoding to UTF-8 for proper multilingual character display
        Console.OutputEncoding = Encoding.UTF8;
        
        // Resolve model directory: ..\..\..\..\..\.models\all-minilm-l6-v2
        var modelDirectory = ResolveModelDirectory(ModelId);
        
        // Load JSON samples from ..\..\..\..\..\.data\embeddings\*.json
        var samplePayloads = LoadSamples();

        // Load tokenizer configuration and special tokens from model directory
        // ApplyTokenizerDefaults=true ensures [CLS], [SEP], [PAD] tokens are properly set
        using var tokenizer = AutoTokenizer.Load(modelDirectory, new AutoTokenizerLoadOptions
        {
            ApplyTokenizerDefaults = true,
            LoadGenerationConfig = true
        });

        // Load quantized ONNX model (model_quantized.onnx)
        // QINT8 model ~40-50 MB, reduces inference time vs. FP32
        using var session = CreateSession(modelDirectory);

        Console.WriteLine($"Loaded '{ModelId}' tokenizer and ONNX model from: {modelDirectory}");
        Console.WriteLine();

        // Process each text sample
        foreach (var sample in samplePayloads)
        {
            Console.WriteLine($"Sample '{sample.Id}' text:");
            Console.WriteLine(sample.Text);
            Console.WriteLine();

            // Tokenize text: "Programming language..." → [101, 4730, 2653, ..., 102, 0, 0, ...]
            // 101 = [CLS] (start token)
            // 102 = [SEP] (end token)
            // 0 = [PAD] (padding to max_length=512)
            var encoding = tokenizer.Tokenizer.Encode(sample.Text);
            
            // Compute embedding: Run ONNX inference on token IDs
            var embedding = ComputeEmbedding(session, encoding);

            Console.WriteLine("Token IDs:");
            Console.WriteLine(string.Join(", ", encoding.Ids));
            Console.WriteLine();

            // Display first 6 components of 384-dimensional embedding
            // Shows vector structure without overwhelming output
            Console.WriteLine("First 6 embedding components:");
            Console.WriteLine(string.Join(", ", embedding.Take(6).Select(value => value.ToString("F4"))));

            // Compute L2 norm: sqrt(sum of squared components)
            // L2 norm enables cosine similarity computation: normalized dot-product approximation
            var norm = Math.Sqrt(embedding.Select(value => value * value).Sum());
            Console.WriteLine($"Embedding L2 norm: {norm:F4}");
            Console.WriteLine(new string('-', 72));
        }
    }

    private static IReadOnlyList<EmbeddingSample> LoadSamples()
    {
        // Construct path relative to binary: ..\..\..\..\..\.data\embeddings
        var relative = Path.Combine("..", "..", "..", "..", "..", ".data", "embeddings");
        var samplesDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, relative));
        if (!Directory.Exists(samplesDirectory))
        {
            throw new DirectoryNotFoundException($"Samples directory '{samplesDirectory}' was not found.");
        }

        // Load all JSON files matching pattern: *.json
        // Format: { "id": "standard-tiny-en", "single": { "text": "..." } }
        var results = new List<EmbeddingSample>();
        foreach (var filePath in Directory.EnumerateFiles(samplesDirectory, "*.json", SearchOption.TopDirectoryOnly))
        {
            var json = File.ReadAllText(filePath, Encoding.UTF8);
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            
            // Extract sample ID from JSON or use filename
            var sampleId = root.TryGetProperty("id", out var idNode) 
                ? idNode.GetString() ?? Path.GetFileNameWithoutExtension(filePath) 
                : Path.GetFileNameWithoutExtension(filePath);
            
            // Extract text from nested structure: root["single"]["text"]
            var text = root.GetProperty("single").GetProperty("text").GetString();
            if (string.IsNullOrWhiteSpace(text))
            {
                continue;
            }

            results.Add(new EmbeddingSample(sampleId, text));
        }

        return results;
    }

    private static InferenceSession CreateSession(string modelDirectory)
    {
        // Load quantized ONNX model from directory
        // Model file: model_quantized.onnx (QINT8 format, ~40-50 MB for BERT-base)
        var modelPath = Path.Combine(modelDirectory, "model_quantized.onnx");
        if (!File.Exists(modelPath))
        {
            throw new FileNotFoundException($"Quantized model not found at '{modelPath}'.", modelPath);
        }

        // Session options: disable logging except warnings, enable all graph optimizations
        // Graph optimization: Fuses operations (e.g., add+relu), reduces memory/compute
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
        // Convert token IDs to long array (ONNX expects int64)
        var inputIds = encoding.Ids.Select(id => (long)id).ToArray();
        
        // Attention mask: 1 for real tokens, 0 for padding
        // Tells model which positions to process (ignore [PAD] tokens)
        var attentionMask = encoding.AttentionMask.Count == encoding.Length
            ? encoding.AttentionMask.Select(value => (long)value).ToArray()
            : Enumerable.Repeat(1L, encoding.Length).ToArray();
        
        // Token type IDs: Distinguishes sentence A vs B (single sentence = all 0)
        var tokenTypeIds = encoding.TypeIds.Count == encoding.Length
            ? encoding.TypeIds.Select(value => (long)value).ToArray()
            : new long[encoding.Length];

        // Prepare ONNX inputs with shape (batch_size=1, sequence_length)
        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("input_ids", CreateTensor(inputIds))
        };

        // Add optional inputs if model expects them (depends on model architecture)
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
            // Run ONNX inference
            // Input: (1, sequence_length) tensors with token IDs and attention mask
            // Output: last_hidden_state (1, sequence_length, 384) - embeddings for each token
            using var results = session.Run(inputs);
            return ExtractEmbedding(results);
        }
        finally
        {
            // Clean up input tensors
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
        // Create 2D tensor (1, sequence_length) for ONNX batch input
        // Batch size = 1 (single sample)
        // Sequence length = number of tokens
        return new DenseTensor<long>(values, new[] { 1, values.Length });
    }

    private static float[] ExtractEmbedding(IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results)
    {
        // ONNX returns multiple outputs, we need the embedding output
        // Try matching by type (float tensor) or by specific name
        foreach (var result in results)
        {
            if (result.Value is DenseTensor<float> dense)
            {
                // Extract embedding from dense float tensor
                return FlattenDenseTensor(dense);
            }

            if (result.Value is IEnumerable<float> enumerable)
            {
                return enumerable.ToArray();
            }

            if (result.Value is DenseTensor<double> denseDouble)
            {
                // Convert double to float if necessary
                return FlattenDenseTensor(denseDouble).Select(value => (float)value).ToArray();
            }
        }

        throw new InvalidOperationException("Unable to locate a floating-point embedding in the model outputs.");
    }

    private static float[] FlattenDenseTensor(DenseTensor<float> tensor)
    {
        // Flatten tensor based on rank
        // Rank 1: (384,) → direct array
        // Rank 2: (1, 384) → extract row 0
        // Rank 3: (1, 1, 384) → extract [0, 0, :]
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
        // Extract single row from 2D tensor: tensor[row, :]
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
        // Extract single slice from 3D tensor: tensor[batch, token, :]
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
        // Construct absolute path to model directory
        // Path: examples/.models/{modelId}
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
