using AAExamManagementSystem.Models.Dtos;
using AAExamManagementSystem.Models.Entities;
using AAExamManagementSystem.Repository;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AAExamManagementSystem.Pages.Departments;

public class IndexModel : PageModel
{
    private readonly IGenericRepository<Department> _repository;
    private readonly IMapper _mapper;

    public IndexModel(IGenericRepository<Department> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public IList<DepartmentDto> Departments { get; set; } = new List<DepartmentDto>();

    [BindProperty]
    public DepartmentCreateUpdateDto NewDepartment { get; set; } = new();

    public bool ShowCreateModal { get; set; }

    public async Task OnGetAsync()
    {
        await LoadDepartmentsAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadDepartmentsAsync();
            ShowCreateModal = true;
            return Page();
        }

        var department = _mapper.Map<Department>(NewDepartment);
        await _repository.AddAsync(department);
        await _repository.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Department '{department.Name}' created successfully.";
        return RedirectToPage("Index");
    }

    public async Task<IActionResult> OnPostDeactivateAsync(string id)
    {
        var department = await _repository.GetByIdAsync(id);
        if (department is null)
        {
            return NotFound();
        }

        department.IsActive = false;
        department.DateUpdated = DateTime.UtcNow;
        _repository.Update(department);
        await _repository.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Department '{department.Name}' deactivated successfully.";
        return RedirectToPage("Index");
    }

    public async Task<IActionResult> OnPostActivateAsync(string id)
    {
        var department = await _repository.GetByIdAsync(id);
        if (department is null)
        {
            return NotFound();
        }

        department.IsActive = true;
        department.DateUpdated = DateTime.UtcNow;
        _repository.Update(department);
        await _repository.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Department '{department.Name}' activated successfully.";
        return RedirectToPage("Index");
    }

    private async Task LoadDepartmentsAsync()
    {
        var departments = await _repository.GetAllAsync();
        Departments = _mapper.Map<IList<DepartmentDto>>(departments.OrderBy(d => d.Name));
    }
}
