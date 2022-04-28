using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using OhMyWord.Api.Requests.Words;
using OhMyWord.Api.Responses.Words;
using OhMyWord.Core.Models;
using OhMyWord.Services.Data.Repositories;

namespace OhMyWord.Api.Controllers;


public class WordsController : AuthorizedControllerBase
{
    private readonly IWordsRepository wordsRepository;
    private readonly IMapper mapper;

    public WordsController(IWordsRepository wordsRepository, IMapper mapper)
    {
        this.wordsRepository = wordsRepository;
        this.mapper = mapper;
    }

    [HttpGet]
    [RequiredScope("words.read")]
    public async Task<ActionResult<IEnumerable<WordResponse>>> GetAllWordsAsync()
    {
        var words = await wordsRepository.GetAllWordsAsync();
        return Ok(mapper.Map<IEnumerable<WordResponse>>(words));
    }

    [HttpGet("{partOfSpeech}/{value}")]
    [RequiredScope("words.read")]
    public async Task<ActionResult<WordResponse>> GetWordByValue(PartOfSpeech partOfSpeech, string value)
    {
        var result = await wordsRepository.GetWordByValueAsync(partOfSpeech, value);
        return result.Success
            ? Ok(mapper.Map<WordResponse>(result.Resource))
            : GetErrorResult(result.StatusCode, result.Message);
    }

    [HttpPost]
    [RequiredScope("words.write")]
    public async Task<IActionResult> CreateWord(CreateWordRequest request)
    {
        var result = await wordsRepository.CreateWordAsync(request.ToWord());
        var word = result.Resource ?? Word.Default;
        return result.Success
            ? CreatedAtAction(nameof(GetWordByValue), new { word.PartOfSpeech, word.Value },
                mapper.Map<WordResponse>(word))
            : GetErrorResult(result.StatusCode, result.Message);
    }

    [HttpPut("{partOfSpeech}/{value}")]
    [RequiredScope("words.write")]
    public async Task<IActionResult> UpdateWord(PartOfSpeech partOfSpeech, string value, UpdateWordRequest request)
    {
        var result = await wordsRepository.UpdateWordAsync(request.ToWord(partOfSpeech, value));
        return result.Success
           ? Ok(mapper.Map<WordResponse>(result.Resource))
           : GetErrorResult(result.StatusCode, result.Message);
    }

    [HttpDelete("{partOfSpeech}/{value}")]
    [RequiredScope("words.write")]
    public async Task<IActionResult> DeleteWord(PartOfSpeech partOfSpeech, string value)
    {
        var result = await wordsRepository.DeleteWordAsync(partOfSpeech, value);
        return result.Success 
            ? NoContent() 
            : GetErrorResult(result.StatusCode, result.Message);
    }
}
