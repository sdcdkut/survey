using Microsoft.EntityFrameworkCore.Migrations;

namespace Surveyapp.Migrations
{
    public partial class subjecttypeundo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ResponseType_SubjectId",
                table: "ResponseType");

            migrationBuilder.CreateIndex(
                name: "IX_ResponseType_SubjectId",
                table: "ResponseType",
                column: "SubjectId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ResponseType_SubjectId",
                table: "ResponseType");

            migrationBuilder.CreateIndex(
                name: "IX_ResponseType_SubjectId",
                table: "ResponseType",
                column: "SubjectId",
                unique: true);
        }
    }
}
