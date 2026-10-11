using AppRoles = AAExamManagementSystem.Models.Entities.Roles;
using DepartmentNames = AAExamManagementSystem.Models.Entities.Departments;
using AAExamManagementSystem.Models.Entities;
using AAExamManagementSystem.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AAExamManagementSystem.Pages.Analytics;

[Authorize(Roles = AppRoles.Admin + "," + AppRoles.Instructor + "," + AppRoles.Staffer + "," + AppRoles.Editor)]
public class IndexModel : PageModel
{
    private const int MonthsInTrend = 12;
    private const int ScoreBucketCount = 10;

    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public IndexModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public int TotalApplicants { get; private set; }
    public int ActiveApplicants { get; private set; }
    public int ScholarApplicants { get; private set; }
    public int TotalAttempts { get; private set; }
    public double? AverageScore { get; private set; }
    public int? HighestScore { get; private set; }
    public int TotalQuestions { get; private set; }
    public int TotalStaff { get; private set; }

    public bool IsAdmin { get; private set; }
    public string? CurrentUserSectionName { get; private set; }

    public IList<LabeledCount> ApplicantsPerMonth { get; private set; } = new List<LabeledCount>();
    public IList<ChoiceCount> DepartmentChoices { get; private set; } = new List<ChoiceCount>();
    public IList<LabeledCount> QuestionsPerSection { get; private set; } = new List<LabeledCount>();
    public IList<LabeledCount> QuestionsPerType { get; private set; } = new List<LabeledCount>();
    public IList<LabeledCount> ScoreDistribution { get; private set; } = new List<LabeledCount>();
    public IList<LabeledCount> StaffPerDepartment { get; private set; } = new List<LabeledCount>();
    public IList<TopAttempt> TopAttempts { get; private set; } = new List<TopAttempt>();

    public async Task OnGetAsync()
    {
        IsAdmin = User.IsInRole(AppRoles.Admin);
        int? currentUserSectionId = null;

        if (!IsAdmin)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            currentUserSectionId = currentUser?.SectionId;
            if (currentUserSectionId is not null)
            {
                CurrentUserSectionName = await _db.Sections.AsNoTracking()
                    .Where(s => s.Id == currentUserSectionId)
                    .Select(s => s.Name)
                    .FirstOrDefaultAsync();
            }
        }

        var scopedToSection = !IsAdmin && CurrentUserSectionName is not null;
        var noSectionAssigned = !IsAdmin && CurrentUserSectionName is null;

        var applicants = _db.Applicants.AsNoTracking().AsQueryable();
        var attempts = _db.Attempts.AsNoTracking().Where(a => a.IsActive);
        var questions = _db.Questions.AsNoTracking().Where(q => q.IsActive);
        var staff = _db.Users.AsNoTracking().Where(u => u.Applicant == null);

        if (noSectionAssigned)
        {
            applicants = applicants.Where(a => false);
            attempts = attempts.Where(a => false);
            questions = questions.Where(q => false);
            staff = staff.Where(u => false);
        }
        else if (scopedToSection)
        {
            applicants = applicants.Where(a => a.FirstChoice == CurrentUserSectionName || a.SecondChoice == CurrentUserSectionName);
            attempts = attempts.Where(a => a.Section == CurrentUserSectionName);
            questions = questions.Where(q => q.SectionId == currentUserSectionId);
            staff = staff.Where(u => u.SectionId == currentUserSectionId);
        }

        TotalApplicants = await applicants.CountAsync();
        ActiveApplicants = await applicants.CountAsync(a => a.IsActive);
        ScholarApplicants = await applicants.CountAsync(a => a.IsScholar);
        TotalAttempts = await attempts.CountAsync();
        AverageScore = await attempts.AverageAsync(a => (double?)a.TotalScore);
        HighestScore = await attempts.MaxAsync(a => (int?)a.TotalScore);
        TotalQuestions = await questions.CountAsync();
        TotalStaff = await staff.CountAsync();

        await LoadApplicantTrendAsync(applicants);
        await LoadDepartmentChoicesAsync(applicants, scopedToSection);
        await LoadQuestionBreakdownAsync(IsAdmin, currentUserSectionId);
        await LoadScoreDistributionAsync(attempts);

        // Staff-per-department breaks down by the separate Department entity, not Section — there is no
        // meaningful "my department" slice for a section-scoped editor, so admins only.
        if (IsAdmin)
        {
            await LoadStaffPerDepartmentAsync();
        }

