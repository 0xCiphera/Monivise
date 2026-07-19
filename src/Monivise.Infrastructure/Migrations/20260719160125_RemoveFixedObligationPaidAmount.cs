using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Monivise.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFixedObligationPaidAmount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaidAmount",
                table: "FixedObligationStatuses");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PaidAmount",
                table: "FixedObligationStatuses",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);
        }
    }
}
