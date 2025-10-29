namespace ErgoX.TokenX.HuggingFace.Tests.Integration.Generation;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using ErgoX.TokenX.HuggingFace;
using ErgoX.TokenX.HuggingFace.Chat;
using ErgoX.TokenX.HuggingFace.Generation;
using ErgoX.TokenX.HuggingFace.Tests;
using ErgoX.TokenX.Parity;
using Xunit;

internal static class GenerationTestUtilities
{
    private const string FixtureFileName = "chat-template.json";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly Dictionary<string, ChatTemplateFixture> Fixtures = new(StringComparer.Ordinal);

    public static void AssertChatTemplateCase(string modelFolder, string caseDescription)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modelFolder);
        ArgumentException.ThrowIfNullOrWhiteSpace(caseDescription);

        var fixture = LoadFixture(modelFolder);
        if (!fixture.Cases.TryGetValue(caseDescription, out var chatCase) || chatCase is null)
        {
            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Chat template case '{0}' is not defined for model '{1}'.", caseDescription, modelFolder));
        }

        var messages = chatCase.Messages.Select(Convert).ToArray();
        var chatOptions = new ChatTemplateOptions
        {
            AddGenerationPrompt = chatCase.AddGenerationPrompt
        };

        using var tokenizer = AutoTokenizer.Load(TestDataPath.GetModelRoot(modelFolder));

        if (tokenizer.GenerationConfig is not null)
        {
            var generationRequest = tokenizer.Generate(messages, chatOptions: chatOptions);
            Assert.Equal(chatCase.Rendered, generationRequest.Prompt);
            Assert.Equal(chatCase.RenderedHash, ParityHashUtilities.HashString(generationRequest.Prompt));
            Assert.NotNull(generationRequest.Messages);
            Assert.Equal(messages.Length, generationRequest.Messages!.Count);

            var streamRequest = tokenizer.GenerateStream(messages, chatOptions: chatOptions);
            Assert.Equal(chatCase.Rendered, streamRequest.Prompt);
            Assert.Equal(chatCase.RenderedHash, ParityHashUtilities.HashString(streamRequest.Prompt));
        }
        else
        {
            Assert.Null(tokenizer.GenerationConfig);
        }

        var encoding = tokenizer.ApplyChatTemplateAsEncoding(messages, chatOptions);
        Assert.Equal(chatCase.TokenIds.Count, encoding.Ids.Count);
        Assert.Equal(chatCase.TokenIdsHash, ParityHashUtilities.HashInt32Sequence(encoding.Ids));
        Assert.Equal(chatCase.TokenIds, encoding.Ids.ToArray());
    }

    private static ChatTemplateFixture LoadFixture(string modelFolder)
    {
        if (Fixtures.TryGetValue(modelFolder, out var cached))
        {
            return cached;
        }

        var fixturePath = Path.Combine(TestDataPath.GetModelRoot(modelFolder), FixtureFileName);
        if (!File.Exists(fixturePath))
        {
            throw new FileNotFoundException(string.Format(CultureInfo.InvariantCulture, "Chat template fixture '{0}' was not found.", fixturePath), fixturePath);
        }

        using var stream = File.OpenRead(fixturePath);
        var payload = JsonSerializer.Deserialize<ChatTemplateFixturePayload>(stream, SerializerOptions)
            ?? throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Chat template fixture '{0}' could not be deserialized.", fixturePath));

        if (payload.Cases.Count == 0)
        {
            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Chat template fixture '{0}' does not define any cases.", fixturePath));
        }

        var cases = new Dictionary<string, ChatTemplateCase>(payload.Cases.Count, StringComparer.Ordinal);
        foreach (var entry in payload.Cases)
        {
            var chatCase = CreateCase(entry, fixturePath);
            if (!cases.TryAdd(chatCase.Description, chatCase))
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Chat template fixture '{0}' defines duplicate case '{1}'.", fixturePath, chatCase.Description));
            }
        }

        cached = new ChatTemplateFixture(cases);
        Fixtures[modelFolder] = cached;
        return cached;
    }

    private static ChatTemplateCase CreateCase(ChatTemplateCasePayload entry, string fixturePath)
    {
        if (string.IsNullOrWhiteSpace(entry.Description))
        {
            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Chat template fixture '{0}' contains a case without a description.", fixturePath));
        }

        if (entry.Messages.Count == 0)
        {
            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Chat template fixture '{0}' case '{1}' does not define any messages.", fixturePath, entry.Description));
        }

        if (string.IsNullOrWhiteSpace(entry.Rendered) || string.IsNullOrWhiteSpace(entry.RenderedHash))
        {
            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Chat template fixture '{0}' case '{1}' does not define rendered output hashes.", fixturePath, entry.Description));
        }

        if (entry.TokenIds.Count == 0 || string.IsNullOrWhiteSpace(entry.TokenIdsHash))
        {
            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Chat template fixture '{0}' case '{1}' does not define token id expectations.", fixturePath, entry.Description));
        }

        return new ChatTemplateCase(
            entry.Description,
            entry.AddGenerationPrompt,
            entry.Messages,
            entry.Rendered,
            entry.RenderedHash,
            entry.TokenIds,
            entry.TokenIdsHash);
    }

    private static ChatMessage Convert(ChatTemplateMessage definition)
    {
        return definition.Content.ValueKind switch
        {
            JsonValueKind.Array => ChatMessage.FromParts(
                definition.Role,
                MaterializeParts(definition.Content),
                null),
            JsonValueKind.Object => ChatMessage.FromParts(
                definition.Role,
                new[] { DeserializePart(definition.Content) },
                null),
            JsonValueKind.Null => ChatMessage.FromText(definition.Role, string.Empty, null),
            JsonValueKind.String => ChatMessage.FromText(definition.Role, definition.Content.GetString() ?? string.Empty, null),
            _ => ChatMessage.FromText(definition.Role, definition.Content.GetRawText(), null)
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

    private static ChatMessagePart DeserializePart(JsonElement element)
    {
        if (!element.TryGetProperty("type", out var typeProperty))
        {
            throw new InvalidOperationException("Chat template part is missing a 'type' property.");
        }

        var type = typeProperty.GetString();
        return type switch
        {
            "text" => new ChatTextPart(element.GetProperty("text").GetString() ?? string.Empty),
            "image_url" => new ChatImageUrlPart(
                element.GetProperty("image_url").GetProperty("url").GetString() ?? string.Empty,
                element.GetProperty("image_url").TryGetProperty("mime_type", out var mime)
                    ? mime.GetString()
                    : null),
            _ => throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Unsupported chat message part type '{0}'.", type))
        };
    }

    private sealed record ChatTemplateFixture(Dictionary<string, ChatTemplateCase> Cases);

    private sealed record ChatTemplateCase(
        string Description,
        bool AddGenerationPrompt,
        IReadOnlyList<ChatTemplateMessage> Messages,
        string Rendered,
        string RenderedHash,
        IReadOnlyList<int> TokenIds,
        string TokenIdsHash);

    private sealed class ChatTemplateFixturePayload
    {
        public List<ChatTemplateCasePayload> Cases { get; init; } = new();
    }

    private sealed class ChatTemplateCasePayload
    {
        public string Description { get; init; } = string.Empty;

    public bool AddGenerationPrompt { get; init; } = false;

        public List<ChatTemplateMessage> Messages { get; init; } = new();

        public string Rendered { get; init; } = string.Empty;

        public string RenderedHash { get; init; } = string.Empty;

        public List<int> TokenIds { get; init; } = new();

        public string TokenIdsHash { get; init; } = string.Empty;
    }

    private sealed class ChatTemplateMessage
    {
        public string Role { get; init; } = string.Empty;

    public JsonElement Content { get; init; } = default;

        [JsonExtensionData]
        public Dictionary<string, JsonElement> AdditionalProperties { get; init; } = new(StringComparer.Ordinal);
    }
}

