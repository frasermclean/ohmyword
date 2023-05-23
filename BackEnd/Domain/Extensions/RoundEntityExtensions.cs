﻿using OhMyWord.Domain.Models;
using OhMyWord.Infrastructure.Models.Entities;

namespace OhMyWord.Domain.Extensions;

public static class RoundEntityExtensions
{
    public static RoundEntity ToEntity(this Round round) => new()
    {
        Id = round.Id.ToString(),
        Number = round.Number,
        WordId = round.Word.Id,
        DefinitionId = round.WordHint.DefinitionId,
        GuessLimit = round.GuessLimit,
        StartDate = round.StartDate,
        EndDate = round.EndDate,
        EndReason = round.EndReason,
        SessionId = round.SessionId,
        PlayerData = round.GetPlayerData()
    };
}
