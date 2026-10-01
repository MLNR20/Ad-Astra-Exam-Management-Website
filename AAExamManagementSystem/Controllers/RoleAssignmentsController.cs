using AAExamManagementSystem.Models.Dtos;
using AAExamManagementSystem.Models.Entities;
using AAExamManagementSystem.Repository;
using Asp.Versioning;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AAExamManagementSystem.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class RoleAssignmentsController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ApplicationDbContext _dbContext;

    public RoleAssignmentsController(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, ApplicationDbContext dbContext)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _dbContext = dbContext;
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

        await AssignRoleAsync(user.Id, role.Id);

        var assignment = await BuildAssignmentAsync(user.Id, role.Id);
        return CreatedAtAction(nameof(GetById), new { id = assignment!.Id, version = "1.0" }, assignment);
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

        await DeactivateUserRoleAsync(userId, oldRoleId);
        await AssignRoleAsync(userId, newRole.Id);

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

        var deactivated = await DeactivateUserRoleAsync(userId, roleId);
        if (!deactivated) return NotFound();

        return NoContent();
    }

    /// <summary>Creates the user-role row, or reactivates a previously deactivated one, preserving its CreatedAt.</summary>
    private async Task AssignRoleAsync(string userId, string roleId)
    {
        var existing = await _dbContext.UserRoles.IgnoreQueryFilters()
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

        if (existing is null)
        {
            _dbContext.UserRoles.Add(new ApplicationUserRole { UserId = userId, RoleId = roleId });
        }
        else
        {
            existing.IsActive = true;
            existing.DateUpdated = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync();
    }

    private async Task<bool> DeactivateUserRoleAsync(string userId, string roleId)
    {
        var userRole = await _dbContext.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
        if (userRole is null) return false;

        userRole.IsActive = false;
        userRole.DateUpdated = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
        return true;
    }

    private async Task<List<RoleAssignmentDto>> BuildAssignmentsAsync()
    {
        var rows = await (
            from ur in _dbContext.UserRoles.IgnoreQueryFilters()
            join u in _dbContext.Users on ur.UserId equals u.Id
            join r in _dbContext.Roles on ur.RoleId equals r.Id
            orderby u.UserName, r.Name
            select new
            {
                u.Id,
                u.FirstName,
                u.LastName,
                RoleId = r.Id,
                RoleName = r.Name,
                ur.CreatedAt,
                ur.DateUpdated,
                ur.IsActive
            }).ToListAsync();

        return rows.Select(row => new RoleAssignmentDto
        {
            Id = RoleAssignmentId.Combine(row.Id, row.RoleId),
            UserId = row.Id,
            UserName = $"{row.FirstName} {row.LastName}".Trim(),
            RoleId = row.RoleId,
            RoleName = row.RoleName ?? string.Empty,
            IsActive = row.IsActive,
            CreatedAt = row.CreatedAt,
            DateUpdated = row.DateUpdated
        }).ToList();
    }

    private async Task<RoleAssignmentDto?> BuildAssignmentAsync(string userId, string roleId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        var role = await _roleManager.FindByIdAsync(roleId);
        if (user is null || role is null || string.IsNullOrEmpty(role.Name)) return null;

        var userRole = await _dbContext.UserRoles.IgnoreQueryFilters()
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
        if (userRole is null) return null;

        return new RoleAssignmentDto
        {
            Id = RoleAssignmentId.Combine(user.Id, role.Id),
            UserId = user.Id,
            UserName = $"{user.FirstName} {user.LastName}".Trim(),
            RoleId = role.Id,
            RoleName = role.Name ?? string.Empty,
            IsActive = userRole.IsActive,
            CreatedAt = userRole.CreatedAt,
            DateUpdated = userRole.DateUpdated
        };
    }
}
