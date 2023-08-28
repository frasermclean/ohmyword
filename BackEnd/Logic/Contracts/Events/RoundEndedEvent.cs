using FastEndpoints;
using OhMyWord.Logic.Models;

namespace OhMyWord.Logic.Contracts.Events;

public record RoundEndedEvent(RoundSummary Summary) : IEvent;
