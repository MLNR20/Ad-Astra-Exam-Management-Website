using AppRoles = AAExamManagementSystem.Models.Entities.Roles;
using AAExamManagementSystem.Models.Dtos;
using AAExamManagementSystem.Models.Entities;
using AAExamManagementSystem.Repository;
using AAExamManagementSystem.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AAExamManagementSystem.Pages.Questions;

[Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Instructor},{AppRoles.Staffer},{AppRoles.Editor}")]
public class DeleteModel : PageModel
{
    private readonly IGenericRepository<Question> _repository;
    private readonly IGenericRepository<QuestionType> _questionTypeRepository;
    private readonly IGenericRepository<Section> _sectionRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly QuestionChoiceService _choiceService;
    private readonly IMapper _mapper;

    public DeleteModel(
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

    public QuestionDto Question { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var question = await _repository.GetByIdAsync(id);
        if (question is null)
        {
            return NotFound();
        }

        if (!User.IsInRole(AppRoles.Admin))
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.SectionId is null || currentUser.SectionId != question.SectionId)
            {
                return Forbid();
            }
        }

        Question = _mapper.Map<QuestionDto>(question);
        var questionType = await _questionTypeRepository.GetByIdAsync(question.QuestionTypeId);
        var section = await _sectionRepository.GetByIdAsync(question.SectionId);
        Question.QuestionTypeName = questionType?.Name ?? "—";
        Question.SectionName = section?.Name ?? "—";
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        var question = await _repository.GetByIdAsync(id);
        if (question is null)
        {
            return NotFound();
        }

        if (!User.IsInRole(AppRoles.Admin))
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.SectionId is null || currentUser.SectionId != question.SectionId)
            {
                return Forbid();
            }
        }

        await _choiceService.RemoveChoicesAsync(question.Id);
        _repository.Remove(question);
        await _repository.SaveChangesAsync();

        TempData["SuccessMessage"] = "Question deleted successfully.";
        return RedirectToPage("Index");
    }
}
