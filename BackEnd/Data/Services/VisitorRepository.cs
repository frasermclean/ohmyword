﻿using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OhMyWord.Data.Entities;
using OhMyWord.Data.Options;

namespace OhMyWord.Data.Services;

public interface IVisitorRepository
{
    Task<VisitorEntity?> GetVisitorAsync(string visitorId);
    Task<VisitorEntity> CreateVisitorAsync(VisitorEntity visitorEntity);
    Task DeleteVisitorAsync(VisitorEntity visitorEntity);
    Task<VisitorEntity> IncrementRegistrationCountAsync(string visitorId);
    Task<VisitorEntity> IncrementScoreAsync(string visitorId, long value);
}

public class VisitorRepository : Repository<VisitorEntity>, IVisitorRepository
{
    public VisitorRepository(CosmosClient cosmosClient, IOptions<CosmosDbOptions> options,
        ILogger<VisitorRepository> logger)
        : base(cosmosClient, options, logger, "visitors")
    {
    }

    public Task<VisitorEntity?> GetVisitorAsync(string visitorId) => ReadItemAsync(visitorId, visitorId);

    public async Task<VisitorEntity> CreateVisitorAsync(VisitorEntity visitorEntity)
    {
        await CreateItemAsync(visitorEntity);
        return visitorEntity;
    }

    public Task DeleteVisitorAsync(VisitorEntity visitorEntity) => DeleteItemAsync(visitorEntity);

    public Task<VisitorEntity> IncrementRegistrationCountAsync(string visitorId) => PatchItemAsync(visitorId,
        visitorId, new[] { PatchOperation.Increment("/registrationCount", 1) });

    public Task<VisitorEntity> IncrementScoreAsync(string visitorId, long value) =>
        PatchItemAsync(visitorId, visitorId, new[] { PatchOperation.Increment("/score", value) });
}
