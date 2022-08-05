using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Surveyapp.Migrations
{
    public partial class courseDep : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CourseId",
                table: "Survey",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "Survey",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ForStaff",
                table: "Survey",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ForStudents",
                table: "Survey",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Survey_CourseId",
                table: "Survey",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Survey_DepartmentId",
                table: "Survey",
                column: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Survey_Courses_CourseId",
                table: "Survey",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Survey_Departments_DepartmentId",
                table: "Survey",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Survey_Courses_CourseId",
                table: "Survey");

            migrationBuilder.DropForeignKey(
                name: "FK_Survey_Departments_DepartmentId",
                table: "Survey");

            migrationBuilder.DropIndex(
                name: "IX_Survey_CourseId",
                table: "Survey");

            migrationBuilder.DropIndex(
                name: "IX_Survey_DepartmentId",
                table: "Survey");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "Survey");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Survey");

            migrationBuilder.DropColumn(
                name: "ForStaff",
                table: "Survey");

            migrationBuilder.DropColumn(
                name: "ForStudents",
                table: "Survey");
        }
    }
}
