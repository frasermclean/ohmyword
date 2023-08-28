using System.Net;

namespace OhMyWord.Logic.Contracts.Parameters;

public record struct GetPlayerParameters(Guid PlayerId, string VisitorId, string ConnectionId,
    IPAddress IpAddress, Guid? UserId = default);
