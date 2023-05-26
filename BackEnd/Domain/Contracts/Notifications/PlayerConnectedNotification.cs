using MediatR;
using System.Net;

namespace OhMyWord.Domain.Contracts.Notifications;

public record PlayerConnectedNotification(string ConnectionId, IPAddress IpAddress) : INotification;
