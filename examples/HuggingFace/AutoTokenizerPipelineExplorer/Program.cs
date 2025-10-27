namespace Examples.HuggingFace.AutoTokenizerPipelineExplorer;

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Options;

/// <summary>
/// AutoTokenizer diagnostic tool: Inspects tokenizer configurations without running inference.
/// 
/// Purpose: Validate tokenizer setup, compare special tokens, verify model readiness
/// This is a diagnostic/validation example, NOT for production inference.
/// 
/// Unlike inference examples, this does NOT load ONNX models:
/// - No quantization concerns (tokenization is deterministic)
/// - Fully production-ready for validation pipelines
/// - Fast execution (config parsing only)
/// 
/// Pipeline:
/// 1. Load each tokenizer config from model directory
/// 2. Extract metadata: vocab size, special tokens, chat template, generation config
/// 3. Display configuration for comparison
/// 4. Run sample tokenization to verify behavior
/// </summary>
internal static class Program
{
    private static readonly string[] ModelIds =
    {
        "all-minilm-l6-v2",
        "e5-small-v2",
        "multilingual-e5-small"
    };

    private static async Task Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.WriteLine("Inspecting local Hugging Face tokenizer pipelines...");
        Console.WriteLine();

        // Load and inspect each model's tokenizer configuration
        foreach (var modelId in ModelIds)
        {
            await DescribeModelAsync(modelId).ConfigureAwait(false);
            Console.WriteLine(new string('-', 72));
        }
    }

    private static async Task DescribeModelAsync(string modelId)
    {
        // Resolve and load tokenizer asynchronously
        var modelDirectory = ResolveModelDirectory(modelId);
        using var tokenizer = await AutoTokenizer.LoadAsync(modelDirectory, new AutoTokenizerLoadOptions
        {
            ApplyTokenizerDefaults = true,
            LoadGenerationConfig = true
        }).ConfigureAwait(false);

        Console.WriteLine($"Model: {modelId}");
        Console.WriteLine($"Path:  {modelDirectory}");

        // Extract vocabulary size from tokenizer config
        // Vocab: 0 means config file is missing vocab snapshot, but tokenizer still works
        var vocabCount = tokenizer.TokenizerConfig?.Vocab.Count;
        if (vocabCount is int count)
        {
            Console.WriteLine($"Vocab: {count:N0} tokens");
        }
        else
        {
            Console.WriteLine("Vocab: unavailable (tokenizer_config.json missing vocab snapshot)");
        }
        
        // Chat template: Jinja2 template for multi-turn conversations (LLMs have this, encoders don't)
        Console.WriteLine($"Chat template available: {tokenizer.SupportsChatTemplate}");
        
        // Generation config: Default parameters for autoregressive decoding (encoders don't have this)
        Console.WriteLine($"Generation defaults available: {tokenizer.SupportsGenerationDefaults}");

        // Tokenizer version for tracking compatibility
        if (!string.IsNullOrWhiteSpace(tokenizer.TokenizerConfig?.Version))
        {
            Console.WriteLine($"Tokenizer config version: {tokenizer.TokenizerConfig?.Version}");
        }

        // Display special tokens (same for all samples, depends on tokenizer type)
        // BERT: [CLS]=101, [SEP]=102, [PAD]=0, [UNK]=100
        // XLM: <s>=0, </s>=2, <pad>=1, <unk>=3
        if (tokenizer.SpecialTokens is { } specialTokens)
        {
            PrintSpecialToken("BOS", specialTokens.BosToken);
            PrintSpecialToken("EOS", specialTokens.EosToken);
            PrintSpecialToken("UNK", specialTokens.UnknownToken);
            PrintSpecialToken("PAD", specialTokens.PadToken);

            // Additional custom special tokens beyond standard BOS/EOS/UNK/PAD
            if (specialTokens.AdditionalSpecialTokens.Count > 0)
            {
                Console.WriteLine("Additional special tokens:");
                foreach (var token in specialTokens.AdditionalSpecialTokens)
                {
                    Console.WriteLine($"  {(token.Id?.ToString() ?? "-").PadLeft(6)} -> {token.Content ?? string.Empty}");
                }
            }
        }

        // Sample tokenization to verify behavior
        // Designed to test: query prefix recognition, punctuation handling, token boundaries
        var sample = "query: Explain knowledge-grounded summarisation";
        var preview = tokenizer.Tokenizer.Encode(sample);
        Console.WriteLine("Sample token IDs:");
        Console.WriteLine(string.Join(", ", preview.Ids));
    }

    private static void PrintSpecialToken(string label, SpecialTokensMap.TokenDefinition? token)
    {
        // Format: LABEL (ID) -> CONTENT
        // Example: BOS (0) -> <s>  or  BOS (101) -> [CLS]
        if (token is null)
        {
            return;
        }

        var idText = token.Id?.ToString() ?? "-";
        var content = string.IsNullOrWhiteSpace(token.Content) ? "<none>" : token.Content;
        Console.WriteLine($"{label.PadRight(8)}: {idText.PadLeft(6)} -> {content}");
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
}
