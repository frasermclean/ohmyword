namespace OhMyWord.Api.Responses.Game;

public class RegisterPlayerResponse
{
    public bool RoundActive { get; init; }
    public int PlayerCount { get; init; }
    public long Score { get; init; }
    public RoundStartResponse? RoundStart { get; init; }
    public RoundEndResponse? RoundEnd { get; init; }
}
