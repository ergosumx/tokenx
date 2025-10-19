namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Chat;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Options;

internal static class ChatTemplatePayloadBuilder
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static string? BuildVariablesJson(
        TokenizerConfig? config,
        SpecialTokensMap? specialTokens,
        IReadOnlyDictionary<string, JsonNode?>? userVariables)
    {
        var payload = new JsonObject();

        AddCoreTokens(payload, config, specialTokens);
        AddTemplateRoles(payload, config);
        AddAdditionalTokens(payload, specialTokens);
        ApplyUserVariables(payload, userVariables);

        return payload.Count == 0 ? null : payload.ToJsonString(SerializerOptions);
    }

    private static void AddCoreTokens(
        JsonObject payload,
        TokenizerConfig? config,
        SpecialTokensMap? specialTokens)
    {
        AddToken(payload, "bos_token", config?.BosToken, specialTokens?.BosToken);
        AddToken(payload, "eos_token", config?.EosToken, specialTokens?.EosToken);
        AddToken(payload, "pad_token", config?.PadToken, specialTokens?.PadToken);
        AddToken(payload, "unk_token", config?.UnknownToken, specialTokens?.UnknownToken);

        AddTokenId(payload, "bos_token_id", config?.BosTokenId, specialTokens?.BosToken);
        AddTokenId(payload, "eos_token_id", config?.EosTokenId, specialTokens?.EosToken);
        AddTokenId(payload, "pad_token_id", config?.PadTokenId, specialTokens?.PadToken);
        AddTokenId(payload, "unk_token_id", config?.UnknownTokenId, specialTokens?.UnknownToken);

        if (config?.ModelMaxLength is int maxLength && maxLength > 0)
        {
            payload["model_max_length"] = JsonValue.Create(maxLength);
        }
    }

    private static void AddTemplateRoles(JsonObject payload, TokenizerConfig? config)
    {
        if (config?.ChatTemplateRoles is not { Count: > 0 })
        {
            return;
        }

        var rolesObject = new JsonObject();
        foreach (var pair in config.ChatTemplateRoles)
        {
            rolesObject[pair.Key] = JsonValue.Create(pair.Value);
        }

        payload["chat_template_roles"] = rolesObject;
    }

    private static void AddAdditionalTokens(JsonObject payload, SpecialTokensMap? specialTokens)
    {
        if (specialTokens?.AdditionalSpecialTokens is not { Count: > 0 } tokens)
        {
            return;
        }

        var array = new JsonArray();
        foreach (var token in tokens.Where(token => !string.IsNullOrWhiteSpace(token.Content)))
        {
            array.Add(JsonValue.Create(token.Content));
        }

        if (array.Count > 0)
        {
            payload["additional_special_tokens"] = array;
        }
    }

    private static void ApplyUserVariables(JsonObject payload, IReadOnlyDictionary<string, JsonNode?>? userVariables)
    {
        if (userVariables is null)
        {
            return;
        }

        foreach (var pair in userVariables)
        {
            if (string.IsNullOrWhiteSpace(pair.Key))
            {
                continue;
            }

            payload[pair.Key] = pair.Value?.DeepClone();
        }
    }

    private static void AddToken(
        JsonObject payload,
        string key,
        string? configValue,
        SpecialTokensMap.TokenDefinition? tokenDefinition)
    {
        var value = ResolveToken(configValue, tokenDefinition);
        if (!string.IsNullOrEmpty(value))
        {
            payload[key] = JsonValue.Create(value);
        }
    }

    private static void AddTokenId(
        JsonObject payload,
        string key,
        int? configValue,
        SpecialTokensMap.TokenDefinition? tokenDefinition)
    {
        var value = configValue ?? tokenDefinition?.Id;
        if (value.HasValue)
        {
            payload[key] = JsonValue.Create(value.Value);
        }
    }

    private static string? ResolveToken(string? configValue, SpecialTokensMap.TokenDefinition? tokenDefinition)
    {
        if (!string.IsNullOrEmpty(configValue))
        {
            return configValue;
        }

        if (!string.IsNullOrEmpty(tokenDefinition?.Content))
        {
            return tokenDefinition!.Content;
        }

        return null;
    }
}
