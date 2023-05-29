﻿using MediatR;
using OhMyWord.Domain.Contracts.Requests;
using OhMyWord.Domain.Contracts.Results;
using OhMyWord.Domain.Services;

namespace OhMyWord.Domain.Contracts.Handlers;

public class RegisterPlayerHandler : IRequestHandler<RegisterPlayerRequest, RegisterPlayerResult>
{
    private readonly IPlayerService playerService;
    private readonly IStateManager stateManager;

    public RegisterPlayerHandler(IPlayerService playerService, IStateManager stateManager)
    {
        this.playerService = playerService;
        this.stateManager = stateManager;
    }

    public async Task<RegisterPlayerResult> Handle(RegisterPlayerRequest request, CancellationToken cancellationToken)
    {
        var player = await playerService.GetPlayerAsync(request.PlayerId, request.VisitorId,
            request.ConnectionId, request.IpAddress, request.UserId, cancellationToken);
        var isSuccessful = stateManager.PlayerState.AddPlayer(player);

        return new RegisterPlayerResult
        {
            IsSuccessful = isSuccessful,
            PlayerId = player.Id,
            PlayerCount = stateManager.PlayerState.PlayerCount,
            Score = player.Score,
            RegistrationCount = player.RegistrationCount,
            StateSnapshot = stateManager.GetStateSnapshot()
        };
    }
}
