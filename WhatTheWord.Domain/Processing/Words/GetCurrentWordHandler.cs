using MediatR;

namespace WhatTheWord.Domain.Processing.Words;
public class GetCurrentWordHandler : IRequestHandler<GetCurrentWordRequest, GetCurrentWordResponse>
{
    public Task<GetCurrentWordResponse> Handle(GetCurrentWordRequest request, CancellationToken cancellationToken)
    {
        var response = new GetCurrentWordResponse();
        return Task.FromResult(response);
    }
}
