# AllMiniLmL6V2Console Example

## Overview

This example demonstrates **sentence embedding generation** using the **all-minilm-l6-v2** model with ONNX inference. The console loads multilingual text samples (English, Spanish, French, Hindi) and generates dense vector embeddings for semantic similarity tasks.

## What It Does

1. **Loads the tokenizer** from the local model directory (`examples/.models/all-minilm-l6-v2`)
2. **Loads the ONNX model** (`model_quantized.onnx`) for CPU-optimized inference
3. **Tokenizes text samples** from JSON files in `examples/.data/embeddings`
4. **Generates embeddings** for each sample text
5. **Outputs embedding statistics** (first 6 components, L2 norm)

## Model Details

- **Model**: `all-minilm-l6-v2` (BERT-based sentence embeddings)
- **Task**: Semantic text representation for similarity search, clustering, and retrieval
- **Output dimension**: 384 (dense vector per sentence)
- **Input**: Arbitrary text (tokenized and padded to max sequence length)
- **Languages supported**: Multilingual via BERT tokenizer (English, Spanish, French, Hindi, etc.)

## Running the Example

```bash
cd examples/HuggingFace/AllMiniLmL6V2Console
dotnet run
```

## Sample Output

```
Loaded 'all-minilm-l6-v2' tokenizer and ONNX model from: C:\...\examples\.models\all-minilm-l6-v2

Sample 'standard-tiny-en' text:
Programming language paradigms define computational models: imperative (command sequences modifying state), 
functional (pure functions, immutability, higher-order functions), object-oriented (encapsulation, inheritance, 
polymorphism), declarative (specify what not how). Multi-paradigm languages (Python, Java, JavaScript) blend approaches.

Token IDs:
101, 4730, 2653, 20680, 2015, 9375, 15078, 4275, 1024, 23934, 1006, 3094, 10071, 29226, 2110, 1007, ...

First 6 embedding components:
0.1819, 0.0203, 0.1682, -0.1927, -0.1210, 0.1163

Embedding L2 norm: 6.3687
------------------------------------------------------------------------

Sample 'standard-tiny-es' text:
Los paradigmas de programación definen modelos computacionales: imperativo (secuencias de comandos modificando 
estado), funcional (funciones puras, inmutabilidad, funciones de orden superior), ...

Token IDs:
101, 3050, 20680, 3022, 2139, 2565, 21736, 9375, 2078, 2944, 2891, 4012, 18780, 21736, 23266, 1024, ...

First 6 embedding components:
0.1684, -0.1075, 0.0739, -0.2122, -0.1924, -0.1479

Embedding L2 norm: 6.6310
------------------------------------------------------------------------
```

## Key Components

### AutoTokenizer Pipeline
- **Purpose**: Converts raw text → token IDs using BERT tokenizer
- **Special tokens**: `[CLS]` (start), `[SEP]` (separator), `[PAD]` (padding)
- **Output**: Sequence of integer token IDs (e.g., `101, 4730, 2653, ...`)

### Code Comments Breakdown

#### 1. Loading the Tokenizer
```csharp
// Constructs path to tokenizer files in model directory
// E.g., examples/.models/all-minilm-l6-v2/tokenizer.json
var modelDirectory = Path.Combine("examples", ".models", "all-minilm-l6-v2");

// AutoTokenizer.LoadAsync() reads:
// - tokenizer.json: BPE vocabulary and merge rules
// - tokenizer_config.json: Special tokens ([CLS], [SEP], [PAD], [UNK])
// - config.json: Model configuration (max_position_embeddings, vocab_size)
// ApplyTokenizerDefaults=true ensures special tokens are properly set
using var tokenizer = await AutoTokenizer.LoadAsync(modelDirectory, new AutoTokenizerLoadOptions
{
    ApplyTokenizerDefaults = true
});
```

#### 2. Creating ONNX Session
```csharp
// ONNX Runtime loads quantized model file (typically 40–50 MB for BERT-base)
// Note: Quantization (QINT8) reduces size 4× but may have 1–2% accuracy impact
// For production: Use full-precision FP32 models
var modelPath = Path.Combine(modelDirectory, "model_quantized.onnx");
var session = new InferenceSession(modelPath, new SessionOptions { GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL });
```

#### 3. Tokenizing Text
```csharp
// Tokenizer.Encode() performs:
// 1. Lowercasing and accent removal
// 2. Whitespace tokenization ("Programming language" → ["Programming", "language"])
// 3. WordPiece subword splitting ("untokenized" → ["un", "##token", "##ized"])
// 4. [CLS] prepend and [SEP] append (BERT format)
// 5. Padding to max_length=512 with [PAD] (ID: 0)
var encoding = tokenizer.Tokenizer.Encode(sampleText);
var inputIds = new long[] { encoding.Ids }; // Shape: (1, sequence_length)
var attentionMask = new long[] { encoding.AttentionMask };  // 1 for real tokens, 0 for padding
var tokenTypeIds = new long[] { encoding.TypeIds };  // All 0 for single sequence
```

