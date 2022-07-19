using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using OhMyWord.Core.Requests.Words;
using OhMyWord.Core.Responses.Words;
using OhMyWord.Data.Models;
using OhMyWord.Data.Services;

namespace OhMyWord.Api.Controllers;

public sealed class WordsController : AuthorizedControllerBase
{
    private readonly IWordsRepository wordsRepository;
    private readonly IMapper mapper;

    public WordsController(IWordsRepository wordsRepository, IMapper mapper)
    {
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
        var result = await wordsRepository.CreateWordAsync(request.ToWord());
        var word = result.Resource ?? Word.Default;
        return result.Success
            ? CreatedAtAction(nameof(GetWord), new { word.PartOfSpeech, word.Id },
                mapper.Map<WordResponse>(word))
            : GetErrorResult(result.StatusCode, result.Message);
    }

    [HttpPut("{partOfSpeech}/{id:guid}")]
    public async Task<IActionResult> UpdateWord(PartOfSpeech partOfSpeech, Guid id, UpdateWordRequest request)
    {
        var result = await wordsRepository.UpdateWordAsync(request.ToWord(partOfSpeech, id));
        return result.Success
           ? Ok(mapper.Map<WordResponse>(result.Resource))
           : GetErrorResult(result.StatusCode, result.Message);
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
