using AAExamManagementSystem.Models.Dtos;
using AAExamManagementSystem.Models.Entities;
using Asp.Versioning;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AAExamManagementSystem.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class RoleAssignmentsController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public RoleAssignmentsController(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoleAssignmentDto>>> GetAll()
    {
        return Ok(await BuildAssignmentsAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RoleAssignmentDto>> GetById(string id)
    {
        if (!RoleAssignmentId.TryParse(id, out var userId, out var roleId))
        {
            return NotFound();
        }

        var assignment = await BuildAssignmentAsync(userId, roleId);
        if (assignment is null) return NotFound();

        return Ok(assignment);
    }

    [HttpPost]
    public async Task<ActionResult<RoleAssignmentDto>> Create(RoleAssignmentCreateDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.UserId);
        if (user is null) return BadRequest("User not found.");

        var role = await _roleManager.FindByIdAsync(dto.RoleId);
        if (role is null || string.IsNullOrEmpty(role.Name)) return BadRequest("Role not found.");

        if (await _userManager.IsInRoleAsync(user, role.Name))
            return BadRequest($"User is already assigned to role '{role.Name}'.");

        var result = await _userManager.AddToRoleAsync(user, role.Name);
        if (!result.Succeeded) return BadRequest(result.Errors);

        var assignment = ToDto(user, role);
        return CreatedAtAction(nameof(GetById), new { id = assignment.Id, version = "1.0" }, assignment);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, RoleAssignmentEditDto dto)
    {
        if (!RoleAssignmentId.TryParse(id, out var userId, out var oldRoleId))
        {
            return NotFound();
        }

        var user = await _userManager.FindByIdAsync(userId);
        var oldRole = await _roleManager.FindByIdAsync(oldRoleId);
        if (user is null || oldRole is null || string.IsNullOrEmpty(oldRole.Name)) return NotFound();

        var newRole = await _roleManager.FindByIdAsync(dto.RoleId);
        if (newRole is null || string.IsNullOrEmpty(newRole.Name)) return BadRequest("Role not found.");

        if (string.Equals(oldRole.Id, newRole.Id, StringComparison.Ordinal))
        {
            return NoContent();
        }

        if (await _userManager.IsInRoleAsync(user, newRole.Name))
            return BadRequest($"User is already assigned to role '{newRole.Name}'.");

        var removeResult = await _userManager.RemoveFromRoleAsync(user, oldRole.Name);
        if (!removeResult.Succeeded) return BadRequest(removeResult.Errors);

        var addResult = await _userManager.AddToRoleAsync(user, newRole.Name);
        if (!addResult.Succeeded) return BadRequest(addResult.Errors);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        if (!RoleAssignmentId.TryParse(id, out var userId, out var roleId))
        {
            return NotFound();
        }

        var user = await _userManager.FindByIdAsync(userId);
        var role = await _roleManager.FindByIdAsync(roleId);
        if (user is null || role is null || string.IsNullOrEmpty(role.Name)) return NotFound();

        var result = await _userManager.RemoveFromRoleAsync(user, role.Name);
        if (!result.Succeeded) return BadRequest(result.Errors);

        return NoContent();
    }

    private async Task<List<RoleAssignmentDto>> BuildAssignmentsAsync()
    {
        var users = _userManager.Users.OrderBy(u => u.UserName).ToList();
        var assignments = new List<RoleAssignmentDto>();

        foreach (var user in users)
        {
            var roleNames = await _userManager.GetRolesAsync(user);
            foreach (var roleName in roleNames.OrderBy(r => r))
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role is null) continue;

                assignments.Add(ToDto(user, role));
            }
        }

        return assignments;
    }

    private async Task<RoleAssignmentDto?> BuildAssignmentAsync(string userId, string roleId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        var role = await _roleManager.FindByIdAsync(roleId);
        if (user is null || role is null || string.IsNullOrEmpty(role.Name)) return null;

        if (!await _userManager.IsInRoleAsync(user, role.Name)) return null;

        return ToDto(user, role);
    }

    private static RoleAssignmentDto ToDto(ApplicationUser user, ApplicationRole role) => new()
    {
        Id = RoleAssignmentId.Combine(user.Id, role.Id),
        UserId = user.Id,
        UserName = $"{user.FirstName} {user.LastName}".Trim(),
        RoleId = role.Id,
        RoleName = role.Name ?? string.Empty
    };
}
