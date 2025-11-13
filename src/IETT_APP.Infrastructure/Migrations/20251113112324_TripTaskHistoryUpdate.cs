using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IETT_APP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TripTaskHistoryUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChangedAt",
                table: "TripTaskHistories");

            migrationBuilder.DropColumn(
                name: "ChangedByUserId",
                table: "TripTaskHistories");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TripTaskHistories");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "TripTaskHistories");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "TripTaskHistories");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "TripTaskHistories");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "TripTaskHistories");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "TripTaskHistories");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ChangedAt",
                table: "TripTaskHistories",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "ChangedByUserId",
                table: "TripTaskHistories",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TripTaskHistories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "TripTaskHistories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "TripTaskHistories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "TripTaskHistories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "TripTaskHistories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "TripTaskHistories",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
