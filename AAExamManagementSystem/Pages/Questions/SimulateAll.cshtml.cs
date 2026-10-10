using AppRoles = AAExamManagementSystem.Models.Entities.Roles;
using AppDepartments = AAExamManagementSystem.Models.Entities.Departments;
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

[Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Editor}")]
public class SimulateAllModel : PageModel
{
    private const string SqlSchemaImagePrefix = "/images/sql-schema-";

    private readonly IGenericRepository<Question> _repository;
    private readonly IGenericRepository<QuestionType> _questionTypeRepository;
    private readonly IGenericRepository<Section> _sectionRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly QuestionChoiceService _choiceService;
    private readonly IMapper _mapper;

    public SimulateAllModel(
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
    public int PageNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int? SectionId { get; set; }

    public bool IsAdmin { get; set; }

    public bool RequiresSectionSelection { get; set; }

    public bool NoDepartmentAssigned { get; set; }

    public IList<Section> SelectableSections { get; set; } = new List<Section>();

    public string CurrentSectionName { get; set; } = string.Empty;

    public string StageTitle { get; set; } = string.Empty;

    public string StageSubtitle { get; set; } = string.Empty;

    public string StageHeading { get; set; } = "Multiple Choice Questions";

    public IList<QuestionDto> Questions { get; set; } = new List<QuestionDto>();

    public bool HasNextPage { get; set; }

    public bool HasPreviousPage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        IsAdmin = User.IsInRole(AppRoles.Admin);

        var allSections = (await _sectionRepository.GetAllAsync()).ToList();
        var selectableSections = allSections
            .Where(s => s.IsActive && s.Name != AppDepartments.GeneralKnowledge)
            .OrderBy(s => s.Name)
            .ToList();

        int? effectiveSectionId;

        if (IsAdmin)
        {
            SelectableSections = selectableSections;

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

        var section = allSections.FirstOrDefault(s => s.Id == effectiveSectionId && s.IsActive);
        if (section is null)
        {
            NoDepartmentAssigned = true;
            return Page();
        }

        CurrentSectionName = section.Name;
        SectionId = section.Id;

        var questionTypes = await _questionTypeRepository.GetAllAsync();
        var typeNamesById = questionTypes.ToDictionary(qt => qt.Id, qt => qt.Name);
        var simulatedTypeIds = questionTypes
            .Where(qt => qt.Name == QuestionTypes.MultipleChoice || qt.Name == QuestionTypes.Identification)
            .Select(qt => qt.Id)
            .ToHashSet();

        var activeQuestions = (await _repository.GetAllAsync())
            .Where(q => q.IsActive && simulatedTypeIds.Contains(q.QuestionTypeId))
            .OrderBy(q => q.DateCreated)
            .ToList();

        var departmentQuestions = _mapper.Map<IList<QuestionDto>>(
            activeQuestions.Where(q => q.SectionId == section.Id));
        var sqlQuestions = departmentQuestions
            .Where(q => q.Image != null && q.Image.StartsWith(SqlSchemaImagePrefix))
            .ToList();
        var departmentMainQuestions = departmentQuestions.Except(sqlQuestions).ToList();

        var generalKnowledgeSection = allSections.FirstOrDefault(s => s.Name == AppDepartments.GeneralKnowledge);
        var generalKnowledgeQuestions = generalKnowledgeSection is null
            ? new List<QuestionDto>()
            : _mapper.Map<IList<QuestionDto>>(
                activeQuestions.Where(q => q.SectionId == generalKnowledgeSection.Id));

        var stages = new List<(string Title, string Subtitle, string Heading, IList<QuestionDto> Questions)>
        {
            (section.Name,
                $"This section aims to gauge your familiarity in {section.Name} concepts.",
                "Multiple Choice Questions",
                departmentMainQuestions)
        };

        if (sqlQuestions.Count > 0)
        {
            stages.Add(("SQL Queries",
                "Write the SQL query that answers each question about the schema shown.",
                "SQL Query Questions",
                sqlQuestions));
        }

        if (generalKnowledgeQuestions.Count > 0)
        {
            stages.Add((AppDepartments.GeneralKnowledge,
                "Wrap up with a few general knowledge questions about Ad Astra.",
                "Multiple Choice Questions",
                generalKnowledgeQuestions));
        }

        if (PageNumber < 1)
        {
            PageNumber = 1;
        }
        else if (PageNumber > stages.Count && stages.Count > 0)
        {
            PageNumber = stages.Count;
        }

        var currentStage = stages.Count > 0 ? stages[PageNumber - 1] : (
            Title: section.Name,
            Subtitle: string.Empty,
            Heading: "Multiple Choice Questions",
            Questions: (IList<QuestionDto>)new List<QuestionDto>());

        StageTitle = currentStage.Title;
        StageSubtitle = currentStage.Subtitle;
        StageHeading = currentStage.Heading;
        Questions = currentStage.Questions;
        HasNextPage = PageNumber < stages.Count;
        HasPreviousPage = PageNumber > 1;

        foreach (var question in Questions)
        {
            question.QuestionTypeName = typeNamesById.GetValueOrDefault(question.QuestionTypeId, string.Empty);
            question.Choices = await _choiceService.GetChoicesAsync(question.Id);
        }

        return Page();
    }
}
