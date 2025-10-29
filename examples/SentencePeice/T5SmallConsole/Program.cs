namespace Examples.SentencePiece.T5SmallConsole;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using ErgoX.TokenX.SentencePiece.Options;
using ErgoX.TokenX.SentencePiece.Processing;

/// <summary>
/// T5 Small SentencePiece Tokenizer Example
/// 
/// This console app demonstrates the workflow of loading and using a SentencePiece model (t5-small)
/// for multilingual tokenization. SentencePiece is a subword tokenizer that:
/// - Treats text as a sequence of bytes and learns merge rules (similar to BPE)
/// - Supports unicode natively, making it ideal for non-Latin scripts (CJK, Indic, Arabic)
/// - Produces lossless round-trip encoding/decoding: input == decode(encode(input))
/// - T5 variant has vocab size 32,000; padding token ID is 0, end-of-sequence is ID 1
/// 
/// Key concepts:
/// - SentencePiece encodes subword pieces and marks word boundaries with ▁ (underscore symbol)
/// - After decoding, ▁ is converted back to spaces
/// - For rare/unseen characters (e.g., Hindi scripts outside training), the tokenizer falls back to <unk> (ID 2)
/// - Seq2seq models like T5 can inject special tokens (BOS/EOS) via EncodeOptions for proper sequence marking
/// - EncodePieces shows the string representation; EncodeIds shows the numeric representation
/// </summary>
internal static class Program
{
    private const string ModelId = "t5-small";
    private const int IdPreviewCount = 32;      // Show first 32 token IDs before truncating
    private const int PiecePreviewCount = 16;   // Show first 16 pieces before truncating

    private static void Main()
    {
        // Enable UTF-8 output so multilingual characters render correctly in the console window
        Console.OutputEncoding = Encoding.UTF8;

        // Resolve the SentencePiece model file from the .models directory relative to the binary location
        // Path: ../../../../.models/t5-small/spiece.model
        var modelPath = ResolveModelPath();
        
        // Load sample texts from the shared embeddings dataset (English, Spanish, French, Hindi samples)
        var samples = LoadSamples();

        // Create and initialize the SentencePiece processor with the binary model
        using var processor = new SentencePieceProcessor();
        processor.Load(modelPath);

        Console.WriteLine($"Loaded SentencePiece model from: {modelPath}");
        Console.WriteLine($"Vocabulary size: {processor.VocabSize:N0}");
        
        // T5 special token IDs (these are model-dependent):
        // - unk:2         = unknown/OOV token (used for characters outside the training set)
        // - bos:-1        = beginning-of-sequence (T5 doesn't use this, but some models do)
        // - eos:1         = end-of-sequence (ID 1; used to mark end of generation)
        // - pad:0         = padding token (T5 uses 0 for padding; EncodeOptions can inject this)
        Console.WriteLine($"Special IDs -> unk:{processor.UnknownId}, bos:{processor.BosId}, eos:{processor.EosId}, pad:{processor.PadId}");
        Console.WriteLine(new string('-', 72));

        // Process each sample: encode to IDs/pieces, decode to verify round-trip
        foreach (var sample in samples)
        {
            Console.WriteLine($"Sample '{sample.Id}' text:");
            Console.WriteLine(sample.Text);
            Console.WriteLine();

            // Encode text to token IDs (integer sequence that can be fed to a neural model)
            // SentencePiece performs deterministic greedy encoding:
            // It builds the sequence by always selecting the highest-priority merge at each step.
            // This is different from probabilistic/sampling approaches.
            var ids = processor.EncodeIds(sample.Text);
            Console.WriteLine("Token IDs:");
            Console.WriteLine(FormatPreview(ids, IdPreviewCount));
            Console.WriteLine($"Sequence length: {ids.Length}");
            Console.WriteLine();

            // Encode text to pieces (string representation of tokens)
            // - Each piece prefixed with ▁ means it represents a new word boundary
            // - For example: "Hello world" → ["▁Hello", "▁world"]
            // - Non-latin text may show multiple ▁ marks between tokens if characters are sparse
            var pieces = processor.EncodePieces(sample.Text);
            Console.WriteLine("Pieces:");
            Console.WriteLine(FormatPreview(pieces, PiecePreviewCount));
            Console.WriteLine();

            // Verify lossless round-trip: decode IDs back to original text
            // This should always succeed for SentencePiece because all bytes are recoverable
            // Exception: Out-of-vocabulary (OOV) tokens replace unseen characters, breaking exact recovery
            var decoded = processor.DecodeIds(ids);
            Console.WriteLine($"Round-trip matches input: {string.Equals(decoded, sample.Text, StringComparison.Ordinal)}");
            Console.WriteLine(new string('-', 72));
        }

        // Demonstrate seq2seq usage: add EOS for T5 encoder input
        var prompt = "translate English to German: Transformers are amazing.";
        Console.WriteLine("Prompt with EOS enabled:");
        Console.WriteLine(prompt);

        // EncodeOptions controls special token injection and encoding behavior:
        // - AddBos: prepend beginning-of-sequence token (if model defines one; T5 typically doesn't)
        // - AddEos: append end-of-sequence token (T5 defines EOS as ID 1; shown at sequence end)
        // - Reverse: reverse the token sequence (useful for certain architectures)
        // - EnableSampling: use stochastic sampling instead of greedy decoding (experimental)
        var options = new EncodeOptions
        {
            // Note: T5 BOS is -1 (undefined), so AddBos causes decode errors. Use AddEos only.
            AddBos = false,
            AddEos = true   // Adds EOS token (ID 1) at sequence end
        };

        // Encode the prompt with special tokens inserted
        // Note: We only call EncodeIds here to avoid potential issues with EncodePieces + EncodeOptions
        var promptIds = processor.EncodeIds(prompt, options);
        Console.WriteLine("IDs:");
        Console.WriteLine(string.Join(", ", promptIds));
        Console.WriteLine();

        // Demonstrate decode: reconstruct original text from token IDs
        // This shows that encoding is deterministic and reversible
        var decodedPrompt = processor.DecodeIds(promptIds);
        Console.WriteLine("Decoded back to text:");
        Console.WriteLine(decodedPrompt);
        Console.WriteLine();

        // Additional decode example: manually create token sequence
        // This demonstrates that any valid token ID sequence can be decoded
        var customIds = new[] { 7106, 53, 1612, 20491 };  // From the first sample
        var customDecoded = processor.DecodeIds(customIds);
        Console.WriteLine("Custom token sequence:");
        Console.WriteLine(string.Join(", ", customIds));
        Console.WriteLine("Decodes to:");
        Console.WriteLine(customDecoded);
    }

