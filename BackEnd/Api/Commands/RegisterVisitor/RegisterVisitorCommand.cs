namespace OhMyWord.Api.Commands.RegisterVisitor;

public class RegisterVisitorCommand : ICommand<RegisterVisitorResponse>
{
    public string VisitorId { get; init; } = string.Empty;
    public string ConnectionId { get; init; } = string.Empty;
}
