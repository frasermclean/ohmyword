using AutoMapper;
using OhMyWord.Core.Events;
using OhMyWord.Core.Responses.Game;
using OhMyWord.Core.Responses.Words;
using OhMyWord.Data.Models;

namespace OhMyWord.Api.Mapping;

public class ResponsesProfile : Profile
{
    public ResponsesProfile()
    {
        CreateMap<Word, WordResponse>();

        CreateMap<RoundStartedEventArgs, RoundStartResponse>()
            .ForMember(response => response.RoundNumber, options => options.MapFrom(args => args.Round.Number))
            .ForMember(response => response.RoundId, options => options.MapFrom(args => args.Round.Id))
            .ForMember(response => response.RoundStarted, options => options.MapFrom(args => args.Round.StartTime))
            .ForMember(response => response.RoundEnds, options => options.MapFrom(args => args.Round.EndTime))
            .ForMember(response => response.WordHint, options => options.MapFrom(args => args.Round.WordHint));

        CreateMap<RoundEndedEventArgs, RoundEndResponse>()
            .ForMember(response => response.RoundNumber, options => options.MapFrom(args => args.Round.Number))
            .ForMember(response => response.RoundId, options => options.MapFrom(args => args.Round.Id))
            .ForMember(response => response.Word, options => options.MapFrom(args => args.Round.Word.Value))
            .ForMember(response => response.EndReason, options => options.MapFrom(args => args.Round.EndReason))
            .ForMember(response => response.NextRoundStart, options => options.MapFrom(args => args.NextRoundStart));
    }
}
