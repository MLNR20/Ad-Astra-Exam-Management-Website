using System.ComponentModel.DataAnnotations;

namespace AAExamManagementSystem.Models.Entities;

public class AuditLog
{
    [Key]
    public Guid AuditId { get; set; } = Guid.NewGuid();
    public string Type { get; set; } = string.Empty;
    public string? ResponseBody { get; set; }
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public DateTime DateUpdated { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public string? CreatedById { get; set; }
    public ApplicationUser? CreatedBy { get; set; }
}
