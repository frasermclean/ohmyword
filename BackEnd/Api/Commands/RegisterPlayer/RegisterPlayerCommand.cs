using System.Net;

namespace OhMyWord.Api.Commands.RegisterPlayer;

public class RegisterPlayerCommand : ICommand<RegisterPlayerResponse>
{
    public required string VisitorId { get; init; }
    public required string ConnectionId { get; init; }
    public required Guid? UserId { get; init; }
    public required IPAddress IpAddress { get; init; }
}
