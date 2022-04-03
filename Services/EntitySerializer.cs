using Azure.Core.Serialization;
using Microsoft.Azure.Cosmos;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OhMyWord.Services;

internal class EntitySerializer : CosmosSerializer
{
    private readonly JsonObjectSerializer serializer;

    public EntitySerializer()
    {
        serializer = new JsonObjectSerializer(new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        });
    }

    public override T FromStream<T>(Stream stream)
    {
        using (stream)
        {
            if (stream.CanSeek && stream.Length == 0)
                return default!;

            if (typeof(Stream).IsAssignableFrom(typeof(T)))
                return (T)(object)stream;

            return (T)serializer.Deserialize(stream, typeof(T), default)!;
        }
    }

    public override Stream ToStream<T>(T input)
    {
        MemoryStream stream = new();
        serializer.Serialize(stream, input, typeof(T), default);
        stream.Position = 0;

        return stream;
    }
}
