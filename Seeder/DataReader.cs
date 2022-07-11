using OhMyWord.Core.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OhMyWord.Seeder;

internal class DataReader
{
    private const string Filename = "data.json";
    private readonly Data data;

    public IEnumerable<Word> Words => data.Words;

    public DataReader()
    {
        var json = File.ReadAllText(Filename);
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        };

        data = JsonSerializer.Deserialize<Data>(json, options) ?? new Data();
    }
}
