using AutoMapper;
using FluentResults;
using OhMyWord.Core.Models;
using OhMyWord.Core.Services;
using OhMyWord.Data.CosmosDb.Models;

namespace OhMyWord.Data.CosmosDb.Services;

public class GamesRepository : IGamesRepository
{
    private readonly IMapper mapper;
    private readonly IContainerManager containerManager;

    public GamesRepository(IMapper mapper, ContainerManagerFactory containerManagerFactory)
    {
        this.mapper = mapper;
        containerManager = containerManagerFactory.Create(ContainerIds.Games);
    }

    public async Task<Result<Session>> CreateSessionAsync(Session session,
        CancellationToken cancellationToken = default)
    {
        var sessionItem = mapper.Map<SessionItem>(session);
        var result = await containerManager.CreateItemAsync(sessionItem, cancellationToken);
        return result.IsSuccess
            ? mapper.Map<Session>(result.Value)
            : result.ToResult();
    }

    public async Task<Result<Round>> CreateRoundAsync(Round item, CancellationToken cancellationToken = default)
    {
        var roundItem = mapper.Map<RoundItem>(item);
        var result = await containerManager.CreateItemAsync(roundItem, cancellationToken);
        return result.IsSuccess
            ? mapper.Map<Round>(result.Value)
            : result.ToResult();
    }
}