    private static string ResolveModelPath()
    {
        // Navigate from binary location to the model file
        // Relative path from bin/Debug/net8.0 back to examples/.models/t5-small/spiece.model
        var relative = Path.Combine("..", "..", "..", "..", "..", ".models", ModelId, "spiece.model");
        var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, relative));
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"SentencePiece model not found at '{path}'.", path);
        }

        return path;
    }

    private static IReadOnlyList<Sample> LoadSamples()
    {
        // Load sample texts from the shared embeddings dataset
        // Path: ../../../../.data/embeddings/*.json
        var relative = Path.Combine("..", "..", "..", "..", "..", ".data", "embeddings");
        var samplesDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, relative));
        if (!Directory.Exists(samplesDirectory))
        {
            throw new DirectoryNotFoundException($"Samples directory '{samplesDirectory}' was not found.");
        }

        var results = new List<Sample>();
        foreach (var filePath in Directory.EnumerateFiles(samplesDirectory, "*.json", SearchOption.TopDirectoryOnly))
        {
            var json = File.ReadAllText(filePath, Encoding.UTF8);
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            // Expected JSON structure:
            // {
            //   "id": "sample-name",
            //   "single": {
            //     "text": "..."
            //   }
            // }
            if (!root.TryGetProperty("single", out var single) || !single.TryGetProperty("text", out var textNode))
            {
                continue;
            }

            var text = textNode.GetString();
            if (string.IsNullOrWhiteSpace(text))
            {
                continue;
            }

            // Use explicit ID if provided, otherwise derive from filename
            var id = root.TryGetProperty("id", out var idNode) && !string.IsNullOrWhiteSpace(idNode.GetString())
                ? idNode.GetString()!
                : Path.GetFileNameWithoutExtension(filePath);

            results.Add(new Sample(id, text));
        }

        return results;
    }

    private static string FormatPreview(IReadOnlyList<int> values, int maxCount)
    {
        // Format token ID list for console display, truncating if longer than maxCount
        if (values.Count == 0)
        {
            return string.Empty;
        }

        if (values.Count <= maxCount)
        {
            return string.Join(", ", values);
        }

        return string.Join(", ", values.Take(maxCount)) + ", ...";
    }

    private static string FormatPreview(IReadOnlyList<string> values, int maxCount)
    {
        // Format piece list for console display, truncating if longer than maxCount
        if (values.Count == 0)
        {
            return string.Empty;
        }

        if (values.Count <= maxCount)
        {
            return string.Join(" | ", values);
        }

        return string.Join(" | ", values.Take(maxCount)) + " | ...";
    }

    private sealed record Sample(string Id, string Text);
}

