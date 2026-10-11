using AppRoles = AAExamManagementSystem.Models.Entities.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AAExamManagementSystem.Pages
{
    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Instructor},{AppRoles.Staffer},{AppRoles.Editor}")]
    public class DashboardModel : PageModel
    {
        public void OnGet()
        {

        }
    }
}
