namespace Examples.HuggingFace.Quickstart;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ErgoX.TokenX.HuggingFace;
using ErgoX.TokenX.HuggingFace.Chat;
using ErgoX.TokenX.HuggingFace.Options;

/// <summary>
/// HuggingFace Quickstart - Comprehensive tokenization examples for transformer models
/// 
/// Demonstrates ALL HuggingFace Tokenizer features:
/// 1. Basic tokenization
/// 2. Batch processing
/// 3. Token-to-text conversion
/// 4. Special tokens handling
/// 5. Padding strategies (left/right)
/// 6. Truncation strategies
/// 7. Text pair encoding (for classification)
/// 8. Attention masks and type IDs
/// 9. Offset mapping (character positions)
/// 10. Word IDs and sequence IDs
/// 11. Chat template rendering
/// 12. Custom padding and truncation
/// 13. Vocabulary access (token ↔ ID)
/// 14. Working with multiple models (WordPiece, Unigram, BPE)
/// 15. Overflowing tokens (stride/windowing)
/// 16. Working with special tokens and vocabulary inspection
/// </summary>
internal static class Program
{
    private static void Main()
    {
        Console.WriteLine("=== HuggingFace Tokenizer Comprehensive Quickstart ===");
        Console.WriteLine("This quickstart demonstrates ALL features of the HuggingFace Tokenizer library.");
        Console.WriteLine();

        // Core Features (Examples 1-4)
        BasicTokenization();
        BatchProcessing();
        TokenToTextConversion();
        SpecialTokens();

        // Advanced Features (Examples 5-16)
        PaddingStrategies();
        TruncationStrategies();
        TextPairEncoding();
        AttentionMasksAndTypeIds();
        OffsetMapping();
        WordAndSequenceIds();
        ChatTemplateRendering();
        CustomPaddingAndTruncation();
        VocabularyAccess();
        MultimpleModels();
        OverflowingTokens();
        AddingCustomTokens();
    }

    private static void BasicTokenization()
    {
        Console.WriteLine("--- Example 1: Basic Tokenization ---");
        
        var modelDirectory = ResolveModelDirectory("all-minilm-l6-v2");
        
        using var tokenizer = AutoTokenizer.Load(modelDirectory, new AutoTokenizerLoadOptions
        {
            ApplyTokenizerDefaults = true
        });

        string text = "Hello, how are you?";
        var encoding = tokenizer.Tokenizer.Encode(text);
        
        Console.WriteLine($"Text: {text}");
        Console.WriteLine($"Token IDs: [{string.Join(", ", encoding.Ids)}]");
        Console.WriteLine($"Tokens: [{string.Join(", ", encoding.Tokens)}]");
        Console.WriteLine($"Token count: {encoding.Length}");
        
        // Decode back to text
        var decoded = tokenizer.Tokenizer.Decode(encoding.Ids.ToArray());
        Console.WriteLine($"Decoded: {decoded}");
        Console.WriteLine();
    }

    private static void BatchProcessing()
    {
        Console.WriteLine("--- Example 2: Batch Processing ---");
        
        var modelDirectory = ResolveModelDirectory("all-minilm-l6-v2");
        
        using var tokenizer = AutoTokenizer.Load(modelDirectory, new AutoTokenizerLoadOptions
        {
            ApplyTokenizerDefaults = true
        });

        var texts = new[]
        {
            "Machine learning is fascinating.",
            "Natural language processing enables computers to understand text.",
            "Transformers revolutionized AI."
        };

        Console.WriteLine("Processing multiple texts:");
        foreach (var text in texts)
        {
            var encoding = tokenizer.Tokenizer.Encode(text);
            Console.WriteLine($"  '{text}' → {encoding.Length} tokens");
        }
        Console.WriteLine();
    }

