using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AAExamManagementSystem.Repository.Migrations
{
    /// <summary>
    /// Restructures exam pages into Page (ExamPages) -> Section (PageSections) -> Questions.
    /// Existing PageSections rows (one per question) are folded into ExamPages (one per department + page number)
    /// and PageSections (one per page + question type); each question now points at its section.
    /// </summary>
    public partial class RestructureExamPages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExamPages",
                schema: "AAExam",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SectionId = table.Column<int>(type: "int", nullable: false),
                    PageNumber = table.Column<int>(type: "int", nullable: false),
                    Instruction = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubInstruction = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamPages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamPages_Sections_SectionId",
                        column: x => x.SectionId,
                        principalSchema: "AAExam",
                        principalTable: "Sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExamPages_SectionId_PageNumber",
                schema: "AAExam",
                table: "ExamPages",
                columns: new[] { "SectionId", "PageNumber" },
                unique: true);

            migrationBuilder.AddColumn<int>(
                name: "ExamPageId",
                schema: "AAExam",
                table: "PageSections",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PageOrder",
                schema: "AAExam",
                table: "Questions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PageSectionId",
                schema: "AAExam",
                table: "Questions",
                type: "int",
                nullable: true);

            // 1. One ExamPage per (department, page number). Heading text comes from the first row on the page that has any.
            migrationBuilder.Sql(@"
INSERT INTO [AAExam].[ExamPages] ([SectionId], [PageNumber], [Instruction], [SubInstruction], [DateCreated], [DateUpdated], [IsActive])
SELECT g.[SectionId], g.[PageNumber], i.[Instruction], i.[SubInstruction], g.[DateCreated], SYSUTCDATETIME(), 1
FROM (
    SELECT q.[SectionId], ps.[PageNumber], MIN(ps.[DateCreated]) AS [DateCreated]
    FROM [AAExam].[PageSections] ps
    INNER JOIN [AAExam].[Questions] q ON q.[Id] = ps.[QuestionId]
    GROUP BY q.[SectionId], ps.[PageNumber]
) g
OUTER APPLY (
    SELECT TOP 1 ps2.[Instruction], ps2.[SubInstruction]
    FROM [AAExam].[PageSections] ps2
    INNER JOIN [AAExam].[Questions] q2 ON q2.[Id] = ps2.[QuestionId]
    WHERE q2.[SectionId] = g.[SectionId]
      AND ps2.[PageNumber] = g.[PageNumber]
      AND (NULLIF(LTRIM(RTRIM(ps2.[Instruction])), '') IS NOT NULL OR NULLIF(LTRIM(RTRIM(ps2.[SubInstruction])), '') IS NOT NULL)
    ORDER BY ps2.[Order], ps2.[Id]
) i;");

            // 2. Point every legacy row at its page.
            migrationBuilder.Sql(@"
UPDATE ps
SET ps.[ExamPageId] = ep.[Id]
FROM [AAExam].[PageSections] ps
INNER JOIN [AAExam].[Questions] q ON q.[Id] = ps.[QuestionId]
INNER JOIN [AAExam].[ExamPages] ep ON ep.[SectionId] = q.[SectionId] AND ep.[PageNumber] = ps.[PageNumber];");

            // 3. Questions move onto the surviving (lowest-id) section of their page + question type, keeping their order.
            migrationBuilder.Sql(@"
UPDATE q
SET q.[PageSectionId] = rep.[RepId], q.[PageOrder] = ps.[Order]
FROM [AAExam].[Questions] q
INNER JOIN [AAExam].[PageSections] ps ON ps.[QuestionId] = q.[Id]
INNER JOIN (
    SELECT [ExamPageId], [QuestionTypeId], MIN([Id]) AS [RepId]
    FROM [AAExam].[PageSections]
    GROUP BY [ExamPageId], [QuestionTypeId]
) rep ON rep.[ExamPageId] = ps.[ExamPageId] AND rep.[QuestionTypeId] = ps.[QuestionTypeId];");

            // 4. Each surviving section sits where its first question used to sit on the page.
            migrationBuilder.Sql(@"
UPDATE ps
SET ps.[Order] = g.[MinOrder]
FROM [AAExam].[PageSections] ps
INNER JOIN (
    SELECT MIN([Id]) AS [RepId], MIN([Order]) AS [MinOrder]
    FROM [AAExam].[PageSections]
    GROUP BY [ExamPageId], [QuestionTypeId]
) g ON g.[RepId] = ps.[Id];");

            // 5. Drop the now-redundant per-question rows.
            migrationBuilder.Sql(@"
DELETE FROM [AAExam].[PageSections]
WHERE [ExamPageId] IS NULL
   OR [Id] NOT IN (
        SELECT MIN([Id])
        FROM [AAExam].[PageSections]
        WHERE [ExamPageId] IS NOT NULL
        GROUP BY [ExamPageId], [QuestionTypeId]);");

            migrationBuilder.DropForeignKey(
                name: "FK_PageSections_Questions_QuestionId",
                schema: "AAExam",
                table: "PageSections");

            migrationBuilder.DropIndex(
                name: "IX_PageSections_QuestionId",
                schema: "AAExam",
                table: "PageSections");

            migrationBuilder.DropColumn(
                name: "Instruction",
                schema: "AAExam",
                table: "PageSections");

            migrationBuilder.DropColumn(
                name: "PageNumber",
                schema: "AAExam",
                table: "PageSections");

            migrationBuilder.DropColumn(
                name: "QuestionId",
                schema: "AAExam",
                table: "PageSections");

            migrationBuilder.DropColumn(
                name: "SubInstruction",
                schema: "AAExam",
                table: "PageSections");

            migrationBuilder.AlterColumn<int>(
                name: "ExamPageId",
                schema: "AAExam",
                table: "PageSections",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PageSections_ExamPageId",
                schema: "AAExam",
                table: "PageSections",
                column: "ExamPageId");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_PageSectionId",
                schema: "AAExam",
                table: "Questions",
                column: "PageSectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_PageSections_ExamPages_ExamPageId",
                schema: "AAExam",
                table: "PageSections",
                column: "ExamPageId",
                principalSchema: "AAExam",
                principalTable: "ExamPages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_PageSections_PageSectionId",
                schema: "AAExam",
                table: "Questions",
                column: "PageSectionId",
                principalSchema: "AAExam",
                principalTable: "PageSections",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PageSections_ExamPages_ExamPageId",
                schema: "AAExam",
                table: "PageSections");

            migrationBuilder.DropForeignKey(
                name: "FK_Questions_PageSections_PageSectionId",
                schema: "AAExam",
                table: "Questions");

            migrationBuilder.AddColumn<Guid>(
                name: "QuestionId",
                schema: "AAExam",
                table: "PageSections",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PageNumber",
                schema: "AAExam",
                table: "PageSections",
                type: "int",
                nullable: true);

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

            // Expand each section back into one row per question, then drop the section rows.
            migrationBuilder.Sql(@"
INSERT INTO [AAExam].[PageSections] ([ExamPageId], [QuestionTypeId], [Order], [DateCreated], [DateUpdated], [IsActive], [QuestionId], [PageNumber], [Instruction], [SubInstruction])
SELECT ps.[ExamPageId], ps.[QuestionTypeId], q.[PageOrder], ps.[DateCreated], ps.[DateUpdated], ps.[IsActive], q.[Id], ep.[PageNumber], ep.[Instruction], ep.[SubInstruction]
FROM [AAExam].[Questions] q
INNER JOIN [AAExam].[PageSections] ps ON ps.[Id] = q.[PageSectionId]
INNER JOIN [AAExam].[ExamPages] ep ON ep.[Id] = ps.[ExamPageId]
WHERE ps.[QuestionId] IS NULL;");

            migrationBuilder.Sql(@"DELETE FROM [AAExam].[PageSections] WHERE [QuestionId] IS NULL;");

            migrationBuilder.DropIndex(
                name: "IX_PageSections_ExamPageId",
                schema: "AAExam",
                table: "PageSections");

            migrationBuilder.DropColumn(
                name: "ExamPageId",
                schema: "AAExam",
                table: "PageSections");

            migrationBuilder.DropTable(
                name: "ExamPages",
                schema: "AAExam");

            migrationBuilder.DropIndex(
                name: "IX_Questions_PageSectionId",
                schema: "AAExam",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "PageOrder",
                schema: "AAExam",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "PageSectionId",
                schema: "AAExam",
                table: "Questions");

            migrationBuilder.AlterColumn<Guid>(
                name: "QuestionId",
                schema: "AAExam",
                table: "PageSections",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PageNumber",
                schema: "AAExam",
                table: "PageSections",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PageSections_QuestionId",
                schema: "AAExam",
                table: "PageSections",
                column: "QuestionId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PageSections_Questions_QuestionId",
                schema: "AAExam",
                table: "PageSections",
                column: "QuestionId",
                principalSchema: "AAExam",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
