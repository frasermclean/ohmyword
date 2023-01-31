using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using OhMyWord.Core.Models;
using OhMyWord.Core.Options;
using OhMyWord.Core.Services;

namespace OhMyWord.Api.Controllers;

public sealed class GameController : AuthorizedControllerBase
{
    private readonly IGameService gameService;
    private readonly IMapper mapper;
    private readonly IWordsService wordsService;

    public GameController(IGameService gameService, IMapper mapper, IWordsService wordsService)
    {
        this.gameService = gameService;
        this.mapper = mapper;
        this.wordsService = wordsService;
    }

    [HttpGet("state")]
    public IActionResult GetState() => Ok(gameService);

    [HttpGet("options")]
    public ActionResult<GameServiceOptions> GetOptions() => Ok(gameService.Options);

    [HttpGet("word-hint")]
    public ActionResult<WordHint> GetWordHint() => Ok(gameService.Round.WordHint);

    [HttpGet("current-word")]
    public ActionResult<Word> GetCurrentWord() => Ok(mapper.Map<Word>(gameService.Round.Word));

    [HttpPost("reload-words")]
    public IActionResult UpdateShouldReloadWords()
    {
        //wordsService.ShouldReloadWords = true;
        return NoContent();
    }
}
