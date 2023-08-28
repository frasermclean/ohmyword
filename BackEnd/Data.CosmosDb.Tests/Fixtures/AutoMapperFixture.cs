using AutoMapper;
using OhMyWord.Data.CosmosDb.Mapping;

namespace OhMyWord.Data.CosmosDb.Tests.Fixtures;

public sealed class AutoMapperFixture
{
    public AutoMapperFixture()
    {
        Configuration = new MapperConfiguration(configuration =>
        {
            configuration.AddProfile<GamesProfile>();
            configuration.AddProfile<WordsProfile>();
        });
        Mapper = Configuration.CreateMapper();
    }

    public MapperConfiguration Configuration { get; }

    public IMapper Mapper { get; }
}
