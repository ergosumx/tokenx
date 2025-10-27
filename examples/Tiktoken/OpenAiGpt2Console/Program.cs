namespace Examples.Tiktoken.OpenAiGpt2Console;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using ErgoX.VecraX.ML.NLP.Tokenizers.Tiktoken;

/// <summary>
/// TikToken GPT-2 Tokenizer Example
/// 
/// This console app demonstrates OpenAI's TikToken byte-pair encoding (BPE) tokenizer,
/// specifically configured for GPT-2 language models. Key features:
/// - Byte-level encoding: Text is first converted to UTF-8 bytes, then merged into subwords
/// - Deterministic: Identical text always produces the same tokens across runs
/// - Regex-based: Uses regex to group bytes before BPE merging (e.g., words, numbers, punctuation)
/// - Special tokens: Handles reserved tokens like <|endoftext|> (ID 50256)
/// - Fast: Optimized C++ backend for rapid encoding/decoding
/// 
/// TikToken vs SentencePiece:
/// - TikToken: Byte-level, regex-aware, optimized for English/code; less multilingual support
/// - SentencePiece: Character/subword-level, language-agnostic, better for multilingual tasks
/// 
/// GPT-2 specifics:
/// - Vocabulary size: 50,257 tokens (50,000 BPE merges + 256 bytes + 1 special token)
/// - Special token: <|endoftext|> (ID 50256; marks end of document/sequence)
/// - Encoding: Uses Python regex pattern for tokenization
/// - Average compression: 1.3–1.5 tokens per word in English text
/// </summary>
internal static class Program
{
    // Encoding metadata: must match the GPT-2 model configuration
    private const string EncodingName = "gpt2";
    private const uint ExplicitVocabularySize = 50257;
    private const int TokenPreviewCount = 24;     // Show first 24 tokens before truncating
    
    // GPT-2 uses a specific regex pattern to pre-tokenize text before BPE merging
    // This pattern groups:
    // - Contractions: 's, 'd, 'm, 'll, 've, 're, 't, ing
    // - Words: sequences of letters
    // - Numbers: sequences of digits
    // - Special chars: everything else except whitespace
    // - Whitespace: sequences of spaces, tabs, newlines
    private const string Pattern = "'(?:[sdmt]|ll|ve|re)| ?\\p{L}+| ?\\p{N}+| ?[^\\s\\p{L}\\p{N}]+|\\s+(?!\\S)|\\s+";

    // Special tokens: reserved IDs with semantic meaning
    private static readonly IReadOnlyDictionary<string, int> SpecialTokens = new Dictionary<string, int>
    {
        ["<|endoftext|>"] = 50256  // End-of-text marker; signals completion of a document/prompt
    };

