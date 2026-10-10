using AppRoles = AAExamManagementSystem.Models.Entities.Roles;
using AAExamManagementSystem.Models.Dtos;
using AAExamManagementSystem.Models.Entities;
using AAExamManagementSystem.Repository;
using AAExamManagementSystem.Services;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AAExamManagementSystem.Pages.Questions;

public class EditModel : PageModel
{
    private readonly IGenericRepository<Question> _repository;
    private readonly IGenericRepository<QuestionType> _questionTypeRepository;
    private readonly IGenericRepository<Section> _sectionRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly QuestionChoiceService _choiceService;
    private readonly IMapper _mapper;

    public EditModel(
        IGenericRepository<Question> repository,
        IGenericRepository<QuestionType> questionTypeRepository,
        IGenericRepository<Section> sectionRepository,
        UserManager<ApplicationUser> userManager,
        QuestionChoiceService choiceService,
        IMapper mapper)
    {
        _repository = repository;
        _questionTypeRepository = questionTypeRepository;
        _sectionRepository = sectionRepository;
        _userManager = userManager;
        _choiceService = choiceService;
        _mapper = mapper;
    }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public QuestionCreateUpdateDto Question { get; set; } = new();

    public SelectList QuestionTypeOptions { get; set; } = new(new List<QuestionType>(), "Id", "Name");
    public SelectList SectionOptions { get; set; } = new(new List<Section>(), "Id", "Name");
    public IList<int> ChoiceTypeIds { get; set; } = new List<int>();

    public bool IsAdmin { get; set; }

    public string? CurrentUserSectionName { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var question = await _repository.GetByIdAsync(Id);
        if (question is null)
        {
            return NotFound();
        }

        IsAdmin = User.IsInRole(AppRoles.Admin);
        if (!IsAdmin)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.SectionId is null || currentUser.SectionId != question.SectionId)
            {
                return Forbid();
            }
        }

        Question = new QuestionCreateUpdateDto
        {
            QuestionTypeId = question.QuestionTypeId,
            SectionId = question.SectionId,
            QuestionTitle = question.QuestionTitle,
            Image = question.Image,
            Score = question.Score,
            IsUpToEvaluation = question.IsUpToEvaluation,
            IsActive = question.IsActive,
            Choices = (await _choiceService.GetChoicesAsync(question.Id))
                .Select(c => new ChoiceCreateUpdateDto { ChoiceText = c.ChoiceText, IsCorrect = c.IsCorrect, IsActive = c.IsActive })
                .ToList()
        };
        await LoadOptionsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var question = await _repository.GetByIdAsync(Id);
        if (question is null)
        {
            return NotFound();
        }

        IsAdmin = User.IsInRole(AppRoles.Admin);
        if (!IsAdmin)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.SectionId is null || currentUser.SectionId != question.SectionId)
            {
                return Forbid();
            }

            Question.SectionId = currentUser.SectionId.Value;
        }

        await _choiceService.NormalizeAndValidateAsync(Question, ModelState, nameof(Question));
        if (!ModelState.IsValid)
        {
            await LoadOptionsAsync();
            return Page();
        }

        question.QuestionTypeId = Question.QuestionTypeId;
        question.SectionId = Question.SectionId;
        question.QuestionTitle = Question.QuestionTitle;
        question.Image = Question.Image;
        question.Score = Question.Score;
        question.IsUpToEvaluation = Question.IsUpToEvaluation;
        question.IsActive = Question.IsActive;
        question.DateUpdated = DateTime.UtcNow;
        _repository.Update(question);
        await _choiceService.ReplaceChoicesAsync(question, Question.Choices);
        await _repository.SaveChangesAsync();

        TempData["SuccessMessage"] = "Question updated successfully.";
        return RedirectToPage("Index");
    }

    private async Task LoadOptionsAsync()
    {
        var questionTypes = await _questionTypeRepository.GetAllAsync();
        var sections = await _sectionRepository.GetAllAsync();
        QuestionTypeOptions = new SelectList(questionTypes.OrderBy(qt => qt.Name), "Id", "Name");
        SectionOptions = new SelectList(sections.OrderBy(s => s.Name), "Id", "Name");
        ChoiceTypeIds = await _choiceService.GetChoiceTypeIdsAsync();

        if (!IsAdmin)
        {
            CurrentUserSectionName = sections.FirstOrDefault(s => s.Id == Question.SectionId)?.Name;
        }
    }
}
