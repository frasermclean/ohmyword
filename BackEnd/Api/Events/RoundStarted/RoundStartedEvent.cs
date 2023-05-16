using OhMyWord.Domain.Models;

namespace OhMyWord.Api.Events.RoundStarted;

public record RoundStartedEvent(RoundStartData Data) : IEvent;
