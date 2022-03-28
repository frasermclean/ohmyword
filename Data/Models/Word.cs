namespace OhMyWord.Data.Models;

public class Word : Entity
{
    public string Value { get; set; } = string.Empty;
    public string Definition { get; set; } = string.Empty;

    public override string ToString() => Value;

    public static readonly Word Default = new();
}
