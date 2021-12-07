using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Surveyapp.Migrations
{
    public partial class quizanswe : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AnswerRequired",
                table: "Question",
                type: "boolean",
                nullable: true,
                defaultValue: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnswerRequired",
                table: "Question");
        }
    }
}
