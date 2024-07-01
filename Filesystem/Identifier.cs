using Shared;

namespace Filesystem;

/// <summary>
/// Represents an identifier with a namespace and a path
/// </summary>
public readonly struct Identifier {
    internal const string BuiltinNamespace = "builtin";

    public readonly string Namespace;
    public readonly string Path;


    /// <summary>
    /// Initalizes a new instance of the <see cref="Identifier"/> struct. 
    /// </summary>
    /// <param name="namespace">The namespace of the identifier</param>
    /// <param name="path">The path of the identifier</param>
    public Identifier(string @namespace, string path) {
        Namespace = @namespace;
        Path = path;
    }

    /// <summary>
    /// Returns the string representation of this <see cref="Identifier"/>
    /// </summary>
    /// <returns></returns>
    public override string ToString() {
        return $"{Namespace}:{Path}";
    }

    private static bool ValidPath(char c) => char.IsLetterOrDigit(c) || c == '_' || c == '/';
    private static bool ValidNamespace(char c) => char.IsLetterOrDigit(c) || c == '_';

    /// <summary>
    /// Alternate constructor for the <see cref="Identifier"/> struct, that splits an identifier from a string. Optionally, an <see cref="ErrorHelper"/> instance can be provided to provide more context to an error if one occurs.
    /// <seealso cref="TryParse(string,out System.Nullable{Filesystem.Identifier},out string?)"/>
    /// </summary>
    /// <param name="str">The string that will be split into an Identifier</param>
    /// <param name="helper">Optional helper class for error context</param>
    /// <exception cref="Exception">Thrown if the Identifier fails to be created from the provided string</exception>
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
    
    /// <summary>
    /// Tries to parse the given string into an Identifier. Returns true when successful, and provides the identifier. Otherwise, it provides an error string that can be used to determine what went wrong.
    /// <seealso cref="Identifier(string, ErrorHelper?)"/>
    /// <seealso cref="TryParse(string,out Filesystem.Identifier)"/>
    /// </summary>
    /// <param name="str">The string that will be split into an Identifier</param>
    /// <param name="identifier">The resulting Identifier instance if the input string is valid</param>
    /// <param name="error">The error message if the input string is invalid</param>
    /// <returns></returns>
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

    /// <summary>
    /// Create a new Identifier instance under the same namespace but with a different path.
    /// </summary>
    /// <param name="path">The new path of the Identifier</param>
    /// <returns>New instance with path changed</returns>
    public Identifier WithPath(string path) {
        return new Identifier(Namespace, path);
    }

    /// <summary>
    /// Tries to parse the given string into an Identifier. Returns true when successful, and provides the identifier.
    /// <seealso cref="Identifier(string, ErrorHelper?)"/>
    /// <seealso cref="TryParse(string,out System.Nullable{Filesystem.Identifier},out string?)"/>
    /// </summary>
    /// <param name="str">The string to be split into an Identifier</param>
    /// <param name="identifier">The resulting Identifier given the string is valid</param>
    /// <returns>Whether the Identifier was parsed without error</returns>
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

    /// <summary>
    /// A class that acts like a template for <see cref="Identifier"/> instances, where identifiers can be validated to use its path.
    /// <example> <code>
    /// Identifier.Factory tileTextures = new Identifier.Factory("assets/textures/tile");
    ///  
    /// // returns "assets/textures/tile/wood0"
    /// tileTextures.Create("wood0");
    ///  
    /// // returns "some_mod:assets/textures/tile/slate4"
    /// tileTextures.Create("some_mod:slate4");
    ///  
    /// // returns "some_mod:assets/textures/tile/slate3"
    /// tileTextures.Validate(new Identifier("some_mod", "slate3")); 
    /// </code> </example>
    /// </summary>
    /// <param name="path">The path of this template</param>
    public class Factory(string path) {
        public readonly string FullPath = path;
        private readonly string[] _dirs = path.Split("/").Select(d => d + '/').ToArray();

        /// <summary>
        /// Makes sure the given <see cref="Identifier"/> uses the path provided by this <see cref="Factory"/>
        /// <seealso cref="Create"/>
        /// </summary>
        /// <param name="id">Identifier to validate</param>
        /// <returns>Validated identifier</returns>
        public Identifier Validate(Identifier id) {
            string path = id.Path;
            foreach (string baseDir in _dirs) {
                if (path.StartsWith(baseDir)) {
                    path = path[baseDir.Length..];
                }
            }


            return new Identifier(id.Namespace, $"{FullPath}/{path}");
        }

        /// <summary>
        /// Creates a new Identifier from this string and validates it.
        /// <seealso cref="Validate"/>
        /// </summary>
        /// <param name="str">The string to be split into an Identifier and validated</param>
        /// <returns>Validated identifier</returns>
        public Identifier Create(string str) => Validate(new Identifier(str));
    }
}