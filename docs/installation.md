# Installation Guide

This guide covers installation and setup for all three tokenizer libraries.

## Prerequisites

- **.NET 8.0 SDK** or later
- **Supported Platforms:**
  - Windows (x64)
  - Linux (x64)
  - macOS (x64, ARM64)

## NuGet Packages

Install the package for your chosen tokenizer:

```bash
# HuggingFace Tokenizers
dotnet add package ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace

# Google SentencePiece
dotnet add package ErgoX.VecraX.ML.NLP.Tokenizers.Google.SentencePiece

# OpenAI TikToken
dotnet add package ErgoX.VecraX.ML.NLP.Tokenizers.OpenAI.Tiktoken
```

## Native Library Deployment

Each package includes native libraries that are automatically deployed to your runtime folder during build:

```
YourProject/
├── bin/
│   └── Debug/
│       └── net8.0/
│           └── runtimes/
│               ├── win-x64/
│               │   └── native/
│               │       ├── tokenx_bridge.dll
│               │       ├── libsentencepiece.dll
│               │       └── tiktoken.dll
│               ├── linux-x64/
│               │   └── native/
│               │       ├── libtokenx_bridge.so
│               │       ├── libsentencepiece.so
│               │       └── libtiktoken.so
│               └── osx-x64/
│                   └── native/
│                       ├── libtokenx_bridge.dylib
│                       ├── libsentencepiece.dylib
│                       └── libtiktoken.dylib
```

The .NET runtime automatically loads the correct native library for your platform.

## Verification

### Verify HuggingFace Installation

```csharp
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace;
using System;

// Test basic functionality
try
{
    var json = @"{""version"":""1.0"",""model"":{""type"":""BPE"",""vocab"":{},""merges"":[]}}";
    using var tokenizer = Tokenizer.FromJson(json);
    Console.WriteLine("✓ HuggingFace Tokenizers installed successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"✗ Installation failed: {ex.Message}");
}
```

### Verify SentencePiece Installation

```csharp
using ErgoX.VecraX.ML.NLP.Tokenizers.Google.SentencePiece.Processing;
using System;

// Test basic functionality
try
{
    using var processor = new SentencePieceProcessor();
    Console.WriteLine("✓ Google SentencePiece installed successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"✗ Installation failed: {ex.Message}");
}
```

### Verify TikToken Installation

```csharp
using ErgoX.VecraX.ML.NLP.Tokenizers.Tiktoken;
using System;

// Test basic functionality
try
{
    var mergeableRanks = new[] 
    { 
        new TiktokenMergeableRank(new byte[] { 104 }, 0)  // 'h'
    };
    var specialTokens = new Dictionary<string, int>();
    using var encoding = TiktokenEncoding.Create(
        "test", 
        ".*", 
        mergeableRanks, 
        specialTokens);
    Console.WriteLine("✓ OpenAI TikToken installed successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"✗ Installation failed: {ex.Message}");
}
```

## Model Files

### HuggingFace Models

HuggingFace tokenizers can load models in two ways:

**1. From Local Files**
```csharp
// Load tokenizer.json directly
using var tokenizer = Tokenizer.FromFile("tokenizer.json");

// Or use AutoTokenizer for automatic configuration
using var tokenizer = AutoTokenizer.Load("path/to/model/directory");
```

**2. From HuggingFace Hub**
```csharp
// Download and cache from HuggingFace Hub
using var tokenizer = AutoTokenizer.LoadFromPretrained("bert-base-uncased");

// With authentication for private models
using var tokenizer = AutoTokenizer.LoadFromPretrained(
    "private-model",
    revision: "main",
    authToken: "hf_your_token_here");
```

### SentencePiece Models

Download SentencePiece models from HuggingFace or train your own:

```csharp
// Load from file
using var processor = new SentencePieceProcessor();
processor.Load("path/to/spiece.model");
```

**Common Models:**
- T5: `t5-small`, `t5-base`, `t5-large`
- mT5: `google/mt5-small`, `google/mt5-base`
- BERT multilingual: Model directories contain `sentencepiece.bpe.model`

### TikToken Vocabularies

Download `.tiktoken` files from the repository or OpenAI:

```csharp
// Load vocabulary
using var encoding = TiktokenEncodingFactory.FromTiktokenFile(
    "gpt2",
    pattern,
    "path/to/mergeable_ranks.tiktoken",
    specialTokens);
```

**Common Vocabularies:**
- GPT-2: `gpt2` (50,257 tokens)
- GPT-3: `r50k_base`, `p50k_base`
- GPT-3.5/GPT-4: `cl100k_base` (100,277 tokens)

Download from: https://github.com/openai/tiktoken/tree/main/tiktoken_ext

