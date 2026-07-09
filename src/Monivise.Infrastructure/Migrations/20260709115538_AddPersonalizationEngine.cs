using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Monivise.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonalizationEngine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
