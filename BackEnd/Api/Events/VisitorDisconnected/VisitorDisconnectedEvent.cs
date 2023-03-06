namespace OhMyWord.Api.Events.VisitorDisconnected;

public record VisitorDisconnectedEvent(string ConnectionId) : IEvent;
