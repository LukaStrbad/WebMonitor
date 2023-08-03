using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebMonitor.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshIntervalChangeAllowedFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RefreshIntervalChange",
                table: "AllowedFeatures",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefreshIntervalChange",
                table: "AllowedFeatures");
        }
    }
}
