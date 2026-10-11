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

[Authorize(Roles = AppRoles.Applicant)]
public class MyAttemptDetailsModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public MyAttemptDetailsModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public AttemptReviewDto? Attempt { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var userId = _userManager.GetUserId(User);

        var attempt = await _context.Attempts
            .Include(a => a.Applicant)
            .Include(a => a.Answers)
            .ThenInclude(ans => ans.Question)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (attempt is null)
        {
            return NotFound();
        }

        if (attempt.Applicant.UserId != userId)
        {
            return Forbid();
        }

        Attempt = new AttemptReviewDto
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

        return Page();
    }
}
