using System.Text.Json;
using AppRoles = AAExamManagementSystem.Models.Entities.Roles;
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

    public string? SectionName { get; set; }

    public string? FirstChoice { get; set; }

    public string? SecondChoice { get; set; }

    public IList<IList<QuestionDto>> Pages { get; set; } = new List<IList<QuestionDto>>();

    public int TotalPages { get; set; }

    public int CurrentPage { get; set; } = 1;

    public IList<QuestionDto> CurrentPageQuestions { get; set; } = new List<QuestionDto>();

    public string CurrentPageHeading { get; set; } = "Multiple Choice";

    public string CurrentPageSubheading { get; set; } = "Select one answer for each question, then continue.";

    private IList<(string? Instruction, string? SubInstruction)> PageInstructions { get; set; } = new List<(string?, string?)>();

    [TempData]
    public int? SubmittedScore { get; set; }

    [TempData]
    public int? SubmittedMaxScore { get; set; }

    [TempData]
    public string? ExamError { get; set; }

    [BindProperty]
    public Dictionary<Guid, Guid> Answers { get; set; } = new();

    [BindProperty]
    public Dictionary<Guid, string> AnswerTexts { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int pageNum = 1)
    {
        var applicant = await GetApplicantAsync();
        if (applicant is null)
        {
            HasApplicantProfile = false;
            return Page();
        }

        FirstChoice = applicant.FirstChoice;
        SecondChoice = applicant.SecondChoice;

        var existingAttempt = await _context.Attempts
            .Where(a => a.ApplicantId == applicant.Id)
            .OrderByDescending(a => a.DateCreated)
            .FirstOrDefaultAsync();

        AlreadyAttempted = existingAttempt is not null;
        if (AlreadyAttempted)
        {
            SectionName = existingAttempt!.Section;
            ClearProgress(applicant.Id);
            return Page();
        }

        await LoadExamPagesAsync(applicant);

        if (TotalPages == 0)
        {
            return Page();
        }

        CurrentPage = Math.Clamp(pageNum, 1, TotalPages);
        CurrentPageQuestions = Pages[CurrentPage - 1];
        ApplyCurrentPageText();

        var progress = LoadProgress(applicant.Id);
        RehydrateCurrentPageAnswers(progress);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int pageNum, string direction)
    {
        var applicant = await GetApplicantAsync();
        if (applicant is null)
        {
            HasApplicantProfile = false;
            return Page();
        }

        FirstChoice = applicant.FirstChoice;
        SecondChoice = applicant.SecondChoice;

        if (await _context.Attempts.AnyAsync(a => a.ApplicantId == applicant.Id))
        {
            AlreadyAttempted = true;
            return Page();
        }

        await LoadExamPagesAsync(applicant);

        if (TotalPages == 0)
        {
            return Page();
        }

        CurrentPage = Math.Clamp(pageNum, 1, TotalPages);
        CurrentPageQuestions = Pages[CurrentPage - 1];
        ApplyCurrentPageText();

        var progress = LoadProgress(applicant.Id);
        foreach (var question in CurrentPageQuestions)
        {
            if (Answers.TryGetValue(question.Id, out var choiceId) && choiceId != Guid.Empty)
            {
                progress.Choices[question.Id] = choiceId;
            }

            if (AnswerTexts.TryGetValue(question.Id, out var text) && !string.IsNullOrWhiteSpace(text))
            {
                progress.Texts[question.Id] = text;
            }
        }

        SaveProgress(applicant.Id, progress);

        if (direction == "prev")
        {
            return RedirectToPage(new { pageNum = CurrentPage - 1 });
        }

        var currentPageComplete = CurrentPageQuestions.All(q =>
            progress.Choices.ContainsKey(q.Id) || progress.Texts.ContainsKey(q.Id));

        if (direction == "next")
        {
            if (!currentPageComplete)
            {
                ModelState.AddModelError(string.Empty, "Please answer every question on this page before continuing.");
                RehydrateCurrentPageAnswers(progress);
                return Page();
            }

            return RedirectToPage(new { pageNum = CurrentPage + 1 });
        }

        if (direction == "submit")
        {
            if (CurrentPage != TotalPages)
            {
                return RedirectToPage(new { pageNum = CurrentPage });
            }

            var firstIncompletePage = -1;
            for (var i = 0; i < Pages.Count; i++)
            {
                if (Pages[i].Any(q => !progress.Choices.ContainsKey(q.Id) && !progress.Texts.ContainsKey(q.Id)))
                {
                    firstIncompletePage = i + 1;
                    break;
                }
            }

            if (firstIncompletePage != -1)
            {
                ExamError = "Please answer every question before submitting.";
                return RedirectToPage(new { pageNum = firstIncompletePage });
            }

            var allQuestions = Pages.SelectMany(p => p).ToList();

            var attempt = new Attempt
            {
                ApplicantId = applicant.Id,
                AcademicYear = applicant.AcademicYear,
                Section = SectionName,
                DateTaken = DateTime.UtcNow.ToString("o"),
            };

            var totalScore = 0;
            foreach (var question in allQuestions)
            {
                var answer = new Answer
                {
                    ApplicantId = applicant.Id,
                    QuestionId = question.Id,
                };

                if (QuestionTypes.RequiresChoices(question.QuestionTypeName))
                {
                    progress.Choices.TryGetValue(question.Id, out var selectedChoiceId);
                    var selectedChoice = question.Choices.FirstOrDefault(c => c.Id == selectedChoiceId);
                    answer.AnswerText = selectedChoice?.ChoiceText ?? string.Empty;

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
                }
                else
                {
                    progress.Texts.TryGetValue(question.Id, out var text);
                    answer.AnswerText = text ?? string.Empty;
                    answer.Point = 0;
                }

                attempt.Answers.Add(answer);
            }

            attempt.TotalScore = totalScore;
            _context.Attempts.Add(attempt);
            await _context.SaveChangesAsync();

            ClearProgress(applicant.Id);

            SubmittedScore = totalScore;
            SubmittedMaxScore = allQuestions.Sum(q => q.Score);

            return RedirectToPage();
        }

        return RedirectToPage(new { pageNum = CurrentPage });
    }

    private async Task<Applicant?> GetApplicantAsync()
    {
        var userId = _userManager.GetUserId(User);
        return await _context.Applicants.FirstOrDefaultAsync(a => a.UserId == userId);
    }

    private async Task LoadExamPagesAsync(Applicant applicant)
    {
        SectionName = await ResolveSectionNameAsync(applicant.FirstChoice)
            ?? await ResolveSectionNameAsync(applicant.SecondChoice);

        Pages = new List<IList<QuestionDto>>();

        if (SectionName is null)
        {
            TotalPages = 0;
            return;
        }

        var sectionName = SectionName;
        var questionTypeNames = await _context.QuestionTypes.AsNoTracking().ToDictionaryAsync(qt => qt.Id, qt => qt.Name);

        var pageSections = await _context.PageSections
            .Include(ps => ps.Question)
            .ThenInclude(q => q.Section)
            .Where(ps => ps.IsActive && ps.Question.IsActive && ps.Question.Section.Name == sectionName)
            .OrderBy(ps => ps.PageNumber)
            .ThenBy(ps => ps.Order)
            .ThenBy(ps => ps.Id)
            .ToListAsync();

        PageInstructions = new List<(string?, string?)>();

        if (pageSections.Count > 0)
        {
            var pageGroups = pageSections
                .GroupBy(ps => ps.PageNumber)
                .OrderBy(g => g.Key)
                .ToList();

            foreach (var group in pageGroups)
            {
                var questions = group.Select(ps => ps.Question).ToList();
                Pages.Add(await BuildQuestionDtosAsync(questions, questionTypeNames));

                var match = group.FirstOrDefault(ps => !string.IsNullOrWhiteSpace(ps.Instruction) || !string.IsNullOrWhiteSpace(ps.SubInstruction));
                PageInstructions.Add((match?.Instruction, match?.SubInstruction));
            }
        }
        else
        {
            var questions = await _context.Questions
                .Include(q => q.Section)
                .Where(q => q.IsActive && q.Section.Name == sectionName)
                .OrderBy(q => q.DateCreated)
                .Take(QuestionCount)
                .ToListAsync();

            Pages.Add(await BuildQuestionDtosAsync(questions, questionTypeNames));
            PageInstructions.Add((null, null));
        }

        TotalPages = Pages.Count;
    }

    private async Task<IList<QuestionDto>> BuildQuestionDtosAsync(IList<Question> questions, IReadOnlyDictionary<int, string> questionTypeNames)
    {
        var dtos = questions.Select(q => new QuestionDto
        {
            Id = q.Id,
            QuestionTypeId = q.QuestionTypeId,
            QuestionTypeName = questionTypeNames.GetValueOrDefault(q.QuestionTypeId, string.Empty),
            SectionId = q.SectionId,
            QuestionTitle = q.QuestionTitle,
            Image = q.Image,
            Score = q.Score,
            IsUpToEvaluation = q.IsUpToEvaluation,
            IsActive = q.IsActive,
        }).ToList();

        foreach (var dto in dtos)
        {
            dto.Choices = await _choiceService.GetChoicesAsync(dto.Id);
        }

        return dtos;
    }

    private async Task<string?> ResolveSectionNameAsync(string? choice)
    {
        if (string.IsNullOrWhiteSpace(choice))
        {
            return null;
        }

        var trimmedChoice = choice.Trim();

        var hasQuestions = await _context.Questions
            .AnyAsync(q => q.IsActive && q.Section.IsActive && q.Section.Name.ToLower() == trimmedChoice.ToLower());

        if (!hasQuestions)
        {
            return null;
        }

        return await _context.Sections
            .Where(s => s.IsActive && s.Name.ToLower() == trimmedChoice.ToLower())
            .Select(s => s.Name)
            .FirstAsync();
    }

    private void ApplyCurrentPageText()
    {
        var instruction = PageInstructions.Count >= CurrentPage ? PageInstructions[CurrentPage - 1] : (null, null);

        CurrentPageHeading = string.IsNullOrWhiteSpace(instruction.Item1) ? "Multiple Choice" : instruction.Item1!;
        CurrentPageSubheading = string.IsNullOrWhiteSpace(instruction.Item2) ? "Select one answer for each question, then continue." : instruction.Item2!;
    }

    private void RehydrateCurrentPageAnswers(ExamProgress progress)
    {
        foreach (var question in CurrentPageQuestions)
        {
            if (progress.Choices.TryGetValue(question.Id, out var choiceId))
            {
                Answers[question.Id] = choiceId;
            }

            if (progress.Texts.TryGetValue(question.Id, out var text))
            {
                AnswerTexts[question.Id] = text;
            }
        }
    }

    private static string SessionKey(int applicantId) => $"examProgress:{applicantId}";

    private ExamProgress LoadProgress(int applicantId)
    {
        var json = HttpContext.Session.GetString(SessionKey(applicantId));
        if (string.IsNullOrEmpty(json))
        {
            return new ExamProgress();
        }

        return JsonSerializer.Deserialize<ExamProgress>(json) ?? new ExamProgress();
    }

    private void SaveProgress(int applicantId, ExamProgress progress)
    {
        HttpContext.Session.SetString(SessionKey(applicantId), JsonSerializer.Serialize(progress));
    }

    private void ClearProgress(int applicantId)
    {
        HttpContext.Session.Remove(SessionKey(applicantId));
    }

    private sealed class ExamProgress
    {
        public Dictionary<Guid, Guid> Choices { get; set; } = new();

        public Dictionary<Guid, string> Texts { get; set; } = new();
    }
}
