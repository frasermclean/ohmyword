using MediatR;
using Microsoft.AspNetCore.Mvc;
using WhatTheWord.Domain.Requests.Game;

namespace WhatTheWord.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GameController : ControllerBase
{
    private readonly IMediator mediator;

    public GameController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterClientAsync(RegisterClientRequest request)
    {
        var response = await mediator.Send(request);
        return Ok(response);
    }

    [HttpGet("hint")]
    public async Task<IActionResult> GetHintAsync()
    {
        var response = await mediator.Send(new GetHintRequest());
        return Ok(response);
    }
}
