using AAExamManagementSystem.Models.Dtos;
using AAExamManagementSystem.Models.Entities;
using AAExamManagementSystem.Repository;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace AAExamManagementSystem.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class CoursesController : ControllerBase
{
    private readonly IGenericRepository<Course> _repository;
    private readonly IMapper _mapper;

    public CoursesController(IGenericRepository<Course> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CourseDto>>> GetAll()
    {
        var courses = await _repository.GetAllAsync();
        return Ok(_mapper.Map<IEnumerable<CourseDto>>(courses));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CourseDto>> GetById(int id)
    {
        var course = await _repository.GetByIdAsync(id);
        if (course is null) return NotFound();
        return Ok(_mapper.Map<CourseDto>(course));
    }

    [HttpPost]
    public async Task<ActionResult<CourseDto>> Create(CourseCreateUpdateDto dto)
    {
        var course = _mapper.Map<Course>(dto);
        await _repository.AddAsync(course);
        await _repository.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = course.Id, version = "1.0" }, _mapper.Map<CourseDto>(course));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CourseCreateUpdateDto dto)
    {
        var course = await _repository.GetByIdAsync(id);
        if (course is null) return NotFound();

        course.Name = dto.Name;
        course.IsActive = dto.IsActive;
        course.DateUpdated = DateTime.UtcNow;
        _repository.Update(course);
        await _repository.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var course = await _repository.GetByIdAsync(id);
        if (course is null) return NotFound();

        course.IsActive = false;
        course.DateUpdated = DateTime.UtcNow;
        _repository.Update(course);
        await _repository.SaveChangesAsync();
        return NoContent();
    }
}
