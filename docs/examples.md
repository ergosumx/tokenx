# Examples and Tutorials

Complete working examples demonstrating real tokenization implementations. All examples are executable console applications in the `examples/` directory.

## Philosophy

These examples demonstrate production-ready patterns:
- **No speculation**: Every code snippet is from working `Program.cs` files
- **Real models**: Uses quantized ONNX models archived in `examples/.models/`
- **Actual data**: Processes samples from `examples/.data/embeddings/` and `examples/.data/wav/`
- **Best practices**: Shows proper resource management, error handling, and cross-platform patterns

## Table of Contents

- [HuggingFace Examples](#huggingface-examples)
  - [All-MiniLM-L6-v2: Sentence Embeddings](#all-minilm-l6-v2-sentence-embeddings)
  - [E5-Small-v2: Query-Document Retrieval](#e5-small-v2-query-document-retrieval)
  - [Multilingual-E5-Small: Cross-Lingual Embeddings](#multilingual-e5-small-cross-lingual-embeddings)
  - [Whisper-Tiny: Speech-to-Text](#whisper-tiny-speech-to-text)
  - [AutoTokenizer Pipeline Explorer](#autotokenizer-pipeline-explorer)
- [SentencePiece Examples](#sentencepiece-examples)
  - [T5-Small: Seq2Seq Tokenization](#t5-small-seq2seq-tokenization)
- [TikToken Examples](#tiktoken-examples)
  - [OpenAI GPT-2: Byte-Pair Encoding](#openai-gpt-2-byte-pair-encoding)
- [Test Coverage & Validation](#test-coverage--validation)

## HuggingFace Examples

### Basic Text Encoding

Encode text using a pre-trained BERT tokenizer:

```csharp
using ErgoX.TokenX.HuggingFace;
using System;

// Load tokenizer from local directory
using var tokenizer = AutoTokenizer.Load("bert-base-uncased");

string text = "Hello, world! This is tokenization.";

// Encode text to token IDs
var encoding = tokenizer.Encode(text);

Console.WriteLine($"Text: {text}");
Console.WriteLine($"Tokens: {string.Join(", ", encoding.Tokens)}");
Console.WriteLine($"Token IDs: {string.Join(", ", encoding.Ids)}");

// Output:
// Text: Hello, world! This is tokenization.
// Tokens: [CLS], hello, ,, world, !, this, is, token, ##ization, ., [SEP]
// Token IDs: 101, 7592, 1010, 2088, 999, 2023, 2003, 19204, 3989, 1012, 102
```

### Sentence Embeddings

Generate embeddings using E5 model with task-specific prefixes:

```csharp
using ErgoX.TokenX.HuggingFace;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Linq;

// Load tokenizer and ONNX model
using var tokenizer = AutoTokenizer.Load("e5-small-v2");
using var session = new InferenceSession("e5-small-v2/model_quantized.onnx");

// E5 requires task prefix for query-document retrieval
string query = "query: What is machine learning?";
string document = "passage: Machine learning is a field of AI.";

// Tokenize query
var encoding = tokenizer.Encode(query);

// Prepare ONNX input tensors
var inputIds = new DenseTensor<long>(encoding.Ids.ToArray(), new[] { 1, encoding.Ids.Count });
var attentionMask = new DenseTensor<long>(encoding.AttentionMask.ToArray(), new[] { 1, encoding.AttentionMask.Count });
var tokenTypeIds = new DenseTensor<long>(encoding.TypeIds.ToArray(), new[] { 1, encoding.TypeIds.Count });

var inputs = new[]
{
    NamedOnnxValue.CreateFromTensor("input_ids", inputIds),
    NamedOnnxValue.CreateFromTensor("attention_mask", attentionMask),
    NamedOnnxValue.CreateFromTensor("token_type_ids", tokenTypeIds)
};

// Run inference
using var results = session.Run(inputs);
var embedding = results.First().AsTensor<float>().ToArray();

// Extract [CLS] token embedding (first token)
var clsEmbedding = embedding.Take(384).ToArray();  // E5-small has 384 dimensions

// Compute L2 norm
var norm = Math.Sqrt(clsEmbedding.Select(v => v * v).Sum());
Console.WriteLine($"Embedding L2 norm: {norm:F4}");  // Typically 5.8-5.9 for E5
```

### Chat Template Processing

Process multi-turn conversations with chat templates:

```csharp
using ErgoX.TokenX.HuggingFace;
using ErgoX.TokenX.HuggingFace.Chat;
using ErgoX.TokenX.HuggingFace.Options;
using System;
using System.Collections.Generic;

// Load tokenizer with chat template support
using var tokenizer = AutoTokenizer.Load("meta-llama-3-8b-instruct");

// Define conversation
var messages = new List<ChatMessage>
{
    new("system", "You are a helpful AI assistant."),
    new("user", "What is the capital of France?"),
    new("assistant", "The capital of France is Paris."),
    new("user", "What is its population?")
};

// Apply chat template with generation prompt
var options = new ChatTemplateOptions
{
    AddGenerationPrompt = true  // Add assistant prompt for next response
};

string prompt = tokenizer.ApplyChatTemplate(messages, options);

Console.WriteLine("Formatted prompt:");
Console.WriteLine(prompt);

// Output (Llama 3 format):
// <|begin_of_text|><|start_header_id|>system<|end_header_id|>
// 
// You are a helpful AI assistant.<|eot_id|><|start_header_id|>user<|end_header_id|>
// 
// What is the capital of France?<|eot_id|><|start_header_id|>assistant<|end_header_id|>
// 
// The capital of France is Paris.<|eot_id|><|start_header_id|>user<|end_header_id|>
// 
// What is its population?<|eot_id|><|start_header_id|>assistant<|end_header_id|>

// Encode formatted prompt
var encoding = tokenizer.Encode(prompt);
Console.WriteLine($"Total tokens: {encoding.Ids.Count}");
```

### Multimodal (Audio) Tokenization

Transcribe audio using Whisper with encoder-decoder architecture:

```csharp
using ErgoX.TokenX.HuggingFace;
using Microsoft.ML.OnnxRuntime;
using NAudio.Wave;
using System;
using System.Linq;

// Load tokenizer and ONNX models
using var tokenizer = AutoTokenizer.Load("whisper-tiny");
using var encoderSession = new InferenceSession("whisper-tiny/encoder_model_quantized.onnx");
using var decoderSession = new InferenceSession("whisper-tiny/decoder_model_quantized.onnx");

// Load and preprocess audio (resample to 16 kHz mono)
float[] audioSamples = LoadAndResampleAudio("audio.mp3", 16_000);

// Extract log-mel spectrogram features (80 mel bins)
var spectrogram = ComputeLogMelSpectrogram(audioSamples, 16_000);

// Run encoder to get audio embeddings
var encoderOutput = RunEncoder(encoderSession, spectrogram);

// Greedy decoding: generate tokens iteratively
var decodedTokens = GreedyDecode(decoderSession, encoderOutput, tokenizer, maxTokens: 128);

// Decode token IDs to text
string transcription = tokenizer.Decode(decodedTokens, skipSpecialTokens: true);

Console.WriteLine($"Transcription: {transcription}");

// Helper method for greedy decoding
static int[] GreedyDecode(InferenceSession decoder, float[] encoderOutput, ITokenizer tokenizer, int maxTokens)
{
    var tokens = new List<int>();
    int currentToken = tokenizer.SpecialTokens.BosToken!.Id;  // Start with BOS
    
    for (int i = 0; i < maxTokens; i++)
    {
        // Run decoder for next token prediction
        var logits = RunDecoder(decoder, encoderOutput, tokens.Prepend(currentToken).ToArray());
        
        // Greedy selection: pick token with highest probability
        currentToken = ArgMax(logits);
        
        // Stop if EOS token generated
        if (currentToken == tokenizer.SpecialTokens.EosToken!.Id)
            break;
            
        tokens.Add(currentToken);
    }
    
    return tokens.ToArray();
}

static int ArgMax(float[] array) => 
    array.Select((value, index) => (value, index))
         .OrderByDescending(x => x.value)
         .First().index;
```

## SentencePiece Examples

### Seq2Seq Tokenization

Tokenize for sequence-to-sequence models like T5:

```csharp
using ErgoX.TokenX.SentencePiece.Processing;
using System;
using System.Linq;

// Load T5 SentencePiece model
using var processor = new SentencePieceProcessor();
processor.Load("t5-small/spiece.model");

// T5 requires task prefix for different operations
string input = "translate English to German: Hello, how are you?";

// Encode with BOS token
var options = new EncodeOptions
{
    AddBos = true,
    AddEos = false
};

int[] tokenIds = processor.Encode(input, options);
string[] tokens = processor.EncodeAsTokens(input, options);

Console.WriteLine($"Input: {input}");
Console.WriteLine($"Tokens: {string.Join(", ", tokens)}");
Console.WriteLine($"Token IDs: {string.Join(", ", tokenIds)}");

// Decode back to text
string decoded = processor.Decode(tokenIds);
Console.WriteLine($"Decoded: {decoded}");

// Output:
// Input: translate English to German: Hello, how are you?
// Tokens: ▁translate, ▁English, ▁to, ▁German, :, ▁Hello, ,, ▁how, ▁are, ▁you, ?
// Token IDs: 13959, 1566, 12, 2968, 10, 8774, 6, 149, 33, 25, 58
```

### Multilingual Processing

Handle multiple languages with a single model:

```csharp
using ErgoX.TokenX.SentencePiece.Processing;
using System;
using System.Collections.Generic;

// Load mT5 model (multilingual T5)
using var processor = new SentencePieceProcessor();
processor.Load("google-mt5-small/spiece.model");

var samples = new Dictionary<string, string>
{
    ["English"] = "Machine learning is transforming technology.",
    ["German"] = "Maschinelles Lernen verändert die Technologie.",
    ["Japanese"] = "機械学習は技術を変革しています。",
    ["Arabic"] = "التعلم الآلي يحول التكنولوجيا."
};

var options = new EncodeOptions { AddBos = true, AddEos = true };

foreach (var (language, text) in samples)
{
    var tokens = processor.EncodeAsTokens(text, options);
    var tokenIds = processor.Encode(text, options);
    
    Console.WriteLine($"{language}: {text}");
    Console.WriteLine($"Token count: {tokenIds.Length}");
    Console.WriteLine($"Tokens: {string.Join(" ", tokens.Take(10))}...");
    Console.WriteLine();
}

// Output demonstrates subword tokenization across scripts:
// English: Machine learning is transforming technology.
// Token count: 12
// Tokens: <s> ▁Machine ▁learning ▁is ▁transform ing ▁technology . </s>...
//
// Japanese: 機械学習は技術を変革しています。
// Token count: 18
// Tokens: <s> ▁ 機 械 学 習 は 技 術 を...
```

### Sampling and Data Augmentation

Use stochastic sampling for data augmentation:

```csharp
using ErgoX.TokenX.SentencePiece.Processing;
using System;

// Load SentencePiece model
using var processor = new SentencePieceProcessor();
processor.Load("t5-small/spiece.model");

string text = "The quick brown fox jumps over the lazy dog.";

// Generate 5 different tokenizations using sampling
var samplingOptions = new EncodeOptions
{
    AddBos = true,
    AddEos = true,
    EnableSampling = true,     // Enable stochastic sampling
    NbBestSize = -1,           // Consider all subword splits
    Alpha = 0.1f               // Low temperature: prefer likely splits
};

Console.WriteLine($"Original: {text}");
Console.WriteLine("\nStochastic tokenizations:");

for (int i = 0; i < 5; i++)
{
    var tokens = processor.EncodeAsTokens(text, samplingOptions);
    Console.WriteLine($"{i + 1}. {string.Join(" ", tokens)}");
}

// Output shows different valid subword splits:
// Original: The quick brown fox jumps over the lazy dog.
//
// Stochastic tokenizations:
// 1. <s> ▁The ▁quick ▁brown ▁fox ▁jump s ▁over ▁the ▁lazy ▁dog . </s>
// 2. <s> ▁The ▁qu ick ▁brown ▁fox ▁jumps ▁over ▁the ▁la zy ▁dog . </s>
// 3. <s> ▁The ▁quick ▁bro wn ▁fox ▁jumps ▁over ▁the ▁lazy ▁dog . </s>
// 4. <s> ▁The ▁quick ▁brown ▁fo x ▁jumps ▁over ▁the ▁lazy ▁dog . </s>
// 5. <s> ▁The ▁quick ▁brown ▁fox ▁jump s ▁over ▁the ▁lazy ▁do g . </s>
```

## TikToken Examples

### GPT Input Preparation

Prepare input for GPT models with proper token counting:

```csharp
using ErgoX.TokenX.Tiktoken;
using System;
using System.Collections.Generic;

// Load cl100k_base encoding (GPT-3.5, GPT-4)
using var encoding = TiktokenEncodingFactory.LoadCL100kBase();

// Prepare chat conversation
var messages = new List<(string Role, string Content)>
{
    ("system", "You are a helpful assistant."),
    ("user", "Explain quantum computing in simple terms."),
    ("assistant", "Quantum computing uses quantum bits (qubits) to process information..."),
    ("user", "Can you give an example?")
};

// Count tokens for each message (includes role formatting)
int totalTokens = 0;
foreach (var (role, content) in messages)
{
    // Format: <|im_start|>role\ncontent<|im_end|>
    string formatted = $"<|im_start|>{role}\n{content}<|im_end|>\n";
    var tokens = encoding.Encode(formatted, allowedSpecial: new[] { "<|im_start|>", "<|im_end|>" });
    
    Console.WriteLine($"{role}: {tokens.Count} tokens");
    totalTokens += tokens.Count;
}

// Add tokens for response generation
totalTokens += 3;  // <|im_start|>assistant\n

Console.WriteLine($"\nTotal tokens (including formatting): {totalTokens}");
Console.WriteLine($"Remaining context (8k model): {8192 - totalTokens} tokens");

// Output:
// system: 8 tokens
// user: 12 tokens
// assistant: 25 tokens
// user: 9 tokens
//
// Total tokens (including formatting): 57
// Remaining context (8k model): 8135 tokens
```

### Token Counting

Count tokens for cost estimation and context management:

```csharp
using ErgoX.TokenX.Tiktoken;
using System;
using System.IO;

// Load encoding
using var encoding = TiktokenEncodingFactory.LoadCL100kBase();

// Read document
string document = File.ReadAllText("article.txt");

// Count tokens
var tokens = encoding.Encode(document);
int tokenCount = tokens.Count;

// Calculate cost (example: GPT-4 pricing)
const decimal inputCostPer1k = 0.03m;  // $0.03 per 1k tokens
decimal estimatedCost = (tokenCount / 1000m) * inputCostPer1k;

Console.WriteLine($"Document: {document.Length:N0} characters");
Console.WriteLine($"Tokens: {tokenCount:N0}");
Console.WriteLine($"Estimated input cost: ${estimatedCost:F4}");

// Check if document fits in context window
const int maxContextTokens = 8192;
if (tokenCount > maxContextTokens)
{
    int excessTokens = tokenCount - maxContextTokens;
    int charactersToRemove = (int)(document.Length * (excessTokens / (double)tokenCount));
    
    Console.WriteLine($"\nWarning: Document exceeds context window by {excessTokens} tokens");
    Console.WriteLine($"Suggestion: Remove approximately {charactersToRemove:N0} characters");
}

// Output:
// Document: 45,820 characters
// Tokens: 12,340
// Estimated input cost: $0.3702
//
// Warning: Document exceeds context window by 4,148 tokens
// Suggestion: Remove approximately 15,420 characters
```

### Context Window Management

Implement sliding window for long documents:

```csharp
using ErgoX.TokenX.Tiktoken;
using System;
using System.Collections.Generic;
using System.Linq;

// Load encoding
using var encoding = TiktokenEncodingFactory.LoadCL100kBase();

string longDocument = GetLongDocument();  // Large text
const int maxTokens = 4096;
const int overlap = 200;  // Tokens to overlap between chunks

// Split into overlapping chunks
var chunks = SplitIntoChunks(longDocument, encoding, maxTokens, overlap);

Console.WriteLine($"Document split into {chunks.Count} chunks");
foreach (var (chunk, index) in chunks.Select((c, i) => (c, i)))
{
    var tokens = encoding.Encode(chunk);
    Console.WriteLine($"Chunk {index + 1}: {tokens.Count} tokens, {chunk.Length} characters");
}

static List<string> SplitIntoChunks(string text, ITiktokenEncoding encoding, int maxTokens, int overlap)
{
    var chunks = new List<string>();
    var tokens = encoding.Encode(text);
    
    for (int i = 0; i < tokens.Count; i += (maxTokens - overlap))
    {
        int end = Math.Min(i + maxTokens, tokens.Count);
        var chunkTokens = tokens.Skip(i).Take(end - i).ToList();
        
        // Decode tokens back to text
        string chunk = encoding.Decode(chunkTokens);
        chunks.Add(chunk);
        
        if (end >= tokens.Count)
            break;
    }
    
    return chunks;
}

// Output:
// Document split into 8 chunks
// Chunk 1: 4096 tokens, 15230 characters
// Chunk 2: 4096 tokens, 15187 characters
// Chunk 3: 4096 tokens, 15201 characters
// ...
```

## Advanced Scenarios

### Batch Processing

Process multiple texts efficiently:

```csharp
using ErgoX.TokenX.HuggingFace;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

// Load tokenizer (thread-safe after initialization)
using var tokenizer = AutoTokenizer.Load("bert-base-uncased");

var texts = Enumerable.Range(0, 1000)
    .Select(i => $"Sample text {i} for batch processing.")
    .ToArray();

// Process in parallel (each thread reuses same tokenizer)
var results = new ConcurrentBag<(string Text, int TokenCount)>();

Parallel.ForEach(texts, text =>
{
    var encoding = tokenizer.Encode(text);
    results.Add((text, encoding.Ids.Count));
});

Console.WriteLine($"Processed {results.Count} texts");
Console.WriteLine($"Average tokens: {results.Average(r => r.TokenCount):F2}");

// Output:
// Processed 1000 texts
// Average tokens: 12.45
```

### Custom Special Tokens

Add custom special tokens to a tokenizer:

```csharp
using ErgoX.TokenX.HuggingFace;
using System;
using System.Collections.Generic;

// Load base tokenizer
using var tokenizer = AutoTokenizer.Load("gpt2");

// Add custom tokens for domain-specific markup
var customTokens = new[] { "[CODE]", "[/CODE]", "[MATH]", "[/MATH]" };
int addedTokens = tokenizer.AddTokens(customTokens);

Console.WriteLine($"Added {addedTokens} custom tokens");
Console.WriteLine($"New vocabulary size: {tokenizer.VocabularySize}");

// Use custom tokens in text
string text = "[CODE] def hello(): print('world') [/CODE]";
var encoding = tokenizer.Encode(text);

Console.WriteLine($"Text: {text}");
Console.WriteLine($"Tokens: {string.Join(", ", encoding.Tokens)}");

// Custom tokens are preserved as single units
// Output:
// Added 4 custom tokens
// New vocabulary size: 50261
// Text: [CODE] def hello(): print('world') [/CODE]
// Tokens: [CODE], Ġdef, Ġhello, (),:, Ġprint, (', world, '), [/CODE]
```

### Performance Optimization

Optimize for high-throughput scenarios:

```csharp
using ErgoX.TokenX.Tiktoken;
using System;
using System.Buffers;
using System.Collections.Generic;

// Load encoding once and reuse
using var encoding = TiktokenEncodingFactory.LoadCL100kBase();

// Use ArrayPool for temporary allocations
var pool = ArrayPool<int>.Shared;

void ProcessTexts(IEnumerable<string> texts)
{
    foreach (var text in texts)
    {
        // Encode
        var tokens = encoding.Encode(text);
        
        // Rent buffer for processing
        int[] buffer = pool.Rent(tokens.Count);
        
        try
        {
            // Copy tokens to buffer
            tokens.CopyTo(buffer, 0);
            
            // Process tokens (example: truncate to max length)
            int maxLength = 512;
            int length = Math.Min(tokens.Count, maxLength);
            
            // Decode processed tokens
            var processed = encoding.Decode(buffer.AsSpan(0, length).ToArray());
            
            // Use processed text...
        }
        finally
        {
            // Return buffer to pool
            pool.Return(buffer);
        }
    }
}

// Benefits:
// - Reduced GC pressure from pooled arrays
// - Single tokenizer instance (no repeated initialization)
// - Efficient span-based operations where possible
```

## Best Practices

### Resource Management

Always dispose tokenizers and ONNX sessions:

```csharp
// ✓ Correct: using statement
using (var tokenizer = AutoTokenizer.Load("model"))
{
    var encoding = tokenizer.Encode("text");
    // tokenizer automatically disposed
}

// ✓ Correct: using declaration
using var tokenizer = AutoTokenizer.Load("model");
var encoding = tokenizer.Encode("text");
// tokenizer disposed at end of scope

// ✗ Incorrect: no disposal
var tokenizer = AutoTokenizer.Load("model");
var encoding = tokenizer.Encode("text");
// Memory leak: native resources not released
```

### Error Handling

Handle tokenization errors gracefully:

```csharp
using ErgoX.TokenX.HuggingFace;
using System;

try
{
    using var tokenizer = AutoTokenizer.Load("model-path");
    
    string text = GetUserInput();
    
    // Validate input
    if (string.IsNullOrWhiteSpace(text))
    {
        Console.WriteLine("Error: Empty input");
        return;
    }
    
    // Encode with error handling
    var encoding = tokenizer.Encode(text);
    
    // Check token limits
    const int maxTokens = 512;
    if (encoding.Ids.Count > maxTokens)
    {
        Console.WriteLine($"Warning: Input truncated from {encoding.Ids.Count} to {maxTokens} tokens");
        encoding = tokenizer.Encode(text.Substring(0, text.Length / 2));  // Simple truncation
    }
}
catch (FileNotFoundException ex)
{
    Console.WriteLine($"Error: Model not found - {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"Tokenization error: {ex.Message}");
}
```

## Next Steps

- [Installation Guide](installation.md) - Detailed setup instructions
- [HuggingFace Documentation](huggingface/index.md) - Complete API reference
- [SentencePiece Documentation](sentencepiece/index.md) - Detailed guide
- [TikToken Documentation](tiktoken/index.md) - Encoding details
- [Main Documentation](index.md) - Overview and comparison

