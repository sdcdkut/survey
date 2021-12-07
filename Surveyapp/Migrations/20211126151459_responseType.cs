using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Surveyapp.Migrations
{
    public partial class responseType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ResponseTypeId",
                table: "SurveySubject",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SurveySubject_ResponseTypeId",
                table: "SurveySubject",
                column: "ResponseTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_SurveySubject_Survey_ResponseTypeId",
                table: "SurveySubject",
                column: "ResponseTypeId",
                principalTable: "Survey",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SurveySubject_Survey_ResponseTypeId",
                table: "SurveySubject");

            migrationBuilder.DropIndex(
                name: "IX_SurveySubject_ResponseTypeId",
                table: "SurveySubject");

            migrationBuilder.DropColumn(
                name: "ResponseTypeId",
                table: "SurveySubject");
        }
    }
}
