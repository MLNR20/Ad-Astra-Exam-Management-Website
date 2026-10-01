using AAExamManagementSystem.Models.Dtos;
using AAExamManagementSystem.Models.Entities;
using AAExamManagementSystem.Repository;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AAExamManagementSystem.Pages.Courses;

public class EditModel : PageModel
{
    private readonly IGenericRepository<Course> _repository;
    private readonly IMapper _mapper;

    public EditModel(IGenericRepository<Course> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    [BindProperty]
    public CourseCreateUpdateDto Course { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var course = await _repository.GetByIdAsync(Id);
        if (course is null)
        {
            return NotFound();
        }

        Course = new CourseCreateUpdateDto { Name = course.Name, SchoolName = course.SchoolName, IsActive = course.IsActive };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var course = await _repository.GetByIdAsync(Id);
        if (course is null)
        {
            return NotFound();
        }

        course.Name = Course.Name;
        course.SchoolName = Course.SchoolName;
        course.IsActive = Course.IsActive;
        course.DateUpdated = DateTime.UtcNow;
        _repository.Update(course);
        await _repository.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Course '{course.Name}' updated successfully.";
        return RedirectToPage("Index");
    }
}
