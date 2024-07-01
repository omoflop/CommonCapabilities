using Shared;

namespace Filesystem.Loaders;

using System.Diagnostics;
using AssetManagementBase;
using Microsoft.Xna.Framework.Graphics;


internal static class EffectLoader {
    public static Effect LoadEffect(this NamespacedAssetManager manager, Identifier id, GraphicsDevice device) {
        return manager.UseLoader(id, Loader, new ErrorHelper("Effect", id.ToString()), device)!;
    }

    public static readonly AssetLoader<Effect> Loader = NamespacedAssetManager.CreateLoader<Effect, GraphicsDevice>((manager, path, id, error, device) => {
        if (!manager.Exists($"{path}.fx"))
            throw error.Create("File does not exist");

#if DEBUG
        string fullPath = Path.GetFullPath($"../../../resources{path}");
        
        File.Delete($"{fullPath}.xnb");
        if (!Bash($"{Environment.GetEnvironmentVariable("HOME")}/.dotnet/tools/mgfxc", 10000, [$"{fullPath}.fx", $"{fullPath}.xnb"]))
            throw error.Create("Took too long to compile! (10+ seconds)");
        Thread.Sleep(100);
        while (!File.Exists($"{fullPath}.xnb")) Thread.Sleep(100);
#endif

        Effect e = new(device, manager.ReadAsByteArray($"{path}.xnb"));
        return e;
    });

    private static bool Bash(string command, int maxMs, params string[] args) {
        Process process = new() {
            StartInfo = new ProcessStartInfo(command, args) {
                CreateNoWindow = true,
                UseShellExecute = true,
                EnvironmentVariables = {
                    ["DOTNET_ROOT"] = $"{Environment.GetEnvironmentVariable("HOME")}/.dotnet", 
                    ["PATH"] = $"{Environment.GetEnvironmentVariable("PATH")}:{Environment.GetEnvironmentVariable("HOME")}/.dotnet:{Environment.GetEnvironmentVariable("HOME")}/.dotnet/tools",
                    ["MGFXC_WINE_PATH"] = $"{Environment.GetEnvironmentVariable("HOME")}/.wine"
                }
            }
        };

        process.ErrorDataReceived += (sender, eventArgs) => Console.WriteLine(eventArgs.Data);

        process.Start();
        return process.WaitForExit(maxMs);
    }
}