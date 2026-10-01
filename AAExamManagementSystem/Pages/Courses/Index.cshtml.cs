using AAExamManagementSystem.Models.Dtos;
using AAExamManagementSystem.Models.Entities;
using AAExamManagementSystem.Repository;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AAExamManagementSystem.Pages.Courses;

public class IndexModel : PageModel
{
    private readonly IGenericRepository<Course> _repository;
    private readonly IMapper _mapper;

    public IndexModel(IGenericRepository<Course> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public IList<CourseDto> Courses { get; set; } = new List<CourseDto>();

    [BindProperty]
    public CourseCreateUpdateDto NewCourse { get; set; } = new();

    public bool ShowCreateModal { get; set; }

    public async Task OnGetAsync()
    {
        await LoadCoursesAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadCoursesAsync();
            ShowCreateModal = true;
            return Page();
        }

        var course = _mapper.Map<Course>(NewCourse);
        await _repository.AddAsync(course);
        await _repository.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Course '{course.Name}' created successfully.";
        return RedirectToPage("Index");
    }

    public async Task<IActionResult> OnPostDeactivateAsync(int id)
    {
        var course = await _repository.GetByIdAsync(id);
        if (course is null)
        {
            return NotFound();
        }

        course.IsActive = false;
        course.DateUpdated = DateTime.UtcNow;
        _repository.Update(course);
        await _repository.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Course '{course.Name}' deactivated successfully.";
        return RedirectToPage("Index");
    }

    public async Task<IActionResult> OnPostActivateAsync(int id)
    {
        var course = await _repository.GetByIdAsync(id);
        if (course is null)
        {
            return NotFound();
        }

        course.IsActive = true;
        course.DateUpdated = DateTime.UtcNow;
        _repository.Update(course);
        await _repository.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Course '{course.Name}' activated successfully.";
        return RedirectToPage("Index");
    }

    private async Task LoadCoursesAsync()
    {
        var courses = await _repository.GetAllAsync();
        Courses = _mapper.Map<IList<CourseDto>>(courses.OrderBy(c => c.Name));
    }
}
