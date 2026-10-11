using AppRoles = AAExamManagementSystem.Models.Entities.Roles;
using AAExamManagementSystem.Models.Dtos;
using AAExamManagementSystem.Models.Entities;
using AAExamManagementSystem.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AAExamManagementSystem.Pages.Attempts;

[Authorize(Roles = AppRoles.Applicant)]
public class MyAttemptsModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public MyAttemptsModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public IList<AttemptListItemDto> Attempts { get; set; } = new List<AttemptListItemDto>();

    public async Task OnGetAsync()
    {
        var userId = _userManager.GetUserId(User);
        var applicant = await _context.Applicants.FirstOrDefaultAsync(a => a.UserId == userId);
        if (applicant is null)
        {
            return;
        }

        var attempts = await _context.Attempts
            .Include(a => a.Answers)
            .ThenInclude(ans => ans.Question)
            .Where(a => a.ApplicantId == applicant.Id)
            .OrderByDescending(a => a.DateCreated)
            .ToListAsync();

        Attempts = attempts.Select(a => new AttemptListItemDto
        {
            Id = a.Id,
            ApplicantName = $"{applicant.FirstName} {applicant.LastName}",
            StudentNo = applicant.StudentNo,
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
