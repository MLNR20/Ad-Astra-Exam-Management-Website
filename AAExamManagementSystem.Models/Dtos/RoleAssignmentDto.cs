using System.ComponentModel.DataAnnotations;

namespace AAExamManagementSystem.Models.Dtos;

public class RoleAssignmentDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string RoleId { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime DateUpdated { get; set; }
}

public class RoleAssignmentCreateDto
{
    [Required(ErrorMessage = "User is required.")]
    [Display(Name = "User")]
    public string UserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role is required.")]
    [Display(Name = "Role")]
    public string RoleId { get; set; } = string.Empty;
}

public class RoleAssignmentEditDto
{
    [Required(ErrorMessage = "Role is required.")]
    [Display(Name = "Role")]
    public string RoleId { get; set; } = string.Empty;
}

public static class RoleAssignmentId
{
    private const string Separator = "_";

    public static string Combine(string userId, string roleId) => $"{userId}{Separator}{roleId}";

    public static bool TryParse(string id, out string userId, out string roleId)
    {
        userId = string.Empty;
        roleId = string.Empty;

        if (string.IsNullOrEmpty(id))
        {
            return false;
        }

        var parts = id.Split(Separator);
        if (parts.Length != 2 || parts[0].Length == 0 || parts[1].Length == 0)
        {
            return false;
        }

        userId = parts[0];
        roleId = parts[1];
        return true;
    }
}
