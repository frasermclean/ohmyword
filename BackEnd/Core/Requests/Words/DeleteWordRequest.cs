using MediatR;
using OhMyWord.Data.Models;

namespace OhMyWord.Core.Requests.Words;

public class DeleteWordRequest : IRequest
{
    public PartOfSpeech? PartOfSpeech { get; init; }
    public Guid? Id { get; init; }
}