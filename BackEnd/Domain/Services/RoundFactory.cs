using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OhMyWord.Domain.Models;
using OhMyWord.Domain.Options;

namespace OhMyWord.Domain.Services;

public interface IRoundFactory
{
    /// <summary>
    /// Create a new round with the given word and round number.
    /// </summary>
    /// <param name="word">The <see cref="Word"/> to use for the round.</param>
    /// <param name="roundNumber">The round number to assign.</param>
    /// <returns>A new <see cref="Round"/> object.</returns>
    Round CreateRound(Word word, int roundNumber);

    /// <summary>
    /// Create a new round with the given round number and automatically select a random word.
    /// </summary>
    /// <param name="roundNumber">The round number to assign.</param>
    /// <param name="cancellationToken">Task cancellation token</param>
    /// <returns>A new <see cref="Round"/> object.</returns>
    Task<Round> CreateRoundAsync(int roundNumber, CancellationToken cancellationToken = default);
}

public class RoundFactory : IRoundFactory
{
    private readonly ILogger<RoundFactory> logger;
    private readonly IPlayerService playerService;
    private readonly IWordsService wordsService;
    private readonly RoundOptions options;

    public RoundFactory(IOptions<RoundOptions> options, ILogger<RoundFactory> logger, IPlayerService playerService,
        IWordsService wordsService)
    {
        this.options = options.Value;
        this.logger = logger;
        this.playerService = playerService;
        this.wordsService = wordsService;
    }

    public Round CreateRound(Word word, int roundNumber)
    {
        logger.LogInformation("Creating round {RoundNumber} with word {Word}", roundNumber, word);

        return new Round(word, options, playerService.PlayerIds)
        {
            Id = Guid.NewGuid(), Number = roundNumber, SessionId = Guid.NewGuid(),
        };
    }

    public async Task<Round> CreateRoundAsync(int roundNumber, CancellationToken cancellationToken = default)
    {
        var word = await wordsService.GetRandomWordAsync(cancellationToken);
        return CreateRound(word, roundNumber);
    }
}
