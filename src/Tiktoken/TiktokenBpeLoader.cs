namespace ErgoX.TokenX.Tiktoken;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

/// <summary>
/// Parses TikToken mergeable rank files (*.tiktoken).
/// </summary>
public static class TiktokenBpeLoader
{
    public static IReadOnlyList<TiktokenMergeableRank> Load(Stream stream)
    {
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        using var reader = new StreamReader(stream, Encoding.ASCII, detectEncodingFromByteOrderMarks: false, bufferSize: 4096, leaveOpen: true);
        var result = new List<TiktokenMergeableRank>();
        var lineNumber = 0;

        while (reader.ReadLine() is { } line)
        {
            lineNumber++;

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length != 2)
            {
                throw new FormatException($"Malformed TikToken BPE line at {lineNumber}.");
            }

            var tokenBytes = DecodeBase64(parts[0], lineNumber);
            if (!int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var rank))
            {
                throw new FormatException($"Invalid rank value at line {lineNumber}.");
            }

            result.Add(new TiktokenMergeableRank(tokenBytes, rank));
        }

        return result;
    }

    public static IReadOnlyList<TiktokenMergeableRank> Load(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path must not be null or whitespace.", nameof(path));
        }

        using var stream = File.OpenRead(path);
        return Load(stream);
    }

    private static ReadOnlyMemory<byte> DecodeBase64(string value, int lineNumber)
    {
        if (string.IsNullOrEmpty(value))
        {
            return ReadOnlyMemory<byte>.Empty;
        }

        try
        {
            var bytes = Convert.FromBase64String(value);
            return new ReadOnlyMemory<byte>(bytes);
        }
        catch (FormatException ex)
        {
            throw new FormatException($"Invalid base64 token at line {lineNumber}.", ex);
        }
    }
}

