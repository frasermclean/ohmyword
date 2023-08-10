using FastEndpoints;
using OhMyWord.Domain.Models;

namespace OhMyWord.Domain.Contracts.Events;

public record RoundEndedEvent(RoundSummary Summary) : IEvent;
