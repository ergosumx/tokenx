# Examples and Tutorials# Examples and Tutorials# Examples and Tutorials



Complete working examples demonstrating real tokenization implementations. All examples are executable console applications in the `examples/` directory.



> **Note**: For OpenAI GPT models, consider using [Microsoft.ML.Tokenizers](https://www.nuget.org/packages/Microsoft.ML.Tokenizers/) which provides optimized `TiktokenTokenizer` implementation.Complete working examples demonstrating real tokenization implementations. All examples are executable console applications in the `examples/` directory.Complete working examples demonstrating real tokenization implementations. All examples are executable console applications in the `examples/` directory.



## Philosophy



These examples demonstrate production-ready patterns:## Philosophy## Philosophy

- **No speculation**: Every code snippet is from working `Program.cs` files

- **Real models**: Uses quantized ONNX models archived in `examples/.models/`

- **Actual data**: Processes samples from `examples/.data/embeddings/` and `examples/.data/wav/`

- **Best practices**: Shows proper resource management, error handling, and cross-platform patternsThese examples demonstrate production-ready patterns:These examples demonstrate production-ready patterns:



## Table of Contents- **No speculation**: Every code snippet is from working `Program.cs` files- **No speculation**: Every code snippet is from working `Program.cs` files



- [HuggingFace Examples](#huggingface-examples)- **Real models**: Uses quantized ONNX models archived in `examples/.models/`- **Real models**: Uses quantized ONNX models archived in `examples/.models/`

  - [Basic Text Encoding](#basic-text-encoding)

  - [Sentence Embeddings](#sentence-embeddings)- **Actual data**: Processes samples from `examples/.data/embeddings/` and `examples/.data/wav/`- **Actual data**: Processes samples from `examples/.data/embeddings/` and `examples/.data/wav/`

  - [Chat Template Processing](#chat-template-processing)

  - [Multimodal (Audio) Tokenization](#multimodal-audio-tokenization)- **Best practices**: Shows proper resource management, error handling, and cross-platform patterns- **Best practices**: Shows proper resource management, error handling, and cross-platform patterns

- [SentencePiece Examples](#sentencepiece-examples)

  - [Seq2Seq Tokenization](#seq2seq-tokenization)

  - [Multilingual Processing](#multilingual-processing)

  - [Sampling and Data Augmentation](#sampling-and-data-augmentation)## Table of Contents## Table of Contents

- [Advanced Scenarios](#advanced-scenarios)

  - [Batch Processing](#batch-processing)

  - [Custom Special Tokens](#custom-special-tokens)

  - [Performance Optimization](#performance-optimization)- [HuggingFace Examples](#huggingface-examples)- [HuggingFace Examples](#huggingface-examples)

- [Best Practices](#best-practices)

  - [All-MiniLM-L6-v2: Sentence Embeddings](#all-minilm-l6-v2-sentence-embeddings)  - [All-MiniLM-L6-v2: Sentence Embeddings](#all-minilm-l6-v2-sentence-embeddings)

## HuggingFace Examples

  - [E5-Small-v2: Query-Document Retrieval](#e5-small-v2-query-document-retrieval)  - [E5-Small-v2: Query-Document Retrieval](#e5-small-v2-query-document-retrieval)

### Basic Text Encoding

  - [Multilingual-E5-Small: Cross-Lingual Embeddings](#multilingual-e5-small-cross-lingual-embeddings)  - [Multilingual-E5-Small: Cross-Lingual Embeddings](#multilingual-e5-small-cross-lingual-embeddings)

Encode text using a pre-trained BERT tokenizer:

  - [Whisper-Tiny: Speech-to-Text](#whisper-tiny-speech-to-text)  - [Whisper-Tiny: Speech-to-Text](#whisper-tiny-speech-to-text)

```csharp

using ErgoX.TokenX.HuggingFace;  - [AutoTokenizer Pipeline Explorer](#autotokenizer-pipeline-explorer)  - [AutoTokenizer Pipeline Explorer](#autotokenizer-pipeline-explorer)

using System;

- [TikToken Examples](#tiktoken-examples)- [SentencePiece Examples](#sentencepiece-examples)

// Load tokenizer from local directory

using var tokenizer = AutoTokenizer.Load("bert-base-uncased");  - [OpenAI GPT-2: Byte-Pair Encoding](#openai-gpt-2-byte-pair-encoding)  - [T5-Small: Seq2Seq Tokenization](#t5-small-seq2seq-tokenization)



string text = "Hello, world! This is tokenization.";- [TikToken Examples](#tiktoken-examples)



// Encode text to token IDs## HuggingFace Examples  - [OpenAI GPT-2: Byte-Pair Encoding](#openai-gpt-2-byte-pair-encoding)

var encoding = tokenizer.Encode(text);

- [Test Coverage & Validation](#test-coverage--validation)

Console.WriteLine($"Text: {text}");

Console.WriteLine($"Tokens: {string.Join(", ", encoding.Tokens)}");### All-MiniLM-L6-v2: Sentence Embeddings

Console.WriteLine($"Token IDs: {string.Join(", ", encoding.Ids)}");

## HuggingFace Examples

// Output:

// Text: Hello, world! This is tokenization.**Location**: `examples/HuggingFace/AllMiniLmL6V2Console`  

// Tokens: [CLS], hello, ,, world, !, this, is, token, ##ization, ., [SEP]

// Token IDs: 101, 7592, 1010, 2088, 999, 2023, 2003, 19204, 3989, 1012, 102**Run**: `dotnet run --project examples/HuggingFace/AllMiniLmL6V2Console`### Basic Text Encoding

```



### Sentence Embeddings

Generate semantic embeddings for sentence similarity and search tasks.Encode text using a pre-trained BERT tokenizer:

Generate embeddings using E5 model with task-specific prefixes:



```csharp

using ErgoX.TokenX.HuggingFace;```csharp```csharp

using Microsoft.ML.OnnxRuntime;

using Microsoft.ML.OnnxRuntime.Tensors;using ErgoX.TokenX.HuggingFace;using ErgoX.TokenX.HuggingFace;

using System;

using System.Linq;using Microsoft.ML.OnnxRuntime;using System;



// Load tokenizer and ONNX model

using var tokenizer = AutoTokenizer.Load("e5-small-v2");

using var session = new InferenceSession("e5-small-v2/model_quantized.onnx");// Load tokenizer with configuration// Load tokenizer from local directory



// E5 requires task prefix for query-document retrievalusing var tokenizer = AutoTokenizer.Load("all-minilm-l6-v2", new AutoTokenizerLoadOptionsusing var tokenizer = AutoTokenizer.Load("bert-base-uncased");

string query = "query: What is machine learning?";

string document = "passage: Machine learning is a field of AI.";{



// Tokenize query    ApplyTokenizerDefaults = true,string text = "Hello, world! This is tokenization.";

var encoding = tokenizer.Encode(query);

    LoadGenerationConfig = true

// Prepare ONNX input tensors

var inputIds = new DenseTensor<long>(encoding.Ids.ToArray(), new[] { 1, encoding.Ids.Count });});// Encode text to token IDs

var attentionMask = new DenseTensor<long>(encoding.AttentionMask.ToArray(), new[] { 1, encoding.AttentionMask.Count });

var tokenTypeIds = new DenseTensor<long>(encoding.TypeIds.ToArray(), new[] { 1, encoding.TypeIds.Count });var encoding = tokenizer.Encode(text);



var inputs = new[]// Load ONNX model

{

    NamedOnnxValue.CreateFromTensor("input_ids", inputIds),using var session = new InferenceSession("all-minilm-l6-v2/model_quantized.onnx");Console.WriteLine($"Text: {text}");

    NamedOnnxValue.CreateFromTensor("attention_mask", attentionMask),

    NamedOnnxValue.CreateFromTensor("token_type_ids", tokenTypeIds)Console.WriteLine($"Tokens: {string.Join(", ", encoding.Tokens)}");

};

string text = "Machine learning enables computers to learn from data.";Console.WriteLine($"Token IDs: {string.Join(", ", encoding.Ids)}");

// Run inference

using var results = session.Run(inputs);

var embedding = results.First().AsTensor<float>().ToArray();

// Tokenize (BERT adds [CLS], [SEP], [PAD])// Output:

// Extract [CLS] token embedding (first token)

var clsEmbedding = embedding.Take(384).ToArray();  // E5-small has 384 dimensionsvar encoding = tokenizer.Encode(text);// Text: Hello, world! This is tokenization.



// Compute L2 norm// Tokens: [CLS], hello, ,, world, !, this, is, token, ##ization, ., [SEP]

var norm = Math.Sqrt(clsEmbedding.Select(v => v * v).Sum());

Console.WriteLine($"Embedding L2 norm: {norm:F4}");  // Typically 5.8-5.9 for E5// Prepare ONNX inputs// Token IDs: 101, 7592, 1010, 2088, 999, 2023, 2003, 19204, 3989, 1012, 102

```

var inputIds = CreateTensor(encoding.Ids);```

### Chat Template Processing

var attentionMask = CreateTensor(encoding.AttentionMask);

Process multi-turn conversations with chat templates:

var tokenTypeIds = CreateTensor(encoding.TypeIds);### Sentence Embeddings

```csharp

using ErgoX.TokenX.HuggingFace;

using ErgoX.TokenX.HuggingFace.Chat;

using ErgoX.TokenX.HuggingFace.Options;// Run inferenceGenerate embeddings using E5 model with task-specific prefixes:

using System;

using System.Collections.Generic;using var results = session.Run(new[]



// Load tokenizer with chat template support{```csharp

using var tokenizer = AutoTokenizer.Load("meta-llama-3-8b-instruct");

    NamedOnnxValue.CreateFromTensor("input_ids", inputIds),using ErgoX.TokenX.HuggingFace;

// Define conversation

var messages = new List<ChatMessage>    NamedOnnxValue.CreateFromTensor("attention_mask", attentionMask),using Microsoft.ML.OnnxRuntime;

{

    new("system", "You are a helpful AI assistant."),    NamedOnnxValue.CreateFromTensor("token_type_ids", tokenTypeIds)using Microsoft.ML.OnnxRuntime.Tensors;

    new("user", "What is the capital of France?"),

    new("assistant", "The capital of France is Paris."),});using System;

    new("user", "What is its population?")

};using System.Linq;



// Apply chat template with generation prompt// Extract [CLS] token embedding (384 dimensions)

var options = new ChatTemplateOptions

{var embedding = results.First().AsTensor<float>().ToArray();// Load tokenizer and ONNX model

    AddGenerationPrompt = true  // Add assistant prompt for next response

};var clsEmbedding = embedding.Take(384).ToArray();using var tokenizer = AutoTokenizer.Load("e5-small-v2");



string prompt = tokenizer.ApplyChatTemplate(messages, options);using var session = new InferenceSession("e5-small-v2/model_quantized.onnx");



Console.WriteLine("Formatted prompt:");Console.WriteLine($"Embedding L2 norm: {ComputeNorm(clsEmbedding):F4}");

Console.WriteLine(prompt);

```// E5 requires task prefix for query-document retrieval

// Output (Llama 3 format):

// <|begin_of_text|><|start_header_id|>system<|end_header_id|>string query = "query: What is machine learning?";

// 

// You are a helpful AI assistant.<|eot_id|><|start_header_id|>user<|end_header_id|>**Key Features**:string document = "passage: Machine learning is a field of AI.";

// 

// What is the capital of France?<|eot_id|><|start_header_id|>assistant<|end_header_id|>- BERT-based sentence embedding

// 

// The capital of France is Paris.<|eot_id|><|start_header_id|>user<|end_header_id|>- Quantized ONNX model (4× smaller)// Tokenize query

// 

// What is its population?<|eot_id|><|start_header_id|>assistant<|end_header_id|>- 384-dimensional vectorsvar encoding = tokenizer.Encode(query);



// Encode formatted prompt- Use case: semantic search, clustering

var encoding = tokenizer.Encode(prompt);

Console.WriteLine($"Total tokens: {encoding.Ids.Count}");// Prepare ONNX input tensors

```

### E5-Small-v2: Query-Document Retrievalvar inputIds = new DenseTensor<long>(encoding.Ids.ToArray(), new[] { 1, encoding.Ids.Count });

### Multimodal (Audio) Tokenization

var attentionMask = new DenseTensor<long>(encoding.AttentionMask.ToArray(), new[] { 1, encoding.AttentionMask.Count });

Transcribe audio using Whisper with encoder-decoder architecture:

**Location**: `examples/HuggingFace/E5SmallV2Console`  var tokenTypeIds = new DenseTensor<long>(encoding.TypeIds.ToArray(), new[] { 1, encoding.TypeIds.Count });

```csharp

using ErgoX.TokenX.HuggingFace;**Run**: `dotnet run --project examples/HuggingFace/E5SmallV2Console`

using Microsoft.ML.OnnxRuntime;

using NAudio.Wave;var inputs = new[]

using System;

using System.Linq;Implement semantic search with query/passage prefixes for optimal retrieval.{



// Load tokenizer and ONNX models    NamedOnnxValue.CreateFromTensor("input_ids", inputIds),

using var tokenizer = AutoTokenizer.Load("whisper-tiny");

using var encoderSession = new InferenceSession("whisper-tiny/encoder_model_quantized.onnx");```csharp    NamedOnnxValue.CreateFromTensor("attention_mask", attentionMask),

using var decoderSession = new InferenceSession("whisper-tiny/decoder_model_quantized.onnx");

using ErgoX.TokenX.HuggingFace;    NamedOnnxValue.CreateFromTensor("token_type_ids", tokenTypeIds)

// Load and preprocess audio (resample to 16 kHz mono)

float[] audioSamples = LoadAndResampleAudio("audio.mp3", 16_000);};



// Extract log-mel spectrogram features (80 mel bins)using var tokenizer = AutoTokenizer.Load("e5-small-v2");

var spectrogram = ComputeLogMelSpectrogram(audioSamples, 16_000);

// Run inference

// Run encoder to get audio embeddings

var encoderOutput = RunEncoder(encoderSession, spectrogram);// E5 requires task-specific prefixesusing var results = session.Run(inputs);



// Greedy decoding: generate tokens iterativelystring query = "query: What is machine learning?";var embedding = results.First().AsTensor<float>().ToArray();

var decodedTokens = GreedyDecode(decoderSession, encoderOutput, tokenizer, maxTokens: 128);

string passage = "passage: Machine learning is a field of AI that enables computers to learn.";

// Decode token IDs to text

string transcription = tokenizer.Decode(decodedTokens, skipSpecialTokens: true);// Extract [CLS] token embedding (first token)



Console.WriteLine($"Transcription: {transcription}");// Tokenize queryvar clsEmbedding = embedding.Take(384).ToArray();  // E5-small has 384 dimensions



// Helper method for greedy decodingvar queryEncoding = tokenizer.Encode(query);

static int[] GreedyDecode(InferenceSession decoder, float[] encoderOutput, ITokenizer tokenizer, int maxTokens)

{Console.WriteLine($"Query tokens: {queryEncoding.Ids.Count}");// Compute L2 norm

    var tokens = new List<int>();

    int currentToken = tokenizer.SpecialTokens.BosToken!.Id;  // Start with BOSConsole.WriteLine($"Tokens: {string.Join(", ", queryEncoding.Tokens)}");var norm = Math.Sqrt(clsEmbedding.Select(v => v * v).Sum());

    

    for (int i = 0; i < maxTokens; i++)Console.WriteLine($"Embedding L2 norm: {norm:F4}");  // Typically 5.8-5.9 for E5

    {

        // Run decoder for next token prediction// Tokenize passage```

        var logits = RunDecoder(decoder, encoderOutput, tokens.Prepend(currentToken).ToArray());

        var passageEncoding = tokenizer.Encode(passage);

        // Greedy selection: pick token with highest probability

        currentToken = ArgMax(logits);Console.WriteLine($"Passage tokens: {passageEncoding.Ids.Count}");### Chat Template Processing

        

        // Stop if EOS token generated

        if (currentToken == tokenizer.SpecialTokens.EosToken!.Id)

            break;// Use embeddings for similarity scoringProcess multi-turn conversations with chat templates:

        

        tokens.Add(currentToken);```

    }

    ```csharp

    return tokens.ToArray();

}**Key Features**:using ErgoX.TokenX.HuggingFace;



static int ArgMax(float[] array) => - Task-specific prefixes (`query:`, `passage:`)using ErgoX.TokenX.HuggingFace.Chat;

    array.Select((value, index) => (value, index))

         .OrderByDescending(x => x.value)- Optimized for retrieval tasksusing ErgoX.TokenX.HuggingFace.Options;

         .First().index;

```- Batch processing supportusing System;



## SentencePiece Examplesusing System.Collections.Generic;



### Seq2Seq Tokenization### Multilingual-E5-Small: Cross-Lingual Embeddings



Tokenize for sequence-to-sequence models like T5:// Load tokenizer with chat template support



```csharp**Location**: `examples/HuggingFace/MultilingualE5SmallConsole`  using var tokenizer = AutoTokenizer.Load("meta-llama-3-8b-instruct");

using ErgoX.TokenX.SentencePiece.Processing;

using System;**Run**: `dotnet run --project examples/HuggingFace/MultilingualE5SmallConsole`

using System.Linq;

// Define conversation

// Load T5 SentencePiece model

using var processor = new SentencePieceProcessor();Process text in multiple languages with shared embedding space.var messages = new List<ChatMessage>

processor.Load("t5-small/spiece.model");

{

// T5 requires task prefix for different operations

string input = "translate English to German: Hello, how are you?";```csharp    new("system", "You are a helpful AI assistant."),



// Encode with BOS tokenusing ErgoX.TokenX.HuggingFace;    new("user", "What is the capital of France?"),

var options = new EncodeOptions

{    new("assistant", "The capital of France is Paris."),

    AddBos = true,

    AddEos = falseusing var tokenizer = AutoTokenizer.Load("multilingual-e5-small");    new("user", "What is its population?")

};

};

int[] tokenIds = processor.Encode(input, options);

string[] tokens = processor.EncodeAsTokens(input, options);var samples = new Dictionary<string, string>



Console.WriteLine($"Input: {input}");{// Apply chat template with generation prompt

Console.WriteLine($"Tokens: {string.Join(", ", tokens)}");

Console.WriteLine($"Token IDs: {string.Join(", ", tokenIds)}");    ["English"] = "query: Artificial intelligence is transforming technology.",var options = new ChatTemplateOptions



// Decode back to text    ["Spanish"] = "query: La inteligencia artificial está transformando la tecnología.",{

string decoded = processor.Decode(tokenIds);

Console.WriteLine($"Decoded: {decoded}");    ["Chinese"] = "query: 人工智能正在改变技术。",    AddGenerationPrompt = true  // Add assistant prompt for next response



// Output:    ["Arabic"] = "query: الذكاء الاصطناعي يحول التكنولوجيا."};

// Input: translate English to German: Hello, how are you?

// Tokens: ▁translate, ▁English, ▁to, ▁German, :, ▁Hello, ,, ▁how, ▁are, ▁you, ?};

// Token IDs: 13959, 1566, 12, 2968, 10, 8774, 6, 149, 33, 25, 58

```string prompt = tokenizer.ApplyChatTemplate(messages, options);



### Multilingual Processingforeach (var (language, text) in samples)



Handle multiple languages with a single model:{Console.WriteLine("Formatted prompt:");



```csharp    var encoding = tokenizer.Encode(text);Console.WriteLine(prompt);

using ErgoX.TokenX.SentencePiece.Processing;

using System;    Console.WriteLine($"{language}: {encoding.Ids.Count} tokens");

using System.Collections.Generic;

    Console.WriteLine($"First 5 tokens: {string.Join(", ", encoding.Tokens.Take(5))}");// Output (Llama 3 format):

// Load mT5 model (multilingual T5)

using var processor = new SentencePieceProcessor();}// <|begin_of_text|><|start_header_id|>system<|end_header_id|>

processor.Load("google-mt5-small/spiece.model");

```// 

var samples = new Dictionary<string, string>

{// You are a helpful AI assistant.<|eot_id|><|start_header_id|>user<|end_header_id|>

    ["English"] = "Machine learning is transforming technology.",

    ["German"] = "Maschinelles Lernen verändert die Technologie.",**Key Features**:// 

    ["Japanese"] = "機械学習は技術を変革しています。",

    ["Arabic"] = "التعلم الآلي يحول التكنولوجيا."- Supports 100+ languages// What is the capital of France?<|eot_id|><|start_header_id|>assistant<|end_header_id|>

};

- Shared embedding space// 

var options = new EncodeOptions { AddBos = true, AddEos = true };

- Cross-lingual similarity// The capital of France is Paris.<|eot_id|><|start_header_id|>user<|end_header_id|>

foreach (var (language, text) in samples)

{// 

    var tokens = processor.EncodeAsTokens(text, options);

    var tokenIds = processor.Encode(text, options);### Whisper-Tiny: Speech-to-Text// What is its population?<|eot_id|><|start_header_id|>assistant<|end_header_id|>

    

    Console.WriteLine($"{language}: {text}");

    Console.WriteLine($"Token count: {tokenIds.Length}");

    Console.WriteLine($"Tokens: {string.Join(" ", tokens.Take(10))}...");**Location**: `examples/HuggingFace/WhisperTinyConsole`  // Encode formatted prompt

    Console.WriteLine();

}**Run**: `dotnet run --project examples/HuggingFace/WhisperTinyConsole`var encoding = tokenizer.Encode(prompt);



// Output demonstrates subword tokenization across scripts:Console.WriteLine($"Total tokens: {encoding.Ids.Count}");

// English: Machine learning is transforming technology.

// Token count: 12Transcribe audio using encoder-decoder architecture with multimodal tokenization.```

// Tokens: <s> ▁Machine ▁learning ▁is ▁transform ing ▁technology . </s>...

//

// Japanese: 機械学習は技術を変革しています。

// Token count: 18```csharp### Multimodal (Audio) Tokenization

// Tokens: <s> ▁ 機 械 学 習 は 技 術 を...

```using ErgoX.TokenX.HuggingFace;



### Sampling and Data Augmentationusing Microsoft.ML.OnnxRuntime;Transcribe audio using Whisper with encoder-decoder architecture:



Use stochastic sampling for data augmentation:



```csharp// Load tokenizer and models```csharp

using ErgoX.TokenX.SentencePiece.Processing;

using System;using var tokenizer = AutoTokenizer.Load("whisper-tiny");using ErgoX.TokenX.HuggingFace;



// Load SentencePiece modelusing var encoderSession = new InferenceSession("whisper-tiny/encoder_model_quantized.onnx");using Microsoft.ML.OnnxRuntime;

using var processor = new SentencePieceProcessor();

processor.Load("t5-small/spiece.model");using var decoderSession = new InferenceSession("whisper-tiny/decoder_model_quantized.onnx");using NAudio.Wave;



string text = "The quick brown fox jumps over the lazy dog.";using System;



// Generate 5 different tokenizations using sampling// Load audio (16kHz mono)using System.Linq;

var samplingOptions = new EncodeOptions

{float[] audioSamples = LoadAudio("sample.mp3");

    AddBos = true,

    AddEos = true,// Load tokenizer and ONNX models

    EnableSampling = true,     // Enable stochastic sampling

    NbBestSize = -1,           // Consider all subword splits// Extract mel-spectrogram featuresusing var tokenizer = AutoTokenizer.Load("whisper-tiny");

    Alpha = 0.1f               // Low temperature: prefer likely splits

};var spectrogram = ComputeMelSpectrogram(audioSamples);using var encoderSession = new InferenceSession("whisper-tiny/encoder_model_quantized.onnx");



Console.WriteLine($"Original: {text}");using var decoderSession = new InferenceSession("whisper-tiny/decoder_model_quantized.onnx");

Console.WriteLine("\nStochastic tokenizations:");

// Run encoder

for (int i = 0; i < 5; i++)

{var encoderOutput = RunEncoder(encoderSession, spectrogram);// Load and preprocess audio (resample to 16 kHz mono)

    var tokens = processor.EncodeAsTokens(text, samplingOptions);

    Console.WriteLine($"{i + 1}. {string.Join(" ", tokens)}");float[] audioSamples = LoadAndResampleAudio("audio.mp3", 16_000);

}

// Greedy decode tokens

// Output shows different valid subword splits:

// Original: The quick brown fox jumps over the lazy dog.var tokens = new List<int> { tokenizer.SpecialTokens.BosToken!.Id };// Extract log-mel spectrogram features (80 mel bins)

//

// Stochastic tokenizations:var spectrogram = ComputeLogMelSpectrogram(audioSamples, 16_000);

// 1. <s> ▁The ▁quick ▁brown ▁fox ▁jump s ▁over ▁the ▁lazy ▁dog . </s>

// 2. <s> ▁The ▁qu ick ▁brown ▁fox ▁jumps ▁over ▁the ▁la zy ▁dog . </s>for (int i = 0; i < 128; i++)

// 3. <s> ▁The ▁quick ▁bro wn ▁fox ▁jumps ▁over ▁the ▁lazy ▁dog . </s>

// 4. <s> ▁The ▁quick ▁brown ▁fo x ▁jumps ▁over ▁the ▁lazy ▁dog . </s>{// Run encoder to get audio embeddings

// 5. <s> ▁The ▁quick ▁brown ▁fox ▁jump s ▁over ▁the ▁lazy ▁do g . </s>

```    var logits = RunDecoder(decoderSession, encoderOutput, tokens.ToArray());var encoderOutput = RunEncoder(encoderSession, spectrogram);



## Advanced Scenarios    int nextToken = ArgMax(logits);



### Batch Processing    // Greedy decoding: generate tokens iteratively



Process multiple texts efficiently:    if (nextToken == tokenizer.SpecialTokens.EosToken!.Id)var decodedTokens = GreedyDecode(decoderSession, encoderOutput, tokenizer, maxTokens: 128);



```csharp        break;

using ErgoX.TokenX.HuggingFace;

using System;    // Decode token IDs to text

using System.Collections.Concurrent;

using System.Linq;    tokens.Add(nextToken);string transcription = tokenizer.Decode(decodedTokens, skipSpecialTokens: true);

using System.Threading.Tasks;

}

// Load tokenizer (thread-safe after initialization)

using var tokenizer = AutoTokenizer.Load("bert-base-uncased");Console.WriteLine($"Transcription: {transcription}");



var texts = Enumerable.Range(0, 1000)// Decode to text

    .Select(i => $"Sample text {i} for batch processing.")

    .ToArray();string transcription = tokenizer.Decode(tokens, skipSpecialTokens: true);// Helper method for greedy decoding



// Process in parallel (each thread reuses same tokenizer)Console.WriteLine($"Transcription: {transcription}");static int[] GreedyDecode(InferenceSession decoder, float[] encoderOutput, ITokenizer tokenizer, int maxTokens)

var results = new ConcurrentBag<(string Text, int TokenCount)>();

```{

Parallel.ForEach(texts, text =>

{    var tokens = new List<int>();

    var encoding = tokenizer.Encode(text);

    results.Add((text, encoding.Ids.Count));**Key Features**:    int currentToken = tokenizer.SpecialTokens.BosToken!.Id;  // Start with BOS

});

- Encoder-decoder architecture    

Console.WriteLine($"Processed {results.Count} texts");

Console.WriteLine($"Average tokens: {results.Average(r => r.TokenCount):F2}");- Audio tokenization    for (int i = 0; i < maxTokens; i++)



// Output:- Multilingual speech recognition    {

// Processed 1000 texts

// Average tokens: 12.45        // Run decoder for next token prediction

```

### AutoTokenizer Pipeline Explorer        var logits = RunDecoder(decoder, encoderOutput, tokens.Prepend(currentToken).ToArray());

### Custom Special Tokens

        

Add custom special tokens to a tokenizer:

**Location**: `examples/HuggingFace/AutoTokenizerPipelineExplorer`          // Greedy selection: pick token with highest probability

```csharp

using ErgoX.TokenX.HuggingFace;**Run**: `dotnet run --project examples/HuggingFace/AutoTokenizerPipelineExplorer`        currentToken = ArgMax(logits);

using System;

using System.Collections.Generic;        



// Load base tokenizerInspect tokenizer metadata, special tokens, and configuration across models.        // Stop if EOS token generated

using var tokenizer = AutoTokenizer.Load("gpt2");

        if (currentToken == tokenizer.SpecialTokens.EosToken!.Id)

// Add custom tokens for domain-specific markup

var customTokens = new[] { "[CODE]", "[/CODE]", "[MATH]", "[/MATH]" };```csharp            break;

int addedTokens = tokenizer.AddTokens(customTokens);

using ErgoX.TokenX.HuggingFace;            

Console.WriteLine($"Added {addedTokens} custom tokens");

Console.WriteLine($"New vocabulary size: {tokenizer.VocabularySize}");        tokens.Add(currentToken);



// Use custom tokens in textvar models = new[] { "bert-base-uncased", "gpt2", "t5-small" };    }

string text = "[CODE] def hello(): print('world') [/CODE]";

var encoding = tokenizer.Encode(text);    



Console.WriteLine($"Text: {text}");foreach (var modelId in models)    return tokens.ToArray();

Console.WriteLine($"Tokens: {string.Join(", ", encoding.Tokens)}");

{}

// Custom tokens are preserved as single units

// Output:    using var tokenizer = AutoTokenizer.Load(modelId);

// Added 4 custom tokens

// New vocabulary size: 50261    static int ArgMax(float[] array) => 

// Text: [CODE] def hello(): print('world') [/CODE]

// Tokens: [CODE], Ġdef, Ġhello, (),:, Ġprint, (', world, '), [/CODE]    Console.WriteLine($"\n=== {modelId} ===");    array.Select((value, index) => (value, index))

```

    Console.WriteLine($"Vocabulary size: {tokenizer.VocabularySize}");         .OrderByDescending(x => x.value)

### Performance Optimization

    Console.WriteLine($"Model max length: {tokenizer.ModelMaxLength}");         .First().index;

Optimize for high-throughput scenarios:

    ```

```csharp

using ErgoX.TokenX.HuggingFace;    // Special tokens

using System;

using System.Buffers;    var special = tokenizer.SpecialTokens;## SentencePiece Examples

using System.Collections.Generic;

    Console.WriteLine($"BOS: {special.BosToken?.Content} (ID: {special.BosToken?.Id})");

// Load tokenizer once and reuse

using var tokenizer = AutoTokenizer.Load("bert-base-uncased");    Console.WriteLine($"EOS: {special.EosToken?.Content} (ID: {special.EosToken?.Id})");### Seq2Seq Tokenization



// Use ArrayPool for temporary allocations    Console.WriteLine($"PAD: {special.PadToken?.Content} (ID: {special.PadToken?.Id})");

var pool = ArrayPool<int>.Shared;

    Tokenize for sequence-to-sequence models like T5:

void ProcessTexts(IEnumerable<string> texts)

{    // Sample encoding

    foreach (var text in texts)

    {    var encoding = tokenizer.Encode("Hello, world!");```csharp

        // Encode

        var encoding = tokenizer.Encode(text);    Console.WriteLine($"Tokens: {string.Join(", ", encoding.Tokens)}");using ErgoX.TokenX.SentencePiece.Processing;

        

        // Rent buffer for processing}using System;

        int[] buffer = pool.Rent(encoding.Ids.Count);

        ```using System.Linq;

        try

        {

            // Copy tokens to buffer

            encoding.Ids.CopyTo(buffer, 0);**Key Features**:// Load T5 SentencePiece model

            

            // Process tokens (example: truncate to max length)- Introspection of tokenizer configurationusing var processor = new SentencePieceProcessor();

            int maxLength = 512;

            int length = Math.Min(encoding.Ids.Count, maxLength);- Special token discoveryprocessor.Load("t5-small/spiece.model");

            

            // Use processed tokens...- Cross-model comparison

        }

        finally// T5 requires task prefix for different operations

        {

            // Return buffer to pool## TikToken Examplesstring input = "translate English to German: Hello, how are you?";

            pool.Return(buffer);

        }

    }

}### OpenAI GPT-2: Byte-Pair Encoding// Encode with BOS token



// Benefits:var options = new EncodeOptions

// - Reduced GC pressure from pooled arrays

// - Single tokenizer instance (no repeated initialization)**Location**: `examples/Tiktoken/OpenAiGpt2Console`  {

// - Efficient span-based operations where possible

```**Run**: `dotnet run --project examples/Tiktoken/OpenAiGpt2Console`    AddBos = true,



## Best Practices    AddEos = false



### Resource ManagementTokenize text using OpenAI's TikToken BPE implementation optimized for GPT models.};



Always dispose tokenizers and ONNX sessions:



```csharp```csharpint[] tokenIds = processor.Encode(input, options);

// ✓ Correct: using statement

using (var tokenizer = AutoTokenizer.Load("model"))using ErgoX.TokenX.Tiktoken;string[] tokens = processor.EncodeAsTokens(input, options);

{

    var encoding = tokenizer.Encode("text");

    // tokenizer automatically disposed

}// GPT-2 configurationConsole.WriteLine($"Input: {input}");



// ✓ Correct: using declarationconst string pattern = "'(?:[sdmt]|ll|ve|re)| ?\\p{L}+| ?\\p{N}+| ?[^\\s\\p{L}\\p{N}]+|\\s+(?!\\S)|\\s+";Console.WriteLine($"Tokens: {string.Join(", ", tokens)}");

using var tokenizer = AutoTokenizer.Load("model");

var encoding = tokenizer.Encode("text");var specialTokens = new Dictionary<string, int> { ["<|endoftext|>"] = 50256 };Console.WriteLine($"Token IDs: {string.Join(", ", tokenIds)}");

// tokenizer disposed at end of scope



// ✗ Incorrect: no disposal

var tokenizer = AutoTokenizer.Load("model");// Load encoding from mergeable ranks file// Decode back to text

var encoding = tokenizer.Encode("text");

// Memory leak: native resources not releasedusing var encoding = TiktokenEncodingFactory.FromTiktokenFile(string decoded = processor.Decode(tokenIds);

```

    "gpt2",Console.WriteLine($"Decoded: {decoded}");

### Error Handling

    pattern,

Handle tokenization errors gracefully:

    "gpt2/mergeable_ranks.tiktoken",// Output:

```csharp

using ErgoX.TokenX.HuggingFace;    specialTokens);// Input: translate English to German: Hello, how are you?

using System;

// Tokens: ▁translate, ▁English, ▁to, ▁German, :, ▁Hello, ,, ▁how, ▁are, ▁you, ?

try

{string text = "Machine learning enables computers to learn from data without explicit programming.";// Token IDs: 13959, 1566, 12, 2968, 10, 8774, 6, 149, 33, 25, 58

    using var tokenizer = AutoTokenizer.Load("model-path");

    ```

    string text = GetUserInput();

    // Encode

    // Validate input

    if (string.IsNullOrWhiteSpace(text))var tokens = encoding.EncodeOrdinary(text);### Multilingual Processing

    {

        Console.WriteLine("Error: Empty input");Console.WriteLine($"Text: {text}");

        return;

    }Console.WriteLine($"Token count: {tokens.Count}");Handle multiple languages with a single model:

    

    // Encode with error handlingConsole.WriteLine($"Tokens: {string.Join(", ", tokens.Take(10))}...");

    var encoding = tokenizer.Encode(text);

    ```csharp

    // Check token limits

    const int maxTokens = 512;// Decodeusing ErgoX.TokenX.SentencePiece.Processing;

    if (encoding.Ids.Count > maxTokens)

    {var decoded = encoding.Decode(tokens.ToArray());using System;

        Console.WriteLine($"Warning: Input truncated from {encoding.Ids.Count} to {maxTokens} tokens");

        encoding = tokenizer.Encode(text.Substring(0, text.Length / 2));  // Simple truncationConsole.WriteLine($"Decoded: {decoded}");using System.Collections.Generic;

    }

}

catch (FileNotFoundException ex)

{// Token-by-token breakdown// Load mT5 model (multilingual T5)

    Console.WriteLine($"Error: Model not found - {ex.Message}");

}for (int i = 0; i < Math.Min(tokens.Count, 10); i++)using var processor = new SentencePieceProcessor();

catch (Exception ex)

{{processor.Load("google-mt5-small/spiece.model");

    Console.WriteLine($"Tokenization error: {ex.Message}");

}    var singleToken = encoding.Decode(new[] { tokens[i] });

```

    Console.WriteLine($"  Token {i}: {tokens[i]:D5} → '{singleToken}'");var samples = new Dictionary<string, string>

## Test Coverage & Validation

}{

All examples are tested for parity with Python implementations. To regenerate test fixtures:

```    ["English"] = "Machine learning is transforming technology.",

```bash

# Regenerate HuggingFace test fixtures    ["German"] = "Maschinelles Lernen verändert die Technologie.",

python tests/Py/Huggingface/generate_benchmarks.py

```**Key Features**:    ["Japanese"] = "機械学習は技術を変革しています。",



## Next Steps- Byte-level BPE    ["Arabic"] = "التعلم الآلي يحول التكنولوجيا."



- [Installation Guide](installation.md) - Detailed setup instructions- Regex-based pre-tokenization};

- [HuggingFace Documentation](huggingface/index.md) - Complete API reference

- [SentencePiece Documentation](sentencepiece/index.md) - Detailed guide- Deterministic encoding

- [Main Documentation](index.md) - Overview and comparison

- Special token handlingvar options = new EncodeOptions { AddBos = true, AddEos = true };



**TikToken vs HuggingFace**:foreach (var (language, text) in samples)

- **TikToken**: Optimized for speed, byte-level, English/code focused{

- **HuggingFace**: More flexible, supports multiple algorithms, better multilingual    var tokens = processor.EncodeAsTokens(text, options);

    var tokenIds = processor.Encode(text, options);

## Best Practices    

    Console.WriteLine($"{language}: {text}");

### Resource Management    Console.WriteLine($"Token count: {tokenIds.Length}");

    Console.WriteLine($"Tokens: {string.Join(" ", tokens.Take(10))}...");

Always dispose tokenizers and sessions:    Console.WriteLine();

}

```csharp

// ✓ Correct// Output demonstrates subword tokenization across scripts:

using var tokenizer = AutoTokenizer.Load("model");// English: Machine learning is transforming technology.

var encoding = tokenizer.Encode("text");// Token count: 12

// Tokens: <s> ▁Machine ▁learning ▁is ▁transform ing ▁technology . </s>...

// ✗ Incorrect - memory leak//

var tokenizer = AutoTokenizer.Load("model");// Japanese: 機械学習は技術を変革しています。

var encoding = tokenizer.Encode("text");// Token count: 18

// Missing: tokenizer.Dispose()// Tokens: <s> ▁ 機 械 学 習 は 技 術 を...

``````



### Error Handling### Sampling and Data Augmentation



Handle tokenization failures gracefully:Use stochastic sampling for data augmentation:



```csharp```csharp

tryusing ErgoX.TokenX.SentencePiece.Processing;

{using System;

    using var tokenizer = AutoTokenizer.Load(modelPath);

    var encoding = tokenizer.Encode(text);// Load SentencePiece model

    using var processor = new SentencePieceProcessor();

    if (encoding.Ids.Count > maxTokens)processor.Load("t5-small/spiece.model");

    {

        Console.WriteLine($"Warning: Truncating from {encoding.Ids.Count} to {maxTokens} tokens");string text = "The quick brown fox jumps over the lazy dog.";

    }

}// Generate 5 different tokenizations using sampling

catch (FileNotFoundException ex)var samplingOptions = new EncodeOptions

{{

    Console.WriteLine($"Model not found: {ex.Message}");    AddBos = true,

}    AddEos = true,

catch (Exception ex)    EnableSampling = true,     // Enable stochastic sampling

{    NbBestSize = -1,           // Consider all subword splits

    Console.WriteLine($"Tokenization failed: {ex.Message}");    Alpha = 0.1f               // Low temperature: prefer likely splits

}};

```

Console.WriteLine($"Original: {text}");

### Batch ProcessingConsole.WriteLine("\nStochastic tokenizations:");



Process multiple texts efficiently:for (int i = 0; i < 5; i++)

{

```csharp    var tokens = processor.EncodeAsTokens(text, samplingOptions);

using var tokenizer = AutoTokenizer.Load("bert-base-uncased");    Console.WriteLine($"{i + 1}. {string.Join(" ", tokens)}");

}

var texts = new[] { "First text", "Second text", "Third text" };

var encodings = texts.Select(t => tokenizer.Encode(t)).ToList();// Output shows different valid subword splits:

// Original: The quick brown fox jumps over the lazy dog.

Console.WriteLine($"Processed {encodings.Count} texts");//

Console.WriteLine($"Average tokens: {encodings.Average(e => e.Ids.Count):F2}");// Stochastic tokenizations:

```// 1. <s> ▁The ▁quick ▁brown ▁fox ▁jump s ▁over ▁the ▁lazy ▁dog . </s>

// 2. <s> ▁The ▁qu ick ▁brown ▁fox ▁jumps ▁over ▁the ▁la zy ▁dog . </s>

## Running Examples// 3. <s> ▁The ▁quick ▁bro wn ▁fox ▁jumps ▁over ▁the ▁lazy ▁dog . </s>

// 4. <s> ▁The ▁quick ▁brown ▁fo x ▁jumps ▁over ▁the ▁lazy ▁dog . </s>

All examples can be run from the repository root:// 5. <s> ▁The ▁quick ▁brown ▁fox ▁jump s ▁over ▁the ▁lazy ▁do g . </s>

```

```bash

# HuggingFace examples## TikToken Examples

dotnet run --project examples/HuggingFace/AllMiniLmL6V2Console

dotnet run --project examples/HuggingFace/E5SmallV2Console### GPT Input Preparation

dotnet run --project examples/HuggingFace/MultilingualE5SmallConsole

dotnet run --project examples/HuggingFace/WhisperTinyConsolePrepare input for GPT models with proper token counting:

dotnet run --project examples/HuggingFace/AutoTokenizerPipelineExplorer

```csharp

# TikToken examplesusing ErgoX.TokenX.Tiktoken;

dotnet run --project examples/Tiktoken/OpenAiGpt2Consoleusing System;

```using System.Collections.Generic;



## Next Steps// Load cl100k_base encoding (GPT-3.5, GPT-4)

using var encoding = TiktokenEncodingFactory.LoadCL100kBase();

- [Installation Guide](installation.md) - Setup and deployment

- [HuggingFace Documentation](huggingface/index.md) - Complete API reference// Prepare chat conversation

- [TikToken Documentation](tiktoken/index.md) - Encoding detailsvar messages = new List<(string Role, string Content)>

- [Main Documentation](index.md) - Overview{

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
using ErgoX.TokenX.HuggingFace;
using System;
using System.Buffers;
using System.Collections.Generic;

// Load tokenizer once and reuse
using var tokenizer = AutoTokenizer.Load("bert-base-uncased");

// Use ArrayPool for temporary allocations
var pool = ArrayPool<int>.Shared;

void ProcessTexts(IEnumerable<string> texts)
{
    foreach (var text in texts)
    {
        // Encode
        var encoding = tokenizer.Encode(text);
        
        // Rent buffer for processing
        int[] buffer = pool.Rent(encoding.Ids.Count);
        
        try
        {
            // Copy tokens to buffer
            encoding.Ids.CopyTo(buffer, 0);
            
            // Process tokens (example: truncate to max length)
            int maxLength = 512;
            int length = Math.Min(encoding.Ids.Count, maxLength);
            
            // Use processed tokens...
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

## Test Coverage & Validation

All examples are tested for parity with Python implementations. To regenerate test fixtures:

```bash
# Regenerate HuggingFace test fixtures
python tests/Py/Huggingface/generate_benchmarks.py
```

## Next Steps

- [Installation Guide](installation.md) - Detailed setup instructions
- [HuggingFace Documentation](huggingface/index.md) - Complete API reference
- [SentencePiece Documentation](sentencepiece/index.md) - Detailed guide
- [Main Documentation](index.md) - Overview and comparison

