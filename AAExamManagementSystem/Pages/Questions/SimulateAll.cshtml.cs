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
    private readonly IGenericRepository<QuestionType> _questionTypeRepository;
    private readonly QuestionChoiceService _choiceService;
    private readonly IMapper _mapper;

    public SimulateAllModel(
        IGenericRepository<Question> repository,
        IGenericRepository<QuestionType> questionTypeRepository,
        QuestionChoiceService choiceService,
        IMapper mapper)
    {
        _repository = repository;
        _questionTypeRepository = questionTypeRepository;
        _choiceService = choiceService;
        _mapper = mapper;
    }

    public IList<QuestionDto> Questions { get; set; } = new List<QuestionDto>();

    public async Task<IActionResult> OnGetAsync()
    {
        var questionTypes = await _questionTypeRepository.GetAllAsync();
        var typeNamesById = questionTypes.ToDictionary(qt => qt.Id, qt => qt.Name);
        var simulatedTypeIds = questionTypes
            .Where(qt => qt.Name == QuestionTypes.MultipleChoice || qt.Name == QuestionTypes.Identification)
            .Select(qt => qt.Id)
            .ToHashSet();

        var questions = await _repository.GetAllAsync();
        Questions = _mapper.Map<IList<QuestionDto>>(questions
            .Where(q => q.IsActive && simulatedTypeIds.Contains(q.QuestionTypeId))
            .OrderBy(q => q.DateCreated));

        foreach (var question in Questions)
        {
            question.QuestionTypeName = typeNamesById.GetValueOrDefault(question.QuestionTypeId, string.Empty);
            question.Choices = await _choiceService.GetChoicesAsync(question.Id);
        }

        return Page();
    }
}
