using Microsoft.AspNetCore.Mvc;
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

    public async Task<IActionResult> GetAllWordsAsync()
    {
        var words = await wordsRepository.GetAllWordsAsync();
        return Ok(words);
    }

    [HttpGet("current")]
    public IActionResult GetCurrentWord()
    {
        var currentWord = gameService.CurrentWord;
        return Ok(currentWord);
    }
}
