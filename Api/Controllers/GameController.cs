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

    public GameController(IGameService gameService, IMapper mapper)
    {
        this.gameService = gameService;
        this.mapper = mapper;
    }

    [HttpGet("state")]
    public IActionResult GetState() => Ok(gameService);

    [HttpGet("word-hint")]
    public ActionResult<WordHint> GetWordHint() => Ok(gameService.Round.WordHint);

    [HttpGet("current-word")]
    public ActionResult<WordResponse> GetCurrentWord() => Ok(mapper.Map<WordResponse>(gameService.Round.Word));
}
