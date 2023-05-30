using FastEndpoints;
using OhMyWord.Domain.Models;

namespace OhMyWord.Domain.Contracts.Events;

public record LetterHintAddedEvent(LetterHint LetterHint) : IEvent;
