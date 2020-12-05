using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DatabaseHandler.Migrations
{
    public partial class Seeding : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "CodeInformations",
                columns: new[] { "Id", "Code", "CodeName", "Example", "ShareTime" },
                values: new object[] { new Guid("29a1a908-0690-49ee-a1b0-fc214bb277c7"), "CodeFor1", "Code1", true, new DateTime(2020, 12, 5, 1, 56, 29, 63, DateTimeKind.Local).AddTicks(5511) });

            migrationBuilder.InsertData(
                table: "CodeInformations",
                columns: new[] { "Id", "Code", "CodeName", "Example", "ShareTime" },
                values: new object[] { new Guid("e09144d9-cb21-4461-938b-f8d4e1feaa20"), "CodeFor2", "Code2", true, new DateTime(2020, 12, 5, 1, 56, 29, 65, DateTimeKind.Local).AddTicks(7235) });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CodeInformations",
                keyColumn: "Id",
                keyValue: new Guid("29a1a908-0690-49ee-a1b0-fc214bb277c7"));

            migrationBuilder.DeleteData(
                table: "CodeInformations",
                keyColumn: "Id",
                keyValue: new Guid("e09144d9-cb21-4461-938b-f8d4e1feaa20"));
        }
    }
}
