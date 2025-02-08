using System.Diagnostics;
using Newtonsoft.Json;

namespace FoxTail.Configuration;

public static class ConfigHelper
{
    public static TConfig GetOrCreateConfig<TConfig>(HeadlessContext context, string filename) where TConfig : class, new()
    {
        context.Logger.LogTrace(ResoCategory.Config, $"Trying to load config {filename}");
        Debug.Assert(filename.EndsWith(".json"));

        TConfig? config;
        if (!File.Exists(filename))
        {
            context.Logger.LogTrace(ResoCategory.Config, "File does not exist, creating & writing");
            config = new TConfig();
            File.WriteAllText(filename, JsonConvert.SerializeObject(config));
            context.Logger.LogInfo(ResoCategory.Config, $"The config file `{filename}` did not exist. A new one was created in the current directory.");
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