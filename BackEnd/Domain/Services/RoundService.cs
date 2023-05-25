using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OhMyWord.Domain.Extensions;
using OhMyWord.Domain.Models;
using OhMyWord.Domain.Notifications;
using OhMyWord.Domain.Options;
using OhMyWord.Infrastructure.Models.Entities;
using OhMyWord.Infrastructure.Services;

namespace OhMyWord.Domain.Services;

public interface IRoundService
{
    Task<Round> CreateRoundAsync(int roundNumber, Guid sessionId, CancellationToken cancellationToken = default);
    Task ExecuteRoundAsync(Round round, CancellationToken cancellationToken = default);
    Task SaveRoundAsync(Round round, CancellationToken cancellationToken = default);
}

public class RoundService : IRoundService
{
    private readonly ILogger<RoundService> logger;
    private readonly IPublisher publisher;
    private readonly IPlayerState playerState;
    private readonly IWordsService wordsService;
    private readonly IRoundsRepository roundsRepository;

    private readonly double letterHintDelay;
    private readonly int guessLimit;

    public RoundService(ILogger<RoundService> logger, IOptions<RoundOptions> options, IPublisher publisher,
        IPlayerState playerState, IWordsService wordsService, IRoundsRepository roundsRepository)
    {
        this.logger = logger;
        this.publisher = publisher;
        this.playerState = playerState;
        this.wordsService = wordsService;
        this.roundsRepository = roundsRepository;

        letterHintDelay = options.Value.LetterHintDelay;
        guessLimit = options.Value.GuessLimit;
    }

    public async Task<Round> CreateRoundAsync(int roundNumber, Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        var word = await wordsService.GetRandomWordAsync(cancellationToken);

        return new Round(word, letterHintDelay, playerState.PlayerIds)
        {
            Number = roundNumber, GuessLimit = guessLimit, SessionId = sessionId,
        };
    }

    public async Task ExecuteRoundAsync(Round round, CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting round {RoundNumber} for session {SessionId}", round.Number, round.SessionId);

        // send letter hints
        await SendLetterHintsAsync(round, cancellationToken);

        // round ended due to timeout
        if (round.EndReason is null)
            round.EndRound(RoundEndReason.Timeout);
    }

    public async Task SaveRoundAsync(Round round, CancellationToken cancellationToken)
    {
        await roundsRepository.CreateRoundAsync(round.ToEntity(), cancellationToken);
    }

    private async Task SendLetterHintsAsync(Round round, CancellationToken cancellationToken)
    {
        var previousIndices = new List<int>();

        // create linked cancellation token source
        using var linkedTokenSource =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, round.CancellationToken);

        try
        {
            while (previousIndices.Count < round.Word.Length && !linkedTokenSource.Token.IsCancellationRequested)
            {
                var delay = (round.EndDate - round.StartDate) / round.Word.Length;
                await Task.Delay(delay, round.CancellationToken);

                int index;
                do index = Random.Shared.Next(round.Word.Length);
                while (previousIndices.Contains(index));
                previousIndices.Add(index);

                var letterHint = round.Word.GetLetterHint(index + 1);
                round.WordHint.AddLetterHint(letterHint);
                await SendLetterHintAddedNotificationAsync(letterHint, linkedTokenSource.Token);
            }
        }
        catch (TaskCanceledException)
        {
            logger.LogInformation("Round {RoundNumber} has been terminated early. Reason: {EndReason}",
                round.Number, round.EndReason);
        }
    }

    private Task SendLetterHintAddedNotificationAsync(LetterHint letterHint, CancellationToken cancellationToken)
    {
        var notification = new LetterHintAddedNotification(letterHint);
        return publisher.Publish(notification, cancellationToken);
    }
}
