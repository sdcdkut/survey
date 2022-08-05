using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Surveyapp.Migrations
{
    public partial class userno : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RegNo",
                table: "AspNetUsers",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RegNo",
                table: "AspNetUsers");
        }
    }
}
