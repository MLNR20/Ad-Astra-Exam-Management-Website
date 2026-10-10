using AppRoles = AAExamManagementSystem.Models.Entities.Roles;
using AAExamManagementSystem.Models.Dtos;
using AAExamManagementSystem.Models.Entities;
using AAExamManagementSystem.Repository;
using AAExamManagementSystem.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AAExamManagementSystem.Pages.Questions;

[Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Editor}")]
public class SimulateModel : PageModel
{
    private readonly IGenericRepository<Question> _repository;
    private readonly QuestionChoiceService _choiceService;
    private readonly IMapper _mapper;

    public SimulateModel(
        IGenericRepository<Question> repository,
        QuestionChoiceService choiceService,
        IMapper mapper)
    {
        _repository = repository;
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

        Question = _mapper.Map<QuestionDto>(question);
        Question.Choices = await _choiceService.GetChoicesAsync(question.Id);
        return Page();
    }
}
