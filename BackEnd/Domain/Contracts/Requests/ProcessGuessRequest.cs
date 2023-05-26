using MediatR;
using OhMyWord.Domain.Contracts.Results;

namespace OhMyWord.Domain.Contracts.Requests;

public record ProcessGuessRequest(string ConnectionId, Guid RoundId, string Value) : IRequest<ProcessGuessResult>;
