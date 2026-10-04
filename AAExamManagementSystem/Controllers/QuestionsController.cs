using AAExamManagementSystem.Models.Dtos;
using AAExamManagementSystem.Models.Entities;
using AAExamManagementSystem.Repository;
using AAExamManagementSystem.Services;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AAExamManagementSystem.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class QuestionsController : ControllerBase
{
    private readonly IGenericRepository<Question> _repository;
    private readonly IGenericRepository<QuestionType> _questionTypeRepository;
    private readonly IGenericRepository<Section> _sectionRepository;
    private readonly QuestionChoiceService _choiceService;
    private readonly IMapper _mapper;

    public QuestionsController(
        IGenericRepository<Question> repository,
        IGenericRepository<QuestionType> questionTypeRepository,
        IGenericRepository<Section> sectionRepository,
        QuestionChoiceService choiceService,
        IMapper mapper)
    {
        _repository = repository;
        _questionTypeRepository = questionTypeRepository;
        _sectionRepository = sectionRepository;
        _choiceService = choiceService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<QuestionDto>>> GetAll()
    {
        var questions = await _repository.GetAllAsync();
        var dtos = _mapper.Map<IList<QuestionDto>>(questions);
        await PopulateLookupsAsync(dtos);
        return Ok(dtos);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<QuestionDto>> GetById(Guid id)
    {
        var question = await _repository.GetByIdAsync(id);
        if (question is null) return NotFound();

        var dto = _mapper.Map<QuestionDto>(question);
        await PopulateLookupsAsync(new[] { dto });
        dto.Choices = await _choiceService.GetChoicesAsync(id);
        return Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<QuestionDto>> Create(QuestionCreateUpdateDto dto)
    {
        await _choiceService.NormalizeAndValidateAsync(dto, ModelState, string.Empty);
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var question = _mapper.Map<Question>(dto);
        await _repository.AddAsync(question);
        await _choiceService.ReplaceChoicesAsync(question, dto.Choices);
        await _repository.SaveChangesAsync();

        var result = _mapper.Map<QuestionDto>(question);
        await PopulateLookupsAsync(new[] { result });
        result.Choices = await _choiceService.GetChoicesAsync(question.Id);
        return CreatedAtAction(nameof(GetById), new { id = question.Id, version = "1.0" }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, QuestionCreateUpdateDto dto)
    {
        var question = await _repository.GetByIdAsync(id);
        if (question is null) return NotFound();

        await _choiceService.NormalizeAndValidateAsync(dto, ModelState, string.Empty);
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        question.QuestionTypeId = dto.QuestionTypeId;
        question.SectionId = dto.SectionId;
        question.QuestionTitle = dto.QuestionTitle;
        question.Image = dto.Image;
        question.Score = dto.Score;
        question.IsUpToEvaluation = dto.IsUpToEvaluation;
        question.IsActive = dto.IsActive;
        question.DateUpdated = DateTime.UtcNow;
        _repository.Update(question);
        await _choiceService.ReplaceChoicesAsync(question, dto.Choices);
        await _repository.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var question = await _repository.GetByIdAsync(id);
        if (question is null) return NotFound();

        await _choiceService.RemoveChoicesAsync(question.Id);
        _repository.Remove(question);
        await _repository.SaveChangesAsync();
        return NoContent();
    }

    private async Task PopulateLookupsAsync(IEnumerable<QuestionDto> dtos)
    {
        var questionTypes = await _questionTypeRepository.GetAllAsync();
        var sections = await _sectionRepository.GetAllAsync();
        var questionTypeNames = questionTypes.ToDictionary(qt => qt.Id, qt => qt.Name);
        var sectionNames = sections.ToDictionary(s => s.Id, s => s.Name);
        foreach (var dto in dtos)
        {
            dto.QuestionTypeName = questionTypeNames.GetValueOrDefault(dto.QuestionTypeId, string.Empty);
            dto.SectionName = sectionNames.GetValueOrDefault(dto.SectionId, string.Empty);
        }
    }
}
