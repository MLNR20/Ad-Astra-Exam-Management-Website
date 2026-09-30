using AAExamManagementSystem.Models.Dtos;

namespace AAExamManagementSystem.Tests;

public class RoleAssignmentIdTests
{
    [Fact]
    public void Combine_ThenTryParse_RoundTrips()
    {
        var id = RoleAssignmentId.Combine("user-1", "role-1");

        var parsed = RoleAssignmentId.TryParse(id, out var userId, out var roleId);

        Assert.True(parsed);
        Assert.Equal("user-1", userId);
        Assert.Equal("role-1", roleId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("nounderscore")]
    [InlineData("_missinguserid")]
    [InlineData("missingroleid_")]
    public void TryParse_InvalidInput_ReturnsFalse(string id)
    {
        var parsed = RoleAssignmentId.TryParse(id, out var userId, out var roleId);

        Assert.False(parsed);
        Assert.Equal(string.Empty, userId);
        Assert.Equal(string.Empty, roleId);
    }

    [Fact]
    public void TryParse_MultipleUnderscores_UsesAllSegments()
    {
        var id = RoleAssignmentId.Combine("guid-with-hyphen", "role_with_underscore");

        var parsed = RoleAssignmentId.TryParse(id, out _, out _);

        Assert.False(parsed);
    }
}
