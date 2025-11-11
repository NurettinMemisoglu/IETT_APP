using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IETT_APP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignedToVehicle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAssigned",
                table: "Vehicles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "LineId",
                table: "Vehicles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_LineId",
                table: "Vehicles",
                column: "LineId");

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_Lines_LineId",
                table: "Vehicles",
                column: "LineId",
                principalTable: "Lines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_Lines_LineId",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_LineId",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "IsAssigned",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "LineId",
                table: "Vehicles");
        }
    }
}
