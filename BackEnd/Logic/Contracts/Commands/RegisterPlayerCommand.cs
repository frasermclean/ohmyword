using FastEndpoints;
using OhMyWord.Logic.Models;
using System.Net;

namespace OhMyWord.Logic.Contracts.Commands;

public record RegisterPlayerCommand(string ConnectionId, Guid PlayerId, string VisitorId, IPAddress IpAddress,
    Guid? UserId) : ICommand<RegisterPlayerResult>;
