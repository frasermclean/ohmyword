using FastEndpoints;
using OhMyWord.Core.Models;

namespace OhMyWord.Logic.Contracts.Events;

public record LetterHintAddedEvent(LetterHint LetterHint) : IEvent;
