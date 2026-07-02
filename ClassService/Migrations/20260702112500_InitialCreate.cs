using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "classes");

            migrationBuilder.CreateTable(
                name: "CachedSubjects",
                schema: "classes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    GradeLevel = table.Column<int>(type: "integer", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CachedSubjects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CachedUsers",
                schema: "classes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserCode = table.Column<string>(type: "text", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    StudentStatus = table.Column<string>(type: "text", nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CachedUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Classes",
                schema: "classes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    GradeLevel = table.Column<int>(type: "integer", nullable: false),
                    SchoolYear = table.Column<string>(type: "text", nullable: false),
                    Capacity = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HomeroomAssignments",
                schema: "classes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TeacherId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassId = table.Column<Guid>(type: "uuid", nullable: false),
                    SchoolYear = table.Column<string>(type: "text", nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomeroomAssignments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Schedules",
                schema: "classes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeacherId = table.Column<Guid>(type: "uuid", nullable: false),
                    DayOfWeek = table.Column<int>(type: "integer", nullable: false),
                    Period = table.Column<int>(type: "integer", nullable: false),
                    Room = table.Column<string>(type: "text", nullable: false),
                    SchoolYear = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StudentClasses",
                schema: "classes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassId = table.Column<Guid>(type: "uuid", nullable: false),
                    SchoolYear = table.Column<string>(type: "text", nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false),
                    LeftDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    PromotionStatus = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentClasses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TeachingAssignments",
                schema: "classes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TeacherId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassId = table.Column<Guid>(type: "uuid", nullable: false),
                    SchoolYear = table.Column<string>(type: "text", nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeachingAssignments", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentClasses_ClassId_IsCurrent",
                schema: "classes",
                table: "StudentClasses",
                columns: new[] { "ClassId", "IsCurrent" });

            migrationBuilder.CreateIndex(
                name: "IX_StudentClasses_StudentId",
                schema: "classes",
                table: "StudentClasses",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentClasses_StudentId_SchoolYear_IsCurrent",
                schema: "classes",
                table: "StudentClasses",
                columns: new[] { "StudentId", "SchoolYear", "IsCurrent" },
                unique: true,
                filter: "\"IsCurrent\" = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CachedSubjects",
                schema: "classes");

            migrationBuilder.DropTable(
                name: "CachedUsers",
                schema: "classes");

            migrationBuilder.DropTable(
                name: "Classes",
                schema: "classes");

            migrationBuilder.DropTable(
                name: "HomeroomAssignments",
                schema: "classes");

            migrationBuilder.DropTable(
                name: "Schedules",
                schema: "classes");

            migrationBuilder.DropTable(
                name: "StudentClasses",
                schema: "classes");

            migrationBuilder.DropTable(
                name: "TeachingAssignments",
                schema: "classes");
        }
    }
}
