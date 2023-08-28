using FastEndpoints;

namespace OhMyWord.Logic.Contracts.Events;

public record PlayerDisconnectedEvent(string ConnectionId) : IEvent;
