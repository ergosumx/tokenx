namespace ErgoX.TokenX.HuggingFace.Tests.Integration.Chat;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using ErgoX.TokenX.HuggingFace;
using ErgoX.TokenX.HuggingFace.Chat;
using ErgoX.TokenX.Parity;
using ErgoX.TokenX.HuggingFace.Tests;
using Xunit;

[Trait(TestCategories.Category, TestCategories.Integration)]
[Trait(TestCategories.Filter, TestCategories.Integration)]
public sealed class ChatTemplateIntegrationTests
{
    private const string ChatFixtureFileName = "chat-template.json";
    private const string SolutionFileName = "TokenX.sln";

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
        return element.GetProperty("type").GetString() switch
        {
            "text" => new ChatTextPart(element.GetProperty("text").GetString() ?? string.Empty),
            "image_url" => new ChatImageUrlPart(
                element.GetProperty("image_url").GetProperty("url").GetString() ?? string.Empty,
                element.GetProperty("image_url").TryGetProperty("mime_type", out var mime)
                    ? mime.GetString()
                    : null),
            _ => throw new InvalidOperationException($"Unsupported chat message part type '{element.GetProperty("type").GetString()}'.")
        };
    }

    private static IEnumerable<ChatMessagePart> MaterializeParts(JsonElement array)
    {
        using var enumerator = array.EnumerateArray();
        while (enumerator.MoveNext())
        {
            yield return DeserializePart(enumerator.Current);
        }
    }

    private static string GetBenchmarksDataRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var solutionCandidate = Path.Combine(directory.FullName, SolutionFileName);
            if (File.Exists(solutionCandidate))
            {
                return Path.Combine(directory.FullName, "tests", "_huggingface");
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Unable to locate repository root from test context.");
    }
}

