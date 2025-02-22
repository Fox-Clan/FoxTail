using Microsoft.EntityFrameworkCore;
using NotEnoughLogs;
using StargateNetwork.Types;
using WebSocketSharp.Server;

namespace StargateNetwork;

public class StargateServer : IDisposable
{
    internal static StargateServer Instance { get; private set; }

    internal readonly Logger Logger;
    
    private readonly WebSocketServer _wsServer;
    private readonly StargateBunkumServer _bunkumServer;
    private readonly StargateConfiguration _config;

    private bool _shouldRun;

    public StargateServer(Logger logger, StargateConfiguration config)
    {
        Instance = this;

        this.Logger = logger;
        this._config = config;
        
        this._bunkumServer = new StargateBunkumServer();
        
        this._wsServer = new WebSocketServer(config.WebsocketHostUrl);
        this._wsServer.AddWebSocketService<StargateWebsocketClient>("/Echo");
    }

    public void Start()
    {
        this._shouldRun = true;
        
        using (StargateContext db = new())
        {
            db.Database.Migrate();
        }
        
        if(this._config.WebsocketEnabled)
            this._wsServer.Start();
        
        if(this._config.BunkumEnabled)
            this._bunkumServer.Start();
        
        Thread dbCleanThread = new(CleanStaleDb)
        {
            Name = "Stargate Database Cleaner",
            Priority = ThreadPriority.Lowest
        };
        dbCleanThread.Start();
    }

    public void Stop()
    {
        this._shouldRun = false;
        this._wsServer.Stop();
        this._bunkumServer.Stop();
    }
    
    private void CleanStaleDb()
    {
        try
        {
            while (this._shouldRun)
            {
                using (StargateContext db = new())
                {
                    IEnumerable<Stargate> gates = db.FindAllGates(true);

                    foreach (Stargate gate in gates)
                    {
                        if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - gate.UpdateDate <= 60) continue;

                        db.Remove(gate);
                        Logger.LogTrace("Stargate", "Cleaned stale stargate from database");
                    }

                    db.SaveChanges();
                }

                Thread.Sleep(60_000);
            }
        }
        catch (Exception e)
        {
            Logger.LogError("Stargate", "Exception caught when cleaning state gates: " + e);
        }
    }

    public void Dispose()
    {
        if (!this._shouldRun)
            return;
        
        Stop();
        GC.SuppressFinalize(this);
    }
}