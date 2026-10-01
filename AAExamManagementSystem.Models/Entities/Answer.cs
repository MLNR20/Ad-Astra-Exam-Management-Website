namespace AAExamManagementSystem.Models.Entities;

public class Answer
{
    public int Id { get; set; }
    public int ApplicantId { get; set; }
    public Applicant Applicant { get; set; } = null!;
    public int AttemptId { get; set; }
    public Attempt Attempt { get; set; } = null!;
    public string? CheckedBy { get; set; }
    public Guid QuestionId { get; set; }
    public Question Question { get; set; } = null!;
    public string AnswerText { get; set; } = string.Empty;
    public string? IsCorrect { get; set; }
    public int Point { get; set; }
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public DateTime DateUpdated { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}
