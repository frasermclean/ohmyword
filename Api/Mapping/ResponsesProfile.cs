using AutoMapper;
using OhMyWord.Api.Responses.Game;
using OhMyWord.Api.Responses.Words;
using OhMyWord.Core.Models;
using OhMyWord.Services.Events;

namespace OhMyWord.Api.Mapping;

public class ResponsesProfile : Profile
{
    public ResponsesProfile()
    {
        CreateMap<Word, WordResponse>();
        CreateMap<RoundStartedEventArgs, RoundStartResponse>();
        CreateMap<RoundEndedEventArgs, RoundEndResponse>()
            .ForMember(response => response.RoundNumber, options => options.MapFrom(args => args.Round.Number))
            .ForMember(response => response.RoundId, options => options.MapFrom(args => args.Round.Id))
            .ForMember(response => response.Word, options => options.MapFrom(args => args.Round.Word.Value))
            .ForMember(response => response.EndReason, options => options.MapFrom(args => args.Round.EndReason))
            .ForMember(response => response.NextRoundStart, options => options.MapFrom(args => args.NextRoundStart));
    }
}
