using Microsoft.AspNetCore.Mvc;
using OhMyWord.Api.Requests.Words;
using OhMyWord.Api.Services;
using OhMyWord.Data.Repositories;

namespace OhMyWord.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WordsController : ControllerBase
{
    private readonly IWordsRepository wordsRepository;
    private readonly IGameService gameService;

    public WordsController(IWordsRepository wordsRepository, IGameService gameService)
    {
        this.wordsRepository = wordsRepository;
        this.gameService = gameService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllWordsAsync([FromQuery] bool current)
    {
        return current ?
            Ok(gameService.CurrentWord) :
            Ok(await wordsRepository.GetAllWordsAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetWordById(string id)
    {
        var word = await wordsRepository.GetWordById(id);
        return word is not null ? Ok(word) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> CreateWord(CreateWordRequest request)
    {
        var word = await wordsRepository.CreateWordAsync(request.ToWord());
        return CreatedAtRoute(nameof(GetWordById), new { word.Id }, word);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteWord(string id)
    {
        var wasDeleted = await wordsRepository.DeleteWordAsync(id);
        return wasDeleted ? NoContent() : NotFound();
    }
}
