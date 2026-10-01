using AAExamManagementSystem.Models.Dtos;
using AAExamManagementSystem.Models.Entities;
using AAExamManagementSystem.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AAExamManagementSystem.Pages.RoleAssignments;

public class IndexModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ApplicationDbContext _dbContext;

    public IndexModel(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, ApplicationDbContext dbContext)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _dbContext = dbContext;
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

        await AssignRoleAsync(user.Id, role.Id);

        TempData["SuccessMessage"] = $"Role '{role.Name}' assigned to '{user.UserName}' successfully.";
        return RedirectToPage("Index");
    }

    public async Task<IActionResult> OnPostDeactivateAsync(string id)
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

        var deactivated = await DeactivateUserRoleAsync(userId, roleId);
        TempData["SuccessMessage"] = deactivated
            ? $"Role '{role.Name}' deactivated for '{user.UserName}' successfully."
            : "Role assignment could not be found.";

        return RedirectToPage("Index");
    }

    public async Task<IActionResult> OnPostActivateAsync(string id)
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

        await AssignRoleAsync(userId, roleId);
        TempData["SuccessMessage"] = $"Role '{role.Name}' activated for '{user.UserName}' successfully.";

        return RedirectToPage("Index");
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

    private async Task LoadAssignmentsAsync()
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

        Assignments = rows.Select(row => new RoleAssignmentDto
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
