using FastEndpoints;
using OhMyWord.Logic.Models;

namespace OhMyWord.Logic.Contracts.Commands;

public record ProcessGuessCommand(string ConnectionId, Guid RoundId, string Value) : ICommand<ProcessGuessResult>;
