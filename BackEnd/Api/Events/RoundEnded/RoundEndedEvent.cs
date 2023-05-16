using OhMyWord.Domain.Models;

namespace OhMyWord.Api.Events.RoundEnded;

public record RoundEndedEvent(RoundEndData Data) : IEvent;
