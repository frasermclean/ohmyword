using Microsoft.Extensions.Logging;
using OhMyWord.Core.Models;

namespace OhMyWord.Domain.Services.State;

public interface ISessionState : IState
{
    Guid SessionId { get; }
    int RoundCount { get; }
    Session NextSession();
    int IncrementRoundCount();
}

public class SessionState : ISessionState
{
    private readonly ILogger<SessionState> logger;

    private Session session = Session.Default;

    public SessionState(ILogger<SessionState> logger)
    {
        this.logger = logger;
    }

    public bool IsDefault => session == Session.Default;
    public Guid SessionId => session.Id;
    public int RoundCount => session.RoundCount;

    public Session NextSession()
    {
        session = new Session();
        logger.LogInformation("Created session with ID: {SessionId}", session.Id);

        return session;
    }

    public int IncrementRoundCount()
    {
        if (IsDefault)
            NextSession();

        return ++session.RoundCount;
    }

    public void Reset()
    {
        session = Session.Default;
    }
}
