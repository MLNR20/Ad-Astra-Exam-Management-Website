using AAExamManagementSystem.Models.Dtos;
using AAExamManagementSystem.Models.Entities;
using AAExamManagementSystem.Repository;
using AAExamManagementSystem.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AAExamManagementSystem.Pages.Questions;

public class SimulateAllModel : PageModel
{
    private readonly IGenericRepository<Question> _repository;
    private readonly QuestionChoiceService _choiceService;
    private readonly IMapper _mapper;

    public SimulateAllModel(
        IGenericRepository<Question> repository,
        QuestionChoiceService choiceService,
        IMapper mapper)
    {
        _repository = repository;
        _choiceService = choiceService;
        _mapper = mapper;
    }

    public IList<QuestionDto> Questions { get; set; } = new List<QuestionDto>();

    public async Task<IActionResult> OnGetAsync()
    {
        var questions = await _repository.GetAllAsync();
        Questions = _mapper.Map<IList<QuestionDto>>(questions.Where(q => q.IsActive).OrderBy(q => q.DateCreated));

        foreach (var question in Questions)
        {
            question.Choices = await _choiceService.GetChoicesAsync(question.Id);
        }

        return Page();
    }
}
