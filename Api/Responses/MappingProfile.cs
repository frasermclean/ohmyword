using AutoMapper;
using OhMyWord.Api.Responses.Words;
using OhMyWord.Data.Models;

namespace OhMyWord.Api.Responses;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Word, WordResponse>();
    }
}

