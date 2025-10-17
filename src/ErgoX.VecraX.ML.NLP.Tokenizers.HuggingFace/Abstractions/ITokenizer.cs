using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Abstractions;

/// <summary>
/// Defines the contract for a tokenizer that can encode text into token IDs and decode token IDs back to text.
/// </summary>
public interface ITokenizer : IDisposable
{
    /// <summary>
    /// Gets the configuration for this tokenizer.
    /// </summary>
    object Config { get; }

    /// <summary>
    /// Encodes a single text input into token IDs.
    /// </summary>
    /// <param name="text">The input text to encode.</param>
    /// <param name="addSpecialTokens">Whether to add special tokens (e.g., [CLS], [SEP]).</param>
    /// <returns>An <see cref="EncodingResult"/> containing the token IDs and related information.</returns>
    EncodingResult Encode(string text, bool addSpecialTokens = true);

    /// <summary>
    /// Encodes a single text input into token IDs asynchronously.
    /// </summary>
    /// <param name="text">The input text to encode.</param>
    /// <param name="addSpecialTokens">Whether to add special tokens (e.g., [CLS], [SEP]).</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing an <see cref="EncodingResult"/>.</returns>
    Task<EncodingResult> EncodeAsync(string text, bool addSpecialTokens = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Encodes a pair of text inputs into token IDs (e.g., for sentence pair classification).
    /// </summary>
    /// <param name="text">The first input text to encode.</param>
    /// <param name="textPair">The second input text to encode.</param>
    /// <param name="addSpecialTokens">Whether to add special tokens (e.g., [CLS], [SEP]).</param>
    /// <returns>An <see cref="EncodingResult"/> containing the token IDs and related information.</returns>
    EncodingResult Encode(string text, string textPair, bool addSpecialTokens = true);

    /// <summary>
    /// Encodes multiple text inputs in a batch.
    /// </summary>
    /// <param name="inputs">The collection of input texts to encode.</param>
    /// <param name="addSpecialTokens">Whether to add special tokens (e.g., [CLS], [SEP]).</param>
    /// <returns>A list of <see cref="EncodingResult"/> objects, one for each input.</returns>
    IReadOnlyList<EncodingResult> EncodeBatch(IEnumerable<string> inputs, bool addSpecialTokens = true);

    /// <summary>
    /// Decodes a sequence of token IDs back into text.
    /// </summary>
    /// <param name="ids">The token IDs to decode.</param>
    /// <param name="skipSpecialTokens">Whether to skip special tokens in the output.</param>
    /// <returns>The decoded text string.</returns>
    string Decode(IReadOnlyList<int> ids, bool skipSpecialTokens = true);

    /// <summary>
    /// Decodes multiple sequences of token IDs back into text in a batch.
    /// </summary>
    /// <param name="sequences">The collection of token ID sequences to decode.</param>
    /// <param name="skipSpecialTokens">Whether to skip special tokens in the output.</param>
    /// <returns>A list of decoded text strings.</returns>
    IReadOnlyList<string> DecodeBatch(IEnumerable<IReadOnlyList<int>> sequences, bool skipSpecialTokens = true);

    /// <summary>
    /// Gets the vocabulary size (number of unique tokens) of the tokenizer.
    /// </summary>
    /// <returns>The vocabulary size.</returns>
    int GetVocabSize();

    /// <summary>
    /// Converts a token string to its corresponding token ID.
    /// </summary>
    /// <param name="token">The token string.</param>
    /// <returns>The token ID, or null if the token is not in the vocabulary.</returns>
    int? TokenToId(string token);

    /// <summary>
    /// Converts a token ID to its corresponding token string.
    /// </summary>
    /// <param name="id">The token ID.</param>
    /// <returns>The token string, or null if the ID is out of range.</returns>
    string? IdToToken(int id);
}
