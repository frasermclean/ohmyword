using AutoMapper;
using OhMyWord.Api.Responses.Game;
using OhMyWord.Api.Responses.Words;
using OhMyWord.Core.Models;

namespace OhMyWord.Api.Mapping;

public class ResponsesProfile : Profile
{
    public ResponsesProfile()
    {
        CreateMap<Word, WordResponse>();
        CreateMap<Player, RegisterPlayerResponse>()
            .ForMember(response => response.PlayerId, options => options.MapFrom(player => player.Id));
    }
}
