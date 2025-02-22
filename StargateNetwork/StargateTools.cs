using Microsoft.EntityFrameworkCore;
using StargateNetwork.Types;

namespace StargateNetwork;

public static class StargateTools
{
    public static async Task<Stargate> FindGateByAddress(string address, StargateContext ctx)
    {
        Stargate? gate = await ctx.Stargates
            .SingleOrDefaultAsync(b => b.GateAddress == address);

        return gate ?? Stargate.Null;
    }
    
    public static async Task<Stargate> FindGateById(string id, StargateContext ctx)
    {
        Stargate? gate = await ctx.Stargates
            .SingleOrDefaultAsync(b => b.Id == id);

        return gate ?? Stargate.Null;
    }
    
    public static async Task<Stargate> FindGateByDialedId(string id, StargateContext ctx)
    {
        Stargate? gate = await ctx.Stargates
            .SingleOrDefaultAsync(b => b.DialedGateId == id);

        return gate ?? Stargate.Null;
    }

    public static List<Stargate> FindAllGates(StargateContext ctx, bool onlyNonPersistent, bool onlyPublic=false)
    {
        List<Stargate> gates = new();
        
        if (onlyNonPersistent)
        {
            gates = ctx.Stargates
                .Where(b => b.IsPersistent == false)
                .ToList();
        }
        else if (onlyPublic)
        {
            gates = ctx.Stargates
                .Where(b => b.PublicGate == true)
                .ToList();
        }
        else
        {
            gates = ctx.Stargates.ToList();
        }
        
        return gates;
    }

    public static void RemoveGate(Stargate gate, StargateContext ctx)
    {
        ctx.Remove(gate);
        Console.WriteLine("Removing gate: " + gate.Id);
    }

}