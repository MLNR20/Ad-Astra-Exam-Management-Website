using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AAExamManagementSystem.Pages
{
    public class ForgotPasswordConfirmationModel : PageModel
    {
        public string? ResetLink { get; set; }

        public void OnGet()
        {
            ResetLink = TempData["ResetLink"] as string;
        }
    }
}
