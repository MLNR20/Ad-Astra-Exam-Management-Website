using System.ComponentModel.DataAnnotations;

namespace AAExamManagementSystem.Models.Dtos;

public class ChoiceDto
{
    public Guid Id { get; set; }
    public string ChoiceText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public bool IsActive { get; set; } = true;
    public string? SectionName { get; set; }
}

public class ChoiceCreateUpdateDto
{
    [Required, StringLength(500)]
    [Display(Name = "Choice Text")]
    public string ChoiceText { get; set; } = string.Empty;

    [Display(Name = "Correct Answer")]
    public bool IsCorrect { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}
