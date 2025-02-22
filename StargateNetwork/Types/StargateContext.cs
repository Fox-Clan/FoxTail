using Microsoft.EntityFrameworkCore;

namespace StargateNetwork.Types;

public class StargateContext : DbContext
{
    public DbSet<Stargate> Stargates { get; set; }
    
    public string DbPath { get; set; }

    public StargateContext()
    {
        this.DbPath = Path.Join(Environment.CurrentDirectory, "stargates.db");
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");
}