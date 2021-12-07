using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Surveyapp.Migrations
{
    public partial class surveydb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Question_QuestionGroups_QuestionGroupId",
                table: "Question");

            migrationBuilder.DropForeignKey(
                name: "FK_SurveyResponse_Question_QuestionId",
                table: "SurveyResponse");

            migrationBuilder.DropForeignKey(
                name: "FK_SurveySubject_SurveyCategory_CategoryId",
                table: "SurveySubject");

            migrationBuilder.CreateIndex(
                name: "IX_SurveySubject_Id",
                table: "SurveySubject",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_SurveyResponse_Id",
                table: "SurveyResponse",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_SurveyCategory_Id",
                table: "SurveyCategory",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ResponseType_Id",
                table: "ResponseType",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionGroups_Id",
                table: "QuestionGroups",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Question_QuestionGroups_QuestionGroupId",
                table: "Question",
                column: "QuestionGroupId",
                principalTable: "QuestionGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SurveyResponse_Question_QuestionId",
                table: "SurveyResponse",
                column: "QuestionId",
                principalTable: "Question",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SurveySubject_SurveyCategory_CategoryId",
                table: "SurveySubject",
                column: "CategoryId",
                principalTable: "SurveyCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Question_QuestionGroups_QuestionGroupId",
                table: "Question");

            migrationBuilder.DropForeignKey(
                name: "FK_SurveyResponse_Question_QuestionId",
                table: "SurveyResponse");

            migrationBuilder.DropForeignKey(
                name: "FK_SurveySubject_SurveyCategory_CategoryId",
                table: "SurveySubject");

            migrationBuilder.DropIndex(
                name: "IX_SurveySubject_Id",
                table: "SurveySubject");

            migrationBuilder.DropIndex(
                name: "IX_SurveyResponse_Id",
                table: "SurveyResponse");

            migrationBuilder.DropIndex(
                name: "IX_SurveyCategory_Id",
                table: "SurveyCategory");

            migrationBuilder.DropIndex(
                name: "IX_ResponseType_Id",
                table: "ResponseType");

            migrationBuilder.DropIndex(
                name: "IX_QuestionGroups_Id",
                table: "QuestionGroups");

            migrationBuilder.AddForeignKey(
                name: "FK_Question_QuestionGroups_QuestionGroupId",
                table: "Question",
                column: "QuestionGroupId",
                principalTable: "QuestionGroups",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SurveyResponse_Question_QuestionId",
                table: "SurveyResponse",
                column: "QuestionId",
                principalTable: "Question",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SurveySubject_SurveyCategory_CategoryId",
                table: "SurveySubject",
                column: "CategoryId",
                principalTable: "SurveyCategory",
                principalColumn: "Id");
        }
    }
}
