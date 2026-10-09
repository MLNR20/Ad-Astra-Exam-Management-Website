using AAExamManagementSystem.Models.Entities;
using AAExamManagementSystem.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AAExamManagementSystem.Pages
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;

        public LogoutModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ApplicationDbContext db)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _db = db;
        }

        public IActionResult OnGet()
        {
            return RedirectToPage("/Login");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            await _signInManager.SignOutAsync();

            if (user is not null)
            {
                _db.AuditLogs.Add(new AuditLog
                {
                    Type = "Logout",
                    ResponseBody = $"Logout succeeded for '{user.UserName}'.",
                    IsActive = true,
                    CreatedById = user.Id,
                });
                await _db.SaveChangesAsync();
            }

            return RedirectToPage("/Login");
        }
    }
}
