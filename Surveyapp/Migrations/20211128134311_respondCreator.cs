using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Surveyapp.Migrations
{
    public partial class respondCreator : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SurveySubject_ResponseType_ResponseTypesId",
                table: "SurveySubject");

            migrationBuilder.DropForeignKey(
                name: "FK_SurveySubject_Survey_ResponseTypeId",
                table: "SurveySubject");

            migrationBuilder.DropIndex(
                name: "IX_SurveySubject_ResponseTypesId",
                table: "SurveySubject");

            migrationBuilder.DropColumn(
                name: "ResponseTypesId",
                table: "SurveySubject");

            migrationBuilder.AddColumn<string>(
                name: "CreatorId",
                table: "ResponseType",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResponseType_CreatorId",
                table: "ResponseType",
                column: "CreatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_ResponseType_AspNetUsers_CreatorId",
                table: "ResponseType",
                column: "CreatorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SurveySubject_ResponseType_ResponseTypeId",
                table: "SurveySubject",
                column: "ResponseTypeId",
                principalTable: "ResponseType",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ResponseType_AspNetUsers_CreatorId",
                table: "ResponseType");

            migrationBuilder.DropForeignKey(
                name: "FK_SurveySubject_ResponseType_ResponseTypeId",
                table: "SurveySubject");

            migrationBuilder.DropIndex(
                name: "IX_ResponseType_CreatorId",
                table: "ResponseType");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "ResponseType");

            migrationBuilder.AddColumn<int>(
                name: "ResponseTypesId",
                table: "SurveySubject",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SurveySubject_ResponseTypesId",
                table: "SurveySubject",
                column: "ResponseTypesId");

            migrationBuilder.AddForeignKey(
                name: "FK_SurveySubject_ResponseType_ResponseTypesId",
                table: "SurveySubject",
                column: "ResponseTypesId",
                principalTable: "ResponseType",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SurveySubject_Survey_ResponseTypeId",
                table: "SurveySubject",
                column: "ResponseTypeId",
                principalTable: "Survey",
                principalColumn: "Id");
        }
    }
}