    private static void TokenToTextConversion()
    {
        Console.WriteLine("--- Example 3: Token-to-Text Conversion ---");
        
        var modelDirectory = ResolveModelDirectory("all-minilm-l6-v2");
        
        using var tokenizer = AutoTokenizer.Load(modelDirectory, new AutoTokenizerLoadOptions
        {
            ApplyTokenizerDefaults = true
        });

        string text = "Tokenization splits text into subwords.";
        var encoding = tokenizer.Tokenizer.Encode(text);
        
        Console.WriteLine($"Original: {text}");
        Console.WriteLine("Individual tokens:");
        
        for (int i = 0; i < encoding.Length; i++)
        {
            var tokenId = encoding.Ids[i];
            var tokenText = encoding.Tokens[i];
            Console.WriteLine($"  Token {i}: ID={tokenId}, Text='{tokenText}'");
        }
        Console.WriteLine();
    }

    private static void SpecialTokens()
    {
        Console.WriteLine("--- Example 4: Special Tokens ---");
        
        var modelDirectory = ResolveModelDirectory("all-minilm-l6-v2");
        
        using var tokenizer = AutoTokenizer.Load(modelDirectory, new AutoTokenizerLoadOptions
        {
            ApplyTokenizerDefaults = true
        });

        string text = "Understanding special tokens";
        var encoding = tokenizer.Tokenizer.Encode(text);
        
        Console.WriteLine($"Text: {text}");
        Console.WriteLine($"Full encoding with special tokens:");
        Console.WriteLine($"  Tokens: [{string.Join(", ", encoding.Tokens)}]");
        Console.WriteLine($"  IDs: [{string.Join(", ", encoding.Ids)}]");
        
        // BERT adds [CLS] at start and [SEP] at end
        Console.WriteLine($"First token (CLS): '{encoding.Tokens[0]}' (ID: {encoding.Ids[0]})");
        Console.WriteLine($"Last token (SEP): '{encoding.Tokens[encoding.Length - 1]}' (ID: {encoding.Ids[encoding.Length - 1]})");
        Console.WriteLine();
    }

    private static void PaddingStrategies()
    {
        Console.WriteLine("--- Example 5: Padding Strategies (Right vs Left) ---");

        var modelDirectory = ResolveModelDirectory("all-minilm-l6-v2");

        using var tokenizer = AutoTokenizer.Load(modelDirectory, new AutoTokenizerLoadOptions
        {
            ApplyTokenizerDefaults = false  // Manual control
        });

        string text = "Hello, world!";

        // Right padding (default for encoder models like BERT)
        tokenizer.Tokenizer.EnablePadding(new PaddingOptions(
            direction: PaddingDirection.Right,
            padId: 0,
            padTypeId: 0,
            padToken: "[PAD]",
            length: 20));

        var rightPadded = tokenizer.Tokenizer.Encode(text);
        Console.WriteLine($"Right-padded to 20 tokens:");
        Console.WriteLine($"  Tokens: [{string.Join(", ", rightPadded.Tokens.Take(20))}]");

        // Left padding (common for decoder models like GPT)
        tokenizer.Tokenizer.EnablePadding(new PaddingOptions(
            direction: PaddingDirection.Left,
            padId: 0,
            padTypeId: 0,
            padToken: "[PAD]",
            length: 20));

        var leftPadded = tokenizer.Tokenizer.Encode(text);
        Console.WriteLine($"\nLeft-padded to 20 tokens:");
        Console.WriteLine($"  First 5 tokens: [{string.Join(", ", leftPadded.Tokens.Take(5))}]");
        Console.WriteLine($"  Last 5 tokens: [{string.Join(", ", leftPadded.Tokens.TakeLast(5))}]");

        tokenizer.Tokenizer.DisablePadding();
        Console.WriteLine();
    }

