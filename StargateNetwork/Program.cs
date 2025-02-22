using WebSocketSharp.Server;
using StargateNetwork.Types;

namespace StargateNetwork;

internal static class Program
{
    private static void CleanStaleDb()
    {
        try
        {
            while (true)
            {
                using (StargateContext db = new())
                {
                    List<Stargate> gates = StargateTools.FindAllGates(db, true);

                    foreach (Stargate gate in gates)
                    {
                        if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - gate.UpdateDate <= 60) continue;
                        
                        StargateTools.RemoveGate(gate, db);
                        Console.WriteLine("Cleaned stale stargate from database");
                    }

                    db.SaveChanges();
                }

                Thread.Sleep(60000);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception caught when cleaning state gates: " + e.Message);
        }
    }


    private static void Main(string[] args)
    {
        //get env vars
        string? wsUri = Environment.GetEnvironmentVariable("WS_URI");
        if (string.IsNullOrEmpty(wsUri))
        {
            wsUri = "ws://192.168.1.14:27015";
        }

        //start websocket server
        WebSocketServer server = new(wsUri);
        server.AddWebSocketService<StargateClient>("/Echo");
        server.Start();
        Console.WriteLine("server started on: " + wsUri);

        //start bunkum http server
        BunKum.StartBunKum();

        //start database cleaner thread
        Thread dbCleanThread = new(CleanStaleDb);
        dbCleanThread.Start();
        Console.WriteLine("Db cleaner started");

        Console.ReadKey();
        server.Stop();
    }
}