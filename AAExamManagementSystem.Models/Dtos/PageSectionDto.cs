namespace AAExamManagementSystem.Models.Dtos;

public class PageSectionDto
{
    public int PageSectionId { get; set; }
    public int ExamPageId { get; set; }
    public Guid QuestionId { get; set; }
    public string QuestionTitle { get; set; } = string.Empty;
    public int QuestionTypeId { get; set; }
    public string QuestionTypeName { get; set; } = string.Empty;
    public int SectionId { get; set; }
    public int Score { get; set; }
    public bool IsUpToEvaluation { get; set; }
    public int PageNumber { get; set; }
    public int SectionOrder { get; set; }
    public int Order { get; set; }
    public string? Instruction { get; set; }
    public string? SubInstruction { get; set; }
}