    private static void TruncationStrategies()
    {
        Console.WriteLine("--- Example 6: Truncation Strategies ---");

        var modelDirectory = ResolveModelDirectory("all-minilm-l6-v2");

        using var tokenizer = AutoTokenizer.Load(modelDirectory, new AutoTokenizerLoadOptions
        {
            ApplyTokenizerDefaults = false
        });

        string longText = "This is a very long text that will be truncated. " +
                         "It contains many words and will exceed the maximum token limit. " +
                         "The truncation strategy determines which tokens to keep. " +
                         "This demonstrates different approaches to handling overflow.";

        // Strategy 1: Truncate from the right (keep beginning)
        tokenizer.Tokenizer.EnableTruncation(new TruncationOptions(
            maxLength: 20,
            stride: 0,
            strategy: TruncationStrategy.LongestFirst,
            direction: TruncationDirection.Right));

        var rightTrunc = tokenizer.Tokenizer.Encode(longText);
        Console.WriteLine($"Right truncation (max 20 tokens):");
        Console.WriteLine($"  Kept: {rightTrunc.Length} tokens");
        Console.WriteLine($"  First 10: [{string.Join(", ", rightTrunc.Tokens.Take(10))}]");

        // Strategy 2: Truncate from the left (keep ending)
        tokenizer.Tokenizer.EnableTruncation(new TruncationOptions(
            maxLength: 20,
            stride: 0,
            strategy: TruncationStrategy.LongestFirst,
            direction: TruncationDirection.Left));

        var leftTrunc = tokenizer.Tokenizer.Encode(longText);
        Console.WriteLine($"\nLeft truncation (max 20 tokens):");
        Console.WriteLine($"  Kept: {leftTrunc.Length} tokens");
        Console.WriteLine($"  Last 10: [{string.Join(", ", leftTrunc.Tokens.TakeLast(10))}]");

        tokenizer.Tokenizer.DisableTruncation();
        Console.WriteLine();
    }

    private static void TextPairEncoding()
    {
        Console.WriteLine("--- Example 7: Text Pair Encoding (Sentence Classification) ---");

        var modelDirectory = ResolveModelDirectory("all-minilm-l6-v2");

        using var tokenizer = AutoTokenizer.Load(modelDirectory, new AutoTokenizerLoadOptions
        {
            ApplyTokenizerDefaults = true
        });

        // Encode a text pair (e.g., for semantic similarity or NLI)
        string text1 = "The cat sits on the mat.";
        string text2 = "A feline rests on the rug.";

        var pairEncoding = tokenizer.Tokenizer.Encode(text1, text2, addSpecialTokens: true);

        Console.WriteLine($"Text 1: {text1}");
        Console.WriteLine($"Text 2: {text2}");
        Console.WriteLine($"\nPair encoding:");
        Console.WriteLine($"  Total tokens: {pairEncoding.Length}");
        Console.WriteLine($"  Tokens: [{string.Join(", ", pairEncoding.Tokens)}]");
        Console.WriteLine($"\nType IDs (0=first, 1=second):");
        Console.WriteLine($"  [{string.Join(", ", pairEncoding.TypeIds)}]");
        Console.WriteLine();
    }

    private static void AttentionMasksAndTypeIds()
    {
        Console.WriteLine("--- Example 8: Attention Masks and Type IDs ---");

        var modelDirectory = ResolveModelDirectory("all-minilm-l6-v2");

        using var tokenizer = AutoTokenizer.Load(modelDirectory, new AutoTokenizerLoadOptions
        {
            ApplyTokenizerDefaults = false
        });

        // Enable padding to see attention mask clearly
        tokenizer.Tokenizer.EnablePadding(new PaddingOptions(
            direction: PaddingDirection.Right,
            padId: 0,
            padTypeId: 0,
            padToken: "[PAD]",
            length: 15));

        string text = "Attention is all you need.";
        var encoding = tokenizer.Tokenizer.Encode(text);

        Console.WriteLine($"Text: {text}");
        Console.WriteLine($"Token count: {encoding.Length}");
        Console.WriteLine($"\nTokens:");
        Console.WriteLine($"  [{string.Join(", ", encoding.Tokens)}]");
        Console.WriteLine($"\nAttention Mask (1=real, 0=padding):");
        Console.WriteLine($"  [{string.Join(", ", encoding.AttentionMask)}]");
        Console.WriteLine($"\nType IDs (segment IDs):");
        Console.WriteLine($"  [{string.Join(", ", encoding.TypeIds)}]");
        Console.WriteLine($"\nSpecial Tokens Mask (1=special, 0=regular):");
        Console.WriteLine($"  [{string.Join(", ", encoding.SpecialTokensMask)}]");

        tokenizer.Tokenizer.DisablePadding();
        Console.WriteLine();
    }

