namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Chat;

using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

internal static class ChatMessageSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static string Serialize(IReadOnlyList<ChatMessage> messages)
    {
        ArgumentNullException.ThrowIfNull(messages);
        if (messages.Count == 0)
        {
            throw new ArgumentException("At least one chat message is required.", nameof(messages));
        }

        var array = new JsonArray();
        foreach (var message in messages)
        {
            ArgumentNullException.ThrowIfNull(message);

            var payload = new JsonObject
            {
                ["role"] = JsonValue.Create(message.Role)
            };

            foreach (var pair in message.Metadata)
            {
                if (string.Equals(pair.Key, "role", StringComparison.Ordinal) || string.Equals(pair.Key, "content", StringComparison.Ordinal))
                {
                    continue;
                }

                payload[pair.Key] = pair.Value?.DeepClone();
            }

            if (message.Parts.Count > 0)
            {
                var partsArray = new JsonArray();
                foreach (var part in message.Parts)
                {
                    partsArray.Add(part.ToJson());
                }

                payload["content"] = partsArray;
            }
            else
            {
                payload["content"] = JsonValue.Create(message.Content ?? string.Empty);
            }

            array.Add(payload);
        }

        return array.ToJsonString(SerializerOptions);
    }
}
