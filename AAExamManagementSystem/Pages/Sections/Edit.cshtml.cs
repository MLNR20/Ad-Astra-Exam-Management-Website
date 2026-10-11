using AppRoles = AAExamManagementSystem.Models.Entities.Roles;
using AAExamManagementSystem.Models.Dtos;
using AAExamManagementSystem.Models.Entities;
using AAExamManagementSystem.Repository;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AAExamManagementSystem.Pages.Sections;

[Authorize(Roles = AppRoles.Admin)]
public class EditModel : PageModel
{
    private readonly IGenericRepository<Section> _repository;
    private readonly IMapper _mapper;

    public EditModel(IGenericRepository<Section> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    [BindProperty]
    public SectionCreateUpdateDto Section { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var section = await _repository.GetByIdAsync(Id);
        if (section is null)
        {
            return NotFound();
        }

        Section = new SectionCreateUpdateDto { Name = section.Name };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var section = await _repository.GetByIdAsync(Id);
        if (section is null)
        {
            return NotFound();
        }

        section.Name = Section.Name;
        _repository.Update(section);
        await _repository.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Section '{section.Name}' updated successfully.";
        return RedirectToPage("Index");
    }
}
