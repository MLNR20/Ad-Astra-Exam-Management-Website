namespace AAExamManagementSystem.Models.Entities;

public class Question
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int QuestionTypeId { get; set; }
    public QuestionType QuestionType { get; set; } = null!;
    public int SectionId { get; set; }
    public Section Section { get; set; } = null!;
    public int? PageSectionId { get; set; }
    public PageSection? PageSection { get; set; }
    public int PageOrder { get; set; }
    public string QuestionTitle { get; set; } = string.Empty;
    public string? Image { get; set; }
    public int Score { get; set; } = 1;
    public bool IsUpToEvaluation { get; set; }
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public DateTime DateUpdated { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public ICollection<QuestionAndChoice> QuestionAndChoices { get; set; } = new List<QuestionAndChoice>();
    public ICollection<Answer> Answers { get; set; } = new List<Answer>();
}
