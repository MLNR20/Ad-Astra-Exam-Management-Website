using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AAExamManagementSystem.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddInstructionFieldsToPageSection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Instruction",
                schema: "AAExam",
                table: "PageSections",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubInstruction",
                schema: "AAExam",
                table: "PageSections",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Instruction",
                schema: "AAExam",
                table: "PageSections");

            migrationBuilder.DropColumn(
                name: "SubInstruction",
                schema: "AAExam",
                table: "PageSections");
        }
    }
}
