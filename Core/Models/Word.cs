namespace OhMyWord.Core.Models;

public class Word : Entity
{
    public string Value => Id;
    public string Definition { get; init; } = string.Empty;
    public PartOfSpeech PartOfSpeech { get; init; }

    public override string ToString() => Id;

    public static readonly Word Default = new()
    {
        Id = "default",
        Definition = "Default word"
    };
}
