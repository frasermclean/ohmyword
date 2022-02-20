using MediatR;
using Microsoft.AspNetCore.Mvc;
using WhatTheWord.Domain.Processing.Clients;

namespace WhatTheWord.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ClientController : ControllerBase
{
    private readonly IMediator mediator;

    public ClientController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> RegisterClientAsync(RegisterClientRequest request)
    {
        var response = await mediator.Send(request);
        return Ok(response);
    }
}
