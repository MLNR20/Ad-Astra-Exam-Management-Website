using AppRoles = AAExamManagementSystem.Models.Entities.Roles;
using AAExamManagementSystem.Models.Dtos;
using AAExamManagementSystem.Models.Entities;
using AAExamManagementSystem.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AAExamManagementSystem.Pages.Attempts;

[Authorize(Roles = AppRoles.Admin + "," + AppRoles.Instructor + "," + AppRoles.Staffer + "," + AppRoles.Editor)]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public IndexModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public IList<AttemptListItemDto> Attempts { get; set; } = new List<AttemptListItemDto>();

    public bool IsAdmin { get; set; }

    public string? CurrentUserSectionName { get; set; }

    public async Task OnGetAsync()
    {
        IsAdmin = User.IsInRole(AppRoles.Admin);
        if (!IsAdmin)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.SectionId is not null)
            {
                CurrentUserSectionName = await _context.Sections
                    .Where(s => s.Id == currentUser.SectionId)
                    .Select(s => s.Name)
                    .FirstOrDefaultAsync();
            }
        }

        var query = _context.Attempts
            .Include(a => a.Applicant)
            .Include(a => a.Answers)
            .ThenInclude(ans => ans.Question)
            .AsQueryable();

        if (!IsAdmin)
        {
            query = CurrentUserSectionName is null
                ? query.Where(a => false)
                : query.Where(a => a.Section == CurrentUserSectionName);
        }

        var attempts = await query
            .OrderByDescending(a => a.DateCreated)
            .ToListAsync();

        Attempts = attempts.Select(a => new AttemptListItemDto
        {
            Id = a.Id,
            ApplicantName = $"{a.Applicant.FirstName} {a.Applicant.LastName}",
            StudentNo = a.Applicant.StudentNo,
            Section = a.Section,
            DateTaken = a.DateTaken,
            TotalScore = a.TotalScore,
            MaxScore = a.Answers.Sum(ans => ans.Question.Score),
            PendingEvaluationCount = a.Answers.Count(ans => ans.IsCorrect == null),
            IsApproved = a.IsApproved,
            DateCreated = a.DateCreated,
        }).ToList();
    }
}
