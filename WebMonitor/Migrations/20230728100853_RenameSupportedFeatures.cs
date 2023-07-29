using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebMonitor.Migrations
{
    /// <inheritdoc />
    public partial class RenameSupportedFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_SupportedFeatures_AllowedFeaturesId",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SupportedFeatures",
                table: "SupportedFeatures");

            migrationBuilder.RenameTable(
                name: "SupportedFeatures",
                newName: "AllowedFeatures");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AllowedFeatures",
                table: "AllowedFeatures",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_AllowedFeatures_AllowedFeaturesId",
                table: "Users",
                column: "AllowedFeaturesId",
                principalTable: "AllowedFeatures",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_AllowedFeatures_AllowedFeaturesId",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AllowedFeatures",
                table: "AllowedFeatures");

            migrationBuilder.RenameTable(
                name: "AllowedFeatures",
                newName: "SupportedFeatures");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SupportedFeatures",
                table: "SupportedFeatures",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_SupportedFeatures_AllowedFeaturesId",
                table: "Users",
                column: "AllowedFeaturesId",
                principalTable: "SupportedFeatures",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
