using MediatR;

namespace OhMyWord.Domain.Contracts.Notifications;

public record PlayerDisconnectedNotification(string ConnectionId) : INotification;
