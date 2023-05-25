using MediatR;
using OhMyWord.Domain.Models;

namespace OhMyWord.Domain.Notifications;

public record LetterHintAddedNotification(LetterHint LetterHint) : INotification;
