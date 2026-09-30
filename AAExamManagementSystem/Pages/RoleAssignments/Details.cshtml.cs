using AAExamManagementSystem.Models.Dtos;
using AAExamManagementSystem.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AAExamManagementSystem.Pages.RoleAssignments;

public class DetailsModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public DetailsModel(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
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

        if (!await _userManager.IsInRoleAsync(user, role.Name))
        {
            return NotFound();
        }

        Assignment = new RoleAssignmentDto
        {
            Id = id,
            UserId = user.Id,
            UserName = $"{user.FirstName} {user.LastName}".Trim(),
            RoleId = role.Id,
            RoleName = role.Name
        };
        return Page();
    }
}
