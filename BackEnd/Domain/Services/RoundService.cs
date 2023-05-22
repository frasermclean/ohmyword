using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OhMyWord.Domain.Models;
using OhMyWord.Domain.Options;

namespace OhMyWord.Domain.Services;

public interface IRoundService
{
    /// <summary>
    /// Create a new round with the given round number and automatically select a random word.
    /// </summary>
    /// <param name="roundNumber">The round number to assign.</param>
    /// <param name="cancellationToken">Task cancellation token</param>
    /// <returns>A new <see cref="Round"/> object.</returns>
    Task<Round> CreateRoundAsync(int roundNumber, CancellationToken cancellationToken = default);
}

public class RoundService : IRoundService
{
    private readonly ILogger<RoundService> logger;
    private readonly IPlayerService playerService;
    private readonly IWordsService wordsService;
    private readonly RoundOptions options;

    public RoundService(IOptions<RoundOptions> options, ILogger<RoundService> logger, IPlayerService playerService,
        IWordsService wordsService)
    {
        this.options = options.Value;
        this.logger = logger;
        this.playerService = playerService;
        this.wordsService = wordsService;
    }

    public async Task<Round> CreateRoundAsync(int roundNumber, CancellationToken cancellationToken)
    {
        var word = await wordsService.GetRandomWordAsync(cancellationToken);
        logger.LogInformation("Creating round {RoundNumber} with word {Word}", roundNumber, word);

        return new Round(word, options, playerService.PlayerIds)
        {
            Id = Guid.NewGuid(), Number = roundNumber, SessionId = Guid.NewGuid(),
        };
    }
}
