using AutoMapper;
using OhMyWord.Core.Models;
using OhMyWord.Data.CosmosDb.Models;

namespace OhMyWord.Data.CosmosDb.Mapping;

public class GamesProfile : Profile
{
    public GamesProfile()
    {
        CreateMap<Session, SessionItem>()
            .ForMember(item => item.Timestamp, opt => opt.MapFrom<TimestampResolver>())
            .ReverseMap();
    }
}
