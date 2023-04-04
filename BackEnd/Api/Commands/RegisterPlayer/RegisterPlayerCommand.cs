namespace OhMyWord.Api.Commands.RegisterPlayer;

public class RegisterPlayerCommand : ICommand<RegisterPlayerResponse>
{
    public string VisitorId { get; init; } = string.Empty;
    public string ConnectionId { get; init; } = string.Empty;
}
