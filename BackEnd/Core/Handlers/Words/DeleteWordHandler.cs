using MediatR;
using OhMyWord.Core.Requests.Words;
using OhMyWord.Data.Services;

namespace OhMyWord.Core.Handlers.Words;

public class DeleteWordHandler : AsyncRequestHandler<DeleteWordRequest>
{
    private readonly IWordsRepository wordsRepository;

    public DeleteWordHandler(IWordsRepository wordsRepository)
    {
        this.wordsRepository = wordsRepository;
    }

    protected override async Task Handle(DeleteWordRequest request, CancellationToken cancellationToken)
    {
        await wordsRepository.DeleteWordAsync(request.PartOfSpeech.GetValueOrDefault(), request.Id.GetValueOrDefault());
    }
}