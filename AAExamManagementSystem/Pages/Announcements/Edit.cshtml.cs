using AAExamManagementSystem.Models.Dtos;
using AAExamManagementSystem.Models.Entities;
using AAExamManagementSystem.Repository;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AAExamManagementSystem.Pages.Announcements;

public class EditModel : PageModel
{
    private readonly IGenericRepository<Announcement> _repository;
    private readonly IMapper _mapper;

    public EditModel(IGenericRepository<Announcement> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    [BindProperty(SupportsGet = true)]
    public string Id { get; set; } = string.Empty;

    [BindProperty]
    public AnnouncementCreateUpdateDto Announcement { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var announcement = await _repository.GetByIdAsync(Id);
        if (announcement is null)
        {
            return NotFound();
        }

        Announcement = new AnnouncementCreateUpdateDto
        {
            Title = announcement.Title,
            Subheader = announcement.Subheader,
            Body = announcement.Body,
            IsActive = announcement.IsActive
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var announcement = await _repository.GetByIdAsync(Id);
        if (announcement is null)
        {
            return NotFound();
        }

        announcement.Title = Announcement.Title;
        announcement.Subheader = Announcement.Subheader;
        announcement.Body = Announcement.Body;
        announcement.IsActive = Announcement.IsActive;
        announcement.DateUpdated = DateTime.UtcNow;
        _repository.Update(announcement);
        await _repository.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Announcement '{announcement.Title}' updated successfully.";
        return RedirectToPage("Index");
    }
}
