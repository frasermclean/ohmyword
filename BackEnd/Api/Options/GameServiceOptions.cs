using System.ComponentModel.DataAnnotations;

namespace OhMyWord.Api.Options;

public class GameServiceOptions
{
    public const string SectionName = "Game";

    [Range(1, 10)] public double LetterHintDelay { get; init; } = 3;
    [Range(1, 60)] public double PostRoundDelay { get; init; } = 5;
}
