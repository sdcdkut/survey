using Microsoft.EntityFrameworkCore.Migrations;

namespace Surveyapp.Migrations
{
    public partial class adminapprovsurvey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "approvalStatus",
                table: "Survey",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "approvalStatus",
                table: "Survey");
        }
    }
}
