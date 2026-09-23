using Microsoft.EntityFrameworkCore;
using SkiApi.Models;

namespace SkiApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<SkiModel> SkiModels { get; set; }
    public DbSet<Wax> Waxes { get; set; }
    public DbSet<WeatherLog> WeatherLogs { get; set; }
    public DbSet<SelectionLog> SelectionLogs { get; set; }
    public DbSet<StoneGrind> StoneGrinds { get; set; }
    public DbSet<SkiGrindHistory> SkiGrindHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // SkiGrindHistory: два FK на разные таблицы — явно указываем поведение
        modelBuilder.Entity<SkiGrindHistory>()
            .HasOne(h => h.Ski)
            .WithMany()
            .HasForeignKey(h => h.SkiId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SkiGrindHistory>()
            .HasOne(h => h.StoneGrind)
            .WithMany()
            .HasForeignKey(h => h.StoneGrindId)
            .OnDelete(DeleteBehavior.Restrict);

        base.OnModelCreating(modelBuilder);
    }
}


/*
using Microsoft.EntityFrameworkCore;
using SkiApi.Models;

namespace SkiApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<SkiModel> SkiModels { get; set; }
    public DbSet<Wax> Waxes { get; set; }
    public DbSet<WeatherLog> WeatherLogs { get; set; }
    public DbSet<SelectionLog> SelectionLogs { get; set; }

}
*/
