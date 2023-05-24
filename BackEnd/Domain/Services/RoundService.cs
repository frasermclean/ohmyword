using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OhMyWord.Domain.Extensions;
using OhMyWord.Domain.Models;
using OhMyWord.Domain.Options;
using OhMyWord.Infrastructure.Services;

namespace OhMyWord.Domain.Services;

public interface IRoundService
{
    /// <summary>
    /// Create a new round with the given round number and automatically select a random word.
    /// </summary>
    /// <param name="roundNumber">The round number to assign.</param>
    /// <param name="sessionId">The game session ID</param>
    /// <param name="cancellationToken">Task cancellation token</param>
    /// <returns>A new <see cref="Round"/> object.</returns>
    Task<Round> CreateRoundAsync(int roundNumber, Guid sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Save the given round to the database.
    /// </summary>
    /// <param name="round">The round to save.</param>
    /// <param name="cancellationToken">Task cancellation token</param>
    Task SaveRoundAsync(Round round, CancellationToken cancellationToken = default);
}

public class RoundService : IRoundService
{
    private readonly ILogger<RoundService> logger;
    private readonly IPlayerService playerService;
    private readonly IWordsService wordsService;
    private readonly IRoundsRepository roundsRepository;
    private readonly RoundOptions options;

    public RoundService(IOptions<RoundOptions> options, ILogger<RoundService> logger, IPlayerService playerService,
        IWordsService wordsService, IRoundsRepository roundsRepository)
    {
        this.options = options.Value;
        this.logger = logger;
        this.playerService = playerService;
        this.wordsService = wordsService;
        this.roundsRepository = roundsRepository;
    }

    public async Task<Round> CreateRoundAsync(int roundNumber, Guid sessionId, CancellationToken cancellationToken)
    {
        var word = await wordsService.GetRandomWordAsync(cancellationToken);
        logger.LogInformation("Creating round {RoundNumber} with word {Word}", roundNumber, word);

        return new Round(word, options.LetterHintDelay, playerService.PlayerIds)
        {
            Number = roundNumber,
            GuessLimit = options.GuessLimit,
            SessionId = sessionId,
        };
    }

    public async Task SaveRoundAsync(Round round, CancellationToken cancellationToken)
    {
        await roundsRepository.CreateRoundAsync(round.ToEntity(), cancellationToken);
    }
}
