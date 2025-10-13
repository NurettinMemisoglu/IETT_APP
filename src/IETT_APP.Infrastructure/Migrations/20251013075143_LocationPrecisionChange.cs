using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IETT_APP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LocationPrecisionChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Location_Longitude",
                table: "Stops",
                type: "decimal(22,18)",
                precision: 22,
                scale: 18,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(9,7)",
                oldPrecision: 9,
                oldScale: 7);

            migrationBuilder.AlterColumn<decimal>(
                name: "Location_Latitude",
                table: "Stops",
                type: "decimal(22,18)",
                precision: 22,
                scale: 18,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(9,7)",
                oldPrecision: 9,
                oldScale: 7);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Location_Longitude",
                table: "Stops",
                type: "decimal(9,7)",
                precision: 9,
                scale: 7,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(22,18)",
                oldPrecision: 22,
                oldScale: 18);

            migrationBuilder.AlterColumn<decimal>(
                name: "Location_Latitude",
                table: "Stops",
                type: "decimal(9,7)",
                precision: 9,
                scale: 7,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(22,18)",
                oldPrecision: 22,
                oldScale: 18);
        }
    }
}
