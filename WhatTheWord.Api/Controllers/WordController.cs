using MediatR;
using Microsoft.AspNetCore.Mvc;
using WhatTheWord.Domain.Processing.Words;

namespace WhatTheWord.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WordController : ControllerBase
{
    private readonly IMediator mediator;

    public WordController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetCurrentWordAsync()
    {
        var response = await mediator.Send(new GetCurrentWordRequest());
        return Ok(response);
    }
}
