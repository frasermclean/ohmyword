using Azure.Core.Serialization;
using Microsoft.Azure.Cosmos;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OhMyWord.Infrastructure;

internal class EntitySerializer : CosmosSerializer
{
    private static readonly JsonObjectSerializer Serializer = new(new JsonSerializerOptions
    {
        IgnoreReadOnlyProperties = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    });

    private EntitySerializer() { }

    public override T FromStream<T>(Stream stream) => ConvertFromStream<T>(stream);
    public override Stream ToStream<T>(T input) => ConvertToStream(input);
    
    public static EntitySerializer Instance { get; } = new();

    public static T ConvertFromStream<T>(Stream stream)
    {
        using (stream)
        {
            if (stream is { CanSeek: true, Length: 0 })
                return default!;

            if (typeof(Stream).IsAssignableFrom(typeof(T)))
                return (T)(object)stream;

            return (T)Serializer.Deserialize(stream, typeof(T), default)!;
        }
    }

    public static Stream ConvertToStream<T>(T input)
    {
        MemoryStream stream = new();
        Serializer.Serialize(stream, input, typeof(T), default);
        stream.Position = 0;

        return stream;
    }
}
