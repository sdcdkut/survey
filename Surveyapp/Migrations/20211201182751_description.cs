using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Surveyapp.Migrations
{
    public partial class description : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Survey_AspNetUsers_SurveyerId",
                table: "Survey");

            migrationBuilder.DropIndex(
                name: "IX_Survey_SurveyerId",
                table: "Survey");

            migrationBuilder.DropColumn(
                name: "SurveyerId",
                table: "Survey");

            migrationBuilder.RenameColumn(
                name: "SubjectName",
                table: "SurveySubject",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Survey",
                newName: "Name");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "SurveySubject",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Survey",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Survey",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Surveyors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SurveyorId = table.Column<string>(type: "text", nullable: true),
                    SurveyId = table.Column<int>(type: "integer", nullable: false),
                    ActiveStatus = table.Column<bool>(type: "boolean", nullable: false),
                    Permission = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Surveyors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Surveyors_AspNetUsers_SurveyorId",
                        column: x => x.SurveyorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Surveyors_Survey_SurveyId",
                        column: x => x.SurveyId,
                        principalTable: "Survey",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Survey_ApplicationUserId",
                table: "Survey",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Surveyors_SurveyId",
                table: "Surveyors",
                column: "SurveyId");

            migrationBuilder.CreateIndex(
                name: "IX_Surveyors_SurveyorId",
                table: "Surveyors",
                column: "SurveyorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Survey_AspNetUsers_ApplicationUserId",
                table: "Survey",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Survey_AspNetUsers_ApplicationUserId",
                table: "Survey");

            migrationBuilder.DropTable(
                name: "Surveyors");

            migrationBuilder.DropIndex(
                name: "IX_Survey_ApplicationUserId",
                table: "Survey");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "SurveySubject");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Survey");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Survey");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "SurveySubject",
                newName: "SubjectName");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Survey",
                newName: "name");

            migrationBuilder.AddColumn<string>(
                name: "SurveyerId",
                table: "Survey",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Survey_SurveyerId",
                table: "Survey",
                column: "SurveyerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Survey_AspNetUsers_SurveyerId",
                table: "Survey",
                column: "SurveyerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
