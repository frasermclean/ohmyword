﻿using MediatR;
using Microsoft.AspNetCore.Mvc;
using OhMyWord.Core.Requests.Words;
using OhMyWord.Core.Responses.Words;
using OhMyWord.Data.Models;

namespace OhMyWord.Api.Controllers;

public sealed class WordsController : AuthorizedControllerBase
{
    private readonly IMediator mediator;

    public WordsController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<GetWordsResponse>> GetWordsAsync([FromQuery] GetWordsRequest request)
    {
        var response = await mediator.Send(request);
        return Ok(response);
    }

    [HttpGet("{partOfSpeech}/{id:guid}")]
    public async Task<ActionResult<WordResponse>> GetWord(PartOfSpeech partOfSpeech, Guid id)
    {
        var response = await mediator.Send(new GetWordRequest { Id = id, PartOfSpeech = partOfSpeech });
        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> CreateWord(CreateWordRequest request)
    {
        var response = await mediator.Send(request);
        return CreatedAtAction(nameof(GetWord), new { response.PartOfSpeech, response.Id }, response);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateWord(UpdateWordRequest request)
    {
        var response = await mediator.Send(request);
        return Ok(response);
    }

    [HttpDelete("{partOfSpeech}/{id:guid}")]
    public async Task<IActionResult> DeleteWord(PartOfSpeech partOfSpeech, Guid id)
    {
        await mediator.Send(new DeleteWordRequest { PartOfSpeech = partOfSpeech, Id = id });
        return NoContent();
    }
}
