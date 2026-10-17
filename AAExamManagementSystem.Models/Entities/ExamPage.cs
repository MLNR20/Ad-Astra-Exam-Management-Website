namespace AAExamManagementSystem.Models.Entities;

public class ExamPage
{
    public int Id { get; set; }
    public int SectionId { get; set; }
    public Section Section { get; set; } = null!;
    public int PageNumber { get; set; } = 1;
    public string? Instruction { get; set; }
    public string? SubInstruction { get; set; }
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public DateTime DateUpdated { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public ICollection<PageSection> PageSections { get; set; } = new List<PageSection>();
}
