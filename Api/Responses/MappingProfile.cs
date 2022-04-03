using AutoMapper;
using OhMyWord.Api.Responses.Words;
using OhMyWord.Core.Models;

namespace OhMyWord.Api.Responses;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Word, WordResponse>();
    }
}

