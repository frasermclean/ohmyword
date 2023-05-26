using MediatR;
using OhMyWord.Domain.Contracts.Results;
using System.Net;

namespace OhMyWord.Domain.Contracts.Requests;

public record RegisterPlayerRequest(string ConnectionId, Guid PlayerId, string VisitorId, IPAddress IpAddress,
    Guid? UserId) : IRequest<RegisterPlayerResult>;
