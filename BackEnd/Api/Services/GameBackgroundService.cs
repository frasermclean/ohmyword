using OhMyWord.Core.Models;
using OhMyWord.Domain.Services;
using OhMyWord.Domain.Services.State;

namespace OhMyWord.Api.Services;

public class GameBackgroundService : BackgroundService
{
    private readonly IRootState rootState;
    private readonly ISessionService sessionService;

    public GameBackgroundService(IRootState rootState, ISessionService sessionService)
    {
        this.rootState = rootState;
        this.sessionService = sessionService;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            // wait for players to join
            while (rootState.PlayerState.PlayerCount == 0)
            {
                await Task.Delay(1000, cancellationToken);
            }

            // start a new session
            Session session;
            using (session = rootState.SessionState.NextSession())
            {
                await sessionService.ExecuteSessionAsync(session, cancellationToken);
            }

            // save the session to the database
            await sessionService.SaveSessionAsync(session, cancellationToken);

            // reset the state manager
            rootState.Reset();
        }
    }
}