    private static void OffsetMapping()
    {
        Console.WriteLine("--- Example 9: Offset Mapping (Character Positions) ---");

        var modelDirectory = ResolveModelDirectory("all-minilm-l6-v2");

        using var tokenizer = AutoTokenizer.Load(modelDirectory, new AutoTokenizerLoadOptions
        {
            ApplyTokenizerDefaults = true
        });

        string text = "Machine learning is amazing!";
        var encoding = tokenizer.Tokenizer.Encode(text);

        Console.WriteLine($"Text: \"{text}\"");
        Console.WriteLine($"\nToken offsets (character positions):");

        for (int i = 0; i < Math.Min(10, encoding.Length); i++)
        {
            var token = encoding.Tokens[i];
            var (start, end) = encoding.Offsets[i];
            
            if (start > 0 || end > 0)
            {
                string substring = text.Substring(start, end - start);
                Console.WriteLine($"  Token {i}: '{token}' → chars[{start}:{end}] = \"{substring}\"");
            }
            else
            {
                Console.WriteLine($"  Token {i}: '{token}' → special token (no offset)");
            }
        }
        Console.WriteLine();
    }

    private static void WordAndSequenceIds()
    {
        Console.WriteLine("--- Example 10: Word IDs and Sequence IDs ---");

        var modelDirectory = ResolveModelDirectory("all-minilm-l6-v2");

        using var tokenizer = AutoTokenizer.Load(modelDirectory, new AutoTokenizerLoadOptions
        {
            ApplyTokenizerDefaults = true
        });

        string text = "WordPiece tokenization splits words.";
        var encoding = tokenizer.Tokenizer.Encode(text);

        Console.WriteLine($"Text: {text}");
        Console.WriteLine($"\nWord IDs (which word each token belongs to):");

        for (int i = 0; i < Math.Min(12, encoding.Length); i++)
        {
            var token = encoding.Tokens[i];
            var wordId = encoding.WordIds[i];
            var seqId = encoding.SequenceIds[i];

            Console.WriteLine($"  Token {i}: '{token}' → Word {wordId?.ToString() ?? "null"}, Seq {seqId?.ToString() ?? "null"}");
        }
        Console.WriteLine();
    }

    private static void ChatTemplateRendering()
    {
        Console.WriteLine("--- Example 11: Chat Template Rendering ---");

        var modelDirectory = ResolveModelDirectory("meta-llama-3-8b-instruct");

        try
        {
            using var tokenizer = AutoTokenizer.Load(modelDirectory, new AutoTokenizerLoadOptions
            {
                ApplyTokenizerDefaults = true,
                LoadGenerationConfig = true
            });

            if (!tokenizer.SupportsChatTemplate)
            {
                Console.WriteLine("⚠️  This model doesn't have a chat template.");
                Console.WriteLine("    Chat templates are model-specific and defined in tokenizer_config.json");
                Console.WriteLine();
                return;
            }

            // Create a conversation
            var messages = new[]
            {
                ChatMessage.FromText("system", "You are a helpful AI assistant."),
                ChatMessage.FromText("user", "What is machine learning?"),
                ChatMessage.FromText("assistant", "Machine learning is a subset of AI that enables systems to learn from data."),
                ChatMessage.FromText("user", "Can you explain transformers?")
            };

            // Render as prompt text
            var options = new ChatTemplateOptions { AddGenerationPrompt = true };
            string prompt = tokenizer.ApplyChatTemplate(messages, options);

            Console.WriteLine("Conversation:");
            foreach (var msg in messages)
            {
                string contentPreview = msg.Content?.Substring(0, Math.Min(60, msg.Content.Length)) ?? "";
                Console.WriteLine($"  {msg.Role}: {contentPreview}");
            }

            Console.WriteLine($"\nRendered prompt:\n{prompt}");

            // Encode directly to tokens
            var encoding = tokenizer.ApplyChatTemplateAsEncoding(messages, options);
            Console.WriteLine($"\nEncoded tokens: {encoding.Length} tokens");
            Console.WriteLine($"First 20 token IDs: [{string.Join(", ", encoding.Ids.Take(20))}]");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  Chat template example skipped: {ex.Message}");
        }

        Console.WriteLine();
    }

