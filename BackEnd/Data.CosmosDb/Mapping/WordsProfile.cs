using AutoMapper;
using OhMyWord.Core.Models;
using OhMyWord.Data.CosmosDb.Models;

namespace OhMyWord.Data.CosmosDb.Mapping;

public class WordsProfile : Profile
{
    public WordsProfile()
    {
        CreateMap<Word, WordItem>()
            .ForMember(item => item.Timestamp, opt => opt.MapFrom<TimestampResolver>())
            .ReverseMap();
    }
}
