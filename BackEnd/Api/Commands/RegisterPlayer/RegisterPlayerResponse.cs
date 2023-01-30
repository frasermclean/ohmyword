using OhMyWord.Core.Responses.Game;

namespace OhMyWord.Api.Commands.RegisterPlayer;

public class RegisterPlayerResponse
{
    public bool RoundActive { get; init; }
    public int PlayerCount { get; init; }
    public long Score { get; init; }
    public RoundStartResponse? RoundStart { get; init; }
    public RoundEndResponse? RoundEnd { get; init; }
}
