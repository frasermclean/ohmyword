﻿namespace OhMyWord.Core.Models;

public class Word
{
    public const int MinLength = 4;
    public const int MaxLength = 16;
    public const double FrequencyMinValue = 1;
    public const double FrequencyMaxValue = 7;

    public required string Id { get; init; } = string.Empty;

    /// <summary>
    /// Number of characters in the word.
    /// </summary>
    public int Length => Id.Length;

    /// <summary>
    /// Collection of definitions for the word.
    /// </summary>
    public required IEnumerable<Definition> Definitions { get; init; }

    /// <summary>
    /// Logarithmic frequency of the word ranging from 1 to 7. Higher is more frequent.
    /// </summary>
    public required double Frequency { get; init; }

    /// <summary>
    /// Number of points awarded for correctly guessing the word.
    /// </summary>
    public int Bounty => this == Default ? default : CalculateBounty(Length, Frequency);

    /// <summary>
    /// User ID of the user who last modified the word.
    /// </summary>
    public Guid? LastModifiedBy { get; init; }

    public DateTime LastModifiedTime { get; init; } = DateTime.UtcNow;

    public override string ToString() => Id;

    public static readonly Word Default = new()
    {
        Id = "default",
        Definitions = new[] { Definition.Default },
        Frequency = default,
        LastModifiedBy = default,
        LastModifiedTime = DateTime.MinValue,
    };

    public static int CalculateBounty(int length, double frequency) => Convert.ToInt32((length * 10 + 50) / frequency);
}
