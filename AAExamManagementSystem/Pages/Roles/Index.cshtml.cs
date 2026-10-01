using AAExamManagementSystem.Models.Dtos;
using AAExamManagementSystem.Models.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AAExamManagementSystem.Pages.Roles;

public class IndexModel : PageModel
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IMapper _mapper;

    public IndexModel(RoleManager<ApplicationRole> roleManager, IMapper mapper)
    {
        _roleManager = roleManager;
        _mapper = mapper;
    }

    public IList<RoleDto> Roles { get; set; } = new List<RoleDto>();

    [BindProperty]
    public RoleCreateUpdateDto NewRole { get; set; } = new();

    public bool ShowCreateModal { get; set; }

    public void OnGet()
    {
        LoadRoles();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!ModelState.IsValid)
        {
            LoadRoles();
            ShowCreateModal = true;
            return Page();
        }

        if (await _roleManager.RoleExistsAsync(NewRole.Name))
        {
            ModelState.AddModelError("NewRole.Name", $"Role '{NewRole.Name}' already exists.");
            LoadRoles();
            ShowCreateModal = true;
            return Page();
        }

        var result = await _roleManager.CreateAsync(new ApplicationRole(NewRole.Name) { IsActive = NewRole.IsActive });
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            LoadRoles();
            ShowCreateModal = true;
            return Page();
        }

        TempData["SuccessMessage"] = $"Role '{NewRole.Name}' created successfully.";
        return RedirectToPage("Index");
    }

    public async Task<IActionResult> OnPostDeactivateAsync(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role is null)
        {
            return NotFound();
        }

        role.IsActive = false;
        role.DateUpdated = DateTime.UtcNow;
        var result = await _roleManager.UpdateAsync(role);
        TempData["SuccessMessage"] = result.Succeeded
            ? $"Role '{role.Name}' deactivated successfully."
            : string.Join(" ", result.Errors.Select(e => e.Description));

        return RedirectToPage("Index");
    }

    public async Task<IActionResult> OnPostActivateAsync(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role is null)
        {
            return NotFound();
        }

        role.IsActive = true;
        role.DateUpdated = DateTime.UtcNow;
        var result = await _roleManager.UpdateAsync(role);
        TempData["SuccessMessage"] = result.Succeeded
            ? $"Role '{role.Name}' activated successfully."
            : string.Join(" ", result.Errors.Select(e => e.Description));

        return RedirectToPage("Index");
    }

    private void LoadRoles()
    {
        var roles = _roleManager.Roles.OrderBy(r => r.Name).ToList();
        Roles = _mapper.Map<IList<RoleDto>>(roles);
    }
}
