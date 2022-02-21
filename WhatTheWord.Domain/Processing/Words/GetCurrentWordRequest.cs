using MediatR;

namespace WhatTheWord.Domain.Processing.Words;

public class GetCurrentWordRequest : IRequest<GetCurrentWordResponse>
{
}
