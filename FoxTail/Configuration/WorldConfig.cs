using FoxTail.EngineIntegration.LoadManagement;
using FoxTail.Worlds;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace FoxTail.Configuration;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class WorldConfig : IConfigWithExampleLayout
{
    public IEnumerable<KnownWorld> KnownWorlds { get; set; } = [];

    public IEnumerable<FoxWorldStartSettings> CompileAutoLoadWorlds()
    {
        return KnownWorlds
            .Where(w => w.StartWithServer)
            .Select(w => w.Compile());
    }

    public string? GetKnownWorldUrlById(string id)
    {
        return this.KnownWorlds.FirstOrDefault(w => w.KnownWorldId == id)?.RecordUrl;
    }

    public void SetupExampleLayout()
    {
        this.KnownWorlds = new List<KnownWorld>
        {
            new()
        };
    }
}