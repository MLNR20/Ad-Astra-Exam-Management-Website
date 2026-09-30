using AAExamManagementSystem.Models.Dtos;
using AAExamManagementSystem.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AAExamManagementSystem.Pages.RoleAssignments;

public class IndexModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public IndexModel(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public IList<RoleAssignmentDto> Assignments { get; set; } = new List<RoleAssignmentDto>();

    [BindProperty]
    public RoleAssignmentCreateDto NewAssignment { get; set; } = new();

    public SelectList UserOptions { get; set; } = new(new List<object>(), "Id", "DisplayName");
    public SelectList RoleOptions { get; set; } = new(new List<ApplicationRole>(), "Id", "Name");

    public bool ShowCreateModal { get; set; }

    public async Task OnGetAsync()
    {
        await LoadAssignmentsAsync();
        LoadOptions();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadAssignmentsAsync();
            LoadOptions();
            ShowCreateModal = true;
            return Page();
        }

        var user = await _userManager.FindByIdAsync(NewAssignment.UserId);
        var role = await _roleManager.FindByIdAsync(NewAssignment.RoleId);
        if (user is null || role is null || string.IsNullOrEmpty(role.Name))
        {
            ModelState.AddModelError(string.Empty, "Selected user or role could not be found.");
            await LoadAssignmentsAsync();
            LoadOptions();
            ShowCreateModal = true;
            return Page();
        }

        if (await _userManager.IsInRoleAsync(user, role.Name))
        {
            ModelState.AddModelError(string.Empty, $"'{user.UserName}' is already assigned to role '{role.Name}'.");
            await LoadAssignmentsAsync();
            LoadOptions();
            ShowCreateModal = true;
            return Page();
        }

        var result = await _userManager.AddToRoleAsync(user, role.Name);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            await LoadAssignmentsAsync();
            LoadOptions();
            ShowCreateModal = true;
            return Page();
        }

        TempData["SuccessMessage"] = $"Role '{role.Name}' assigned to '{user.UserName}' successfully.";
        return RedirectToPage("Index");
    }

    public async Task<IActionResult> OnPostDeleteAsync(string id)
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

        var result = await _userManager.RemoveFromRoleAsync(user, role.Name);
        TempData["SuccessMessage"] = result.Succeeded
            ? $"Role '{role.Name}' removed from '{user.UserName}' successfully."
            : string.Join(" ", result.Errors.Select(e => e.Description));

        return RedirectToPage("Index");
    }

    private async Task LoadAssignmentsAsync()
    {
        var users = _userManager.Users.OrderBy(u => u.UserName).ToList();
        var list = new List<RoleAssignmentDto>();

        foreach (var user in users)
        {
            var roleNames = await _userManager.GetRolesAsync(user);
            foreach (var roleName in roleNames.OrderBy(r => r))
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role is null) continue;

                list.Add(new RoleAssignmentDto
                {
                    Id = RoleAssignmentId.Combine(user.Id, role.Id),
                    UserId = user.Id,
                    UserName = $"{user.FirstName} {user.LastName}".Trim(),
                    RoleId = role.Id,
                    RoleName = role.Name ?? string.Empty
                });
            }
        }

        Assignments = list;
    }

    private void LoadOptions()
    {
        var users = _userManager.Users.OrderBy(u => u.UserName).ToList();
        UserOptions = new SelectList(
            users.Select(u => new { u.Id, DisplayName = $"{u.FirstName} {u.LastName} ({u.UserName})".Trim() }),
            "Id", "DisplayName");

        var roles = _roleManager.Roles.Where(r => r.IsActive).OrderBy(r => r.Name).ToList();
        RoleOptions = new SelectList(roles, "Id", "Name");
    }
}
