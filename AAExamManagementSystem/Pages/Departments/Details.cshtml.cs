using AppRoles = AAExamManagementSystem.Models.Entities.Roles;
using AAExamManagementSystem.Models.Dtos;
using AAExamManagementSystem.Models.Entities;
using AAExamManagementSystem.Repository;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AAExamManagementSystem.Pages.Departments;

[Authorize(Roles = AppRoles.Admin)]
public class DetailsModel : PageModel
{
    private readonly IGenericRepository<Department> _repository;
    private readonly IMapper _mapper;

    public DetailsModel(IGenericRepository<Department> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public DepartmentDto Department { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(string id)
    {
        var department = await _repository.GetByIdAsync(id);
        if (department is null)
        {
            return NotFound();
        }

        Department = _mapper.Map<DepartmentDto>(department);
        return Page();
    }
}
