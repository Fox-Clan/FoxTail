using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Sqlite;

public class StargateContext : DbContext
{
    public DbSet<Stargate> Stargates { get; set; }
    
    public string DbPath { get; set; }

    public StargateContext()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = System.IO.Path.Join(path, "stargates.db");
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");
}
public class Stargate
{
    public int active_users { get; set; }
    public string gate_address { get; set; }
    public string gate_code { get; set; }
    public string gate_status { get; set; }
    public string id { get; set; }
    public string iris_state { get; set; }
    public bool is_headless { get; set; }
    public int max_users { get; set; }
    public string owner_name { get; set; }
    public bool public_gate { get; set; }
    public string session_name { get; set; }
    public string session_url { get; set; }
    public int update_date { get; set; }
    public int creation_date { get; set; }
    public string dialed_gate_id { get; set; }
    public bool is_persistent { get; set; }
    public string world_record  { get; set; }
};

