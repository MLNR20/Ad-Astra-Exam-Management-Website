namespace AAExamManagementSystem.Models.Entities;

public class PageSection
{
    public int Id { get; set; }
    public int ExamPageId { get; set; }
    public ExamPage ExamPage { get; set; } = null!;
    public int QuestionTypeId { get; set; }
    public QuestionType QuestionType { get; set; } = null!;
    public int Order { get; set; }
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public DateTime DateUpdated { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public ICollection<Question> Questions { get; set; } = new List<Question>();
}
