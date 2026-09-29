using System.ComponentModel.DataAnnotations;

namespace AAExamManagementSystem.Models.Entities;

public class Notification
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Url { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public DateTime DateUpdated { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
}