#### 4. ONNX Inference
```csharp
// ONNX inputs correspond to BERT model:
// - input_ids: Token integer sequence
// - attention_mask: Tells model which positions to attend to (0 = ignore padding)
// - token_type_ids: Differentiates sentence A vs B (both 0 here for single sentence)
var inputs = new List<NamedOnnxValue>
{
    NamedOnnxValue.CreateFromTensor("input_ids", inputTensor),
    NamedOnnxValue.CreateFromTensor("attention_mask", maskTensor),
    NamedOnnxValue.CreateFromTensor("token_type_ids", typeIdTensor)
};

// session.Run() executes ONNX quantized model
// Quantized weights (QINT8) are dequantized during forward pass
// Output: last_hidden_state (1, sequence_length, 384)
using var results = session.Run(inputs);
var lastHiddenState = results.First().AsEnumerable<float>().ToArray();
```

#### 5. Embedding Pooling
```csharp
// Extract [CLS] token embedding (first token, all 384 dimensions)
// [CLS] token is designed to aggregate sentence meaning in BERT
// Alternative: Mean pooling across all tokens (excluding [PAD])
var clsEmbedding = lastHiddenState
    .Skip(0)  // [CLS] is first token
    .Take(384)  // Extract 384-dim embedding
    .ToArray();

// Compute L2 norm: sqrt(sum of squared components)
// Typically 6–7 for this model; used for cosine similarity normalization
var norm = Math.Sqrt(clsEmbedding.Sum(x => x * x));
var normalizedEmbedding = clsEmbedding.Select(x => x / norm).ToArray();
```

### ONNX Embedding Inference
- **Input tensors**:
  - `input_ids`: Long tensor shape `(batch_size, sequence_length)`
  - `attention_mask`: Binary mask for padding tokens
  - `token_type_ids`: Token type IDs (for sentence pairs, all 0 here)
- **Output tensors**:
  - `last_hidden_state`: Shape `(batch_size, sequence_length, 384)` - per-token representations
  - Final embedding: Average or [CLS] pooling of last hidden layer

### Embedding Statistics
- **First 6 components**: Sample values from the 384-dimensional embedding
- **L2 norm**: Magnitude of the embedding vector (typically 6–7 for this model)

## Use Cases

1. **Semantic Search**: Index embeddings and find similar documents
2. **Clustering**: Group documents by embedding similarity
3. **Classification**: Use embeddings as features for downstream models
4. **Deduplication**: Find near-duplicate text by comparing embeddings
5. **Recommendation**: Embed and compare product descriptions, reviews

## ⚠️ Important: Model Quantization Notice

**All models used in these demo examples are heavily quantized to QINT8 format for demonstration and validation purposes only.**

- ✅ **Suitable for**: Testing tokenizer behavior, validating pipeline architecture, quick prototyping, tokenizer verification
- ❌ **NOT suitable for production**: Production systems require full-precision (FP32) or carefully tuned quantization strategies
- **Quantization impact**: Reduced accuracy (typically 1–5% degradation), reduced memory footprint (~4× smaller), faster inference

**Important**: Tokenizer validation (as used in `AutoTokenizerPipelineExplorer`) is **independent of model quantization** and is fully production-ready. The tokenization logic produces identical token sequences regardless of model precision.

## Performance Characteristics

- **Tokenization time**: < 1 ms per sample (no quantization dependency)
- **ONNX inference time**: ~5–20 ms per sample (CPU, quantized model – FP32 would be 2–4× slower)
- **Memory footprint**: ~40 MB (quantized ONNX model – FP32 would be ~150 MB)
- **Batch efficiency**: Not explicitly batched here (per-sample inference)

## JSON Sample Format

Samples are loaded from `examples/.data/embeddings/*.json`:

```json
{
  "id": "standard-tiny-en",
  "single": {
    "text": "Programming language paradigms define computational models..."
  }
}
```

## How to Extend

### Add New Languages
- Create a new JSON file in `examples/.data/embeddings/`
- The tokenizer will automatically handle new language tokens
- No model retraining needed (BERT vocab supports ~100 languages)

### Batch Multiple Samples
Replace single inference with batched tensor operations:
```csharp
var batchSize = 10;
var embeddings = ComputeEmbeddingBatch(session, encodings, batchSize);
```

### Store and Retrieve Embeddings
- Convert embeddings to `float[]` or dense matrix format
- Index with libraries like `Faiss`, `Hnswlib`, or simple distance metrics
- Query by computing embedding of search text and comparing with stored embeddings

## Dependencies

- **ErgoX.VecraX.ML.NLP.Tokenizers**: Tokenizer bindings
- **Microsoft.ML.OnnxRuntime**: ONNX model inference
- **System.Numerics.Tensors**: Dense tensor manipulation

## Model Card

For detailed model information, see:
- HuggingFace: https://huggingface.co/sentence-transformers/all-MiniLM-L6-v2
- Sentence-BERT paper: https://arxiv.org/abs/1908.10084

---

**Last Updated**: October 2025  
**Status**: Tested and verified on .NET 8.0
