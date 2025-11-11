using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IETT_APP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RedoVehicleLine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_Lines_LineId",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_LineId",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "LineId",
                table: "Vehicles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LineId",
                table: "Vehicles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

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
    }
}
