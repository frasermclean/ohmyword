using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OhMyWord.Domain.Models;
using OhMyWord.Domain.Options;

namespace OhMyWord.Domain.Services;

public interface IRoundFactory
{
    Round CreateRound(Word word, int roundNumber);
}

public class RoundFactory : IRoundFactory
{
    private readonly ILogger<RoundFactory> logger;
    private readonly IPlayerService playerService;
    private readonly RoundOptions options;

    public RoundFactory(IOptions<RoundOptions> options, ILogger<RoundFactory> logger, IPlayerService playerService)
    {
        this.options = options.Value;
        this.logger = logger;
        this.playerService = playerService;
    }

    public Round CreateRound(Word word, int roundNumber)
    {
        logger.LogInformation("Creating round {RoundNumber} with word {Word}", roundNumber, word);
        return new Round(word, roundNumber, playerService.PlayerIds, options.LetterHintDelay, options.GuessLimit);
    }
}
