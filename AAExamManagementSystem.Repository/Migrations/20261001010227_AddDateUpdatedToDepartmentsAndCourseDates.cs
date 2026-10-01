using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AAExamManagementSystem.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddDateUpdatedToDepartmentsAndCourseDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateUpdated",
                schema: "AAExam",
                table: "Departments",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "AAExam",
                table: "Courses",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateUpdated",
                schema: "AAExam",
                table: "Courses",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateUpdated",
                schema: "AAExam",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "AAExam",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "DateUpdated",
                schema: "AAExam",
                table: "Courses");
        }
    }
}
