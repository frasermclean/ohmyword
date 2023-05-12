using System.Net;

namespace OhMyWord.Api.Events.PlayerConnected;

public record PlayerConnectedEvent(string ConnectionId, IPAddress IpAddress) : IEvent;
