using System.ComponentModel.DataAnnotations;
using System.Text;
using AAExamManagementSystem.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace AAExamManagementSystem.Pages
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ForgotPasswordModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public ForgotPasswordInput Input { get; set; } = new();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);

            // No email service is configured for this project, so instead of sending an
            // email, the reset link is surfaced directly on the confirmation page.
            string? resetLink = null;
            if (user is not null && await _userManager.IsEmailConfirmedAsync(user))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

                resetLink = Url.Page(
                    "/ResetPassword",
                    pageHandler: null,
                    values: new { userId = user.Id, code = encodedToken },
                    protocol: Request.Scheme);
            }

            TempData["ResetLink"] = resetLink;
            return RedirectToPage("/ForgotPasswordConfirmation");
        }

        public class ForgotPasswordInput
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email address")]
            public string Email { get; set; } = string.Empty;
        }
    }
}
