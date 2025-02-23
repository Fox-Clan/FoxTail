using System.Reflection;
using System.Runtime.InteropServices;

namespace FoxTail;

internal static class ResoniteDllResolver
{
    internal static void Initialize()
    {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            NativeLibrary.SetDllImportResolver(assembly, ResolveNativeAssembly);
        }

        AppDomain.CurrentDomain.AssemblyLoad += (_, args) =>
        {
            NativeLibrary.SetDllImportResolver(args.LoadedAssembly, ResolveNativeAssembly);
        };
    }

    private static IntPtr ResolveNativeAssembly(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        // string log = $"NATIVE LOAD: {libraryName} (from {assembly.FullName}) (sp: {searchPath}) = ";
        IntPtr ret = 0;
        try
        {
            ret = NativeLibrary.Load(libraryName, assembly, searchPath);
        }
        catch (DllNotFoundException)
        {
            string? headlessPath = Environment.GetEnvironmentVariable("HEADLESS_RUNTIMES_PATH");
            bool hadPath = headlessPath != null;

            headlessPath ??= "runtimes";

            try
            {
                string path = Path.Join(headlessPath, RuntimeInformation.RuntimeIdentifier, "native", libraryName);
                ret = NativeLibrary.Load(path);
            }
            catch (DllNotFoundException)
            {
                if(!hadPath)
                    throw new DllNotFoundException($"Could not find the native library '{libraryName}'. " +
                                                   $"Try setting the HEADLESS_RUNTIMES_PATH environment variable to the location of the headless 'runtimes' folder.");
            }
        }
        
        // log += $"0x{ret:X8}";
        // Console.WriteLine(log);

        return ret;
    }

    private static Assembly? ResolveAssembly(object? sender, ResolveEventArgs args)
    {
        Console.BackgroundColor = ConsoleColor.Red;
        Console.WriteLine("ASSEMBLY LOAD FAIL: " + args.Name);
        return null;
    }
}