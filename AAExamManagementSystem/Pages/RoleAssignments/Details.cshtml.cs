using AAExamManagementSystem.Models.Dtos;
using AAExamManagementSystem.Models.Entities;
using AAExamManagementSystem.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AAExamManagementSystem.Pages.RoleAssignments;

public class DetailsModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ApplicationDbContext _dbContext;

    public DetailsModel(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, ApplicationDbContext dbContext)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _dbContext = dbContext;
    }

    public RoleAssignmentDto Assignment { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(string id)
    {
        if (!RoleAssignmentId.TryParse(id, out var userId, out var roleId))
        {
            return NotFound();
        }

        var user = await _userManager.FindByIdAsync(userId);
        var role = await _roleManager.FindByIdAsync(roleId);
        if (user is null || role is null || string.IsNullOrEmpty(role.Name))
        {
            return NotFound();
        }

        var userRole = await _dbContext.UserRoles.IgnoreQueryFilters()
            .FirstOrDefaultAsync(ur => ur.UserId == user.Id && ur.RoleId == role.Id);
        if (userRole is null)
        {
            return NotFound();
        }

        Assignment = new RoleAssignmentDto
        {
            Id = id,
            UserId = user.Id,
            UserName = $"{user.FirstName} {user.LastName}".Trim(),
            RoleId = role.Id,
            RoleName = role.Name,
            IsActive = userRole.IsActive,
            CreatedAt = userRole.CreatedAt,
            DateUpdated = userRole.DateUpdated
        };
        return Page();
    }
}
