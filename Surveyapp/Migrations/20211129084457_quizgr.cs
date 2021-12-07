using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Surveyapp.Migrations
{
    public partial class quizgr : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Question_QuestionGroups_QuestionGroupId",
                table: "Question");

            migrationBuilder.AlterColumn<int>(
                name: "QuestionGroupId",
                table: "Question",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Question_QuestionGroups_QuestionGroupId",
                table: "Question",
                column: "QuestionGroupId",
                principalTable: "QuestionGroups",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Question_QuestionGroups_QuestionGroupId",
                table: "Question");

            migrationBuilder.AlterColumn<int>(
                name: "QuestionGroupId",
                table: "Question",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Question_QuestionGroups_QuestionGroupId",
                table: "Question",
                column: "QuestionGroupId",
                principalTable: "QuestionGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
