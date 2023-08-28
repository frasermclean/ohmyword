using FluentResults;
using OhMyWord.Core.Models;

namespace OhMyWord.Core.Services;

public interface IWordsRepository
{
    public const int OffsetMinimum = 0;
    public const int LimitDefault = 10;
    public const int LimitMinimum = 1;
    public const int LimitMaximum = 100;

    IAsyncEnumerable<Word> GetAllWordsAsync(CancellationToken cancellationToken = default);

    IAsyncEnumerable<Word> SearchWords(int offset = OffsetMinimum, int limit = LimitDefault, string filter = "",
        string orderBy = "", bool isDescending = false, CancellationToken cancellationToken = default);

    IAsyncEnumerable<string> GetAllWordIds(CancellationToken cancellationToken = default);

    Task<int> GetTotalWordCountAsync(CancellationToken cancellationToken = default);

    Task<Result<Word>> GetWordAsync(string id, CancellationToken cancellationToken = default);
    Task<Result<Word>> CreateWordAsync(Word entity, CancellationToken cancellationToken = default);
    Task<Result<Word>> UpdateWordAsync(Word entity, CancellationToken cancellationToken = default);
    Task<Result> DeleteWordAsync(string wordId, CancellationToken cancellationToken = default);
}
