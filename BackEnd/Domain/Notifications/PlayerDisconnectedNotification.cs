using MediatR;

namespace OhMyWord.Domain.Notifications;

public record PlayerDisconnectedNotification(string ConnectionId) : INotification;
