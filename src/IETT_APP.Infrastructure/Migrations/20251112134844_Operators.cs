using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IETT_APP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Operators : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TripTasks_Operator_OperatorId",
                table: "TripTasks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Operator",
                table: "Operator");

            migrationBuilder.RenameTable(
                name: "Operator",
                newName: "Operators");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Operators",
                table: "Operators",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TripTasks_Operators_OperatorId",
                table: "TripTasks",
                column: "OperatorId",
                principalTable: "Operators",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TripTasks_Operators_OperatorId",
                table: "TripTasks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Operators",
                table: "Operators");

            migrationBuilder.RenameTable(
                name: "Operators",
                newName: "Operator");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Operator",
                table: "Operator",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TripTasks_Operator_OperatorId",
                table: "TripTasks",
                column: "OperatorId",
                principalTable: "Operator",
                principalColumn: "Id");
        }
    }
}
