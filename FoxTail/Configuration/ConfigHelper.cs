using System.Diagnostics;
using Newtonsoft.Json;

namespace FoxTail.Configuration;

internal static class ConfigHelper
{
    internal static TConfig GetOrCreateConfig<TConfig>(HeadlessContext context, string filename, ref bool created) where TConfig : class, new()
    {
        context.Logger.LogTrace(ResoCategory.Config, $"Trying to load config {filename}");
        Debug.Assert(filename.EndsWith(".json"));

        TConfig? config;
        if (!File.Exists(filename))
        {
            context.Logger.LogTrace(ResoCategory.Config, "File does not exist, creating & writing");
            config = new TConfig();

            if (config is IConfigWithExampleLayout example)
                example.SetupExampleLayout();
            
            File.WriteAllText(filename, JsonConvert.SerializeObject(config, Formatting.Indented));
            context.Logger.LogWarning(ResoCategory.Config, $"The config file `{filename}` did not exist. A new one was created in the current directory.");
            created = true;
        }
        else
        {
            context.Logger.LogTrace(ResoCategory.Config, "File exists, deserializing.");
            config = JsonConvert.DeserializeObject<TConfig>(File.ReadAllText(filename));

            if (config == null)
                throw new Exception("Could not load config, serialization returned null.");
        }
        
        return config;
    }
}