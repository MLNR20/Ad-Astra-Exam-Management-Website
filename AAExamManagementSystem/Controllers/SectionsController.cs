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
public class SectionsController : ControllerBase
{
    private readonly IGenericRepository<Section> _repository;
    private readonly IMapper _mapper;

    public SectionsController(IGenericRepository<Section> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SectionDto>>> GetAll()
    {
        var sections = await _repository.GetAllAsync();
        return Ok(_mapper.Map<IEnumerable<SectionDto>>(sections));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SectionDto>> GetById(int id)
    {
        var section = await _repository.GetByIdAsync(id);
        if (section is null) return NotFound();
        return Ok(_mapper.Map<SectionDto>(section));
    }

    [HttpPost]
    public async Task<ActionResult<SectionDto>> Create(SectionCreateUpdateDto dto)
    {
        var section = _mapper.Map<Section>(dto);
        await _repository.AddAsync(section);
        await _repository.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = section.Id, version = "1.0" }, _mapper.Map<SectionDto>(section));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, SectionCreateUpdateDto dto)
    {
        var section = await _repository.GetByIdAsync(id);
        if (section is null) return NotFound();

        section.Name = dto.Name;
        _repository.Update(section);
        await _repository.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var section = await _repository.GetByIdAsync(id);
        if (section is null) return NotFound();

        section.IsActive = false;
        _repository.Update(section);
        await _repository.SaveChangesAsync();
        return NoContent();
    }
}
