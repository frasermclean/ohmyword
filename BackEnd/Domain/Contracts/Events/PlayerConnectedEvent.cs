using FastEndpoints;
using System.Net;

namespace OhMyWord.Domain.Contracts.Events;

public record PlayerConnectedEvent(string ConnectionId, IPAddress IpAddress) : IEvent;
