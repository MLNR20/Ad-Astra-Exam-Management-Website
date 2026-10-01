using System.ComponentModel.DataAnnotations;

namespace AAExamManagementSystem.Models.Dtos;

public class RoleDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime DateUpdated { get; set; }
}

public class RoleCreateUpdateDto
{
    [Required(ErrorMessage = "Role name is required.")]
    [StringLength(256, MinimumLength = 2, ErrorMessage = "Role name must be between 2 and 256 characters.")]
    [RegularExpression(@"^[a-zA-Z0-9 ]+$", ErrorMessage = "Role name can only contain letters, numbers, and spaces.")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}
