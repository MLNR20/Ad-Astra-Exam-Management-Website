using System.ComponentModel.DataAnnotations;

namespace AAExamManagementSystem.Models.Dtos;

public class CourseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? SchoolName { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime DateUpdated { get; set; }
}

public class CourseCreateUpdateDto
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(150)]
    [Display(Name = "School Name")]
    public string? SchoolName { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}
