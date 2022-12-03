﻿using MediatR;
using OhMyWord.Core.Responses.Words;
using OhMyWord.Data.Models;

namespace OhMyWord.Core.Requests.Words;

public class GetWordRequest : IRequest<WordResponse>
{
    public PartOfSpeech? PartOfSpeech { get; init; }
    public Guid? Id { get; init; }
}