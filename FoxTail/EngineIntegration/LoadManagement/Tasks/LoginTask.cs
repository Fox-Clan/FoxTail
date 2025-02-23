using FoxTail.Common;
using SkyFrost.Base;

namespace FoxTail.EngineIntegration.LoadManagement.Tasks;

public class LoginTask : InitTask
{
    public override string Name => "Cloud Login";
    public override InitTaskStage Stage => InitTaskStage.Login;

    public override Task ExecuteAsync(HeadlessContext context)
    {
        string? username = Environment.GetEnvironmentVariable("RESO_USER");
        string? password = Environment.GetEnvironmentVariable("RESO_PASS");

        if (username == null || password == null)
        {
            context.Logger.LogWarning(ResoCategory.Cloud, "Credentials were not provided; skipping login. " +
                                                  "You can provide them with the RESO_USER and RESO_PASS environment variables.");

            return Task.CompletedTask;
        }
            
        
        return context.Engine.Cloud.Session.Login(username, new PasswordLogin(password), context.Engine.LocalDB.SecretMachineID, false, "");
    }
}