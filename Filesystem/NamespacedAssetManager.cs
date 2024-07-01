using System.Collections.Concurrent;
using System.Reflection;
using AssetManagementBase;
using Shared;

namespace Filesystem;

public class NamespacedAssetManager {
    private readonly ConcurrentDictionary<string, AssetManager> _managerLookup = [];
    private readonly ConcurrentBag<string> _namespaces = [];
    
    public IEnumerable<string> Namespaces => _namespaces;

    public void AddProvider(string @namespace, Assembly assembly) {
        _managerLookup[@namespace] = AssetManager.CreateResourceAssetManager(assembly, "resources");
        _namespaces.Add(@namespace);
    }
    
    public void AddProvider(string @namespace, string path) {
        _managerLookup[@namespace] = AssetManager.CreateFileAssetManager(path);
        _namespaces.Add(@namespace);
    }

    public T? UseLoader<T>(Identifier assetId, AssetLoader<T> loader, ErrorHelper error) {
        return _managerLookup[assetId.Namespace].UseLoader(loader, assetId.Path, tag: new Tag(assetId, error));
    }

    public T? UseLoader<T>(Identifier assetId, string assetFileExtension, AssetLoader<T> loader, ErrorHelper error) {
        return _managerLookup[assetId.Namespace].UseLoader(loader, $"{assetId.Path}.{assetFileExtension}", tag: new Tag(assetId, error));
    }

    public T? UseLoader<T, TCtx>(Identifier assetId, AssetLoader<T> loader, ErrorHelper error, TCtx ctx) where TCtx : class {
        return _managerLookup[assetId.Namespace].UseLoader(loader, assetId.Path, tag: new Tag(assetId, error, ctx));
    }
    public T? UseLoader<T, TCtx>(Identifier assetId, string assetFileExtension, AssetLoader<T> loader, ErrorHelper error, TCtx ctx) where TCtx : class {
        return _managerLookup[assetId.Namespace].UseLoader(loader, $"{assetId.Path}.{assetFileExtension}", tag: new Tag(assetId, error, ctx));
    }


    public static AssetLoader<T> CreateLoader<T>(Func<AssetManager, string, Identifier, ErrorHelper, T> loader) {
        return (manager, name, _, tag) => {
            (tag as Tag)!.Deconstruct(out Identifier assetId, out ErrorHelper error, out object _);
 
            error.Path = $"{assetId.Namespace}:{name.TrimStart('/')}";
            return loader(manager, name, assetId, error);
        };
    }
    
    public static AssetLoader<T> CreateLoader<T, TCtx>(Func<AssetManager, string, Identifier, ErrorHelper, TCtx, T> loader) where TCtx : class {
        return (manager, name, _, tag) => {
            (tag as Tag)!.Deconstruct(out Identifier assetId, out ErrorHelper error, out object? context);
            
            error.Path = $"{assetId.Namespace}:{name.TrimStart('/')}";
            return loader(manager, name, assetId, error, (context as TCtx)!);
        };
    }
    
    public record Tag(Identifier AssetId, ErrorHelper Error, object? Context = null);

    public void Reset(params (Assembly assembly, string @namespace)[] namespacedAssemblies) {
        _managerLookup.Clear();
        _namespaces.Clear();
        foreach ((Assembly assembly, string @namespace) in namespacedAssemblies) {
            AddProvider(@namespace, assembly);
        }
    }
}