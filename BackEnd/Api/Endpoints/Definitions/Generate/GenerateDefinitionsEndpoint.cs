using OhMyWord.Api.Endpoints.Definitions.Get;
using OhMyWord.Domain.Models;
using OhMyWord.Domain.Services;

namespace OhMyWord.Api.Endpoints.Definitions.Generate;

public class GenerateDefinitionsEndpoint : Endpoint<GenerateDefinitionsRequest, IEnumerable<Definition>>
{
    private readonly IDefinitionsService definitionsService;

    public GenerateDefinitionsEndpoint(IDefinitionsService definitionsService)
    {
        this.definitionsService = definitionsService;
    }

    public override void Configure()
    {
        Get("definitions/{wordId}/generate");
    }

    public override async Task HandleAsync(GenerateDefinitionsRequest request, CancellationToken cancellationToken)
    {
        var definitions = await definitionsService.GenerateDefinitionsAsync(request.WordId, request.PartOfSpeech,
            cancellationToken);

        await SendOkAsync(definitions, cancellationToken);
    }
}
