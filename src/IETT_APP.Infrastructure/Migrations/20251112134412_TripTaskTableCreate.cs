using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IETT_APP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TripTaskTableCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StatusReason",
                table: "Vehicles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Operator",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmployeeNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Operator", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TripTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PassengerCount = table.Column<int>(type: "int", nullable: false),
                    DelayInMinutes = table.Column<int>(type: "int", nullable: false),
                    DelayOutMinutes = table.Column<int>(type: "int", nullable: false),
                    ScheduledDeparture = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ScheduledArrival = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AdjustedDeparture = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AdjustedArrival = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualDeparture = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualArrival = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VehicleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OperatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LineId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RouteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GarageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TripTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TripTasks_Garages_GarageId",
                        column: x => x.GarageId,
                        principalTable: "Garages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TripTasks_Lines_LineId",
                        column: x => x.LineId,
                        principalTable: "Lines",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TripTasks_Operator_OperatorId",
                        column: x => x.OperatorId,
                        principalTable: "Operator",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TripTasks_Routes_RouteId",
                        column: x => x.RouteId,
                        principalTable: "Routes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TripTasks_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TripTaskHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TripTaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FieldName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OldValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NewValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChangedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TripTaskHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TripTaskHistory_TripTasks_TripTaskId",
                        column: x => x.TripTaskId,
                        principalTable: "TripTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TripTaskHistory_TripTaskId",
                table: "TripTaskHistory",
                column: "TripTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TripTasks_GarageId",
                table: "TripTasks",
                column: "GarageId");

            migrationBuilder.CreateIndex(
                name: "IX_TripTasks_LineId",
                table: "TripTasks",
                column: "LineId");

            migrationBuilder.CreateIndex(
                name: "IX_TripTasks_OperatorId",
                table: "TripTasks",
                column: "OperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_TripTasks_RouteId",
                table: "TripTasks",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_TripTasks_VehicleId",
                table: "TripTasks",
                column: "VehicleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TripTaskHistory");

            migrationBuilder.DropTable(
                name: "TripTasks");

            migrationBuilder.DropTable(
                name: "Operator");

            migrationBuilder.DropColumn(
                name: "StatusReason",
                table: "Vehicles");
        }
    }
}
