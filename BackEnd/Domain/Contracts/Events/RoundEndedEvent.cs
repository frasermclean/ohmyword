using FastEndpoints;
using OhMyWord.Core.Models;

namespace OhMyWord.Domain.Contracts.Events;

public record RoundEndedEvent(RoundSummary Summary) : IEvent;
