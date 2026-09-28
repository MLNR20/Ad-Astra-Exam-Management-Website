namespace AAExamManagementSystem.Models.Dtos;

public class AuditLogDto
{
    public Guid AuditId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? ResponseBody { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime DateUpdated { get; set; }
    public bool IsActive { get; set; } = true;
    public string? CreatedByName { get; set; }
}
