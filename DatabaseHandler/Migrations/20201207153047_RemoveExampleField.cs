using Microsoft.EntityFrameworkCore.Migrations;

namespace DatabaseHandler.Migrations
{
    public partial class RemoveExampleField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Example",
                table: "CodeInformations");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Example",
                table: "CodeInformations",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
