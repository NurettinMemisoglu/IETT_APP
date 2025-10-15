using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IETT_APP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StopTypeRedesign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Stops",
                newName: "StopType");

            migrationBuilder.AddColumn<int>(
                name: "SmartStop",
                table: "Stops",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SmartStop",
                table: "Stops");

            migrationBuilder.RenameColumn(
                name: "StopType",
                table: "Stops",
                newName: "Type");
        }
    }
}
