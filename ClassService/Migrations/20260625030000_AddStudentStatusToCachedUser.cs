using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassService.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentStatusToCachedUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StudentStatus",
                table: "CachedUsers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StudentStatus",
                table: "CachedUsers");
        }
    }
}
