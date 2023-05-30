using FastEndpoints;
using OhMyWord.Domain.Contracts.Results;

namespace OhMyWord.Domain.Contracts.Commands;

public record ProcessGuessCommand(string ConnectionId, Guid RoundId, string Value) : ICommand<ProcessGuessResult>;
