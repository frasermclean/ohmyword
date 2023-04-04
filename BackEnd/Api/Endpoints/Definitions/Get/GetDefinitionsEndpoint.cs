using OhMyWord.Domain.Models;
using OhMyWord.Domain.Services;

namespace OhMyWord.Api.Endpoints.Definitions.Get;

public class GetDefinitionsEndpoint : Endpoint<GetDefinitionsRequest, IEnumerable<Definition>>
{
    private readonly IDefinitionsService definitionsService;

    public GetDefinitionsEndpoint(IDefinitionsService definitionsService)
    {
        this.definitionsService = definitionsService;
    }

    public override void Configure()
    {
        Get("definitions/{wordId}");
    }

    public override async Task HandleAsync(GetDefinitionsRequest request, CancellationToken cancellationToken)
    {
        var definitions = await definitionsService.GenerateDefinitionsAsync(request.WordId, request.PartOfSpeech,
            cancellationToken);

        await SendOkAsync(definitions, cancellationToken);
    }
}
