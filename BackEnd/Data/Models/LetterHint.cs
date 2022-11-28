namespace OhMyWord.Data.Models;

public class LetterHint
{
    public int Position { get; }
    public char Value { get; }

    internal LetterHint(int position, char value)
    {
        Position = position;
        Value = value;
    }
}
