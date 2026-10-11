using AppRoles = AAExamManagementSystem.Models.Entities.Roles;
using AAExamManagementSystem.Models.Dtos;
using AAExamManagementSystem.Models.Entities;
using AAExamManagementSystem.Repository;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AAExamManagementSystem.Pages.Courses;

[Authorize(Roles = AppRoles.Admin)]
public class DetailsModel : PageModel
{
    private readonly IGenericRepository<Course> _repository;
    private readonly IMapper _mapper;

    public DetailsModel(IGenericRepository<Course> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public CourseDto Course { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var course = await _repository.GetByIdAsync(id);
        if (course is null)
        {
            return NotFound();
        }

        Course = _mapper.Map<CourseDto>(course);
        return Page();
    }
}
