using MediatR;

namespace OhMyWord.Domain.Models.Notifications;

public record LetterHintAddedNotification(LetterHint LetterHint) : INotification;
