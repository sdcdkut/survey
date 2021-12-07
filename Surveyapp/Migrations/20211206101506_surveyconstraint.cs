using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Surveyapp.Migrations
{
    public partial class surveyconstraint : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Survey_Id",
                table: "Survey",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Survey_Name_Startdate_EndDate_status",
                table: "Survey",
                columns: new[] { "Name", "Startdate", "EndDate", "status" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Survey_Id",
                table: "Survey");

            migrationBuilder.DropIndex(
                name: "IX_Survey_Name_Startdate_EndDate_status",
                table: "Survey");
        }
    }
}
