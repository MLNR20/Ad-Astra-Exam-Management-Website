using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AAExamManagementSystem.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddDateUpdatedAndIsActiveToRolesAndUserRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateUpdated",
                schema: "AAExam",
                table: "AspNetUserRoles",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "AAExam",
                table: "AspNetUserRoles",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateUpdated",
                schema: "AAExam",
                table: "AspNetRoles",
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
                table: "AspNetUserRoles");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "AAExam",
                table: "AspNetUserRoles");

            migrationBuilder.DropColumn(
                name: "DateUpdated",
                schema: "AAExam",
                table: "AspNetRoles");
        }
    }
}
