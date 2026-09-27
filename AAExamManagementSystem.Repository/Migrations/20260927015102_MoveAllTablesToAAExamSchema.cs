using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AAExamManagementSystem.Repository.Migrations
{
    /// <inheritdoc />
    public partial class MoveAllTablesToAAExamSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "AAExam");

            migrationBuilder.RenameTable(
                name: "Sections",
                newName: "Sections",
                newSchema: "AAExam");

            migrationBuilder.RenameTable(
                name: "QuestionTypes",
                newName: "QuestionTypes",
                newSchema: "AAExam");

            migrationBuilder.RenameTable(
                name: "Questions",
                newName: "Questions",
                newSchema: "AAExam");

            migrationBuilder.RenameTable(
                name: "QuestionAndChoices",
                newName: "QuestionAndChoices",
                newSchema: "AAExam");

            migrationBuilder.RenameTable(
                name: "Exams",
                newName: "Exams",
                newSchema: "AAExam");

            migrationBuilder.RenameTable(
                name: "Departments",
                newName: "Departments",
                newSchema: "AAExam");

            migrationBuilder.RenameTable(
                name: "Courses",
                newName: "Courses",
                newSchema: "AAExam");

            migrationBuilder.RenameTable(
                name: "Choices",
                newName: "Choices",
                newSchema: "AAExam");

            migrationBuilder.RenameTable(
                name: "Attempts",
                newName: "Attempts",
                newSchema: "AAExam");

            migrationBuilder.RenameTable(
                name: "AspNetUserTokens",
                newName: "AspNetUserTokens",
                newSchema: "AAExam");

            migrationBuilder.RenameTable(
                name: "AspNetUsers",
                newName: "AspNetUsers",
                newSchema: "AAExam");

            migrationBuilder.RenameTable(
                name: "AspNetUserRoles",
                newName: "AspNetUserRoles",
                newSchema: "AAExam");

            migrationBuilder.RenameTable(
                name: "AspNetUserLogins",
                newName: "AspNetUserLogins",
                newSchema: "AAExam");

            migrationBuilder.RenameTable(
                name: "AspNetUserClaims",
                newName: "AspNetUserClaims",
                newSchema: "AAExam");

            migrationBuilder.RenameTable(
                name: "AspNetRoles",
                newName: "AspNetRoles",
                newSchema: "AAExam");

            migrationBuilder.RenameTable(
                name: "AspNetRoleClaims",
                newName: "AspNetRoleClaims",
                newSchema: "AAExam");

            migrationBuilder.RenameTable(
                name: "Applicants",
                newName: "Applicants",
                newSchema: "AAExam");

            migrationBuilder.RenameTable(
                name: "Answers",
                newName: "Answers",
                newSchema: "AAExam");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Sections",
                schema: "AAExam",
                newName: "Sections");

            migrationBuilder.RenameTable(
                name: "QuestionTypes",
                schema: "AAExam",
                newName: "QuestionTypes");

            migrationBuilder.RenameTable(
                name: "Questions",
                schema: "AAExam",
                newName: "Questions");

            migrationBuilder.RenameTable(
                name: "QuestionAndChoices",
                schema: "AAExam",
                newName: "QuestionAndChoices");

            migrationBuilder.RenameTable(
                name: "Exams",
                schema: "AAExam",
                newName: "Exams");

            migrationBuilder.RenameTable(
                name: "Departments",
                schema: "AAExam",
                newName: "Departments");

            migrationBuilder.RenameTable(
                name: "Courses",
                schema: "AAExam",
                newName: "Courses");

            migrationBuilder.RenameTable(
                name: "Choices",
                schema: "AAExam",
                newName: "Choices");

            migrationBuilder.RenameTable(
                name: "Attempts",
                schema: "AAExam",
                newName: "Attempts");

            migrationBuilder.RenameTable(
                name: "AspNetUserTokens",
                schema: "AAExam",
                newName: "AspNetUserTokens");

            migrationBuilder.RenameTable(
                name: "AspNetUsers",
                schema: "AAExam",
                newName: "AspNetUsers");

            migrationBuilder.RenameTable(
                name: "AspNetUserRoles",
                schema: "AAExam",
                newName: "AspNetUserRoles");

            migrationBuilder.RenameTable(
                name: "AspNetUserLogins",
                schema: "AAExam",
                newName: "AspNetUserLogins");

            migrationBuilder.RenameTable(
                name: "AspNetUserClaims",
                schema: "AAExam",
                newName: "AspNetUserClaims");

            migrationBuilder.RenameTable(
                name: "AspNetRoles",
                schema: "AAExam",
                newName: "AspNetRoles");

            migrationBuilder.RenameTable(
                name: "AspNetRoleClaims",
                schema: "AAExam",
                newName: "AspNetRoleClaims");

            migrationBuilder.RenameTable(
                name: "Applicants",
                schema: "AAExam",
                newName: "Applicants");

            migrationBuilder.RenameTable(
                name: "Answers",
                schema: "AAExam",
                newName: "Answers");
        }
    }
}
