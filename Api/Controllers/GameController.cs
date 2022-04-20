using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using OhMyWord.Api.Responses.Words;
using OhMyWord.Services.Game;

namespace OhMyWord.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GameController : ControllerBase
{
    private readonly IGameService gameService;
    private readonly IMapper mapper;

    public GameController(IGameService gameService, IMapper mapper)
    {
        this.gameService = gameService;
        this.mapper = mapper;
    }

    [HttpGet("status")]
    public ActionResult<GameStatus> GetGameStatus() => Ok(gameService.GameStatus);

    [HttpGet("word-hint")]
    public ActionResult<GameStatus> GetWordHint() => Ok(gameService.WordHint);

    [HttpGet("current-word")]
    public ActionResult<WordResponse> GetCurrentWord() => Ok(mapper.Map<WordResponse>(gameService.CurrentWord));
}
