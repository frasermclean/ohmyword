// using Microsoft.Extensions.Logging;
// using OhMyWord.Core.Models;
// using OhMyWord.Core.Services;
// using OhMyWord.Data.CosmosDb.Services;
// using OhMyWord.Data.CosmosDb.Tests.Fixtures;
//
// namespace OhMyWord.Data.CosmosDb.Tests.Services;
//
// [Collection("CosmosDbEmulator")]
// [Trait("Category", "Integration")]
// public class GamesRepositoryTests : IClassFixture<AutoMapperFixture>
// {
//     private readonly IGamesRepository gamesRepository;
//
//     public GamesRepositoryTests(CosmosDbEmulatorFixture cosmosDbEmulatorFixture, AutoMapperFixture autoMapperFixture)
//     {
//         gamesRepository = new GamesRepository(cosmosDbEmulatorFixture.CosmosClient, cosmosDbEmulatorFixture.Options,
//             Mock.Of<ILogger<GamesRepository>>(), autoMapperFixture.Mapper);
//     }
//
//     [Theory, AutoData]
//     public async Task Test(Session session)
//     {
//         // act
//         await gamesRepository.CreateSessionAsync(session);
//     }
// }