    private static void Main()
    {
        // Enable UTF-8 output so multilingual characters render correctly in the console
        Console.OutputEncoding = Encoding.UTF8;

        // Resolve the TikToken mergeable ranks file from the .models directory relative to the binary location
        // Path: ../../../../.models/openai-gpt2/mergeable_ranks.tiktoken
        var mergeableRanksPath = ResolveMergeableRanksPath();
        
        // Load sample texts from the shared embeddings dataset (multilingual samples)
        var samples = LoadSamples();

        // Create the TikToken GPT-2 encoding from the mergeable ranks file
        // The factory validates vocabulary size and reconstructs the BPE merge table
        using var encoding = TiktokenEncodingFactory.FromTiktokenFile(
            EncodingName,
            Pattern,
            mergeableRanksPath,
            SpecialTokens,
            ExplicitVocabularySize); // Match GPT-2 byte pair merges and regex exactly to stay compatible with the Python tokenizer.

        Console.WriteLine($"Loaded TikToken encoding '{EncodingName}' from: {mergeableRanksPath}");
        Console.WriteLine($"Special tokens: {string.Join(", ", SpecialTokens.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");
        Console.WriteLine(new string('-', 72));

        // Process each sample: encode to tokens and decode back to text
        foreach (var sample in samples)
        {
            Console.WriteLine($"Sample '{sample.Id}' text:");
            Console.WriteLine(sample.Text);
            Console.WriteLine();

            // Encode text to token IDs using ordinary (non-special-token-aware) encoding
            // EncodeOrdinary does NOT check for special tokens; it treats all bytes equally
            // This is the standard encoding path for most text processing
            var tokens = encoding.EncodeOrdinary(sample.Text);
            Console.WriteLine($"Token count: {tokens.Count}");
            Console.WriteLine("Tokens:");
            Console.WriteLine(FormatPreview(tokens, TokenPreviewCount));
            Console.WriteLine();

            // Decode tokens back to original text
            // TikToken decoding is deterministic and lossless: all byte information is recoverable
            // Exception: If a token doesn't correspond to any byte sequence, decode may fail
            var decoded = encoding.Decode(tokens.ToArray());
            Console.WriteLine($"Round-trip matches input: {string.Equals(decoded, sample.Text, StringComparison.Ordinal)}");
            Console.WriteLine(new string('-', 72));
        }

        // Demonstrate special token handling: compare ordinary vs special-token-aware encoding
        var specialPrompt = "Include the <|endoftext|> marker when you are done.";
        Console.WriteLine("Special token handling:");
        Console.WriteLine(specialPrompt);

        // Ordinary encoding: <|endoftext|> is treated as raw text, split into multiple byte tokens
        // Example: "<|endoftext|>" → [27, 91, 58, ...] (6-8 byte tokens)
        var ordinary = encoding.EncodeOrdinary(specialPrompt);
        
        // Special-token-aware encoding: <|endoftext|> is recognized as a single token (ID 50256)
        // Requires passing allowedSpecial list to allow this reserved token
        // Without allowedSpecial, special tokens are still split byte-by-byte
        var allowedSpecial = new[] { "<|endoftext|>" };
        var withSpecial = encoding.Encode(specialPrompt, allowedSpecial);

        Console.WriteLine($"Ordinary encoding length: {ordinary.Count}");
        Console.WriteLine($"Allowing <|endoftext|> collapses to length: {withSpecial.Count}");
        Console.WriteLine("Tokens:");
        Console.WriteLine(string.Join(", ", withSpecial));
        Console.WriteLine("Decoded:");
        Console.WriteLine(encoding.Decode(withSpecial.ToArray()));
        Console.WriteLine();

        // Demonstrate decode examples from generated encodings
        // Decoding is the reverse of encoding: token IDs → original text
        // Important: Decoding is always successful because each token maps to valid UTF-8 bytes
        Console.WriteLine("Decode examples from various encodings:");
        Console.WriteLine();

        // Example 1: Decode tokens from the first sample
        var firstSampleTokens = encoding.EncodeOrdinary(samples[0].Text);
        var firstSampleSlice = firstSampleTokens.Take(Math.Min(10, firstSampleTokens.Count)).ToList();
        var decodedSlice = encoding.Decode(firstSampleSlice.ToArray());
        Console.WriteLine("Sample tokens (first 10 tokens from sample 1):");
        Console.WriteLine(string.Join(", ", firstSampleSlice));
        Console.WriteLine("Decodes to:");
        Console.WriteLine(decodedSlice);
        Console.WriteLine();

        // Example 2: Create a custom token sequence and decode
        // This demonstrates that any valid token ID can be decoded to valid UTF-8 text
        var customTokens = new uint[] { 464, 1256, 318, 2568, 13 };  // "This is great.\n"
        var customDecoded = encoding.Decode(customTokens);
        Console.WriteLine("Custom token sequence:");
        Console.WriteLine(string.Join(", ", customTokens));
        Console.WriteLine("Decodes to:");
        Console.WriteLine(customDecoded);
        Console.WriteLine();

        // Example 3: Demonstrate full round-trip with special tokens
        var roundTripText = "Hello! The model works perfectly.";
        var roundTripTokens = encoding.EncodeOrdinary(roundTripText);
        var roundTripDecoded = encoding.Decode(roundTripTokens.ToArray());
        Console.WriteLine("Full round-trip example:");
        Console.WriteLine($"Original:  {roundTripText}");
        Console.WriteLine($"Tokens:    {string.Join(", ", roundTripTokens)}");
        Console.WriteLine($"Decoded:   {roundTripDecoded}");
        Console.WriteLine($"Match:     {string.Equals(roundTripText, roundTripDecoded, StringComparison.Ordinal)}");
    }

    private static string ResolveMergeableRanksPath()
    {
        // Navigate from binary location to the TikToken vocabulary file
        // Relative path from bin/Debug/net8.0 back to examples/.models/openai-gpt2/mergeable_ranks.tiktoken
        // This file contains the BPE merge operations that define how bytes combine into higher-level tokens
        var relative = Path.Combine("..", "..", "..", "..", "..", ".models", "openai-gpt2", "mergeable_ranks.tiktoken");
        var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, relative));
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"TikToken mergeable ranks file not found at '{path}'.", path);
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

    private static string FormatPreview(IReadOnlyList<uint> values, int maxCount)
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

    private sealed record Sample(string Id, string Text);
}
