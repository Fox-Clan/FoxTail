using FrooxEngine;
using NotEnoughLogs;
using SkyFrost.Base;

namespace JvyHeadlessRunner.EngineIntegration.LoadManagement.Tasks;

public class LoginTask : InitTask
{
    public override string Name => "Cloud Login";
    public override InitTaskStage Stage => InitTaskStage.Login;

    public override Task ExecuteAsync(Logger logger, Engine engine)
    {
        string? username = Environment.GetEnvironmentVariable("RESO_USER");
        string? password = Environment.GetEnvironmentVariable("RESO_PASS");

        if (username == null || password == null)
        {
            logger.LogWarning(ResoCategory.Cloud, "Credentials were not provided; skipping login. " +
                                                  "You can provide them with the RESO_USER and RESO_PASS environment variables.");

            return Task.CompletedTask;
        }
            
        
        return engine.Cloud.Session.Login(username, new PasswordLogin(password), engine.LocalDB.SecretMachineID, false, "");
    }
}