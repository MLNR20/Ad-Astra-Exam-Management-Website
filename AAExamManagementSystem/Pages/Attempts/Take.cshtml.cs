using AppRoles = AAExamManagementSystem.Models.Entities.Roles;
using DepartmentNames = AAExamManagementSystem.Models.Entities.Departments;
using AAExamManagementSystem.Models.Dtos;
using AAExamManagementSystem.Models.Entities;
using AAExamManagementSystem.Repository;
using AAExamManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AAExamManagementSystem.Pages.Attempts;

[Authorize(Roles = AppRoles.Applicant)]
public class TakeModel : PageModel
{
    private const int QuestionCount = 10;

    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly QuestionChoiceService _choiceService;

    public TakeModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager, QuestionChoiceService choiceService)
    {
        _context = context;
        _userManager = userManager;
        _choiceService = choiceService;
    }

    public bool HasApplicantProfile { get; set; } = true;

    public bool AlreadyAttempted { get; set; }

    public IList<QuestionDto> Questions { get; set; } = new List<QuestionDto>();

    [TempData]
    public int? SubmittedScore { get; set; }

    [TempData]
    public int? SubmittedMaxScore { get; set; }

    [BindProperty]
    public Dictionary<Guid, Guid> Answers { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var applicant = await GetApplicantAsync();
        if (applicant is null)
        {
            HasApplicantProfile = false;
            return Page();
        }

        AlreadyAttempted = await _context.Attempts.AnyAsync(a => a.ApplicantId == applicant.Id);
        if (!AlreadyAttempted)
        {
            await LoadQuestionsAsync();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var applicant = await GetApplicantAsync();
        if (applicant is null)
        {
            HasApplicantProfile = false;
            return Page();
        }

        if (await _context.Attempts.AnyAsync(a => a.ApplicantId == applicant.Id))
        {
            AlreadyAttempted = true;
            return Page();
        }

        await LoadQuestionsAsync();

        var attempt = new Attempt
        {
            ApplicantId = applicant.Id,
            AcademicYear = applicant.AcademicYear,
            Section = DepartmentNames.WebDevelopment,
            DateTaken = DateTime.UtcNow.ToString("o"),
        };

        var totalScore = 0;
        foreach (var question in Questions)
        {
            Answers.TryGetValue(question.Id, out var selectedChoiceId);
            var selectedChoice = question.Choices.FirstOrDefault(c => c.Id == selectedChoiceId);

            var answer = new Answer
            {
                ApplicantId = applicant.Id,
                QuestionId = question.Id,
                AnswerText = selectedChoice?.ChoiceText ?? string.Empty,
            };

            if (question.IsUpToEvaluation)
            {
                answer.Point = 0;
            }
            else
            {
                var isCorrect = selectedChoice?.IsCorrect ?? false;
                answer.IsCorrect = isCorrect ? "Yes" : "No";
                answer.Point = isCorrect ? question.Score : 0;
                totalScore += answer.Point;
            }

            attempt.Answers.Add(answer);
        }

        attempt.TotalScore = totalScore;
        _context.Attempts.Add(attempt);
        await _context.SaveChangesAsync();

        SubmittedScore = totalScore;
        SubmittedMaxScore = Questions.Sum(q => q.Score);

        return RedirectToPage();
    }

    private async Task<Applicant?> GetApplicantAsync()
    {
        var userId = _userManager.GetUserId(User);
        return await _context.Applicants.FirstOrDefaultAsync(a => a.UserId == userId);
    }

    private async Task LoadQuestionsAsync()
    {
        var questions = await _context.Questions
            .Include(q => q.Section)
            .Where(q => q.IsActive && q.Section.Name == DepartmentNames.WebDevelopment)
            .OrderBy(q => q.DateCreated)
            .Take(QuestionCount)
            .ToListAsync();

        Questions = questions.Select(q => new QuestionDto
        {
            Id = q.Id,
            QuestionTypeId = q.QuestionTypeId,
            SectionId = q.SectionId,
            QuestionTitle = q.QuestionTitle,
            Image = q.Image,
            Score = q.Score,
            IsUpToEvaluation = q.IsUpToEvaluation,
            IsActive = q.IsActive,
        }).ToList();

        foreach (var question in Questions)
        {
            question.Choices = await _choiceService.GetChoicesAsync(question.Id);
        }
    }
}
