using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using OhMyWord.Api.Requests.Words;
using OhMyWord.Api.Responses.Words;
using OhMyWord.Core.Models;
using OhMyWord.Services.Data.Repositories;

namespace OhMyWord.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WordsController : ControllerBase
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

    [HttpGet("{partOfSpeech}/{value}")]
    public async Task<ActionResult<WordResponse>> GetWordByValue(PartOfSpeech partOfSpeech, string value)
    {
        var result = await wordsRepository.GetWordByValueAsync(partOfSpeech, value);
        return result.Success ?
            Ok(mapper.Map<WordResponse>(result.Resource)) :
            StatusCode(result.StatusCode, new { result.Message });
    }

    [HttpPost]
    public async Task<IActionResult> CreateWord(CreateWordRequest request)
    {
        var result = await wordsRepository.CreateWordAsync(request.ToWord());
        var word = result.Resource ?? Word.Default;
        return result.Success
            ? CreatedAtAction(nameof(GetWordByValue), new {word.PartOfSpeech, word.Value},
                mapper.Map<WordResponse>(word))
            : StatusCode(result.StatusCode, new {result.Message});
    }

    [HttpPut("{partOfSpeech}/{value}")]
    public async Task<IActionResult> UpdateWord(PartOfSpeech partOfSpeech, string value, CreateWordRequest request)
    {
        var result = await wordsRepository.UpdateWordAsync(partOfSpeech, value, request.ToWord());
        return result.Success ?
            Ok(mapper.Map<WordResponse>(result.Resource)) :
            StatusCode(result.StatusCode, new { result.Message });
    }

    [HttpDelete("{partOfSpeech}/{value}")]
    public async Task<IActionResult> DeleteWord(PartOfSpeech partOfSpeech, string value)
    {
        var result = await wordsRepository.DeleteWordAsync(partOfSpeech, value);
        return result.Success ? NoContent() : StatusCode(result.StatusCode, new { result.Message });
    }
}
