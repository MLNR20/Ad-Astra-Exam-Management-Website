using AAExamManagementSystem.Models.Entities;
using AAExamManagementSystem.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AAExamManagementSystem.Pages
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IGenericRepository<Announcement> _announcementRepository;

        public IndexModel(UserManager<ApplicationUser> userManager, IGenericRepository<Announcement> announcementRepository)
        {
            _userManager = userManager;
            _announcementRepository = announcementRepository;
        }

        public List<Announcement> Announcements { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user is not null && await _userManager.IsInRoleAsync(user, AAExamManagementSystem.Models.Entities.Roles.Applicant))
                {
                    return RedirectToPage("/ApplicantPortal");
                }

                return RedirectToPage("/Dashboard");
            }

            var announcements = await _announcementRepository.GetAllAsync();
            Announcements = announcements
                .Where(a => a.IsActive)
                .OrderByDescending(a => a.CreatedAt)
                .ToList();

            return Page();
        }
    }
}
