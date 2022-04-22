using OhMyWord.Core.Extensions;

namespace OhMyWord.Core.Models;

public class Word : Entity
{
    public string Value => Id;
    public string Definition { get; init; } = string.Empty;
    public PartOfSpeech PartOfSpeech { get; init; }

    public override string GetPartition() => PartOfSpeech.ToPartitionKey();

    public override string ToString() => Value;

    public static readonly Word Default = new()
    {
        Id = "default",
        Definition = "Default word"
    };
}
