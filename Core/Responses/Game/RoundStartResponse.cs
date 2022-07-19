﻿using OhMyWord.Data.Models;

namespace OhMyWord.Core.Responses.Game;

public class RoundStartResponse
{
    public Guid RoundId { get; init; }
    public int RoundNumber { get; init; }
    public DateTime RoundStarted { get; init; }
    public DateTime RoundEnds { get; init; }
    public WordHint WordHint { get; init; } = WordHint.Default;
}