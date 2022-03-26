using MediatR;
using Microsoft.AspNetCore.Mvc;
using WhatTheWord.Api.Mediator.Requests.Words;

namespace WhatTheWord.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WordsController : ControllerBase
{
    private readonly IMediator mediator;

    public WordsController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    public async Task<IActionResult> GetAllWordsAsync()
    {
        var response = await mediator.Send(new GetAllWordsRequest());
        return Ok(response);
    }

    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentWordAsync()
    {
        var response = await mediator.Send(new GetCurrentWordRequest());
        return Ok(response);
    }
}
