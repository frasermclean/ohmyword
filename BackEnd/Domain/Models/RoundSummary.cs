﻿using OhMyWord.Infrastructure.Models.Entities;

namespace OhMyWord.Domain.Models;

public class RoundSummary
{
    public required string Word { get; init; }
    public required PartOfSpeech PartOfSpeech { get; init; }
    public required RoundEndReason EndReason { get; init; }
}
