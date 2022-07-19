using FluentValidation;
using MediatR;
using OhMyWord.Core.Requests.Game;
using OhMyWord.Core.Responses.Game;
using OhMyWord.Core.Services;

namespace OhMyWord.Core.Handlers.Game;

public class SubmitGuessHandler : IRequestHandler<SubmitGuessRequest, SubmitGuessResponse>
{
    private readonly IGameService gameService;
    private readonly IValidator<SubmitGuessRequest> validator;

    public SubmitGuessHandler(IGameService gameService, IValidator<SubmitGuessRequest> validator)
    {
        this.gameService = gameService;
        this.validator = validator;
    }
    
    public async Task<SubmitGuessResponse> Handle(SubmitGuessRequest request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);        
        var points = await gameService.ProcessGuessAsync(request.ConnectionId, request.RoundId, request.Value);        
        return new SubmitGuessResponse
        {
            Value = request.Value.ToLowerInvariant(),
            Correct = points > 0,
            Points = points
        };
    }
}