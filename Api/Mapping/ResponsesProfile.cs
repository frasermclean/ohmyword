using AutoMapper;
using OhMyWord.Api.Responses.Words;
using OhMyWord.Core.Models;

namespace OhMyWord.Api.Mapping;

public class ResponsesProfile : Profile
{
    public ResponsesProfile()
    {
        CreateMap<Word, WordResponse>();
    }
}
