﻿using FastEndpoints;
using OhMyWord.Core.Services;

namespace OhMyWord.Api.Commands.SubmitGuess;

public class SubmitGuessHandler : ICommandHandler<SubmitGuessCommand, SubmitGuessResponse>
{
    private readonly IGameService gameService;

    public SubmitGuessHandler(IGameService gameService)
    {
        this.gameService = gameService;
    }

    public async Task<SubmitGuessResponse> ExecuteAsync(SubmitGuessCommand command, CancellationToken cancellationToken)
    {
        //await validator.ValidateAndThrowAsync(request, cancellationToken);
        var points = await gameService.ProcessGuessAsync(command.ConnectionId, command.RoundId, command.Value);
        return new SubmitGuessResponse
        {
            Value = command.Value.ToLowerInvariant(), Correct = points > 0, Points = points
        };
    }
}
