namespace AAExamManagementSystem.Models.Entities;

public class Attempt
{
    public int Id { get; set; }
    public int ApplicantId { get; set; }
    public Applicant Applicant { get; set; } = null!;
    public int TotalScore { get; set; }
    public string? DateTaken { get; set; }
    public DateTime? TimeRemaining { get; set; }
    public int? ExamId { get; set; }
    public string? AcademicYear { get; set; }
    public string? Term { get; set; }
    public string? Section { get; set; }
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public DateTime DateUpdated { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public bool IsApproved { get; set; }
    public string? ApprovedBy { get; set; }

    public ICollection<Answer> Answers { get; set; } = new List<Answer>();
}
