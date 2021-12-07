using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Surveyapp.Migrations
{
    public partial class surveyconstraint1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Survey_Name_Startdate_EndDate_status",
                table: "Survey");

            migrationBuilder.CreateIndex(
                name: "IX_Survey_Name_Startdate_EndDate_status",
                table: "Survey",
                columns: new[] { "Name", "Startdate", "EndDate", "status" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Survey_Name_Startdate_EndDate_status",
                table: "Survey");

            migrationBuilder.CreateIndex(
                name: "IX_Survey_Name_Startdate_EndDate_status",
                table: "Survey",
                columns: new[] { "Name", "Startdate", "EndDate", "status" });
        }
    }
}
