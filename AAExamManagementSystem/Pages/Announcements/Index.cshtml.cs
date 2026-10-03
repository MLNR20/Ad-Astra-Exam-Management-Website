using AAExamManagementSystem.Models.Dtos;
using AAExamManagementSystem.Models.Entities;
using AAExamManagementSystem.Repository;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AAExamManagementSystem.Pages.Announcements;

public class IndexModel : PageModel
{
    private readonly IGenericRepository<Announcement> _repository;
    private readonly IMapper _mapper;

    public IndexModel(IGenericRepository<Announcement> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public IList<AnnouncementDto> Announcements { get; set; } = new List<AnnouncementDto>();

    [BindProperty]
    public AnnouncementCreateUpdateDto NewAnnouncement { get; set; } = new();

    public bool ShowCreateModal { get; set; }

    public async Task OnGetAsync()
    {
        await LoadAnnouncementsAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadAnnouncementsAsync();
            ShowCreateModal = true;
            return Page();
        }

        var announcement = _mapper.Map<Announcement>(NewAnnouncement);
        await _repository.AddAsync(announcement);
        await _repository.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Announcement '{announcement.Title}' created successfully.";
        return RedirectToPage("Index");
    }

    public async Task<IActionResult> OnPostDeactivateAsync(string id)
    {
        var announcement = await _repository.GetByIdAsync(id);
        if (announcement is null)
        {
            return NotFound();
        }

        announcement.IsActive = false;
        announcement.DateUpdated = DateTime.UtcNow;
        _repository.Update(announcement);
        await _repository.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Announcement '{announcement.Title}' deactivated successfully.";
        return RedirectToPage("Index");
    }

    public async Task<IActionResult> OnPostActivateAsync(string id)
    {
        var announcement = await _repository.GetByIdAsync(id);
        if (announcement is null)
        {
            return NotFound();
        }

        announcement.IsActive = true;
        announcement.DateUpdated = DateTime.UtcNow;
        _repository.Update(announcement);
        await _repository.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Announcement '{announcement.Title}' activated successfully.";
        return RedirectToPage("Index");
    }

    private async Task LoadAnnouncementsAsync()
    {
        var announcements = await _repository.GetAllAsync();
        Announcements = _mapper.Map<IList<AnnouncementDto>>(announcements.OrderByDescending(a => a.CreatedAt));
    }
}
