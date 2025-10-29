namespace ErgoX.VecraX.ML.NLP.Tokenizers.Google.SentencePiece.Models;

using System.Collections.Generic;

/// <summary>
/// Represents normalized text with character offset information.
/// </summary>
/// <param name="Text">The normalized text string.</param>
/// <param name="Offsets">Character position offsets mapping the normalized text back to the original input.</param>
public sealed record NormalizedText(string Text, IReadOnlyList<int> Offsets);

/// <summary>
/// Represents a sequence of token IDs with an associated score.
/// Used as a result for N-best and sample-based encoding operations.
/// </summary>
/// <param name="Ids">The token IDs in the sequence.</param>
/// <param name="Score">The score associated with this tokenization.</param>
public sealed record ScoredIdSequence(IReadOnlyList<int> Ids, float Score);

/// <summary>
/// Represents a sequence of token pieces with an associated score.
/// Used as a result for N-best and sample-based encoding operations.
/// </summary>
/// <param name="Pieces">The token pieces in the sequence.</param>
/// <param name="Score">The score associated with this tokenization.</param>
public sealed record ScoredPieceSequence(IReadOnlyList<string> Pieces, float Score);
