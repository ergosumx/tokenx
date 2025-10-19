namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests.Chat;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Chat;
using ErgoX.VecraX.ML.NLP.Tokenizers.Parity;
using Xunit;

public sealed class ChatTemplateTests
{
    private const string ChatFixtureFileName = "chat-template.json";
    private const string SolutionFileName = "TokenX.HF.sln";

    public static IEnumerable<object[]> ModelIdentifiers()
    {
        var root = GetBenchmarksDataRoot();
        foreach (var directory in Directory.EnumerateDirectories(root))
        {
            var fixturePath = Path.Combine(directory, ChatFixtureFileName);
            if (File.Exists(fixturePath))
            {
                yield return new object[] { Path.GetFileName(directory) };
            }
        }
    }

    [Theory]
    [MemberData(nameof(ModelIdentifiers))]
    public void ApplyChatTemplate_matches_python_reference(string modelFolder)
    {
        var root = GetBenchmarksDataRoot();
        var modelRoot = Path.Combine(root, modelFolder);
        var fixture = LoadFixture(modelRoot);

        using var autoTokenizer = AutoTokenizer.Load(modelRoot);
        Assert.Equal(fixture.Cases.Count > 0, autoTokenizer.SupportsChatTemplate);

        foreach (var testCase in fixture.Cases)
        {
            var messages = testCase.Messages.Select(Convert).ToArray();
            var options = new ChatTemplateOptions
            {
                AddGenerationPrompt = testCase.AddGenerationPrompt
            };

            var rendered = autoTokenizer.ApplyChatTemplate(messages, options);
            Assert.Equal(testCase.RenderedHash, ParityHashUtilities.HashString(rendered));
            Assert.Equal(testCase.Rendered, rendered);

            var encoding = autoTokenizer.ApplyChatTemplateAsEncoding(messages, options);
            Assert.Equal(testCase.TokenIds.Count, encoding.Ids.Count);
            Assert.Equal(testCase.TokenIdsHash, ParityHashUtilities.HashInt32Sequence(encoding.Ids));
            Assert.Equal(testCase.TokenIds, encoding.Ids.ToArray());
        }
    }

    private static ChatTemplateFixture LoadFixture(string modelRoot)
    {
        var path = Path.Combine(modelRoot, ChatFixtureFileName);
        using var stream = File.OpenRead(path);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        return JsonSerializer.Deserialize<ChatTemplateFixture>(stream, options)
               ?? throw new InvalidOperationException($"Failed to parse chat template fixture at '{path}'.");
    }

    private static ChatMessage Convert(ChatTemplateMessage definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        var metadata = BuildMetadata(definition.AdditionalProperties);

        return definition.Content.ValueKind switch
        {
            JsonValueKind.Array => ChatMessage.FromParts(
                definition.Role,
                MaterializeParts(definition.Content),
                metadata),
            JsonValueKind.Object => ChatMessage.FromParts(
                definition.Role,
                new[] { DeserializePart(definition.Content) },
                metadata),
            JsonValueKind.Null => ChatMessage.FromText(definition.Role, string.Empty, metadata),
            JsonValueKind.String => ChatMessage.FromText(definition.Role, definition.Content.GetString() ?? string.Empty, metadata),
            _ => ChatMessage.FromText(definition.Role, definition.Content.GetRawText(), metadata)
        };
    }

    private static IReadOnlyDictionary<string, JsonNode?>? BuildMetadata(IReadOnlyDictionary<string, JsonElement> additional)
    {
        if (additional is null || additional.Count == 0)
        {
            return null;
        }

        var buffer = new Dictionary<string, JsonNode?>(additional.Count, StringComparer.Ordinal);
        foreach (var pair in additional)
        {
            if (string.Equals(pair.Key, "role", StringComparison.Ordinal) || string.Equals(pair.Key, "content", StringComparison.Ordinal))
            {
                continue;
            }

            buffer[pair.Key] = pair.Value.ValueKind == JsonValueKind.Null
                ? null
                : JsonNode.Parse(pair.Value.GetRawText());
        }

        return buffer.Count == 0 ? null : buffer;
    }

    private static ChatMessagePart DeserializePart(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => new ChatTextPart(element.GetString() ?? string.Empty),
            JsonValueKind.Object => DeserializeObjectPart(element),
            JsonValueKind.Array => new ChatGenericPart((JsonObject?)JsonNode.Parse(element.GetRawText()) ?? new JsonObject()),
            JsonValueKind.Null => new ChatTextPart(string.Empty),
            _ => new ChatTextPart(element.GetRawText())
        };
    }

    private static ChatMessagePart[] MaterializeParts(JsonElement array)
    {
        var parts = new List<ChatMessagePart>();
        using var enumerator = array.EnumerateArray();
        while (enumerator.MoveNext())
        {
            parts.Add(DeserializePart(enumerator.Current));
        }

        return parts.ToArray();
    }

    private static ChatMessagePart DeserializeObjectPart(JsonElement element)
    {
        var node = JsonNode.Parse(element.GetRawText()) as JsonObject
                   ?? throw new InvalidOperationException("Unable to parse chat message part payload into a JSON object.");

        if (!node.TryGetPropertyValue("type", out var typeNode) || typeNode is not JsonValue typeValue || !typeValue.TryGetValue<string>(out var typeName))
        {
            return new ChatGenericPart(node);
        }

        if (string.Equals(typeName, "text", StringComparison.OrdinalIgnoreCase))
        {
            if (node.TryGetPropertyValue("text", out var textNode) && textNode is JsonValue textValue && textValue.TryGetValue<string>(out var text))
            {
                return new ChatTextPart(text);
            }

            return new ChatTextPart(string.Empty);
        }

        if (string.Equals(typeName, "image_url", StringComparison.OrdinalIgnoreCase)
            && node.TryGetPropertyValue("image_url", out var imageNode)
            && imageNode is JsonObject imageObject)
        {
            var url = TryGetString(imageObject, "url") ?? string.Empty;
            var mimeType = TryGetString(imageObject, "mime_type");
            return new ChatImageUrlPart(url, mimeType);
        }

        return new ChatGenericPart(node);
    }

    private static string? TryGetString(JsonObject payload, string key)
    {
        if (!payload.TryGetPropertyValue(key, out var candidate) || candidate is not JsonValue value)
        {
            return null;
        }

        return value.TryGetValue<string>(out var result) ? result : null;
    }

    private static string GetBenchmarksDataRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var solutionCandidate = Path.Combine(directory.FullName, SolutionFileName);
            if (File.Exists(solutionCandidate))
            {
                return Path.Combine(directory.FullName, "tests", "_TestData");
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Unable to locate repository root from test context.");
    }
}
