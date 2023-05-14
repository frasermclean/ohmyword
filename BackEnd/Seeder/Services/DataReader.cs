using OhMyWord.Infrastructure.Models.Entities;
using OhMyWord.Seeder.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OhMyWord.Seeder.Services;

internal class DataReader
{
    private readonly RootObject rootObject;

    public IEnumerable<WordEntity> Words => rootObject.Words.Select(wordWithDefinitions => new WordEntity
    {
        Id = wordWithDefinitions.Id, DefinitionCount = wordWithDefinitions.Definitions.Count
    });


    public DataReader()
    {
        var json = File.ReadAllText("data.json");
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        };

        rootObject = JsonSerializer.Deserialize<RootObject>(json, options) ?? new RootObject();
    }

    public IEnumerable<DefinitionEntity> GetDefinitions(string wordId)
    {
        var wordWithDefinitions = rootObject.Words.FirstOrDefault(word => word.Id == wordId);
        return wordWithDefinitions?.Definitions.Select(entity => entity with { WordId = wordId })
               ?? Enumerable.Empty<DefinitionEntity>();
    }
}
