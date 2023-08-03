using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebMonitor.Migrations
{
    /// <inheritdoc />
    public partial class RemoveComputerInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ComputerInfo",
                table: "AllowedFeatures");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ComputerInfo",
                table: "AllowedFeatures",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
