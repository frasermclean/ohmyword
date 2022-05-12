using OhMyWord.Core.Models;

namespace OhMyWord.Api.Responses.Game;

public class RegisterPlayerResponse
{
    public string PlayerId { get; init; } = string.Empty;
    public bool RoundActive { get; init; }
    public int RoundNumber { get; init; }
    public string RoundId { get; init; }= string.Empty;
    public WordHint? WordHint { get; init; }
    public int PlayerCount { get; init; }
    public DateTime Expiry { get; init; }
    public long Score { get; init; }
}
