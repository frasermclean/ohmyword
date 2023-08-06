using OhMyWord.Domain.Models;
using OhMyWord.Domain.Services;
using OhMyWord.Infrastructure.Errors;

namespace OhMyWord.Api.Endpoints.Words.Update;

public class UpdateWordEndpoint : Endpoint<UpdateWordRequest, Word>
{
    private readonly IWordsService wordsService;

    public UpdateWordEndpoint(IWordsService wordsService)
    {
        this.wordsService = wordsService;
    }

    public override void Configure()
    {
        Put("words/{wordId}");
    }

    public override async Task HandleAsync(UpdateWordRequest request, CancellationToken cancellationToken)
    {
        var result = await wordsService.UpdateWordAsync(
            new Word
            {
                Id = request.WordId,
                Definitions = request.Definitions,
                Frequency = request.Frequency,
                LastModifiedTime = DateTime.UtcNow
            },
            cancellationToken);

        if (result.HasError<ItemNotFoundError>())
        {
            await SendNotFoundAsync(cancellationToken);
            return;
        }

        await SendOkAsync(result.Value, cancellationToken);
    }
}
