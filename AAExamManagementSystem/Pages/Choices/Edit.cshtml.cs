using AppRoles = AAExamManagementSystem.Models.Entities.Roles;
using AAExamManagementSystem.Models.Dtos;
using AAExamManagementSystem.Models.Entities;
using AAExamManagementSystem.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AAExamManagementSystem.Pages.Choices;

[Authorize(Roles = AppRoles.Admin)]
public class EditModel : PageModel
{
    private readonly IGenericRepository<Choice> _repository;

    public EditModel(IGenericRepository<Choice> repository)
    {
        _repository = repository;
    }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public ChoiceCreateUpdateDto Choice { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var choice = await _repository.GetByIdAsync(Id);
        if (choice is null)
        {
            return NotFound();
        }

        Choice = new ChoiceCreateUpdateDto
        {
            ChoiceText = choice.ChoiceText,
            IsCorrect = choice.IsCorrect,
            IsActive = choice.IsActive
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var choice = await _repository.GetByIdAsync(Id);
        if (choice is null)
        {
            return NotFound();
        }

        choice.ChoiceText = Choice.ChoiceText;
        choice.IsCorrect = Choice.IsCorrect;
        choice.IsActive = Choice.IsActive;
        choice.DateUpdated = DateTime.UtcNow;
        _repository.Update(choice);
        await _repository.SaveChangesAsync();

        TempData["SuccessMessage"] = "Choice updated successfully.";
        return RedirectToPage("Index");
    }
}
