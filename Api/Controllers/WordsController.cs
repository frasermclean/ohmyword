using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using OhMyWord.Core.Requests.Words;
using OhMyWord.Core.Responses.Words;
using OhMyWord.Data.Models;
using OhMyWord.Data.Services;

namespace OhMyWord.Api.Controllers;

public sealed class WordsController : AuthorizedControllerBase
{
    private readonly IMapper mapper;
    private readonly IMediator mediator;
    private readonly IWordsRepository wordsRepository;

    public WordsController(IMediator mediator, IWordsRepository wordsRepository, IMapper mapper)
    {
        this.mediator = mediator;
        this.wordsRepository = wordsRepository;
        this.mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WordResponse>>> GetAllWordsAsync()
    {
        var words = await wordsRepository.GetAllWordsAsync();
        return Ok(mapper.Map<IEnumerable<WordResponse>>(words));
    }

    [HttpGet("{partOfSpeech}/{id:guid}")]
    public async Task<ActionResult<WordResponse>> GetWord(PartOfSpeech partOfSpeech, Guid id)
    {
        var result = await wordsRepository.GetWordAsync(partOfSpeech, id);
        return result.Success
            ? Ok(mapper.Map<WordResponse>(result.Resource))
            : GetErrorResult(result.StatusCode, result.Message);
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
        var result = await wordsRepository.DeleteWordAsync(partOfSpeech, id);
        return result.Success
            ? NoContent()
            : GetErrorResult(result.StatusCode, result.Message);
    }
}