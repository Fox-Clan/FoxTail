using Microsoft.EntityFrameworkCore;


public class StargateTools
{
    public static async Task<Stargate> FindGateByAddress(string address, StargateContext ctx)
    {
        var gate = await ctx.Stargates
            .SingleOrDefaultAsync(b => b.gate_address == address);

        if (gate == null)
        {
            Stargate nullgate = new Stargate()
            {
                id = "NULL"
            };
            return nullgate;
        }
        
        return gate;
    }
    
    public static async Task<Stargate> FindGateById(string id, StargateContext ctx)
    {
        var gate = await ctx.Stargates
            .SingleOrDefaultAsync(b => b.id == id);

        if (gate == null)
        {
            Stargate nullgate = new Stargate()
            {
                id = "NULL"
            };
            return nullgate;
        }
        
        return gate;
    }
    
    public static async Task<Stargate> FindGateByDialedId(string id, StargateContext ctx)
    {
        var gate = await ctx.Stargates
            .SingleOrDefaultAsync(b => b.dialed_gate_id == id);

        if (gate == null)
        {
            Stargate nullgate = new Stargate()
            {
                id = "NULL"
            };
            return nullgate;
        }
        
        return gate;
    }

    public static List<Stargate> FindAllGates(StargateContext ctx, bool onlyNonPersistent, bool onlyPublic=false)
    {
        List<Stargate> gates = new List<Stargate>();
        
        if (onlyNonPersistent)
        {
            gates = ctx.Stargates
                .Where(b => b.is_persistent == false)
                .ToList();
        }
        else if (onlyPublic)
        {
            gates = ctx.Stargates
                .Where(b => b.public_gate == true)
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
        Console.WriteLine("Removing gate: " + gate.id);
    }

}
