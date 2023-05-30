using Microsoft.Extensions.Logging;
using OhMyWord.Domain.Models;

namespace OhMyWord.Domain.Services.State;

public interface IRootState
{
    SessionState SessionState { get; }
    Session? Session { get; }
    Round? Round { get; }
    bool IsDefault { get; }
    DateTime IntervalStart { get; }
    DateTime IntervalEnd { get; }
    IPlayerState PlayerState { get; }

    Session NextSession();
    Task<Round> NextRoundAsync(CancellationToken cancellationToken = default);
    void Reset();
    StateSnapshot GetStateSnapshot();
}

public class RootState : IRootState
{
    private readonly ILogger<RootState> logger;
    private readonly IRoundService roundService;

    public RootState(ILogger<RootState> logger, IRoundService roundService, IPlayerState playerState)
    {
        this.logger = logger;
        this.roundService = roundService;
        PlayerState = playerState;
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
    public IPlayerState PlayerState { get; }
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

        var roundNumber = ++Session.RoundCount;
        Round = await roundService.CreateRoundAsync(roundNumber, Session.Id, cancellationToken);
        logger.LogInformation("Created round with ID: {RoundId}", Round.Id);

        return Round;
    }

    public void Reset()
    {
        Session = null;
        Round = null;
        PlayerState.Reset();
    }

    public StateSnapshot GetStateSnapshot() => new()
    {
        RoundActive = SessionState == SessionState.RoundActive,
        RoundNumber = Round?.Number ?? default,
        RoundId = Round?.Id ?? default,
        Interval = new Interval(IntervalStart, IntervalEnd),        
        WordHint = Round?.WordHint,
    };
}