## Project Setup

### Console Application

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace" Version="*" />
    <PackageReference Include="ErgoX.VecraX.ML.NLP.Tokenizers.Google.SentencePiece" Version="*" />
    <PackageReference Include="ErgoX.VecraX.ML.NLP.Tokenizers.OpenAI.Tiktoken" Version="*" />
  </ItemGroup>
</Project>
```

### ASP.NET Core Application

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <!-- Add tokenizer packages as needed -->
    <PackageReference Include="ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace" Version="*" />
  </ItemGroup>
</Project>
```

Register tokenizers as services:

```csharp
// Program.cs
builder.Services.AddSingleton<ITokenizer>(sp =>
{
    return AutoTokenizer.Load("bert-base-uncased");
});
```

## Troubleshooting

### Native Library Not Found

**Symptom:** `DllNotFoundException` or `Unable to load shared library`

**Solutions:**

1. **Verify package installation:**
   ```bash
   dotnet list package
   ```

2. **Check runtime folder structure:**
   ```bash
   ls -R bin/Debug/net8.0/runtimes/
   ```

3. **Explicitly copy native libraries:**
   ```xml
   <ItemGroup>
     <None Include="runtimes/**" CopyToOutputDirectory="PreserveNewest" />
   </ItemGroup>
   ```

### Version Conflicts

**Symptom:** Assembly version mismatch errors

**Solution:** Ensure all packages use compatible versions:

```xml
<ItemGroup>
  <PackageReference Include="ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace" Version="1.0.0" />
  <PackageReference Include="ErgoX.VecraX.ML.NLP.Tokenizers.Common" Version="1.0.0" />
</ItemGroup>
```

### Model File Errors

**Symptom:** `FileNotFoundException` for model files

**Solution:** Use absolute paths or verify relative paths:

```csharp
var modelPath = Path.GetFullPath("models/tokenizer.json");
if (!File.Exists(modelPath))
{
    throw new FileNotFoundException($"Model not found: {modelPath}");
}
using var tokenizer = Tokenizer.FromFile(modelPath);
```

### Platform-Specific Issues

**Linux:** Ensure libc compatibility
```bash
ldd runtimes/linux-x64/native/libtokenx_bridge.so
```

**macOS:** Code signing may be required
```bash
codesign -s - runtimes/osx-x64/native/libtokenx_bridge.dylib
```

**Windows:** Visual C++ Redistributable may be needed
- Download from: https://aka.ms/vs/17/release/vc_redist.x64.exe

## Environment Configuration

### Setting Environment Variables

```bash
# Set model directory
export TOKENIZER_MODEL_DIR=/path/to/models

# Set cache directory for HuggingFace models
export HF_HOME=/path/to/cache
```

### Configuration in Code

```csharp
// Configure model paths
var modelDir = Environment.GetEnvironmentVariable("TOKENIZER_MODEL_DIR") 
    ?? Path.Combine(AppContext.BaseDirectory, "models");

var tokenizerPath = Path.Combine(modelDir, "tokenizer.json");
using var tokenizer = Tokenizer.FromFile(tokenizerPath);
```

## Docker Deployment

### Dockerfile Example

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["YourApp.csproj", "./"]
RUN dotnet restore "YourApp.csproj"
COPY . .
RUN dotnet build "YourApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "YourApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Copy model files if needed
COPY models/ /app/models/

ENTRYPOINT ["dotnet", "YourApp.dll"]
```

### Docker Compose

```yaml
version: '3.8'
services:
  tokenizer-app:
    build: .
    ports:
      - "5000:5000"
    volumes:
      - ./models:/app/models:ro
    environment:
      - TOKENIZER_MODEL_DIR=/app/models
```

## Performance Tuning

### Memory Management

```csharp
// Reuse tokenizer instances
private static readonly Lazy<ITokenizer> _tokenizer = new(() => 
    AutoTokenizer.Load("model-path"));

public void Process(string text)
{
    var tokenizer = _tokenizer.Value;
    var encoding = tokenizer.Encode(text);
    // Process encoding...
}
```

### Parallel Processing

```csharp
// Process multiple texts in parallel
var texts = GetTexts();
var results = new ConcurrentBag<EncodingResult>();

Parallel.ForEach(texts, text =>
{
    // Each thread should have its own tokenizer instance
    using var tokenizer = Tokenizer.FromFile("tokenizer.json");
    var encoding = tokenizer.Encode(text);
    results.Add(encoding);
});
```

## Next Steps

- [HuggingFace Documentation](huggingface/index.md)
- [SentencePiece Documentation](sentencepiece/index.md)
- [TikToken Documentation](tiktoken/index.md)
- [Examples and Tutorials](examples.md)
