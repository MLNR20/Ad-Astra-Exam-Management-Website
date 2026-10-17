using AppRoles = AAExamManagementSystem.Models.Entities.Roles;
using AAExamManagementSystem.Models.Dtos;
using AAExamManagementSystem.Models.Entities;
using AAExamManagementSystem.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AAExamManagementSystem.Pages.PageSections;

[Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Instructor},{AppRoles.Staffer},{AppRoles.Editor}")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public IndexModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [BindProperty(SupportsGet = true)]
    public int? SectionId { get; set; }

    public bool IsAdmin { get; set; }

    public bool RequiresSectionSelection { get; set; }

    public bool NoDepartmentAssigned { get; set; }

    public IList<Section> SelectableSections { get; set; } = new List<Section>();

    public string CurrentSectionName { get; set; } = string.Empty;

    public List<IGrouping<int, PageSectionDto>> Pages { get; set; } = new();

    public IList<PageSectionDto> UnassignedQuestions { get; set; } = new List<PageSectionDto>();

    public SelectList QuestionTypeOptions { get; set; } = new(new List<QuestionType>(), "Id", "Name");

    public async Task<IActionResult> OnGetAsync()
    {
        IsAdmin = User.IsInRole(AppRoles.Admin);

        var allSections = await _context.Sections
            .AsNoTracking()
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync();

        int? effectiveSectionId;

        if (IsAdmin)
        {
            SelectableSections = allSections;

            if (SectionId is null)
            {
                RequiresSectionSelection = true;
                return Page();
            }

            effectiveSectionId = SectionId;
        }
        else
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.SectionId is null)
            {
                NoDepartmentAssigned = true;
                return Page();
            }

            effectiveSectionId = currentUser.SectionId;
        }

        var section = allSections.FirstOrDefault(s => s.Id == effectiveSectionId);
        if (section is null)
        {
            NoDepartmentAssigned = true;
            return Page();
        }

        CurrentSectionName = section.Name;
        SectionId = section.Id;

        var questionTypes = await _context.QuestionTypes
            .AsNoTracking()
            .Where(qt => qt.IsActive)
            .OrderBy(qt => qt.Name)
            .ToListAsync();
        var typeNamesById = questionTypes.ToDictionary(qt => qt.Id, qt => qt.Name);
        QuestionTypeOptions = new SelectList(questionTypes, "Id", "Name");

        var examPages = await _context.ExamPages
            .AsNoTracking()
            .Include(p => p.PageSections)
            .ThenInclude(ps => ps.Questions)
            .Where(p => p.IsActive && p.SectionId == section.Id)
            .ToListAsync();

        var pageSectionDtos = examPages
            .SelectMany(p => p.PageSections
                .Where(ps => ps.IsActive)
                .SelectMany(ps => ps.Questions
                    .Where(q => q.IsActive)
                    .Select(q => new PageSectionDto
                    {
                        PageSectionId = ps.Id,
                        ExamPageId = p.Id,
                        QuestionId = q.Id,
                        QuestionTitle = q.QuestionTitle,
                        QuestionTypeId = ps.QuestionTypeId,
                        QuestionTypeName = typeNamesById.GetValueOrDefault(ps.QuestionTypeId, "—"),
                        SectionId = q.SectionId,
                        Score = q.Score,
                        IsUpToEvaluation = q.IsUpToEvaluation,
                        PageNumber = p.PageNumber,
                        SectionOrder = ps.Order,
                        Order = q.PageOrder,
                        Instruction = p.Instruction,
                        SubInstruction = p.SubInstruction,
                    })))
            .OrderBy(d => d.PageNumber)
            .ThenBy(d => d.SectionOrder)
            .ThenBy(d => d.PageSectionId)
            .ThenBy(d => d.Order)
            .ToList();

        Pages = pageSectionDtos.GroupBy(d => d.PageNumber).OrderBy(g => g.Key).ToList();

        var unassigned = await _context.Questions
            .AsNoTracking()
            .Where(q => q.IsActive && q.SectionId == section.Id && q.PageSectionId == null)
            .OrderBy(q => q.DateCreated)
            .ToListAsync();

        UnassignedQuestions = unassigned
            .Select(q => new PageSectionDto
            {
                QuestionId = q.Id,
                QuestionTitle = q.QuestionTitle,
                QuestionTypeId = q.QuestionTypeId,
                QuestionTypeName = typeNamesById.GetValueOrDefault(q.QuestionTypeId, "—"),
                SectionId = q.SectionId,
                Score = q.Score,
                IsUpToEvaluation = q.IsUpToEvaluation,
            })
            .ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostBundleAsync(int questionTypeId, int pageNumber)
    {
        var effectiveSectionId = await ResolveEffectiveSectionIdAsync();
        if (effectiveSectionId is null)
        {
            return Forbid();
        }

        if (pageNumber < 1)
        {
            pageNumber = 1;
        }

        var matchingQuestions = await _context.Questions
            .Where(q => q.IsActive && q.SectionId == effectiveSectionId.Value && q.QuestionTypeId == questionTypeId)
            .OrderBy(q => q.DateCreated)
            .ToListAsync();

        if (matchingQuestions.Count == 0)
        {
            TempData["ErrorMessage"] = "No active questions of that type were found in this section.";
            return RedirectToPage(new { SectionId = effectiveSectionId });
        }

        var examPage = await _context.ExamPages
            .Include(p => p.PageSections)
            .FirstOrDefaultAsync(p => p.SectionId == effectiveSectionId.Value && p.PageNumber == pageNumber);

        if (examPage is null)
        {
            examPage = new ExamPage { SectionId = effectiveSectionId.Value, PageNumber = pageNumber };
            _context.ExamPages.Add(examPage);
        }

        var pageSection = examPage.PageSections.FirstOrDefault(ps => ps.QuestionTypeId == questionTypeId);
        if (pageSection is null)
        {
            pageSection = new PageSection
            {
                QuestionTypeId = questionTypeId,
                Order = (examPage.PageSections.Max(ps => (int?)ps.Order) ?? 0) + 1,
            };
            examPage.PageSections.Add(pageSection);
        }

        var orderedQuestions = pageSection.Id == 0
            ? new List<Question>()
            : await _context.Questions
                .Where(q => q.PageSectionId == pageSection.Id)
                .OrderBy(q => q.PageOrder)
                .ThenBy(q => q.DateCreated)
                .ToListAsync();

        var alreadyInSection = orderedQuestions.Select(q => q.Id).ToHashSet();
        foreach (var question in matchingQuestions.Where(q => !alreadyInSection.Contains(q.Id)))
        {
            question.PageSection = pageSection;
            orderedQuestions.Add(question);
        }

        for (var i = 0; i < orderedQuestions.Count; i++)
        {
            orderedQuestions[i].PageOrder = i + 1;
        }

        var now = DateTime.UtcNow;
        examPage.DateUpdated = now;
        pageSection.DateUpdated = now;

        await _context.SaveChangesAsync();
        await RemoveEmptyPagesAndSectionsAsync(effectiveSectionId.Value);

        TempData["SuccessMessage"] = $"Bundled {matchingQuestions.Count} question(s) onto page {pageNumber}.";
        return RedirectToPage(new { SectionId = effectiveSectionId });
    }

    public async Task<IActionResult> OnPostReorderAsync(int pageNumber, Dictionary<Guid, int> order)
    {
        var effectiveSectionId = await ResolveEffectiveSectionIdAsync();
        if (effectiveSectionId is null)
        {
            return Forbid();
        }

        var ids = order.Keys.ToList();
        var questions = await _context.Questions
            .Include(q => q.PageSection)
            .Where(q => ids.Contains(q.Id)
                && q.SectionId == effectiveSectionId.Value
                && q.PageSection != null
                && q.PageSection.ExamPage.PageNumber == pageNumber)
            .ToListAsync();

        var now = DateTime.UtcNow;
        foreach (var question in questions)
        {
            if (order.TryGetValue(question.Id, out var newOrder))
            {
                question.PageOrder = newOrder;
                question.PageSection!.DateUpdated = now;
            }
        }

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Order updated.";
        return RedirectToPage(new { SectionId = effectiveSectionId });
    }

    public async Task<IActionResult> OnPostSetInstructionAsync(int pageNumber, string? instruction, string? subInstruction)
    {
        var effectiveSectionId = await ResolveEffectiveSectionIdAsync();
        if (effectiveSectionId is null)
        {
            return Forbid();
        }

        var examPage = await _context.ExamPages
            .FirstOrDefaultAsync(p => p.SectionId == effectiveSectionId.Value && p.PageNumber == pageNumber);

        if (examPage is null)
        {
            TempData["ErrorMessage"] = $"Page {pageNumber} no longer exists.";
            return RedirectToPage(new { SectionId = effectiveSectionId });
        }

        examPage.Instruction = instruction;
        examPage.SubInstruction = subInstruction;
        examPage.DateUpdated = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Instructions updated for page {pageNumber}.";
        return RedirectToPage(new { SectionId = effectiveSectionId });
    }

    public async Task<IActionResult> OnPostRemoveAsync(Guid questionId)
    {
        var effectiveSectionId = await ResolveEffectiveSectionIdAsync();
        if (effectiveSectionId is null)
        {
            return Forbid();
        }

        var question = await _context.Questions.FirstOrDefaultAsync(q => q.Id == questionId);

        if (question is null || question.PageSectionId is null)
        {
            return NotFound();
        }

        if (question.SectionId != effectiveSectionId.Value)
        {
            return Forbid();
        }

        question.PageSectionId = null;
        question.PageOrder = 0;
        await _context.SaveChangesAsync();
        await RemoveEmptyPagesAndSectionsAsync(effectiveSectionId.Value);

        TempData["SuccessMessage"] = "Question removed from its page.";
        return RedirectToPage(new { SectionId = effectiveSectionId });
    }

    private async Task RemoveEmptyPagesAndSectionsAsync(int sectionId)
    {
        var emptySections = await _context.PageSections
            .Where(ps => ps.ExamPage.SectionId == sectionId && !ps.Questions.Any())
            .ToListAsync();
        _context.PageSections.RemoveRange(emptySections);
        await _context.SaveChangesAsync();

        var emptyPages = await _context.ExamPages
            .Where(p => p.SectionId == sectionId && !p.PageSections.Any())
            .ToListAsync();
        _context.ExamPages.RemoveRange(emptyPages);
        await _context.SaveChangesAsync();
    }

    private async Task<int?> ResolveEffectiveSectionIdAsync()
    {
        if (User.IsInRole(AppRoles.Admin))
        {
            return SectionId;
        }

        var currentUser = await _userManager.GetUserAsync(User);
        return currentUser?.SectionId;
    }
}
