using OhMyWord.Api.Requests.Game;
using OhMyWord.Api.Responses.Game;
using OhMyWord.Data.Models;
using OhMyWord.Data.Repositories;

namespace OhMyWord.Api.Services;

public interface IGameService
{
    Word CurrentWord { get; }
    Hint CurrentHint { get; }
    DateTime CurrentExpiry { get; }

    Task<Word> SelectNextWord(DateTime expiry);
    Task<GuessWordResponse> TestPlayerGuess(GuessWordRequest request);
    Task<Player> RegisterPlayerAsync(string visitorId, string connectionId);
    Task UnregisterPlayerAsync(string connectionId);
}

public class GameService : IGameService
{
    private readonly ILogger<GameService> logger;
    private readonly IWordsRepository wordsRepository;
    private readonly IPlayerRepository playerRepository;
    private readonly List<Word> words = new();

    public Word CurrentWord { get; private set; } = Word.Default;
    public Hint CurrentHint { get; private set; } = Hint.Default;
    public DateTime CurrentExpiry { get; private set; }

    public GameService(
        ILogger<GameService> logger,
        IWordsRepository wordsRepository,
        IPlayerRepository playerRepository)
    {
        this.logger = logger;
        this.wordsRepository = wordsRepository;
        this.playerRepository = playerRepository;
    }


    public async Task<Word> SelectNextWord(DateTime expiry)
    {
        // request new list of words from repository
        if (words.Count == 0) words.AddRange(await wordsRepository.GetAllWordsAsync());

        // set current word to randomly selected one
        var index = Random.Shared.Next(0, words.Count);
        CurrentWord = words[index];
        var wasRemoved = words.Remove(CurrentWord);

        if (!wasRemoved) logger.LogWarning("Word: {word} at index: {index} couldn't be removed from the list.", CurrentWord, index);

        CurrentExpiry = expiry;
        CurrentHint = new Hint(CurrentWord, expiry);

        logger.LogInformation("Current word selected as: {word}", CurrentWord);

        return CurrentWord;
    }

    public async Task<GuessWordResponse> TestPlayerGuess(GuessWordRequest request)
    {
        var correct = string.Equals(request.Value, CurrentWord.Id, StringComparison.InvariantCultureIgnoreCase);

        if (correct)
        {
            var player = await playerRepository.FindPlayerByPlayerId(request.PlayerId);
            //TODO: Perform addition player actions on correct guess
        }

        return new GuessWordResponse
        {
            Value = request.Value.ToLowerInvariant(),
            Correct = correct,
        };
    }

    public async Task<Player> RegisterPlayerAsync(string visitorId, string connectionId)
    {
        var player = await playerRepository.FindPlayerByVisitorIdAsync(visitorId);

        // create new player if existing player not found
        player ??= await playerRepository.CreatePlayerAsync(new Player
        {
            VisitorId = visitorId,
            ConnectionId = connectionId
        });

        return player;
    }

    public async Task UnregisterPlayerAsync(string connectionId)
    {
        var player = await playerRepository.FindPlayerByConnectionIdAsync(connectionId);

        if (player is null)
        {
            logger.LogWarning("Couldn't find a player with connection ID: {connectionId} to unregister.", connectionId);
            return;
        }

        await playerRepository.DeletePlayerAsync(player);
    }
}
