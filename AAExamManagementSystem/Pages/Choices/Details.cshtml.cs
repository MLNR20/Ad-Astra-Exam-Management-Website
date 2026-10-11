using AppRoles = AAExamManagementSystem.Models.Entities.Roles;
using AAExamManagementSystem.Models.Dtos;
using AAExamManagementSystem.Models.Entities;
using AAExamManagementSystem.Repository;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AAExamManagementSystem.Pages.Choices;

[Authorize(Roles = AppRoles.Admin)]
public class DetailsModel : PageModel
{
    private readonly IGenericRepository<Choice> _repository;
    private readonly IMapper _mapper;

    public DetailsModel(IGenericRepository<Choice> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public ChoiceDto Choice { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var choice = await _repository.GetByIdAsync(id);
        if (choice is null)
        {
            return NotFound();
        }

        Choice = _mapper.Map<ChoiceDto>(choice);
        return Page();
    }
}
