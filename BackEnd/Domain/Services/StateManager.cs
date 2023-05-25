﻿using Microsoft.Extensions.Logging;
using OhMyWord.Domain.Models;

namespace OhMyWord.Domain.Services;

public interface IStateManager
{
    SessionState SessionState { get; }
    Session? Session { get; }
    Round? Round { get; }
    bool IsDefault { get; }
    DateTime IntervalStart { get; }
    DateTime IntervalEnd { get; }

    Session NextSession();
    Task<Round> NextRoundAsync(CancellationToken cancellationToken = default);
    void Reset();
}

public class StateManager : IStateManager
{
    private readonly ILogger<StateManager> logger;
    private readonly IRoundService roundService;

    public StateManager(ILogger<StateManager> logger, IRoundService roundService)
    {
        this.logger = logger;
        this.roundService = roundService;
    }

    public SessionState SessionState => this switch
    {
        { IsDefault: false, IsRoundActive: true } => SessionState.RoundActive,
        { IsDefault: false, IsRoundActive: false } => SessionState.RoundEnded,
        _ => SessionState.Waiting,
    };

    public Session? Session { get; private set; }
    public Round? Round { get; private set; }
    public bool IsDefault => Session is null && Round is null;
    public DateTime IntervalStart => Round?.StartDate ?? default;
    public DateTime IntervalEnd => Round?.EndDate ?? default;
    private bool IsRoundActive => Round?.IsActive ?? false;

    public Session NextSession()
    {
        Session = new Session();
        logger.LogInformation("Created session with ID: {SessionId}", Session.Id);

        return Session;
    }

    public async Task<Round> NextRoundAsync(CancellationToken cancellationToken)
    {
        if (Session is null)
        {
            logger.LogWarning("Session should be created before starting a round");
            Session = NextSession();
        }

        var roundNumber = Session.RoundCount++;
        Round = await roundService.CreateRoundAsync(roundNumber, Session.Id, cancellationToken);
        logger.LogInformation("Created round with ID: {RoundId}", Round.Id);

        return Round;
    }

    public void Reset()
    {
        Session = null;
        Round = null;
    }
}
