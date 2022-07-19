using OhMyWord.Core.Extensions;

namespace OhMyWord.Core.Models;

public class Word : Entity
{
    public string Value { get; init; } = string.Empty;
    public string Definition { get; init; } = string.Empty;
    public PartOfSpeech PartOfSpeech { get; init; }

    public WordHint GetWordHint() => new(this);
    public LetterHint GetLetterHint(int position) => new(position, Value[position - 1]);

    public override string GetPartition() => PartOfSpeech.ToPartitionKey();

    public override string ToString() => Value;

    public static readonly Word Default = new()
    {
        Value = "default",
        Definition = "Default word"
    };
}