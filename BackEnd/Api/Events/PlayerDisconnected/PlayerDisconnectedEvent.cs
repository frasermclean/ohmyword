namespace OhMyWord.Api.Events.PlayerDisconnected;

public record PlayerDisconnectedEvent(string ConnectionId) : IEvent;
