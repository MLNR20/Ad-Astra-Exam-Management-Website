using System.ComponentModel.DataAnnotations;
using AAExamManagementSystem.Models.Entities;
using AAExamManagementSystem.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AAExamManagementSystem.Pages
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;

        public LoginModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ApplicationDbContext db)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _db = db;
        }

        [BindProperty]
        public LoginInput Input { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = Input.UserName.Contains('@')
                ? await _userManager.FindByEmailAsync(Input.UserName)
                : await _userManager.FindByNameAsync(Input.UserName);

            if (user is null)
            {
                await LogLoginAttemptAsync(null, Input.UserName, success: false, "User not found.");
                ModelState.AddModelError(string.Empty, "Invalid username/email or password.");
                return Page();
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!, Input.Password, Input.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                await LogLoginAttemptAsync(user, Input.UserName, success: true, "Login succeeded.");

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return LocalRedirect(returnUrl);
                }

                if (await _userManager.IsInRoleAsync(user, AAExamManagementSystem.Models.Entities.Roles.Applicant))
                {
                    return RedirectToPage("/ApplicantPortal");
                }

                return RedirectToPage("/Index");
            }

            if (result.IsLockedOut)
            {
                await LogLoginAttemptAsync(user, Input.UserName, success: false, "Account locked out.");
                ModelState.AddModelError(string.Empty, "This account has been locked out. Please try again later.");
            }
            else
            {
                await LogLoginAttemptAsync(user, Input.UserName, success: false, "Invalid password.");
                ModelState.AddModelError(string.Empty, "Invalid username/email or password.");
            }

            return Page();
        }

        private async Task LogLoginAttemptAsync(ApplicationUser? user, string attemptedUserName, bool success, string reason)
        {
            var auditLog = new AuditLog
            {
                Type = "Login",
                ResponseBody = success
                    ? $"Login succeeded for '{user!.UserName}'."
                    : $"Login failed for '{attemptedUserName}': {reason}",
                IsActive = success,
                CreatedById = user?.Id,
            };

            _db.AuditLogs.Add(auditLog);
            await _db.SaveChangesAsync();
        }

        public class LoginInput
        {
            [Required]
            [Display(Name = "Username or email")]
            public string UserName { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [Display(Name = "Remember me")]
            public bool RememberMe { get; set; }
        }
    }
}
