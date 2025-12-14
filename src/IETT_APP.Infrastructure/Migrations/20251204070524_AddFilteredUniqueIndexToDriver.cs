using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IETT_APP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFilteredUniqueIndexToDriver : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Drivers_EmployeeNumber",
                table: "Drivers");

            migrationBuilder.AlterColumn<string>(
                name: "TCIdentityNumber",
                table: "Drivers",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_EmployeeNumber",
                table: "Drivers",
                column: "EmployeeNumber",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_TCIdentityNumber",
                table: "Drivers",
                column: "TCIdentityNumber",
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Drivers_EmployeeNumber",
                table: "Drivers");

            migrationBuilder.DropIndex(
                name: "IX_Drivers_TCIdentityNumber",
                table: "Drivers");

            migrationBuilder.AlterColumn<string>(
                name: "TCIdentityNumber",
                table: "Drivers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_EmployeeNumber",
                table: "Drivers",
                column: "EmployeeNumber",
                unique: true);
        }
    }
}
