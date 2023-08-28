using AutoMapper;
using OhMyWord.Core.Models;
using OhMyWord.Data.CosmosDb.Mapping;
using OhMyWord.Data.CosmosDb.Models;

namespace OhMyWord.Data.CosmosDb.Tests.Mapping;

public class GamesProfileTests
{
    private readonly MapperConfiguration configuration;
    private readonly IMapper mapper;

    public GamesProfileTests()
    {
        configuration = new MapperConfiguration(config => config.AddProfile<GamesProfile>());
        mapper = configuration.CreateMapper();
    }

    [Fact(DisplayName = "Mapper configuration should be valid")]
    public void MapperConfiguration_Should_BeValid()
    {
        configuration.AssertConfigurationIsValid();
    }

    [Theory(DisplayName = "Mapping from Session to SessionItem should pass"), AutoData]
    public void MappingFromSessionToSessionItem_Should_BeValid(Session session)
    {
        // act
        var sessionItem = mapper.Map<SessionItem>(session);

        // assert
        sessionItem.Id.Should().Be(session.Id);
        sessionItem.StartDate.Should().Be(session.StartDate);
        sessionItem.EndDate.Should().Be(session.EndDate);
        sessionItem.RoundCount.Should().Be(session.RoundCount);
        sessionItem.LastModifiedTime.Should().BeCloseTo(session.LastModifiedTime, TimeSpan.FromHours(12));
        sessionItem.Category.Should().Be(GamesItemCategory.Session);
    }

    [Theory(DisplayName = "Mapping from SessionItem to Session should pass"), AutoData]
    public void MappingFromSessionItemToSession_Should_BeValid(SessionItem sessionItem)
    {
        // act
        var session = mapper.Map<Session>(sessionItem);

        // assert
        session.Id.Should().Be(sessionItem.Id);
        session.StartDate.Should().Be(sessionItem.StartDate);
        session.EndDate.Should().Be(sessionItem.EndDate);
        session.RoundCount.Should().Be(sessionItem.RoundCount);
        session.LastModifiedTime.Should().BeCloseTo(sessionItem.LastModifiedTime, TimeSpan.FromHours(12));
    }
}
