using MediatR;

namespace WhatTheWord.Domain.Processing.Words;

public class GetAllWordsRequest : IRequest<GetAllWordsResponse>
{
}
