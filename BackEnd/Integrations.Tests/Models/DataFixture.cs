using OhMyWord.Core.Models;
using OhMyWord.Infrastructure.Models.Entities;

namespace OhMyWord.Integrations.Tests.Models;

public class DataFixture
{
    public WordEntity TestWord { get; } = new() { Id = "test", DefinitionCount = 1, Timestamp = 123 };

    public DefinitionEntity TestDefinition { get; } = new()
    {
        Id = "10271ba9-60ec-4073-8552-14dbb477a895",
        Value = "Test definition",
        PartOfSpeech = PartOfSpeech.Noun,
        Example = "Test example",
        WordId = "test"
    };

    public PlayerEntity TestPlayer { get; } = new() { Id = "abc123", RegistrationCount = 3, Score = 400 };
}
