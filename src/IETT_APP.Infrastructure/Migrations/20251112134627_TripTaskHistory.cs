using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IETT_APP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TripTaskHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TripTaskHistory_TripTasks_TripTaskId",
                table: "TripTaskHistory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TripTaskHistory",
                table: "TripTaskHistory");

            migrationBuilder.RenameTable(
                name: "TripTaskHistory",
                newName: "TripTaskHistories");

            migrationBuilder.RenameIndex(
                name: "IX_TripTaskHistory_TripTaskId",
                table: "TripTaskHistories",
                newName: "IX_TripTaskHistories_TripTaskId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TripTaskHistories",
                table: "TripTaskHistories",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TripTaskHistories_TripTasks_TripTaskId",
                table: "TripTaskHistories",
                column: "TripTaskId",
                principalTable: "TripTasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TripTaskHistories_TripTasks_TripTaskId",
                table: "TripTaskHistories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TripTaskHistories",
                table: "TripTaskHistories");

            migrationBuilder.RenameTable(
                name: "TripTaskHistories",
                newName: "TripTaskHistory");

            migrationBuilder.RenameIndex(
                name: "IX_TripTaskHistories_TripTaskId",
                table: "TripTaskHistory",
                newName: "IX_TripTaskHistory_TripTaskId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TripTaskHistory",
                table: "TripTaskHistory",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TripTaskHistory_TripTasks_TripTaskId",
                table: "TripTaskHistory",
                column: "TripTaskId",
                principalTable: "TripTasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
