using MediatR;
using Microsoft.AspNetCore.Mvc;
using WhatTheWord.Domain.Processing.Words;

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

    [HttpGet("current-word")]
    public async Task<IActionResult> GetCurrentWordAsync()
    {
        var response = await mediator.Send(new GetCurrentWordRequest());
        return Ok(response);
    }
}
