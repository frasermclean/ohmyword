﻿using OhMyWord.Core.Models;
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
        EndReason = round.EndReason ?? throw new InvalidOperationException("Round end reason has not been set"),
        SessionId = round.SessionId,
        PlayerData = round.GetPlayerData()
    };
}
