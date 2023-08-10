namespace OhMyWord.Core.Models;

public class Definition
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required PartOfSpeech PartOfSpeech { get; init; }
    public required string Value { get; init; }

    /// <summary>
    /// Example of this <see cref="Definition"/> used in a sentence.
    /// </summary>
    public string? Example { get; init; }

    public static readonly Definition Default = new()
    {
        PartOfSpeech = PartOfSpeech.Noun, Value = "Default definition",
    };
}
