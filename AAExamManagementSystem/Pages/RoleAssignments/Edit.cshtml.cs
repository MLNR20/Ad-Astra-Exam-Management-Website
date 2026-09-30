using AAExamManagementSystem.Models.Dtos;
using AAExamManagementSystem.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AAExamManagementSystem.Pages.RoleAssignments;

public class EditModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public EditModel(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [BindProperty(SupportsGet = true)]
    public string Id { get; set; } = string.Empty;

    [BindProperty]
    public RoleAssignmentEditDto Assignment { get; set; } = new();

    public string UserDisplayName { get; set; } = string.Empty;

    public SelectList RoleOptions { get; set; } = new(new List<ApplicationRole>(), "Id", "Name");

    public async Task<IActionResult> OnGetAsync()
    {
        if (!RoleAssignmentId.TryParse(Id, out var userId, out var roleId))
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

        UserDisplayName = $"{user.FirstName} {user.LastName} ({user.UserName})".Trim();
        Assignment = new RoleAssignmentEditDto { RoleId = role.Id };
        LoadRoleOptions(role.Id);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!RoleAssignmentId.TryParse(Id, out var userId, out var oldRoleId))
        {
            return NotFound();
        }

        var user = await _userManager.FindByIdAsync(userId);
        var oldRole = await _roleManager.FindByIdAsync(oldRoleId);
        if (user is null || oldRole is null || string.IsNullOrEmpty(oldRole.Name))
        {
            return NotFound();
        }

        UserDisplayName = $"{user.FirstName} {user.LastName} ({user.UserName})".Trim();

        if (!ModelState.IsValid)
        {
            LoadRoleOptions(oldRoleId);
            return Page();
        }

        var newRole = await _roleManager.FindByIdAsync(Assignment.RoleId);
        if (newRole is null || string.IsNullOrEmpty(newRole.Name))
        {
            ModelState.AddModelError("Assignment.RoleId", "Selected role could not be found.");
            LoadRoleOptions(oldRoleId);
            return Page();
        }

        if (!string.Equals(oldRole.Id, newRole.Id, StringComparison.Ordinal))
        {
            if (await _userManager.IsInRoleAsync(user, newRole.Name))
            {
                ModelState.AddModelError("Assignment.RoleId", $"'{user.UserName}' is already assigned to role '{newRole.Name}'.");
                LoadRoleOptions(oldRoleId);
                return Page();
            }

            var removeResult = await _userManager.RemoveFromRoleAsync(user, oldRole.Name);
            if (!removeResult.Succeeded)
            {
                foreach (var error in removeResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                LoadRoleOptions(oldRoleId);
                return Page();
            }

            var addResult = await _userManager.AddToRoleAsync(user, newRole.Name);
            if (!addResult.Succeeded)
            {
                foreach (var error in addResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                LoadRoleOptions(oldRoleId);
                return Page();
            }
        }

        TempData["SuccessMessage"] = $"Role assignment for '{user.UserName}' updated successfully.";
        return RedirectToPage("Index");
    }

    private void LoadRoleOptions(string currentRoleId)
    {
        var roles = _roleManager.Roles.Where(r => r.IsActive).OrderBy(r => r.Name).ToList();
        RoleOptions = new SelectList(roles, "Id", "Name", currentRoleId);
    }
}
