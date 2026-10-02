using AAExamManagementSystem.Models.Dtos;
using AAExamManagementSystem.Models.Entities;
using AAExamManagementSystem.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AAExamManagementSystem.Pages.Users;

public class CreateModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IGenericRepository<Section> _sectionRepository;

    public CreateModel(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, IGenericRepository<Section> sectionRepository)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _sectionRepository = sectionRepository;
    }

    [BindProperty]
    public UserCreateDto NewUser { get; set; } = new();

    public SelectList SectionOptions { get; set; } = new(new List<Section>(), "Id", "Name");

    public IList<ApplicationRole> RoleOptions { get; set; } = new List<ApplicationRole>();

    public async Task OnGetAsync()
    {
        await LoadOptionsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadOptionsAsync();
            return Page();
        }

        var existingUser = await _userManager.FindByNameAsync(NewUser.UserName)
            ?? await _userManager.FindByEmailAsync(NewUser.Email);
        if (existingUser is not null)
        {
            ModelState.AddModelError(string.Empty, "A user with that username or email already exists.");
            await LoadOptionsAsync();
            return Page();
        }

        var user = new ApplicationUser
        {
            UserName = NewUser.UserName,
            Email = NewUser.Email,
            EmailConfirmed = true,
            FirstName = NewUser.FirstName,
            LastName = NewUser.LastName,
            SectionId = NewUser.SectionId
        };

        var createResult = await _userManager.CreateAsync(user, NewUser.Password);
        if (!createResult.Succeeded)
        {
            foreach (var error in createResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            await LoadOptionsAsync();
            return Page();
        }

        var selectedRoles = (NewUser.SelectedRoles is { Count: > 0 } ? NewUser.SelectedRoles : new List<string> { Models.Entities.Roles.Staffer });
        var roleResult = await _userManager.AddToRolesAsync(user, selectedRoles);
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            foreach (var error in roleResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            await LoadOptionsAsync();
            return Page();
        }

        if (!NewUser.IsActive)
        {
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
        }

        TempData["SuccessMessage"] = $"User '{user.UserName}' created successfully.";
        return RedirectToPage("Index");
    }

    private async Task LoadOptionsAsync()
    {
        var sections = await _sectionRepository.GetAllAsync();
        SectionOptions = new SelectList(sections.OrderBy(s => s.Name), "Id", "Name");
        RoleOptions = _roleManager.Roles.Where(r => r.IsActive).OrderBy(r => r.Name).ToList();
    }
}
