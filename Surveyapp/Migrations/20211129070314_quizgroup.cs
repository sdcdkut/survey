using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Surveyapp.Migrations
{
    public partial class quizgroup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Question_ResponseType_QuestionGroupId",
                table: "Question");

            migrationBuilder.CreateTable(
                name: "QuestionGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    SubjectId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionGroups_SurveySubject_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "SurveySubject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionGroups_SubjectId",
                table: "QuestionGroups",
                column: "SubjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Question_QuestionGroups_QuestionGroupId",
                table: "Question",
                column: "QuestionGroupId",
                principalTable: "QuestionGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Question_QuestionGroups_QuestionGroupId",
                table: "Question");

            migrationBuilder.DropTable(
                name: "QuestionGroups");

            migrationBuilder.AddForeignKey(
                name: "FK_Question_ResponseType_QuestionGroupId",
                table: "Question",
                column: "QuestionGroupId",
                principalTable: "ResponseType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
