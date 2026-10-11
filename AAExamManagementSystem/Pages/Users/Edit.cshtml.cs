using AppRoles = AAExamManagementSystem.Models.Entities.Roles;
using AAExamManagementSystem.Models.Dtos;
using AAExamManagementSystem.Models.Entities;
using AAExamManagementSystem.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AAExamManagementSystem.Pages.Users;

[Authorize(Roles = AppRoles.Admin)]
public class EditModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IGenericRepository<Section> _sectionRepository;

    public EditModel(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, IGenericRepository<Section> sectionRepository)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _sectionRepository = sectionRepository;
    }

    [BindProperty(SupportsGet = true)]
    public string Id { get; set; } = string.Empty;

    [BindProperty]
    public UserEditDto TargetUser { get; set; } = new();

    public SelectList SectionOptions { get; set; } = new(new List<Section>(), "Id", "Name");

    public IList<ApplicationRole> RoleOptions { get; set; } = new List<ApplicationRole>();

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.FindByIdAsync(Id);
        if (user is null)
        {
            return NotFound();
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        TargetUser = new UserEditDto
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            IsActive = !(user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow),
            SectionId = user.SectionId,
            SelectedRoles = currentRoles.ToList()
        };
        await LoadOptionsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadOptionsAsync();
            return Page();
        }

        var user = await _userManager.FindByIdAsync(Id);
        if (user is null)
        {
            return NotFound();
        }

        if (!string.Equals(user.Email, TargetUser.Email, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await _userManager.FindByEmailAsync(TargetUser.Email);
            if (existing is not null && existing.Id != user.Id)
            {
                ModelState.AddModelError("TargetUser.Email", $"Email '{TargetUser.Email}' is already in use.");
                await LoadOptionsAsync();
                return Page();
            }

            await _userManager.SetEmailAsync(user, TargetUser.Email);
        }

        user.FirstName = TargetUser.FirstName;
        user.LastName = TargetUser.LastName;
        user.SectionId = TargetUser.SectionId;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            await LoadOptionsAsync();
            return Page();
        }

        await _userManager.SetLockoutEndDateAsync(user, TargetUser.IsActive ? null : DateTimeOffset.MaxValue);

        var currentRoles = await _userManager.GetRolesAsync(user);
        var selectedRoles = TargetUser.SelectedRoles ?? new List<string>();
        var rolesToRemove = currentRoles.Except(selectedRoles).ToList();
        var rolesToAdd = selectedRoles.Except(currentRoles).ToList();

        if (rolesToRemove.Count > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            if (!removeResult.Succeeded)
            {
                foreach (var error in removeResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                await LoadOptionsAsync();
                return Page();
            }
        }

        if (rolesToAdd.Count > 0)
        {
            var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
            if (!addResult.Succeeded)
            {
                foreach (var error in addResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                await LoadOptionsAsync();
                return Page();
            }
        }

        TempData["SuccessMessage"] = $"User '{user.UserName}' updated successfully.";
        return RedirectToPage("Index");
    }

    private async Task LoadOptionsAsync()
    {
        var sections = await _sectionRepository.GetAllAsync();
        SectionOptions = new SelectList(sections.OrderBy(s => s.Name), "Id", "Name");
        RoleOptions = _roleManager.Roles.Where(r => r.IsActive).OrderBy(r => r.Name).ToList();
    }
}
