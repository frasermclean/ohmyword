using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OhMyWord.Domain.Models;
using OhMyWord.Domain.Options;


namespace OhMyWord.Domain.Services;

public interface ISessionManager
{
    /// <summary>
    /// Starts and executes new session.
    /// </summary>
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}

public sealed class SessionManager : ISessionManager
{
    private readonly ILogger<SessionManager> logger;
    private readonly ISender sender;
    private readonly IServiceProvider serviceProvider;
    private readonly IWordsService wordsService;
    private readonly IPlayerService playerService;
    private readonly RoundOptions options;

    private Session session = Session.Default;
    private int roundCount;

    public SessionManager(ILogger<SessionManager> logger, ISender sender, IServiceProvider serviceProvider,
        IWordsService wordsService, IPlayerService playerService, IOptions<RoundOptions> options)
    {
        this.logger = logger;
        this.sender = sender;
        this.serviceProvider = serviceProvider;
        this.wordsService = wordsService;
        this.playerService = playerService;
        this.options = options.Value;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        // create new session
        session = new Session();
        logger.LogInformation("Starting new session {SessionId}", session.Id);

        while (!cancellationToken.IsCancellationRequested)
        {
            // create new round
            var word = await wordsService.GetRandomWordAsync(cancellationToken);
            var round = new Round(word, options.LetterHintDelay, playerService.PlayerIds)
            {
                Number = ++roundCount, GuessLimit = options.GuessLimit, SessionId = session.Id
            };

            // execute round
            await using var scope = serviceProvider.CreateAsyncScope();
            var roundManager = scope.ServiceProvider.GetRequiredService<IRoundManager>();
            await roundManager.ExecuteAsync(round, cancellationToken);            
        }
    }
}
