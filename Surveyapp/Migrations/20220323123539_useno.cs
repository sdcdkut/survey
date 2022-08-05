using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Surveyapp.Migrations
{
    public partial class useno : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RegNo",
                table: "AspNetUsers",
                newName: "No");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "No",
                table: "AspNetUsers",
                newName: "RegNo");
        }
    }
}
