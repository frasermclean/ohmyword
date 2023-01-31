using FastEndpoints;
using OhMyWord.Core.Models;

namespace OhMyWord.Api.Events.LetterHintAdded;

public record LetterHintAddedEvent(LetterHint LetterHint) : IEvent;
