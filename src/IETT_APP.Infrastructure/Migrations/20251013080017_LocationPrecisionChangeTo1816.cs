using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IETT_APP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LocationPrecisionChangeTo1816 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Location_Longitude",
                table: "Stops",
                type: "decimal(18,16)",
                precision: 18,
                scale: 16,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(22,18)",
                oldPrecision: 22,
                oldScale: 18);

            migrationBuilder.AlterColumn<decimal>(
                name: "Location_Latitude",
                table: "Stops",
                type: "decimal(18,16)",
                precision: 18,
                scale: 16,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(22,18)",
                oldPrecision: 22,
                oldScale: 18);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Location_Longitude",
                table: "Stops",
                type: "decimal(22,18)",
                precision: 22,
                scale: 18,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,16)",
                oldPrecision: 18,
                oldScale: 16);

            migrationBuilder.AlterColumn<decimal>(
                name: "Location_Latitude",
                table: "Stops",
                type: "decimal(22,18)",
                precision: 22,
                scale: 18,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,16)",
                oldPrecision: 18,
                oldScale: 16);
        }
    }
}
