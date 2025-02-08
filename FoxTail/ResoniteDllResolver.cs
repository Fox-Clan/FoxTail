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
        // Console.Write($"NATIVE LOAD: {libraryName} (from {assembly.FullName}) (sp: {searchPath}) = ");
        try
        {
            return NativeLibrary.Load(libraryName, assembly, searchPath);
        }
        catch (DllNotFoundException)
        {
            return NativeLibrary.Load(@"D:\headless\Headless\" + libraryName);
        }
    }

    private static Assembly? ResolveAssembly(object? sender, ResolveEventArgs args)
    {
        Console.BackgroundColor = ConsoleColor.Red;
        Console.WriteLine("ASSEMBLY LOAD FAIL: " + args.Name);
        return null;
    }
}