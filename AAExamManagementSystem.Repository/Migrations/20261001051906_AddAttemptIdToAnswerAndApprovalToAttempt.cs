using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AAExamManagementSystem.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddAttemptIdToAnswerAndApprovalToAttempt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovedBy",
                schema: "AAExam",
                table: "Attempts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                schema: "AAExam",
                table: "Attempts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "AttemptId",
                schema: "AAExam",
                table: "Answers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Answers_AttemptId",
                schema: "AAExam",
                table: "Answers",
                column: "AttemptId");

            migrationBuilder.AddForeignKey(
                name: "FK_Answers_Attempts_AttemptId",
                schema: "AAExam",
                table: "Answers",
                column: "AttemptId",
                principalSchema: "AAExam",
                principalTable: "Attempts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Answers_Attempts_AttemptId",
                schema: "AAExam",
                table: "Answers");

            migrationBuilder.DropIndex(
                name: "IX_Answers_AttemptId",
                schema: "AAExam",
                table: "Answers");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                schema: "AAExam",
                table: "Attempts");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                schema: "AAExam",
                table: "Attempts");

            migrationBuilder.DropColumn(
                name: "AttemptId",
                schema: "AAExam",
                table: "Answers");
        }
    }
}
