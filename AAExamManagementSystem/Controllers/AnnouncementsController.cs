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
public class AnnouncementsController : ControllerBase
{
    private readonly IGenericRepository<Announcement> _repository;
    private readonly IMapper _mapper;

    public AnnouncementsController(IGenericRepository<Announcement> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AnnouncementDto>>> GetAll()
    {
        var announcements = await _repository.GetAllAsync();
        return Ok(_mapper.Map<IEnumerable<AnnouncementDto>>(announcements));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AnnouncementDto>> GetById(string id)
    {
        var announcement = await _repository.GetByIdAsync(id);
        if (announcement is null) return NotFound();
        return Ok(_mapper.Map<AnnouncementDto>(announcement));
    }

    [HttpPost]
    public async Task<ActionResult<AnnouncementDto>> Create(AnnouncementCreateUpdateDto dto)
    {
        var announcement = _mapper.Map<Announcement>(dto);
        await _repository.AddAsync(announcement);
        await _repository.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = announcement.Id, version = "1.0" }, _mapper.Map<AnnouncementDto>(announcement));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, AnnouncementCreateUpdateDto dto)
    {
        var announcement = await _repository.GetByIdAsync(id);
        if (announcement is null) return NotFound();

        announcement.Title = dto.Title;
        announcement.Subheader = dto.Subheader;
        announcement.Body = dto.Body;
        announcement.IsActive = dto.IsActive;
        announcement.DateUpdated = DateTime.UtcNow;
        _repository.Update(announcement);
        await _repository.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var announcement = await _repository.GetByIdAsync(id);
        if (announcement is null) return NotFound();

        announcement.IsActive = false;
        announcement.DateUpdated = DateTime.UtcNow;
        _repository.Update(announcement);
        await _repository.SaveChangesAsync();
        return NoContent();
    }
}
