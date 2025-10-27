using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IETT_APP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RouteDirection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LengthInKm",
                table: "Routes",
                newName: "RouteDirection");

            migrationBuilder.AddColumn<int>(
                name: "LengthInM",
                table: "Routes",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LengthInM",
                table: "Routes");

            migrationBuilder.RenameColumn(
                name: "RouteDirection",
                table: "Routes",
                newName: "LengthInKm");
        }
    }
}
