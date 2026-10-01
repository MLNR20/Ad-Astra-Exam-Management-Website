using AAExamManagementSystem.Models.Dtos;
using AAExamManagementSystem.Models.Entities;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AAExamManagementSystem.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class RolesController : ControllerBase
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IMapper _mapper;

    public RolesController(RoleManager<ApplicationRole> roleManager, IMapper mapper)
    {
        _roleManager = roleManager;
        _mapper = mapper;
    }

    [HttpGet]
    public ActionResult<IEnumerable<RoleDto>> GetAll()
    {
        var roles = _roleManager.Roles.ToList();
        return Ok(_mapper.Map<IEnumerable<RoleDto>>(roles));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RoleDto>> GetById(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role is null) return NotFound();
        return Ok(_mapper.Map<RoleDto>(role));
    }

    [HttpPost]
    public async Task<ActionResult<RoleDto>> Create(RoleCreateUpdateDto dto)
    {
        if (await _roleManager.RoleExistsAsync(dto.Name))
            return BadRequest($"Role '{dto.Name}' already exists.");

        var role = new ApplicationRole(dto.Name) { IsActive = dto.IsActive };
        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded) return BadRequest(result.Errors);

        return CreatedAtAction(nameof(GetById), new { id = role.Id, version = "1.0" }, _mapper.Map<RoleDto>(role));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, RoleCreateUpdateDto dto)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role is null) return NotFound();

        if (!string.Equals(role.Name, dto.Name, StringComparison.OrdinalIgnoreCase)
            && await _roleManager.RoleExistsAsync(dto.Name))
            return BadRequest($"Role '{dto.Name}' already exists.");

        role.Name = dto.Name;
        role.IsActive = dto.IsActive;
        role.DateUpdated = DateTime.UtcNow;
        var result = await _roleManager.UpdateAsync(role);
        if (!result.Succeeded) return BadRequest(result.Errors);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role is null) return NotFound();

        role.IsActive = false;
        role.DateUpdated = DateTime.UtcNow;
        var result = await _roleManager.UpdateAsync(role);
        if (!result.Succeeded) return BadRequest(result.Errors);

        return NoContent();
    }
}
