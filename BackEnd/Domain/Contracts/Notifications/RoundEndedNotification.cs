using MediatR;
using OhMyWord.Domain.Models;

namespace OhMyWord.Domain.Contracts.Notifications;

public record RoundEndedNotification(RoundSummary Summary) : INotification;
