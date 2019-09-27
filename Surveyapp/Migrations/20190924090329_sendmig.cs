using Microsoft.EntityFrameworkCore.Migrations;

namespace Surveyapp.Migrations
{
    public partial class sendmig : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MyDictionary",
                table: "ResponseType");

            migrationBuilder.AddColumn<string>(
                name: "ResponseDictionary",
                table: "ResponseType",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResponseDictionary",
                table: "ResponseType");

            migrationBuilder.AddColumn<string>(
                name: "MyDictionary",
                table: "ResponseType",
                nullable: false,
                defaultValue: "");
        }
    }
}
