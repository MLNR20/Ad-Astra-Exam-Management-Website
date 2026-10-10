using AppRoles = AAExamManagementSystem.Models.Entities.Roles;
using AAExamManagementSystem.Models.Dtos;
using AAExamManagementSystem.Models.Entities;
using AAExamManagementSystem.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AAExamManagementSystem.Pages.Attempts;

[Authorize(Roles = AppRoles.Admin + "," + AppRoles.Instructor + "," + AppRoles.Staffer + "," + AppRoles.Editor)]
public class ReviewModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ReviewModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    private async Task<bool> CanAccessSectionAsync(string? attemptSection)
    {
        if (User.IsInRole(AppRoles.Admin))
        {
            return true;
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser?.SectionId is null)
        {
            return false;
        }

        var currentUserSectionName = await _context.Sections
            .Where(s => s.Id == currentUser.SectionId)
            .Select(s => s.Name)
            .FirstOrDefaultAsync();

        return currentUserSectionName is not null && currentUserSectionName == attemptSection;
    }

    public AttemptReviewDto? Attempt { get; set; }

    [BindProperty]
    public Dictionary<int, int> Points { get; set; } = new();

    [BindProperty]
    public Dictionary<int, string> Corrections { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Attempt = await LoadAttemptAsync(id);
        if (Attempt is null)
        {
            return NotFound();
        }

        if (!await CanAccessSectionAsync(Attempt.Section))
        {
            return Forbid();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostApproveAsync(int id)
    {
        var attempt = await _context.Attempts
            .Include(a => a.Answers)
            .ThenInclude(ans => ans.Question)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (attempt is null)
        {
            return NotFound();
        }

        if (!await CanAccessSectionAsync(attempt.Section))
        {
            return Forbid();
        }

        var currentUser = await _userManager.GetUserAsync(User);
        var evaluatorName = currentUser != null ? $"{currentUser.FirstName} {currentUser.LastName}" : User.Identity?.Name;

        foreach (var answer in attempt.Answers.Where(ans => ans.Question.IsUpToEvaluation))
        {
            if (Points.TryGetValue(answer.Id, out var point))
            {
                answer.Point = Math.Clamp(point, 0, answer.Question.Score);
            }

            if (Corrections.TryGetValue(answer.Id, out var correctness))
            {
                answer.IsCorrect = correctness;
            }

            answer.CheckedBy = evaluatorName;
            answer.DateUpdated = DateTime.UtcNow;
        }

        attempt.TotalScore = attempt.Answers.Sum(ans => ans.Point);
        attempt.IsApproved = true;
        attempt.ApprovedBy = evaluatorName;
        attempt.DateUpdated = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return RedirectToPage(new { id });
    }

    private async Task<AttemptReviewDto?> LoadAttemptAsync(int id)
    {
        var attempt = await _context.Attempts
            .Include(a => a.Applicant)
            .Include(a => a.Answers)
            .ThenInclude(ans => ans.Question)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (attempt is null)
        {
            return null;
        }

        return new AttemptReviewDto
        {
            Id = attempt.Id,
            ApplicantName = $"{attempt.Applicant.FirstName} {attempt.Applicant.LastName}",
            StudentNo = attempt.Applicant.StudentNo,
            Section = attempt.Section,
            DateTaken = attempt.DateTaken,
            TotalScore = attempt.TotalScore,
            MaxScore = attempt.Answers.Sum(ans => ans.Question.Score),
            IsApproved = attempt.IsApproved,
            ApprovedBy = attempt.ApprovedBy,
            Answers = attempt.Answers.Select(ans => new AttemptAnswerReviewDto
            {
                Id = ans.Id,
                QuestionId = ans.QuestionId,
                QuestionTitle = ans.Question.QuestionTitle,
                AnswerText = ans.AnswerText,
                IsCorrect = ans.IsCorrect,
                Point = ans.Point,
                MaxScore = ans.Question.Score,
                IsUpToEvaluation = ans.Question.IsUpToEvaluation,
                CheckedBy = ans.CheckedBy,
            }).ToList(),
        };
    }
}
