namespace OhMyWord.Api.Events.VisitorDisconnected;

public record PlayerDisconnectedEvent(string ConnectionId) : IEvent;