    private static void CustomPaddingAndTruncation()
    {
        Console.WriteLine("--- Example 12: Custom Padding and Truncation Combined ---");

        var modelDirectory = ResolveModelDirectory("all-minilm-l6-v2");

        using var tokenizer = AutoTokenizer.Load(modelDirectory, new AutoTokenizerLoadOptions
        {
            ApplyTokenizerDefaults = false
        });

        // Configure both padding and truncation
        tokenizer.Tokenizer.EnablePadding(new PaddingOptions(
            direction: PaddingDirection.Right,
            padId: 0,
            padTypeId: 0,
            padToken: "[PAD]",
            length: 32));

        tokenizer.Tokenizer.EnableTruncation(new TruncationOptions(
            maxLength: 32,
            stride: 0,
            strategy: TruncationStrategy.LongestFirst,
            direction: TruncationDirection.Right));

        var texts = new[]
        {
            "Short text.",
            "This is a medium-length text with several words.",
            "This is a very long text that will definitely exceed the maximum token limit and will be truncated from the right side while short texts will be padded to reach the target length."
        };

        Console.WriteLine("Processing texts with padding AND truncation (target: 32 tokens):");
        foreach (var text in texts)
        {
            var encoding = tokenizer.Tokenizer.Encode(text);
            int realTokens = encoding.AttentionMask.Count(m => m == 1);
            int padTokens = encoding.AttentionMask.Count(m => m == 0);

            Console.WriteLine($"\n  Input: {text.Substring(0, Math.Min(60, text.Length))}...");
            Console.WriteLine($"  Result: {encoding.Length} tokens (real: {realTokens}, padding: {padTokens})");
        }

        Console.WriteLine();
    }

    private static void VocabularyAccess()
    {
        Console.WriteLine("--- Example 13: Vocabulary Access (Token ↔ ID) ---");

        var modelDirectory = ResolveModelDirectory("all-minilm-l6-v2");

        using var tokenizer = AutoTokenizer.Load(modelDirectory, new AutoTokenizerLoadOptions
        {
            ApplyTokenizerDefaults = true
        });

        // Token to ID lookup
        Console.WriteLine("Token to ID lookup:");
        var sampleTokens = new[] { "[CLS]", "[SEP]", "[PAD]", "[UNK]", "hello", "world", "##ing" };
        
        foreach (var token in sampleTokens)
        {
            int? tokenId = tokenizer.Tokenizer.TokenToId(token);
            if (tokenId.HasValue)
            {
                Console.WriteLine($"  '{token}' → ID {tokenId.Value}");
            }
            else
            {
                Console.WriteLine($"  '{token}' → NOT FOUND");
            }
        }

        // ID to Token lookup
        Console.WriteLine("\nID to Token lookup:");
        var sampleIds = new[] { 101, 102, 0, 100, 7592, 2088, 1010 };

        foreach (var id in sampleIds)
        {
            string? token = tokenizer.Tokenizer.IdToToken(id);
            Console.WriteLine($"  ID {id} → '{token ?? "NOT FOUND"}'");
        }

        Console.WriteLine();
    }

