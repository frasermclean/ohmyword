using FastEndpoints;
using OhMyWord.Logic.Models;

namespace OhMyWord.Logic.Contracts.Events;

public record RoundStartedEvent(RoundStartData Data) : IEvent;
