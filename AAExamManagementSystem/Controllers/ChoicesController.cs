using AAExamManagementSystem.Models.Dtos;
using AAExamManagementSystem.Models.Entities;
using AAExamManagementSystem.Repository;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AAExamManagementSystem.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class ChoicesController : ControllerBase
{
    private readonly IGenericRepository<Choice> _repository;
    private readonly IMapper _mapper;

    public ChoicesController(IGenericRepository<Choice> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ChoiceDto>>> GetAll()
    {
        var choices = await _repository.GetAllAsync();
        var dtos = _mapper.Map<IList<ChoiceDto>>(choices);
        return Ok(dtos);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ChoiceDto>> GetById(Guid id)
    {
        var choice = await _repository.GetByIdAsync(id);
        if (choice is null) return NotFound();

        var dto = _mapper.Map<ChoiceDto>(choice);
        return Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<ChoiceDto>> Create(ChoiceCreateUpdateDto dto)
    {
        var choice = _mapper.Map<Choice>(dto);
        await _repository.AddAsync(choice);
        await _repository.SaveChangesAsync();

        var result = _mapper.Map<ChoiceDto>(choice);
        return CreatedAtAction(nameof(GetById), new { id = choice.Id, version = "1.0" }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, ChoiceCreateUpdateDto dto)
    {
        var choice = await _repository.GetByIdAsync(id);
        if (choice is null) return NotFound();

        choice.ChoiceText = dto.ChoiceText;
        choice.IsCorrect = dto.IsCorrect;
        choice.IsActive = dto.IsActive;
        choice.DateUpdated = DateTime.UtcNow;
        _repository.Update(choice);
        await _repository.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var choice = await _repository.GetByIdAsync(id);
        if (choice is null) return NotFound();

        _repository.Remove(choice);
        await _repository.SaveChangesAsync();
        return NoContent();
    }
}
