namespace OhMyWord.Data.Models;

public class Word : Entity
{
    public string Value { get; init; } = string.Empty;
    public string Definition { get; init; } = string.Empty;

    public override string ToString() => Value;

    public static readonly Word Default = new();
}
