﻿using Microsoft.EntityFrameworkCore;

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
    
    public async Task<Stargate?> FindGateByAddress(string address)
    {
        return await this.Stargates
            .SingleOrDefaultAsync(b => b.GateAddress == address);
    }
    
    public async Task<Stargate?> FindGateById(string id)
    {
        return await this.Stargates
            .SingleOrDefaultAsync(b => b.Id == id);
    }
    
    public async Task<Stargate?> FindGateByDialedId(string id)
    {
        return await this.Stargates
            .SingleOrDefaultAsync(b => b.DialedGateId == id);
    }

    public IEnumerable<Stargate> FindAllGates(bool onlyNonPersistent, bool onlyPublic = false)
    {
        if (onlyNonPersistent)
            return this.Stargates
                .Where(b => b.IsPersistent == false);

        if (onlyPublic)
            return this.Stargates
                .Where(b => b.PublicGate == true);

        return this.Stargates;
    }

    public void RemoveGate(Stargate gate, StargateContext ctx)
    {
        ctx.Remove(gate);
        Console.WriteLine("Removing gate: " + gate.Id);
    }
}