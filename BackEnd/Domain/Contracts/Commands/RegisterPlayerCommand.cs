﻿using FastEndpoints;
using OhMyWord.Domain.Models;
using System.Net;

namespace OhMyWord.Domain.Contracts.Commands;

public record RegisterPlayerCommand(string ConnectionId, Guid PlayerId, string VisitorId, IPAddress IpAddress,
    Guid? UserId) : ICommand<RegisterPlayerResult>;