    private static void MultimpleModels()
    {
        Console.WriteLine("--- Example 14: Working with Multiple Models (WordPiece, BPE, Unigram) ---");

        string testText = "tokenization";

        // WordPiece (BERT-based models)
        Console.WriteLine("WordPiece tokenizer (all-minilm-l6-v2):");
        var bertDir = ResolveModelDirectory("all-minilm-l6-v2");
        using var bertTokenizer = AutoTokenizer.Load(bertDir, new AutoTokenizerLoadOptions
        {
            ApplyTokenizerDefaults = true
        });
        
        var bertEncoding = bertTokenizer.Tokenizer.Encode(testText);
        Console.WriteLine($"  Text: {testText}");
        Console.WriteLine($"  Tokens: [{string.Join(", ", bertEncoding.Tokens)}]");
        Console.WriteLine($"  Token count: {bertEncoding.Length}");

        // Unigram (T5-based models)
        Console.WriteLine("\nUnigram tokenizer (t5-small):");
        var t5Dir = ResolveModelDirectory("t5-small");
        using var t5Tokenizer = AutoTokenizer.Load(t5Dir, new AutoTokenizerLoadOptions
        {
            ApplyTokenizerDefaults = true
        });
        
        var t5Encoding = t5Tokenizer.Tokenizer.Encode(testText);
        Console.WriteLine($"  Text: {testText}");
        Console.WriteLine($"  Tokens: [{string.Join(", ", t5Encoding.Tokens)}]");
        Console.WriteLine($"  Token count: {t5Encoding.Length}");

        Console.WriteLine("\n💡 Different tokenization algorithms:");
        Console.WriteLine("   - WordPiece (BERT): Greedy longest-match-first, uses ## prefix");
        Console.WriteLine("   - Unigram (T5): Probabilistic subword segmentation, uses ▁ prefix");
        Console.WriteLine("   - BPE (GPT): Byte-pair encoding based on merge rules");
        Console.WriteLine("💡 Always use the tokenizer that matches your model!");
        Console.WriteLine();
    }

    private static void OverflowingTokens()
    {
        Console.WriteLine("--- Example 15: Overflowing Tokens (Stride/Windowing) ---");

        var modelDirectory = ResolveModelDirectory("all-minilm-l6-v2");

        using var tokenizer = AutoTokenizer.Load(modelDirectory, new AutoTokenizerLoadOptions
        {
            ApplyTokenizerDefaults = false
        });

        // Enable truncation with stride to capture overflowing tokens
        tokenizer.Tokenizer.EnableTruncation(new TruncationOptions(
            maxLength: 20,
            stride: 5,  // Overlap between chunks
            strategy: TruncationStrategy.LongestFirst,
            direction: TruncationDirection.Right));

        string longText = "This is a very long document that exceeds the maximum token limit. " +
                         "When truncated with stride, the library can capture the overflowing tokens. " +
                         "This is useful for processing long documents in overlapping windows.";

        var encoding = tokenizer.Tokenizer.Encode(longText);

        Console.WriteLine($"Main encoding: {encoding.Length} tokens");
        Console.WriteLine($"Overflowing encodings: {encoding.Overflowing.Count}");

        if (encoding.Overflowing.Count > 0)
        {
            Console.WriteLine("\n📊 Encoding breakdown:");
            Console.WriteLine($"  Main: {encoding.Length} tokens");
            
            for (int i = 0; i < encoding.Overflowing.Count; i++)
            {
                var overflow = encoding.Overflowing[i];
                Console.WriteLine($"  Overflow {i + 1}: {overflow.Length} tokens");
            }

            Console.WriteLine("\n💡 Use stride to process long documents in overlapping chunks");
        }
        else
        {
            Console.WriteLine("  (Text fits within max length, no overflow)");
        }

        Console.WriteLine();
    }

