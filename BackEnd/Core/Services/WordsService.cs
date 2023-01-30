﻿using Microsoft.Extensions.Logging;
using OhMyWord.Core.Models;
using OhMyWord.Data.Entities;
using OhMyWord.Data.Services;

namespace OhMyWord.Core.Services;

public interface IWordsService
{
    /// <summary>
    /// The number of words that are remaining before a reload.
    /// </summary>
    int RemainingWordCount { get; }

    /// <summary>
    /// Set to true to instruct the service to reload all words from the database
    /// the next time that <see cref="GetNextWordAsync"/> is called.
    /// </summary>
    bool ShouldReloadWords { set; }

    IAsyncEnumerable<Word> ListWordsAsync(int offset = WordsRepository.OffsetMinimum,
        int limit = WordsRepository.LimitDefault, string filter = "", ListWordsOrderBy orderBy = ListWordsOrderBy.Id,
        SortDirection direction = SortDirection.Ascending, CancellationToken cancellationToken = default);

    Task<Word?> GetWordAsync(string wordId, CancellationToken cancellationToken = default);

    Task CreateWordAsync(Word word, CancellationToken cancellationToken = default);

    Task<Word> GetNextWordAsync(CancellationToken cancellationToken = default);
}

public class WordsService : IWordsService
{
    private readonly ILogger<WordsService> logger;
    private readonly IWordsRepository wordsRepository;
    private readonly IDefinitionsRepository definitionsRepository;

    private Stack<Word> shuffledWords = new();

    public WordsService(ILogger<WordsService> logger, IWordsRepository wordsRepository,
        IDefinitionsRepository definitionsRepository)
    {
        this.logger = logger;
        this.wordsRepository = wordsRepository;
        this.definitionsRepository = definitionsRepository;
    }

    public int RemainingWordCount => shuffledWords.Count;
    public bool ShouldReloadWords { private get; set; }

    public IAsyncEnumerable<Word> ListWordsAsync(int offset = WordsRepository.OffsetMinimum,
        int limit = WordsRepository.LimitDefault, string filter = "", ListWordsOrderBy orderBy = ListWordsOrderBy.Id,
        SortDirection direction = SortDirection.Ascending, CancellationToken cancellationToken = default) =>
        wordsRepository.ListWords(offset, limit, filter, orderBy, direction, cancellationToken)
            .SelectAwait(async wordEntity => await MapToWordAsync(wordEntity, cancellationToken));

    public async Task<Word?> GetWordAsync(string wordId, CancellationToken cancellationToken = default)
    {
        var wordEntity = await wordsRepository.GetWordAsync(wordId, cancellationToken);
        return wordEntity is null ? default : await MapToWordAsync(wordEntity, cancellationToken);
    }

    public async Task CreateWordAsync(Word word, CancellationToken cancellationToken = default)
    {
        await wordsRepository.CreateWordAsync(
            new WordEntity { Id = word.Id, DefinitionCount = word.Definitions.Count() },
            cancellationToken);

        await Task.WhenAll(word.Definitions.Select(definition =>
            definitionsRepository.CreateDefinitionAsync(new DefinitionEntity
            {
                PartOfSpeech = definition.PartOfSpeech,
                Value = definition.Value,
                Example = definition.Example,
                WordId = word.Id
            })));
    }


    private async Task<Word> MapToWordAsync(Entity wordEntity, CancellationToken cancellationToken) => new()
    {
        Id = wordEntity.Id,
        Definitions = await definitionsRepository.GetDefinitionsAsync(wordEntity.Id, cancellationToken)
            .Select(Definition.FromEntity)
            .ToListAsync(cancellationToken),
        LastModified = wordEntity.LastModifiedTime,
    };

    public async Task<Word> GetNextWordAsync(CancellationToken cancellationToken = default)
    {
        // reload words from database
        if (ShouldReloadWords || shuffledWords.Count == 0)
        {
            shuffledWords = await GetShuffledWordsAsync(cancellationToken);
            ShouldReloadWords = false;
        }

        var word = shuffledWords.Pop();
        logger.LogDebug("Randomly selected word: {WordId}", word.Id);

        return word;
    }

    private async Task<Stack<Word>> GetShuffledWordsAsync(CancellationToken cancellationToken)
    {
        return new Stack<Word>();
        // load all words from the database
        // var allWords = new List<Word>(await wordsRepository.GetAllWordsAsync(cancellationToken));
        // if (allWords.Count == 0)
        // {
        //     logger.LogWarning("No words were retrieved from the database!");
        //     allWords.Add(Word.Default);
        // }
        // else
        // {
        //     logger.LogInformation("Retrieved: {Count} words from database", allWords.Count);
        // }
        //
        // // create a stack of randomly shuffled words
        // var stack = new Stack<Word>();
        // var allWordsIndices = new List<int>(Enumerable.Range(0, allWords.Count));
        // while (allWordsIndices.Count > 0)
        // {
        //     var index = Random.Shared.Next(allWordsIndices.Count);
        //     stack.Push(allWords[allWordsIndices[index]]);
        //     allWordsIndices.RemoveAt(index);
        // }
        //
        // return stack;
    }
}
