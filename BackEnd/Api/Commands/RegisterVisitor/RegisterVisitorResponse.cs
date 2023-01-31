using OhMyWord.Core.Models;

namespace OhMyWord.Api.Commands.RegisterVisitor;

public class RegisterVisitorResponse
{
    public required int VisitorCount { get; init; }
    public required long Score { get; init; }
    public required GameState GameState { get; init; }
}
