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

        var sectionQuestions = await _context.Questions
            .AsNoTracking()
            .Where(q => q.IsActive && q.SectionId == section.Id)
            .ToListAsync();

        var pageSectionEntities = await _context.PageSections
            .AsNoTracking()
            .Include(ps => ps.Question)
            .Where(ps => ps.IsActive && ps.Question.IsActive && ps.Question.SectionId == section.Id)
            .OrderBy(ps => ps.PageNumber)
            .ThenBy(ps => ps.Order)
            .ThenBy(ps => ps.Id)
            .ToListAsync();

        var pageSectionDtos = pageSectionEntities.Select(ps => new PageSectionDto
        {
            Id = ps.Id,
            QuestionId = ps.QuestionId,
            QuestionTitle = ps.Question.QuestionTitle,
            QuestionTypeId = ps.QuestionTypeId,
            QuestionTypeName = typeNamesById.GetValueOrDefault(ps.QuestionTypeId, "—"),
            SectionId = ps.Question.SectionId,
            Score = ps.Question.Score,
            IsUpToEvaluation = ps.Question.IsUpToEvaluation,
            PageNumber = ps.PageNumber,
            Order = ps.Order,
            Instruction = ps.Instruction,
            SubInstruction = ps.SubInstruction,
        }).ToList();

        Pages = pageSectionDtos.GroupBy(p => p.PageNumber).OrderBy(g => g.Key).ToList();

        var assignedQuestionIds = pageSectionEntities.Select(ps => ps.QuestionId).ToHashSet();
        UnassignedQuestions = sectionQuestions
            .Where(q => !assignedQuestionIds.Contains(q.Id))
            .OrderBy(q => q.DateCreated)
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

        var questionIds = matchingQuestions.Select(q => q.Id).ToHashSet();

        var currentPageRows = await _context.PageSections
            .Include(ps => ps.Question)
            .Where(ps => ps.PageNumber == pageNumber && ps.Question.SectionId == effectiveSectionId.Value)
            .OrderBy(ps => ps.Order)
            .ThenBy(ps => ps.Id)
            .ToListAsync();

        var existingForBundle = await _context.PageSections
            .Where(ps => questionIds.Contains(ps.QuestionId))
            .ToListAsync();
        var existingByQuestionId = existingForBundle.ToDictionary(ps => ps.QuestionId);

        var orderedTargetRows = new List<PageSection>(currentPageRows);

        foreach (var question in matchingQuestions)
        {
            if (existingByQuestionId.TryGetValue(question.Id, out var row))
            {
                if (row.PageNumber != pageNumber)
                {
                    row.PageNumber = pageNumber;
                    row.QuestionTypeId = questionTypeId;
                    row.DateUpdated = DateTime.UtcNow;
                    orderedTargetRows.Add(row);
                }
                // Already sitting on this page - keep its existing slot in currentPageRows.
            }
            else
            {
                var newRow = new PageSection
                {
                    QuestionId = question.Id,
                    QuestionTypeId = questionTypeId,
                    PageNumber = pageNumber,
                };
                _context.PageSections.Add(newRow);
                orderedTargetRows.Add(newRow);
            }
        }

        for (var i = 0; i < orderedTargetRows.Count; i++)
        {
            orderedTargetRows[i].Order = i + 1;
        }

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Bundled {matchingQuestions.Count} question(s) onto page {pageNumber}.";
        return RedirectToPage(new { SectionId = effectiveSectionId });
    }

    public async Task<IActionResult> OnPostReorderAsync(int pageNumber, Dictionary<int, int> order)
    {
        var effectiveSectionId = await ResolveEffectiveSectionIdAsync();
        if (effectiveSectionId is null)
        {
            return Forbid();
        }

        var ids = order.Keys.ToList();
        var rows = await _context.PageSections
            .Include(ps => ps.Question)
            .Where(ps => ids.Contains(ps.Id) && ps.PageNumber == pageNumber && ps.Question.SectionId == effectiveSectionId.Value)
            .ToListAsync();

        foreach (var row in rows)
        {
            if (order.TryGetValue(row.Id, out var newOrder))
            {
                row.Order = newOrder;
                row.DateUpdated = DateTime.UtcNow;
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

        var rows = await _context.PageSections
            .Include(ps => ps.Question)
            .Where(ps => ps.PageNumber == pageNumber && ps.Question.SectionId == effectiveSectionId.Value)
            .ToListAsync();

        foreach (var row in rows)
        {
            row.Instruction = instruction;
            row.SubInstruction = subInstruction;
            row.DateUpdated = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Instructions updated for page {pageNumber}.";
        return RedirectToPage(new { SectionId = effectiveSectionId });
    }

    public async Task<IActionResult> OnPostRemoveAsync(int id)
    {
        var effectiveSectionId = await ResolveEffectiveSectionIdAsync();
        if (effectiveSectionId is null)
        {
            return Forbid();
        }

        var row = await _context.PageSections
            .Include(ps => ps.Question)
            .FirstOrDefaultAsync(ps => ps.Id == id);

        if (row is null)
        {
            return NotFound();
        }

        if (row.Question.SectionId != effectiveSectionId.Value)
        {
            return Forbid();
        }

        _context.PageSections.Remove(row);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Question removed from its page.";
        return RedirectToPage(new { SectionId = effectiveSectionId });
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
