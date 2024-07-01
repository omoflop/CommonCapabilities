using AssetManagementBase;
using Shared;

namespace Filesystem.Loaders;

public static class AssetListLoader {
    public static IEnumerable<Identifier> LoadAssetList(this NamespacedAssetManager manager, Identifier.Factory factory, string searchPattern = "*", params string[] ignoredDirectories) {
        List<Identifier> idList = [];
        foreach (string ns in manager.Namespaces) {
            Identifier id = new(ns, factory.FullPath);
            IEnumerable<Identifier> ids = manager.UseLoader(id, Loader, new ErrorHelper("Asset List", id.ToString()), new Args(ignoredDirectories, searchPattern))!.Select(factory.Validate);;
            
            idList.AddRange(ids);
        }

        return idList;
    }

    public static readonly AssetLoader<IEnumerable<Identifier>> Loader = NamespacedAssetManager.CreateLoader<IEnumerable<Identifier>, Args>((manager, path, id, error, args) => {
#if DEBUG
        string d = $"../../../resources/{path}";

        string[] files = Directory.GetFiles(d, args.SearchPattern, new EnumerationOptions {
            RecurseSubdirectories = true
        }).Where(p => !p.EndsWith('_')).Select(p => p[(d.Length + 1)..(p.LastIndexOf('.'))]).Where(p => !args.IgnoredDirs.Any(p.StartsWith)).ToArray();
        File.WriteAllLines($"{d}/.list", files);
        return files.Select(str => new Identifier(id.Namespace, str));
#else
    if (!manager.Exists(path + "/.list"))
            throw error.Create("Asset List does not exist");
        return manager.ReadAsString($"{path}.list").Split("\n").Select(str => new Identifier(id.Namespace, str)).ToArray();
#endif
    });

    private record Args(string[] IgnoredDirs, string SearchPattern);
}