    private static void AddingCustomTokens()
    {
        Console.WriteLine("--- Example 16: Working with Special Tokens and Vocabulary ---");

        var modelDirectory = ResolveModelDirectory("all-minilm-l6-v2");

        using var tokenizer = AutoTokenizer.Load(modelDirectory, new AutoTokenizerLoadOptions
        {
            ApplyTokenizerDefaults = true
        });

        // Inspect existing special tokens
        Console.WriteLine("Existing special tokens:");
        var specialTokens = new[] { "[CLS]", "[SEP]", "[PAD]", "[UNK]", "[MASK]" };
        foreach (var token in specialTokens)
        {
            int? tokenId = tokenizer.Tokenizer.TokenToId(token);
            if (tokenId.HasValue)
            {
                Console.WriteLine($"  {token} → ID {tokenId.Value}");
            }
        }

        // Demonstrate how special tokens are handled
        Console.WriteLine("\nEncoding with vs without special tokens:");
        
        string text = "Hello world";
        var withSpecial = tokenizer.Tokenizer.Encode(text, addSpecialTokens: true);
        var withoutSpecial = tokenizer.Tokenizer.Encode(text, addSpecialTokens: false);

        Console.WriteLine($"  With special tokens: [{string.Join(", ", withSpecial.Tokens.Take(10))}]");
        Console.WriteLine($"    Token count: {withSpecial.Length}");
        Console.WriteLine($"  Without special tokens: [{string.Join(", ", withoutSpecial.Tokens.Take(10))}]");
        Console.WriteLine($"    Token count: {withoutSpecial.Length}");

        // Demonstrate vocabulary lookup
        Console.WriteLine("\nVocabulary lookup examples:");
        var testTokens = new[] { "hello", "##world", "tokenization", "##ization", "AI" };
        foreach (var token in testTokens)
        {
            int? tokenId = tokenizer.Tokenizer.TokenToId(token);
            if (tokenId.HasValue)
            {
                string? roundTrip = tokenizer.Tokenizer.IdToToken(tokenId.Value);
                Console.WriteLine($"  '{token}' → ID {tokenId.Value} → '{roundTrip}'");
            }
            else
            {
                Console.WriteLine($"  '{token}' → NOT IN VOCABULARY");
            }
        }

        // Show how unknown tokens are handled
        Console.WriteLine("\nUnknown token handling:");
        string unknownText = "supercalifragilisticexpialidocious";
        var unknownEncoding = tokenizer.Tokenizer.Encode(unknownText, addSpecialTokens: false);
        Console.WriteLine($"  Text: {unknownText}");
        Console.WriteLine($"  Tokens: [{string.Join(", ", unknownEncoding.Tokens)}]");
        Console.WriteLine($"  (WordPiece splits unknown words into known subwords)");

        Console.WriteLine("\n💡 Key Takeaways:");
        Console.WriteLine("   - Special tokens ([CLS], [SEP], [PAD]) have fixed IDs");
        Console.WriteLine("   - Use addSpecialTokens parameter to control inclusion");
        Console.WriteLine("   - WordPiece tokenization handles unknown words via subword splitting");
        Console.WriteLine("   - TokenToId() and IdToToken() provide bidirectional vocabulary lookup");
        Console.WriteLine("\n💡 Advanced: Adding custom tokens requires modifying tokenizer.json");
        Console.WriteLine("   and retraining or fine-tuning the model with new vocabulary");

        Console.WriteLine();
    }

    private static string ResolveModelDirectory(string modelId)
    {
        // Look in quickstart .models first
        var localRelative = Path.Combine(".models", modelId);
        var localPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, localRelative));

        if (Directory.Exists(localPath))
        {
            return localPath;
        }

        // Fall back to examples root .models
        var fallbackRelative = Path.Combine("..", "..", "..", "..", "..", ".models", modelId);
        var fallbackPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, fallbackRelative));

        if (!Directory.Exists(fallbackPath))
        {
            throw new DirectoryNotFoundException(
                $"Model directory '{modelId}' not found. Tried:\n" +
                $"  1. {localPath}\n" +
                $"  2. {fallbackPath}\n" +
                $"Please ensure the model is in the .models directory.");
        }

        return fallbackPath;
    }
}
