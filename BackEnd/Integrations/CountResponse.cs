﻿using System.Text.Json.Serialization;

namespace OhMyWord.Integrations;

internal class CountResponse
{
    [JsonPropertyName("$1")]
    public int Count { get; init; }
}