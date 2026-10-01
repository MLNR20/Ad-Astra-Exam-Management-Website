using AppRoles = AAExamManagementSystem.Models.Entities.Roles;
using AAExamManagementSystem.Models.Dtos;
using AAExamManagementSystem.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AAExamManagementSystem.Pages.Attempts;

[Authorize(Roles = AppRoles.Admin + "," + AppRoles.Instructor + "," + AppRoles.Staffer + "," + AppRoles.Editor)]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public IList<AttemptListItemDto> Attempts { get; set; } = new List<AttemptListItemDto>();

    public async Task OnGetAsync()
    {
        var attempts = await _context.Attempts
            .Include(a => a.Applicant)
            .Include(a => a.Answers)
            .ThenInclude(ans => ans.Question)
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
