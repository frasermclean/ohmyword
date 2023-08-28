using FastEndpoints;
using System.Net;

namespace OhMyWord.Logic.Contracts.Events;

public record PlayerConnectedEvent(string ConnectionId, IPAddress IpAddress) : IEvent;
