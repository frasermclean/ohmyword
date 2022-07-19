using AutoMapper;
using OhMyWord.Core.Events;
using OhMyWord.Core.Responses.Game;

namespace OhMyWord.Core.Mapping;

public class EventsProfile : Profile
{
    public EventsProfile()
    {
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
