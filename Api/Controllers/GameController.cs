using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using OhMyWord.Api.Responses.Words;
using OhMyWord.Core.Models;
using OhMyWord.Services.Game;

namespace OhMyWord.Api.Controllers;

public class GameController : AuthorizedControllerBase
{
    private readonly IGameService gameService;
    private readonly IMapper mapper;

    public GameController(IGameService gameService, IMapper mapper)
    {
        this.gameService = gameService;
        this.mapper = mapper;
    }

    [HttpGet("state")]
    public IActionResult GetState() => Ok(gameService);

    [HttpGet("word-hint")]
    public ActionResult<WordHint> GetWordHint() =>
        gameService.Round is not null ? Ok(gameService.Round.WordHint) : NoContent();

    [HttpGet("current-word")]
    public ActionResult<WordResponse> GetCurrentWord() =>
        gameService.Round is not null ? Ok(mapper.Map<WordResponse>(gameService.Round.Word)) : NoContent();
}
