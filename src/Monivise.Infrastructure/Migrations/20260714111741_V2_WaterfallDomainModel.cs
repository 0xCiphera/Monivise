using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Monivise.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class V2_WaterfallDomainModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "IntakeItemId",
                table: "Transactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WantCategoryId",
                table: "Transactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BufferBalance",
                table: "BudgetCycles",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "FixedObligationStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BudgetCycleId = table.Column<Guid>(type: "uuid", nullable: false),
                    IntakeItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsPaid = table.Column<bool>(type: "boolean", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FixedObligationStatuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FixedObligationStatuses_BudgetCycles_BudgetCycleId",
                        column: x => x.BudgetCycleId,
                        principalTable: "BudgetCycles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FixedObligationStatuses_IntakeItems_IntakeItemId",
                        column: x => x.IntakeItemId,
                        principalTable: "IntakeItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WantCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    IsUnpriced = table.Column<bool>(type: "boolean", nullable: false),
                    MonthlyAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WantCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WantCategories_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_IntakeItemId",
                table: "Transactions",
                column: "IntakeItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_WantCategoryId",
                table: "Transactions",
                column: "WantCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_FixedObligationStatuses_BudgetCycleId_IntakeItemId",
                table: "FixedObligationStatuses",
                columns: new[] { "BudgetCycleId", "IntakeItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FixedObligationStatuses_IntakeItemId",
                table: "FixedObligationStatuses",
                column: "IntakeItemId");

            migrationBuilder.CreateIndex(
                name: "IX_WantCategories_UserId_IsActive",
                table: "WantCategories",
                columns: new[] { "UserId", "IsActive" });

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_IntakeItems_IntakeItemId",
                table: "Transactions",
                column: "IntakeItemId",
                principalTable: "IntakeItems",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_WantCategories_WantCategoryId",
                table: "Transactions",
                column: "WantCategoryId",
                principalTable: "WantCategories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_IntakeItems_IntakeItemId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_WantCategories_WantCategoryId",
                table: "Transactions");

            migrationBuilder.DropTable(
                name: "FixedObligationStatuses");

            migrationBuilder.DropTable(
                name: "WantCategories");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_IntakeItemId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_WantCategoryId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "IntakeItemId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "WantCategoryId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "BufferBalance",
                table: "BudgetCycles");
        }
    }
}
