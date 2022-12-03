using AutoMapper;
using OhMyWord.Core.Responses.Words;
using OhMyWord.Data.Models;

namespace OhMyWord.Core.Mapping;

public class EntitiesProfile : Profile
{
    public EntitiesProfile()
    {
        CreateMap<Word, WordResponse>();
    }
}