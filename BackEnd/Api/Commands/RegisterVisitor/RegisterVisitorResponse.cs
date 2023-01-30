using OhMyWord.Core.Responses.Game;

namespace OhMyWord.Api.Commands.RegisterVisitor;

public class RegisterVisitorResponse
{
    public bool RoundActive { get; init; }
    public int VisitorCount { get; init; }
    public long Score { get; init; }
    public RoundStartResponse? RoundStart { get; init; }
    public RoundEndResponse? RoundEnd { get; init; }
}
