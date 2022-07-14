using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using OhMyWord.Api.Responses.Words;
using OhMyWord.Core.Models;
using OhMyWord.Services.Game;

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

    [HttpGet("word-hint")]
    public ActionResult<WordHint> GetWordHint() => Ok(gameService.Round.WordHint);

    [HttpGet("current-word")]
    public ActionResult<WordResponse> GetCurrentWord() => Ok(mapper.Map<WordResponse>(gameService.Round.Word));

    [HttpPost("reload-words")]
    public IActionResult UpdateShouldReloadWords()
    {
        wordsService.ShouldReloadWords = true;
        return NoContent();
    }
}
