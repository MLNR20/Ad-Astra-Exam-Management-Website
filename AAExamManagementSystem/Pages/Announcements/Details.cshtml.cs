using AAExamManagementSystem.Models.Dtos;
using AAExamManagementSystem.Models.Entities;
using AAExamManagementSystem.Repository;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AAExamManagementSystem.Pages.Announcements;

public class DetailsModel : PageModel
{
    private readonly IGenericRepository<Announcement> _repository;
    private readonly IMapper _mapper;

    public DetailsModel(IGenericRepository<Announcement> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public AnnouncementDto Announcement { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(string id)
    {
        var announcement = await _repository.GetByIdAsync(id);
        if (announcement is null)
        {
            return NotFound();
        }

        Announcement = _mapper.Map<AnnouncementDto>(announcement);
        return Page();
    }
}
