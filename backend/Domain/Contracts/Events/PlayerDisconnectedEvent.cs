using FastEndpoints;

namespace OhMyWord.Domain.Contracts.Events;

public record PlayerDisconnectedEvent(string ConnectionId) : IEvent;
