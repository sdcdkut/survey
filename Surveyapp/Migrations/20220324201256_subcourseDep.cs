using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Surveyapp.Migrations
{
    public partial class subcourseDep : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CourseId",
                table: "SurveySubject",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "SurveySubject",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "ForStaff",
                table: "Survey",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SurveySubject_CourseId",
                table: "SurveySubject",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_SurveySubject_DepartmentId",
                table: "SurveySubject",
                column: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_SurveySubject_Courses_CourseId",
                table: "SurveySubject",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SurveySubject_Departments_DepartmentId",
                table: "SurveySubject",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SurveySubject_Courses_CourseId",
                table: "SurveySubject");

            migrationBuilder.DropForeignKey(
                name: "FK_SurveySubject_Departments_DepartmentId",
                table: "SurveySubject");

            migrationBuilder.DropIndex(
                name: "IX_SurveySubject_CourseId",
                table: "SurveySubject");

            migrationBuilder.DropIndex(
                name: "IX_SurveySubject_DepartmentId",
                table: "SurveySubject");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "SurveySubject");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "SurveySubject");

            migrationBuilder.AlterColumn<bool>(
                name: "ForStaff",
                table: "Survey",
                type: "boolean",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "boolean");
        }
    }
}
