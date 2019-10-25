using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Surveyapp.Migrations
{
    public partial class otherpropsubjecr : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AppointmentDate",
                table: "SurveySubject");

            migrationBuilder.DropColumn(
                name: "Chairpersion",
                table: "SurveySubject");

            migrationBuilder.DropColumn(
                name: "EndofTerm",
                table: "SurveySubject");

            migrationBuilder.DropColumn(
                name: "StateCorporation",
                table: "SurveySubject");

            migrationBuilder.AddColumn<string>(
                name: "OtherProperties",
                table: "SurveySubject",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OtherProperties",
                table: "SurveySubject");

            migrationBuilder.AddColumn<DateTime>(
                name: "AppointmentDate",
                table: "SurveySubject",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Chairpersion",
                table: "SurveySubject",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndofTerm",
                table: "SurveySubject",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "StateCorporation",
                table: "SurveySubject",
                nullable: true);
        }
    }
}
