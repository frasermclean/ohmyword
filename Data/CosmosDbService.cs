﻿using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;

namespace WhatTheWord.Data;

public interface ICosmosDbService
{
    Task<Container> GetContainerAsync(string containerId, string partitionKeyPath);
}

public class CosmosDbService : ICosmosDbService
{
    private readonly ILogger<CosmosDbService> logger;
    private readonly CosmosClient cosmosClient;
    private readonly Task<Database> databaseTask;

    public CosmosDbService(IOptions<CosmosDbOptions> options, ILogger<CosmosDbService> logger)
    {
        this.logger = logger;
        logger.LogInformation("Creating Cosmos DB client.");

        cosmosClient = new CosmosClient(options.Value.Endpoint, options.Value.PrimaryKey, new CosmosClientOptions()
        {
            SerializerOptions = new CosmosSerializationOptions()
            {
                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase,
            }
        });

        databaseTask = CreateDatabaseAsync(options.Value.DatabaseId);
    }

    private async Task<Database> CreateDatabaseAsync(string databaseId)
    {
        var response = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);

        switch (response.StatusCode)
        {
            case HttpStatusCode.Created:
                logger.LogInformation("Created new databasee with ID: {databaseId}", databaseId);
                break;
            case HttpStatusCode.OK:
                logger.LogInformation("Connected to existing database with ID: {databaseId}", databaseId);
                break;
        }

        return response.Database;
    }

    public async Task<Container> GetContainerAsync(string containerId, string partitionKeyPath)
    {
        var database = await databaseTask;

        var response = await database.CreateContainerIfNotExistsAsync(containerId, partitionKeyPath);

        switch (response.StatusCode)
        {
            case HttpStatusCode.Created:
                logger.LogInformation("Created new container with ID: {containerId} and partition key path: {partitionKeyPath}", containerId, partitionKeyPath);
                break;
            case HttpStatusCode.OK:
                logger.LogInformation("Connected to existing container with ID: {containerId}", containerId);
                break;
        }

        return response.Container;
    }
}
