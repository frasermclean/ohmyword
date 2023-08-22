using OhMyWord.Core.Models;

namespace OhMyWord.Core.Services;

public interface IDictionaryClient
{
    Task<Word?> GetWordAsync(string word, CancellationToken cancellationToken = default);

    Task<Word> GetRandomWordAsync(CancellationToken cancellationToken = default);
}
