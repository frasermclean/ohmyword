﻿using Microsoft.Extensions.Logging;
using OhMyWord.Core.Events;
using OhMyWord.Core.Extensions;
using OhMyWord.Core.Models;
using OhMyWord.Data.Entities;
using OhMyWord.Data.Services;
using System.Collections.Concurrent;

namespace OhMyWord.Core.Services;

public interface IVisitorService
{
    int VisitorCount { get; }

    /// <summary>
    /// Currently connected visitor IDs.
    /// </summary>
    IEnumerable<string> VisitorIds { get; }

    event EventHandler<VisitorEventArgs> VisitorAdded;
    event EventHandler<VisitorEventArgs> VisitorRemoved;

    Task<Visitor> AddVisitorAsync(string visitorId, string connectionId);
    void RemoveVisitor(string connectionId);
    Visitor GetVisitor(string connectionId);
    Task IncrementVisitorScoreAsync(string visitorId, int points);
}

public class VisitorService : IVisitorService
{
    private readonly ILogger<VisitorService> logger;
    private readonly IVisitorRepository visitorRepository;
    private readonly ConcurrentDictionary<string, Visitor> visitors = new();

    public int VisitorCount => visitors.Count;
    public IEnumerable<string> VisitorIds => visitors.Values.Select(visitor => visitor.Id);

    public event EventHandler<VisitorEventArgs>? VisitorAdded;
    public event EventHandler<VisitorEventArgs>? VisitorRemoved;

    public VisitorService(ILogger<VisitorService> logger, IVisitorRepository visitorRepository)
    {
        this.logger = logger;
        this.visitorRepository = visitorRepository;
    }

    public async Task<Visitor> AddVisitorAsync(string visitorId, string connectionId)
    {
        var visitor = (await visitorRepository.GetVisitorAsync(visitorId))?.ToVisitor();
        if (visitor is not null)
        {
            // TODO: Handle multiple connections with same visitor ID
            await visitorRepository.IncrementRegistrationCountAsync(visitor.Id);
            logger.LogDebug("Found existing visitor with ID: {VisitorId}", visitorId);
        }

        // create new visitor if existing visitor not found
        visitor ??= (await visitorRepository.CreateVisitorAsync(new VisitorEntity { Id = visitorId, })).ToVisitor();

        var wasAdded = visitors.TryAdd(connectionId, visitor);
        if (!wasAdded)
            logger.LogWarning("Visitor with connection ID: {ConnectionId} already exists in the local cache",
                connectionId);

        VisitorAdded?.Invoke(this, new VisitorEventArgs(visitor.Id, VisitorCount, connectionId));

        logger.LogInformation("Visitor with ID: {VisitorId} joined the game. Visitor count: {VisitorCount}", visitor.Id,
            VisitorCount);

        return visitor;
    }

    public void RemoveVisitor(string connectionId)
    {
        if (visitors.TryRemove(connectionId, out var visitor))
        {
            VisitorRemoved?.Invoke(this, new VisitorEventArgs(visitor.Id, VisitorCount, connectionId));
            logger.LogInformation("Visitor with ID: {VisitorId} left the game. Visitor count: {VisitorCount}",
                visitor.Id,
                VisitorCount);
        }
        else
        {
            logger.LogError("Couldn't remove visitor with connection ID: {ConnectionId} from cache", connectionId);
        }
    }

    public Visitor GetVisitor(string connectionId) => visitors[connectionId];

    public Task IncrementVisitorScoreAsync(string visitorId, int points) =>
        visitorRepository.IncrementScoreAsync(visitorId, points); // TODO: Update local cache to keep points in sync
}