using OhMyWord.Domain.Models;

namespace OhMyWord.Api.Events.LetterHintAdded;

public record LetterHintAddedEvent(LetterHint LetterHint) : IEvent;
