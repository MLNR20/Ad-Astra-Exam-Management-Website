using AAExamManagementSystem.Models.Dtos;
using AAExamManagementSystem.Models.Entities;
using AAExamManagementSystem.Repository;
using AAExamManagementSystem.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AAExamManagementSystem.Pages.Questions;

public class IndexModel : PageModel
{
    private readonly IGenericRepository<Question> _repository;
    private readonly IGenericRepository<QuestionType> _questionTypeRepository;
    private readonly IGenericRepository<Section> _sectionRepository;
    private readonly QuestionChoiceService _choiceService;
    private readonly IMapper _mapper;

    public IndexModel(
        IGenericRepository<Question> repository,
        IGenericRepository<QuestionType> questionTypeRepository,
        IGenericRepository<Section> sectionRepository,
        QuestionChoiceService choiceService,
        IMapper mapper)
    {
        _repository = repository;
        _questionTypeRepository = questionTypeRepository;
        _sectionRepository = sectionRepository;
        _choiceService = choiceService;
        _mapper = mapper;
    }

    public IList<QuestionDto> Questions { get; set; } = new List<QuestionDto>();

    public SelectList QuestionTypeOptions { get; set; } = new(new List<QuestionType>(), "Id", "Name");
    public SelectList SectionOptions { get; set; } = new(new List<Section>(), "Id", "Name");

    [BindProperty]
    public QuestionCreateUpdateDto NewQuestion { get; set; } = new();

    public IList<int> ChoiceTypeIds { get; set; } = new List<int>();

    public bool ShowCreateModal { get; set; }

    public async Task OnGetAsync()
    {
        await LoadQuestionsAsync();
        await LoadOptionsAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        await _choiceService.NormalizeAndValidateAsync(NewQuestion, ModelState, nameof(NewQuestion));
        if (!ModelState.IsValid)
        {
            await LoadQuestionsAsync();
            await LoadOptionsAsync();
            ShowCreateModal = true;
            return Page();
        }

        var question = _mapper.Map<Question>(NewQuestion);
        await _repository.AddAsync(question);
        await _choiceService.ReplaceChoicesAsync(question, NewQuestion.Choices);
        await _repository.SaveChangesAsync();

        TempData["SuccessMessage"] = "Question created successfully.";
        return RedirectToPage("Index");
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        var question = await _repository.GetByIdAsync(id);
        if (question is null)
        {
            return NotFound();
        }

        await _choiceService.RemoveChoicesAsync(question.Id);
        _repository.Remove(question);
        await _repository.SaveChangesAsync();

        TempData["SuccessMessage"] = "Question deleted successfully.";
        return RedirectToPage("Index");
    }

    private async Task LoadQuestionsAsync()
    {
        var questions = await _repository.GetAllAsync();
        var questionTypes = await _questionTypeRepository.GetAllAsync();
        var sections = await _sectionRepository.GetAllAsync();
        var questionTypeNames = questionTypes.ToDictionary(qt => qt.Id, qt => qt.Name);
        var sectionNames = sections.ToDictionary(s => s.Id, s => s.Name);

        Questions = _mapper.Map<IList<QuestionDto>>(questions.OrderByDescending(q => q.DateCreated));
        foreach (var dto in Questions)
        {
            dto.QuestionTypeName = questionTypeNames.GetValueOrDefault(dto.QuestionTypeId, "—");
            dto.SectionName = sectionNames.GetValueOrDefault(dto.SectionId, "—");
        }
    }

    private async Task LoadOptionsAsync()
    {
        var questionTypes = await _questionTypeRepository.GetAllAsync();
        var sections = await _sectionRepository.GetAllAsync();
        QuestionTypeOptions = new SelectList(questionTypes.OrderBy(qt => qt.Name), "Id", "Name");
        SectionOptions = new SelectList(sections.OrderBy(s => s.Name), "Id", "Name");
        ChoiceTypeIds = await _choiceService.GetChoiceTypeIdsAsync();
    }
}
