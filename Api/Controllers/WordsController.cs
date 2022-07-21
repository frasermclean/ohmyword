using MediatR;
using Microsoft.AspNetCore.Mvc;
using OhMyWord.Core.Requests.Words;
using OhMyWord.Core.Responses.Words;
using OhMyWord.Data.Models;
using OhMyWord.Data.Services;

namespace OhMyWord.Api.Controllers;

public sealed class WordsController : AuthorizedControllerBase
{
    private readonly IMediator mediator;

    public WordsController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WordResponse>>> GetWordsAsync(
        [FromQuery] int offset = WordsRepository.OffsetMinimum,
        [FromQuery] int limit = WordsRepository.LimitDefault)
    {
        var words = await mediator.Send(new GetWordsRequest { Limit = limit, Offset = offset });
        return Ok(words);
    }

    [HttpGet("{partOfSpeech}/{id:guid}")]
    public async Task<ActionResult<WordResponse>> GetWord(PartOfSpeech partOfSpeech, Guid id)
    {
        var word = await mediator.Send(new GetWordRequest { Id = id, PartOfSpeech = partOfSpeech });
        return Ok(word);
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
