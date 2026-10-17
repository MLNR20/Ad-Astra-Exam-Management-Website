namespace AAExamManagementSystem.Models.Entities;

public class PageSection
{
    public int Id { get; set; }
    public Guid QuestionId { get; set; }
    public Question Question { get; set; } = null!;
    public int QuestionTypeId { get; set; }
    public QuestionType QuestionType { get; set; } = null!;
    public int PageNumber { get; set; } = 1;
    public int Order { get; set; }
    public string? Instruction { get; set; }
    public string? SubInstruction { get; set; }
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public DateTime DateUpdated { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}
