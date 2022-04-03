using Microsoft.AspNetCore.Mvc;
using OhMyWord.Api.Requests.Words;
using OhMyWord.Api.Responses.Words;
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
    public async Task<ActionResult<IEnumerable<WordResponse>>> GetAllWordsAsync()
    {
        var words = (await wordsRepository.GetAllWordsAsync())
            .Select(word => new WordResponse(word));

        return Ok(words);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<WordResponse>> GetWordById(string id)
    {
        var word = await wordsRepository.GetWordById(id);
        return word is null ? 
            NotFound() : 
            Ok(new WordResponse(word));
    }

    [HttpPost]
    public async Task<IActionResult> CreateWord(CreateWordRequest request)
    {
        var word = await wordsRepository.CreateWordAsync(request.ToWord());
        return CreatedAtAction(nameof(GetWordById), new { word.Id }, word);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteWord(string id)
    {
        var wasDeleted = await wordsRepository.DeleteWordAsync(id);
        return wasDeleted ? NoContent() : NotFound();
    }
}
