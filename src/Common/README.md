# ErgoX.VecraX.ML.NLP.Tokenizers.Common

Shared utilities, abstractions, and interop primitives for ErgoX TokenX tokenizer libraries.

## Overview

This library provides common functionality used across HuggingFace tokenizer implementations:

- **Native Interop**: P/Invoke declarations, marshaling helpers, and safe handle wrappers
- **Abstractions**: Shared interfaces and base classes for tokenizers
- **Utilities**: Common helper methods for string processing, encoding, and validation
- **Error Handling**: Unified exception types and error reporting

## Package Information

**Package**: `ErgoX.VecraX.ML.NLP.Tokenizers.Common`
**Type**: Internal dependency (automatically referenced by tokenizer packages)
**Platforms**: Platform-agnostic managed code

> **Note**: You typically don't need to install this package directly. It's automatically included as a dependency when you install `ErgoX.TokenX.HuggingFace`.

## Contents

### Interop

Native interop infrastructure for calling Rust/C++ libraries:

- **SafeHandles**: Proper lifetime management for native resources
- **Marshaling**: Conversions between .NET types and native representations
- **Error Handling**: Translating native error codes to .NET exceptions

### Abstractions

Common interfaces and base classes:

- Tokenizer base interfaces
- Encoding result structures
- Configuration abstractions

### Utilities

Helper methods for:

- UTF-8 string handling
- Collection extensions
- Validation and error checking

## Usage

This package is used internally and is not meant to be consumed directly. When you install a tokenizer package, you get the necessary common utilities automatically:

```bash
# Installing this automatically includes Common
dotnet add package ErgoX.TokenX.HuggingFace
```

## Architecture

```
ErgoX.VecraX.ML.NLP.Tokenizers.Common
├── Interop/                # Native interop layer
│   ├── SafeHandles/        # Resource lifetime management
│   ├── Marshaling/         # Type conversions
│   └── ErrorHandling/      # Exception mapping
├── Abstractions/           # Shared interfaces
│   ├── ITokenizer         # Base tokenizer interface
│   └── EncodingResult     # Common encoding structures
└── Utilities/              # Helper methods
    ├── StringExtensions    # UTF-8 utilities
    └── Validation          # Input validation
```

## Dependencies

- .NET 8.0 or later
- System.Runtime.InteropServices (P/Invoke)
- System.Memory (Span<T> support)

## Building

```bash
cd src/Common
dotnet build --configuration Release
```

## Testing

Common utilities are tested indirectly through the HuggingFace integration tests:

```bash
cd ../..
dotnet test --configuration Release
```

## Contributing

When modifying Common:
1. Ensure changes don't break HuggingFace package
2. Add tests via the dependent package
3. Update this README if adding new public APIs
4. Follow coding standards in `.github/instructions/`

## Related Projects

- [ErgoX.TokenX.HuggingFace](../HuggingFace/) - HuggingFace Tokenizers bindings

## License

Apache 2.0 - See [LICENSE](../../LICENSE) for details.