        TopAttempts = await attempts
            .OrderByDescending(a => a.TotalScore)
            .ThenBy(a => a.DateCreated)
            .Take(5)
            .Select(a => new TopAttempt(
                a.Applicant.FirstName + " " + a.Applicant.LastName,
                a.Applicant.StudentNo,
                a.Applicant.FirstChoice,
                a.TotalScore,
                a.DateCreated))
            .ToListAsync();
    }

    private async Task LoadApplicantTrendAsync(IQueryable<Applicant> applicants)
    {
        var now = DateTime.UtcNow;
        var start = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-(MonthsInTrend - 1));

        var counts = await applicants
            .Where(a => a.DateCreated >= start)
            .GroupBy(a => new { a.DateCreated.Year, a.DateCreated.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
            .ToListAsync();

        ApplicantsPerMonth = Enumerable.Range(0, MonthsInTrend)
            .Select(i => start.AddMonths(i))
            .Select(m => new LabeledCount(
                m.ToString("MMM yyyy"),
                counts.FirstOrDefault(c => c.Year == m.Year && c.Month == m.Month)?.Count ?? 0))
            .ToList();
    }

    private async Task LoadDepartmentChoicesAsync(IQueryable<Applicant> applicants, bool scopedToSection)
    {
        var first = await applicants
            .Where(a => a.FirstChoice != null && a.FirstChoice != "")
            .GroupBy(a => a.FirstChoice!)
            .Select(g => new { Name = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Name, x => x.Count);

        var second = await applicants
            .Where(a => a.SecondChoice != null && a.SecondChoice != "")
            .GroupBy(a => a.SecondChoice!)
            .Select(g => new { Name = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Name, x => x.Count);

        var departmentNames = scopedToSection && CurrentUserSectionName is not null
            ? new[] { CurrentUserSectionName }
            : DepartmentNames.All;

        DepartmentChoices = departmentNames
            .Union(first.Keys)
            .Union(second.Keys)
            .Select(name => new ChoiceCount(name, first.GetValueOrDefault(name), second.GetValueOrDefault(name)))
            .OrderByDescending(c => c.FirstChoice + c.SecondChoice)
            .ThenBy(c => c.Name)
            .ToList();
    }

    private async Task LoadQuestionBreakdownAsync(bool isAdmin, int? currentUserSectionId)
    {
        // EF can't translate ordering on a constructor-projected record, so sort an anonymous projection in SQL
        // and build LabeledCount in memory.
        var sectionsQuery = _db.Sections.AsNoTracking().Where(s => s.IsActive);
        if (!isAdmin)
        {
            sectionsQuery = sectionsQuery.Where(s => s.Id == currentUserSectionId);
        }

        var perSection = await sectionsQuery
            .Select(s => new { s.Name, Count = s.Questions.Count(q => q.IsActive) })
            .OrderByDescending(x => x.Count)
            .ToListAsync();
        QuestionsPerSection = perSection.Select(x => new LabeledCount(x.Name, x.Count)).ToList();

        var perType = await _db.QuestionTypes.AsNoTracking()
            .Where(t => t.IsActive)
            .Select(t => new
            {
                t.Name,
                Count = t.Questions.Count(q => q.IsActive && (isAdmin || q.SectionId == currentUserSectionId))
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync();
        QuestionsPerType = perType.Select(x => new LabeledCount(x.Name, x.Count)).ToList();
    }

    private async Task LoadScoreDistributionAsync(IQueryable<Attempt> attempts)
    {
        var scores = await attempts
            .Select(a => a.TotalScore)
            .ToListAsync();

        if (scores.Count == 0)
        {
            ScoreDistribution = new List<LabeledCount>();
            return;
        }

        var max = scores.Max();
        var width = Math.Max(1, (int)Math.Ceiling((max + 1) / (double)ScoreBucketCount));
        var buckets = new List<LabeledCount>();
        for (var low = 0; low <= max; low += width)
        {
            var high = low + width - 1;
            var label = width == 1 ? low.ToString() : $"{low}–{high}";
            buckets.Add(new LabeledCount(label, scores.Count(s => s >= low && s <= high)));
        }

        ScoreDistribution = buckets;
    }

    private async Task LoadStaffPerDepartmentAsync()
    {
        var perDepartment = await _db.Departments.AsNoTracking()
            .Where(d => d.IsActive)
            .Select(d => new { d.Name, Count = d.Users.Count })
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.Name)
            .ToListAsync();
        StaffPerDepartment = perDepartment.Select(x => new LabeledCount(x.Name, x.Count)).ToList();
    }

    public record LabeledCount(string Label, int Count);

    public record ChoiceCount(string Name, int FirstChoice, int SecondChoice);

    public record TopAttempt(string ApplicantName, string StudentNo, string? FirstChoice, int Score, DateTime DateTaken);
}
