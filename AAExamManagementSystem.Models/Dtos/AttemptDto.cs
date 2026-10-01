namespace AAExamManagementSystem.Models.Dtos;

public class AttemptListItemDto
{
    public int Id { get; set; }
    public string ApplicantName { get; set; } = string.Empty;
    public string StudentNo { get; set; } = string.Empty;
    public string? Section { get; set; }
    public string? DateTaken { get; set; }
    public int TotalScore { get; set; }
    public int MaxScore { get; set; }
    public int PendingEvaluationCount { get; set; }
    public bool IsApproved { get; set; }
    public DateTime DateCreated { get; set; }
}

public class AttemptAnswerReviewDto
{
    public int Id { get; set; }
    public Guid QuestionId { get; set; }
    public string QuestionTitle { get; set; } = string.Empty;
    public string AnswerText { get; set; } = string.Empty;
    public string? IsCorrect { get; set; }
    public int Point { get; set; }
    public int MaxScore { get; set; }
    public bool IsUpToEvaluation { get; set; }
    public string? CheckedBy { get; set; }
}

public class AttemptReviewDto
{
    public int Id { get; set; }
    public string ApplicantName { get; set; } = string.Empty;
    public string StudentNo { get; set; } = string.Empty;
    public string? Section { get; set; }
    public string? DateTaken { get; set; }
    public int TotalScore { get; set; }
    public int MaxScore { get; set; }
    public bool IsApproved { get; set; }
    public string? ApprovedBy { get; set; }
    public IList<AttemptAnswerReviewDto> Answers { get; set; } = new List<AttemptAnswerReviewDto>();
}
