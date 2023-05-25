using MediatR;
using System.Net;

namespace OhMyWord.Domain.Notifications;

public record PlayerConnectedNotification(string ConnectionId, IPAddress IpAddress) : INotification;
