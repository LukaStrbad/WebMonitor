using Microsoft.EntityFrameworkCore;
using WebMonitor.Model;
using WebMonitor.Model.Db;

namespace WebMonitor;

public class WebMonitorContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<AllowedFeatures> AllowedFeatures { get; set; } = null!;

    public string DbPath { get; }

    public WebMonitorContext()
    {
        var folder = AppContext.BaseDirectory;
        DbPath = Path.Combine(folder, "WebMonitor.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlite($"Data Source={DbPath}");
}
