using FoxTail.Users;
using HarmonyLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace FoxTail.Configuration;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class UserConfig : IConfigWithExampleLayout
{
    public KnownUser Owner { get; set; } = new();
    public IEnumerable<KnownUser> Users { get; set; } = [];
    
    public void SetupExampleLayout()
    {
        this.Owner.Ids = ["U-JohnDoe", "1337500214105342048"];
        this.Users.AddItem(new KnownUser { Ids = this.Owner.Ids });
    }
}