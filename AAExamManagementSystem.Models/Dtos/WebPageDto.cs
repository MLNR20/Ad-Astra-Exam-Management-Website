namespace AAExamManagementSystem.Models.Dtos;

public class WebPageDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Category { get; set; }
}
