using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using OhMyWord.Api.Requests.Words;
using OhMyWord.Api.Responses.Words;
using OhMyWord.Services.Repositories;

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

    [HttpGet("{id}")]
    public async Task<ActionResult<WordResponse>> GetWordById(string id)
    {
        var word = await wordsRepository.GetWordById(id);
        return word is null ? 
            NotFound() : 
            Ok(mapper.Map<WordResponse>(word));
    }

    [HttpPost]
    public async Task<IActionResult> CreateWord(CreateWordRequest request)
    {
        var word = await wordsRepository.CreateWordAsync(request.ToWord());
        return CreatedAtAction(nameof(GetWordById), new { word.Id }, mapper.Map<WordResponse>(word));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteWord(string id)
    {
        var wasDeleted = await wordsRepository.DeleteWordAsync(id);
        return wasDeleted ? NoContent() : NotFound();
    }
}
