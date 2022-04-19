﻿using AutoMapper;
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
        var word = await wordsRepository.GetWordByValueAsync(partOfSpeech, value);
        return word is null ?
            NotFound() :
            Ok(mapper.Map<WordResponse>(word));
    }

    [HttpPost]
    public async Task<IActionResult> CreateWord(CreateWordRequest request)
    {
        var word = await wordsRepository.CreateWordAsync(request.ToWord());
        return CreatedAtAction(nameof(GetWordByValue), new { word.PartOfSpeech, word.Value }, mapper.Map<WordResponse>(word));
    }

    [HttpPut("{partOfSpeech}/{value}")]
    public async Task<IActionResult> UpdateWord(PartOfSpeech partOfSpeech, string value, CreateWordRequest request)
    {
        var result = await wordsRepository.UpdateWordAsync(partOfSpeech, value, request.ToWord());
        return result.Success ? Ok(mapper.Map<WordResponse>(result.Resource)) : GetErrorResult(result);
    }

    [HttpDelete("{partOfSpeech}/{value}")]
    public async Task<IActionResult> DeleteWord(PartOfSpeech partOfSpeech, string value)
    {
        var wasDeleted = await wordsRepository.DeleteWordAsync(partOfSpeech, value);
        return wasDeleted ? NoContent() : NotFound();
    }

    private static IActionResult GetErrorResult(RepositoryActionResult<Word> result)
    {
        return new StatusCodeResult((int) result.StatusCode);
    }
}
