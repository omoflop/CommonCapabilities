using System.Text.Json.Nodes;
using AssetManagementBase;
using Shared;

namespace Filesystem.Loaders;

public static class JsonLoader {
    public static JsonNode LoadJson(this NamespacedAssetManager manager, Identifier id, bool failSilently = false) {
        return manager.UseLoader(id, "json", Loader, new ErrorHelper("JSON", id.ToString()), new Ctx(failSilently))!;
    }

    public static readonly AssetLoader<JsonNode> Loader = NamespacedAssetManager.CreateLoader<JsonNode, Ctx>((manager, name, id, error, ctx) => {
        if (!manager.Exists(name)) {
            if (ctx.FailSilently) return null!;
            throw error.Create("File does not exist");
        }
        
        JsonNode? node = JsonNode.Parse(manager.ReadAsString(name));
        if (node != null) return node;
        
        if (ctx.FailSilently) return null!;
        throw error.Create("Invalid JSON");

    });

    public record Ctx(bool FailSilently);
}