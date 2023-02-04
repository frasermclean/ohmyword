using FastEndpoints;
using OhMyWord.Api.Models;

namespace OhMyWord.Api.Events.LetterHintAdded;

public record LetterHintAddedEvent(LetterHint LetterHint) : IEvent;
