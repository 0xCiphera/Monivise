using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Monivise.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class V2_AllocationEngineAndSimulator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "UnpricedWantsPoolBalance",
                table: "BudgetCycles",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnpricedWantsPoolBalance",
                table: "BudgetCycles");
        }
    }
}
