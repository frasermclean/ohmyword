using FastEndpoints;

namespace OhMyWord.Api.Commands.RemoveVisitor;

public class RemoveVisitorCommand : ICommand<RemoveVisitorResponse>
{
    public string ConnectionId { get; init; } = string.Empty;
}
