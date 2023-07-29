using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebMonitor.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SupportedFeatures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CpuInfo = table.Column<bool>(type: "INTEGER", nullable: false),
                    MemoryInfo = table.Column<bool>(type: "INTEGER", nullable: false),
                    DiskInfo = table.Column<bool>(type: "INTEGER", nullable: false),
                    CpuUsage = table.Column<bool>(type: "INTEGER", nullable: false),
                    MemoryUsage = table.Column<bool>(type: "INTEGER", nullable: false),
                    DiskUsage = table.Column<bool>(type: "INTEGER", nullable: false),
                    NetworkUsage = table.Column<bool>(type: "INTEGER", nullable: false),
                    NvidiaGpuUsage = table.Column<bool>(type: "INTEGER", nullable: false),
                    AmdGpuUsage = table.Column<bool>(type: "INTEGER", nullable: false),
                    IntelGpuUsage = table.Column<bool>(type: "INTEGER", nullable: false),
                    Processes = table.Column<bool>(type: "INTEGER", nullable: false),
                    FileBrowser = table.Column<bool>(type: "INTEGER", nullable: false),
                    FileDownload = table.Column<bool>(type: "INTEGER", nullable: false),
                    FileUpload = table.Column<bool>(type: "INTEGER", nullable: false),
                    NvidiaRefreshSettings = table.Column<bool>(type: "INTEGER", nullable: false),
                    BatteryInfo = table.Column<bool>(type: "INTEGER", nullable: false),
                    ProcessPriority = table.Column<bool>(type: "INTEGER", nullable: false),
                    ProcessPriorityChange = table.Column<bool>(type: "INTEGER", nullable: false),
                    ProcessAffinity = table.Column<bool>(type: "INTEGER", nullable: false),
                    Terminal = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportedFeatures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    Password = table.Column<string>(type: "TEXT", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: false),
                    IsAdmin = table.Column<bool>(type: "INTEGER", nullable: false),
                    AllowedFeaturesId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_SupportedFeatures_AllowedFeaturesId",
                        column: x => x.AllowedFeaturesId,
                        principalTable: "SupportedFeatures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_AllowedFeaturesId",
                table: "Users",
                column: "AllowedFeaturesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "SupportedFeatures");
        }
    }
}
