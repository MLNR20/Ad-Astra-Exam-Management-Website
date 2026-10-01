using AAExamManagementSystem.Models.Dtos;
using AAExamManagementSystem.Models.Entities;
using AAExamManagementSystem.Repository;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AAExamManagementSystem.Pages.Departments;

public class EditModel : PageModel
{
    private readonly IGenericRepository<Department> _repository;
    private readonly IMapper _mapper;

    public EditModel(IGenericRepository<Department> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    [BindProperty(SupportsGet = true)]
    public string Id { get; set; } = string.Empty;

    [BindProperty]
    public DepartmentCreateUpdateDto Department { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var department = await _repository.GetByIdAsync(Id);
        if (department is null)
        {
            return NotFound();
        }

        Department = new DepartmentCreateUpdateDto { Name = department.Name, IsActive = department.IsActive };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var department = await _repository.GetByIdAsync(Id);
        if (department is null)
        {
            return NotFound();
        }

        department.Name = Department.Name;
        department.IsActive = Department.IsActive;
        department.DateUpdated = DateTime.UtcNow;
        _repository.Update(department);
        await _repository.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Department '{department.Name}' updated successfully.";
        return RedirectToPage("Index");
    }
}
