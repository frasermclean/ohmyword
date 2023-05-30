using FastEndpoints;
using Microsoft.Extensions.Logging;
using OhMyWord.Domain.Contracts.Events;
using OhMyWord.Domain.Extensions;
using OhMyWord.Domain.Models;
using OhMyWord.Infrastructure.Services;

namespace OhMyWord.Domain.Services;

public interface ISessionService
{
    Task ExecuteSessionAsync(Session session, CancellationToken cancellationToken = default);
    Task SaveSessionAsync(Session session, CancellationToken cancellationToken = default);
}

public sealed class SessionService : ISessionService
{
    private readonly ILogger<SessionService> logger;
    private readonly IStateManager stateManager;
    private readonly IRoundService roundService;
    private readonly ISessionsRepository sessionsRepository;

    public SessionService(ILogger<SessionService> logger, IStateManager stateManager, IRoundService roundService,
        ISessionsRepository sessionsRepository)
    {
        this.logger = logger;
        this.stateManager = stateManager;
        this.roundService = roundService;
        this.sessionsRepository = sessionsRepository;
    }

    public async Task ExecuteSessionAsync(Session session, CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting session: {SessionId}", session.Id);

        // create and execute rounds while there are players
        while (stateManager.PlayerState.PlayerCount > 0)
        {
            // load the next round
            using var round = await stateManager.NextRoundAsync(cancellationToken);
            await SendRoundStartedNotificationAsync(round, cancellationToken);

            // execute round            
            await roundService.ExecuteRoundAsync(round, cancellationToken);
            await roundService.SaveRoundAsync(round, cancellationToken);

            // post round delay
            var (postRoundDelay, summary) = roundService.GetRoundEndData(round);
            await Task.WhenAll(
                SendRoundEndedNotificationAsync(summary, cancellationToken),
                Task.Delay(postRoundDelay, cancellationToken));
        }
    }

    public Task SaveSessionAsync(Session session, CancellationToken cancellationToken)
        => sessionsRepository.CreateSessionAsync(session.ToEntity(), cancellationToken);

    private static Task SendRoundStartedNotificationAsync(Round round, CancellationToken cancellationToken)
        => new RoundStartedEvent
        {
            RoundNumber = round.Number,
            RoundId = round.Id,
            WordHint = round.WordHint,
            StartDate = round.StartDate,
            EndDate = round.EndDate
        }.PublishAsync(cancellation: cancellationToken);

    private static Task SendRoundEndedNotificationAsync(RoundSummary summary, CancellationToken cancellationToken)
        => new RoundEndedEvent(summary).PublishAsync(cancellation: cancellationToken);
}
