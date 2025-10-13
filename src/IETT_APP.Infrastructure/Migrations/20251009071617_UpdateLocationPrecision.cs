using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IETT_APP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLocationPrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "Location_Longitude",
                table: "Stops",
                type: "float(9)",
                precision: 9,
                scale: 7,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<double>(
                name: "Location_Latitude",
                table: "Stops",
                type: "float(9)",
                precision: 9,
                scale: 7,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "Location_Longitude",
                table: "Stops",
                type: "float",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float(9)",
                oldPrecision: 9,
                oldScale: 7);

            migrationBuilder.AlterColumn<double>(
                name: "Location_Latitude",
                table: "Stops",
                type: "float",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float(9)",
                oldPrecision: 9,
                oldScale: 7);
        }
    }
}
