using AAExamManagementSystem.Models.Dtos;
using AAExamManagementSystem.Models.Entities;
using AAExamManagementSystem.Repository;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AAExamManagementSystem.Pages.Choices;

public class IndexModel : PageModel
{
    private readonly IGenericRepository<Choice> _repository;
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public IndexModel(IGenericRepository<Choice> repository, ApplicationDbContext context, IMapper mapper)
    {
        _repository = repository;
        _context = context;
        _mapper = mapper;
    }

    public IList<ChoiceDto> Choices { get; set; } = new List<ChoiceDto>();

    [BindProperty]
    public ChoiceCreateUpdateDto NewChoice { get; set; } = new();

    public bool ShowCreateModal { get; set; }

    public async Task OnGetAsync()
    {
        await LoadChoicesAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadChoicesAsync();
            ShowCreateModal = true;
            return Page();
        }

        var choice = _mapper.Map<Choice>(NewChoice);
        await _repository.AddAsync(choice);
        await _repository.SaveChangesAsync();

        TempData["SuccessMessage"] = "Choice created successfully.";
        return RedirectToPage("Index");
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        var choice = await _repository.GetByIdAsync(id);
        if (choice is null)
        {
            return NotFound();
        }

        _repository.Remove(choice);
        await _repository.SaveChangesAsync();

        TempData["SuccessMessage"] = "Choice deleted successfully.";
        return RedirectToPage("Index");
    }

    private async Task LoadChoicesAsync()
    {
        var choices = await _context.Choices
            .Include(c => c.QuestionAndChoices)
            .ThenInclude(qc => qc.Question)
            .ThenInclude(q => q.Section)
            .OrderByDescending(c => c.DateCreated)
            .ToListAsync();

        Choices = _mapper.Map<IList<ChoiceDto>>(choices);
        foreach (var dto in Choices)
        {
            var choice = choices.First(c => c.Id == dto.Id);
            dto.SectionName = choice.QuestionAndChoices
                .Select(qc => qc.Question?.Section?.Name)
                .FirstOrDefault(name => !string.IsNullOrEmpty(name));
        }
    }
}
