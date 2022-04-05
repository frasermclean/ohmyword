﻿using Microsoft.AspNetCore.SignalR;
using OhMyWord.Api.Hubs;
using OhMyWord.Services.Game;

namespace OhMyWord.Api;

public class GameCoordinator : BackgroundService
{
    private readonly ILogger<GameCoordinator> logger;
    private readonly IGameService gameService;
    private readonly IHubContext<GameHub, IGameHub> gameHubContext;

    public GameCoordinator(
        ILogger<GameCoordinator> logger,
        IGameService gameService,
        IHubContext<GameHub, IGameHub> gameHubContext
        )
    {
        this.logger = logger;
        this.gameService = gameService;
        this.gameHubContext = gameHubContext;

    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // start round
                var roundDelay = await gameService.StartRoundAsync();
                await Task.WhenAll(
                    gameHubContext.Clients.All.SendGameStatus(gameService.GetGameStatus()),
                    gameHubContext.Clients.All.SendHint(gameService.GetHint()));

                await Task.Delay(roundDelay, cancellationToken);

                // end of round delay
                var postRoundDelay = await gameService.EndRoundAsync();
                await gameHubContext.Clients.All.SendGameStatus(gameService.GetGameStatus());
                await Task.Delay(postRoundDelay, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception occurred in game coordinator main loop.");
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
        }
    }
}
