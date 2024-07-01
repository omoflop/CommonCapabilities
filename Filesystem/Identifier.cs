using Shared;

namespace Filesystem;

public readonly struct Identifier {
    internal const string BuiltinNamespace = "builtin";

    public readonly string Namespace;
    public readonly string Path;


    public Identifier(string @namespace, string path) {
        Namespace = @namespace;
        Path = path;
    }

    public override string ToString() {
        return $"{Namespace}:{Path}";
    }

    private static bool ValidPath(char c) => char.IsLetterOrDigit(c) || c == '_' || c == '/';
    private static bool ValidNamespace(char c) => char.IsLetterOrDigit(c) || c == '_';

    public Identifier(string str, ErrorHelper? helper = null) {
        if (!str.Equals(str, StringComparison.CurrentCultureIgnoreCase)) {
            throw ErrorHelper.Create(helper, $"{str} is not a valid Identifier! (Hint: Invalid character, valid characters include: a-z, 0-9, '_', '/')");
        }

        if (!str.Contains(':')) {
            Namespace = BuiltinNamespace;
            Path = str;
        }
        else {
            string[] args = str.Split(':');
            if (args.Length > 2)
                throw ErrorHelper.Create(helper, $"{str} is not a valid Identifier! (Hint: Identifiers must only contain one or no instance of the character ':')");
            Namespace = args[0];
            Path = args[1];
        }

        if (!Namespace.All(ValidNamespace))
            throw ErrorHelper.Create(helper, $"{str} is not a valid Identifier! (Hint: Invalid character in Namespace, character must be: a-z, 0-9, '_')");

        if (!Path.All(ValidPath))
            throw ErrorHelper.Create(helper, $"{str} is not a valid Identifier! (Hint: Invalid character in Path, character must be: a-z, 0-9, '_', '/')");
    }
    
    public static bool TryParse(string str, out Identifier? identifier, out string? error) {
        identifier = null;
        error = null;
        
        string ns, path;
        
        if (!str.Equals(str, StringComparison.CurrentCultureIgnoreCase)) {
            error = $"{str} is not a valid Identifier! (Hint: Invalid character, valid characters include: a-z, 0-9, '_', '/')";
            return false;
        }

        if (!str.Contains(':')) {
            ns = BuiltinNamespace;
            path = str;
        }
        else {
            string[] args = str.Split(':');
            if (args.Length > 2) {
                error = $"{str} is not a valid Identifier! (Hint: Identifiers must only contain one or no instance of the character ':')";
                return false;
            }
            ns = args[0];
            path = args[1];
        }

        if (!ns.All(ValidNamespace)) {
            error = $"{str} is not a valid Identifier! (Hint: Invalid character in Namespace, character must be: a-z, 0-9, '_')";
            return false;
        }

        if (!path.All(ValidPath)) {
            error = "{str} is not a valid Identifier! (Hint: Invalid character in Namespace, character must be: a-z, 0-9, '_', '/')";
            return false;
        }

        identifier = new Identifier(ns, path);
        return true;
    }

    public Identifier WithPath(string path) {
        return new Identifier(Namespace, path);
    }

    public static bool TryParse(string str, out Identifier identifier) {
        identifier = default!;
        if (!str.Equals(str, StringComparison.CurrentCultureIgnoreCase)) {
            return false;
        }

        string ns, path;
        
        if (!str.Contains(':')) {
            ns = BuiltinNamespace;
            path = str;
        }
        else {
            string[] args = str.Split(':');
            if (args.Length > 2)
                return false;
            ns = args[0];
            path = args[1];
        }

        if (!ns.All(ValidNamespace) || !path.All(ValidPath)) return false;
        identifier = new Identifier(ns, path);
        return true;
    }

    public class Factory(string path) {
        public readonly string FullPath = path;
        private readonly string[] _dirs = path.Split("/").Select(d => d + '/').ToArray();

        public Identifier Validate(Identifier id) {

            string path = id.Path;
            foreach (string baseDir in _dirs) {
                if (path.StartsWith(baseDir)) {
                    path = path[baseDir.Length..];
                }
            }


            return new Identifier(id.Namespace, $"{FullPath}/{path}");
        }

        public Identifier Create(string str) => Validate(new Identifier(str));
    }
}