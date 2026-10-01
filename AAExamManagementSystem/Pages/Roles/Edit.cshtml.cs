using AAExamManagementSystem.Models.Dtos;
using AAExamManagementSystem.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AAExamManagementSystem.Pages.Roles;

public class EditModel : PageModel
{
    private readonly RoleManager<ApplicationRole> _roleManager;

    public EditModel(RoleManager<ApplicationRole> roleManager)
    {
        _roleManager = roleManager;
    }

    [BindProperty(SupportsGet = true)]
    public string Id { get; set; } = string.Empty;

    [BindProperty]
    public RoleCreateUpdateDto Role { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var role = await _roleManager.FindByIdAsync(Id);
        if (role is null)
        {
            return NotFound();
        }

        Role = new RoleCreateUpdateDto { Name = role.Name ?? string.Empty, IsActive = role.IsActive };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var role = await _roleManager.FindByIdAsync(Id);
        if (role is null)
        {
            return NotFound();
        }

        if (!string.Equals(role.Name, Role.Name, StringComparison.OrdinalIgnoreCase)
            && await _roleManager.RoleExistsAsync(Role.Name))
        {
            ModelState.AddModelError(string.Empty, $"Role '{Role.Name}' already exists.");
            return Page();
        }

        role.Name = Role.Name;
        role.IsActive = Role.IsActive;
        role.DateUpdated = DateTime.UtcNow;
        var result = await _roleManager.UpdateAsync(role);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return Page();
        }

        TempData["SuccessMessage"] = $"Role '{Role.Name}' updated successfully.";
        return RedirectToPage("Index");
    }
}
