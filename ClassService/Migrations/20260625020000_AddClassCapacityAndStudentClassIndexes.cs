using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassService.Migrations
{
    /// <inheritdoc />
    public partial class AddClassCapacityAndStudentClassIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Capacity",
                table: "Classes",
                type: "integer",
                nullable: false,
                defaultValue: 40);

            migrationBuilder.CreateIndex(
                name: "IX_StudentClasses_ClassId_IsCurrent",
                table: "StudentClasses",
                columns: new[] { "ClassId", "IsCurrent" });

            migrationBuilder.CreateIndex(
                name: "IX_StudentClasses_StudentId",
                table: "StudentClasses",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentClasses_StudentId_SchoolYear_IsCurrent",
                table: "StudentClasses",
                columns: new[] { "StudentId", "SchoolYear", "IsCurrent" },
                unique: true,
                filter: "\"IsCurrent\" = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StudentClasses_ClassId_IsCurrent",
                table: "StudentClasses");

            migrationBuilder.DropIndex(
                name: "IX_StudentClasses_StudentId",
                table: "StudentClasses");

            migrationBuilder.DropIndex(
                name: "IX_StudentClasses_StudentId_SchoolYear_IsCurrent",
                table: "StudentClasses");

            migrationBuilder.DropColumn(
                name: "Capacity",
                table: "Classes");
        }
    }
}
