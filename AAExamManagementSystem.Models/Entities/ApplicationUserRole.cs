using Microsoft.AspNetCore.Identity;

namespace AAExamManagementSystem.Models.Entities;

public class ApplicationUserRole : IdentityUserRole<string>
{
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime DateUpdated { get; set; } = DateTime.UtcNow;
}
