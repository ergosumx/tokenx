using System;

namespace ErgoX.TokenX.HuggingFace.Abstractions;

/// <summary>
/// Defines the contract for a decoder that converts token IDs back into text.
/// </summary>
/// <remarks>
/// Decoders handle the reverse transformation from tokens to text, including:
/// - BPE Decoder: Handles byte-pair encoding reversal
/// - ByteLevel Decoder: Converts byte-level tokens back to UTF-8
/// - WordPiece Decoder: Removes word piece markers (e.g., ##)
/// - Metaspace Decoder: Converts metaspace character (▁) back to spaces
/// - CTC Decoder: Removes consecutive duplicates and blank tokens for speech recognition
/// - And others: Fuse, Strip, Replace, ByteFallback
/// </remarks>
public interface IDecoder : IDisposable
{
    /// <summary>
    /// Gets the native handle to the underlying decoder implementation.
    /// </summary>
    IntPtr Handle { get; }
}

