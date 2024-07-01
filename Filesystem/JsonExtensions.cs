using System.Text.Json;
using System.Text.Json.Nodes;
using Shared;

namespace Filesystem;

/// <summary>
/// Generic wrapper functions for parsing JSON objects, also with <see cref="Identifier"/> and <see cref="ErrorHelper"/> support.
/// <seealso cref="Identifier"/>
/// <seealso cref="ErrorHelper"/>
/// </summary>
public static class JsonExtensions {
    public static bool TryGetNumber<TNumber>(this JsonObject self, string key, out TNumber value) {
        value = default!;
        return self[key] != null && self[key]!.GetValueKind() == JsonValueKind.Number && (value = self[key]!.GetValue<TNumber>()) != null;
    }

    public static bool TryGetObject(this JsonObject self, string key, out JsonObject jsonObject) {
        jsonObject = null!;
        return self[key] != null && self[key]!.GetValueKind() == JsonValueKind.Object && (jsonObject = self[key]!.AsObject()) != null;
    }

    public static bool TryGetArray(this JsonObject self, string key, out JsonArray jsonArray) {
        jsonArray = null!;
        return self[key] != null && self[key]!.GetValueKind() == JsonValueKind.Array && (jsonArray = self[key]!.AsArray()) != null;
    }

    public static bool TryGetIdentifier(this JsonObject self, string key, out Identifier identifier) {
        identifier = default!;
        if (self[key] == null || self[key]!.GetValueKind() != JsonValueKind.String) return false;
        return Identifier.TryParse(self[key]!.GetValue<string>(), out identifier);
    }

    public static bool TryGetIdentifier(this JsonObject self, string key, Identifier.Factory factory, out Identifier identifier) {
        if (!self.TryGetIdentifier(key, out identifier)) return false;
        identifier = factory.Validate(identifier);
        return true;
    }

    public static bool TryGetString(this JsonObject self, string key, out string str) {
        str = default!;
        if (self[key] == null || self[key]!.GetValueKind() != JsonValueKind.String) return false;
        str = self[key]!.GetValue<string>();
        return true;
    }

    public static JsonArray GetArray(this JsonObject self, string objectPath, string key, ErrorHelper error) {
        if (!self.TryGetArray(key, out JsonArray val))
            throw error.Create($"{objectPath}.{key} must be an Array");

        return val;
    }

    public static Identifier GetIdentifier(this JsonObject self, string objectPath, string key, ErrorHelper error) {
        if (!self.TryGetString(key, out string str))
            throw error.Create($"{objectPath} must be a String Identifier (ex: 'namespace:path')");

        if (!Identifier.TryParse(str, out Identifier? id, out string? err))
            throw error.Create(err!);
        
        return id!.Value;
    }

    public static Identifier GetIdentifier(this JsonObject self, string objectPath, string key, Identifier.Factory factory, ErrorHelper error) {
        return factory.Validate(self.GetIdentifier(objectPath, key, error));
    }

    public static string GetString(this JsonObject self, string objectPath, string key, ErrorHelper error) {
        if (!self.TryGetString(key, out string id))
            throw error.Create($"{objectPath} must be a String");

        return id;
    }

    public static JsonObject MustBeObject(this JsonNode? self, string objectPath, ErrorHelper error) {
        if (self == null || self.GetValueKind() != JsonValueKind.Object)
            throw error.Create($"{objectPath} must be an Object");
        return self.AsObject();
    }

    public static JsonObject GetObject(this JsonObject self, string objectPath, string key, ErrorHelper error) {
        if (!self.TryGetObject(key, out JsonObject val))
            throw error.Create($"{objectPath}.{key} must be an Object");

        return val;
    }

    public static JsonObject? MustBeObjectOrNull(this JsonNode? self, string objectPath, ErrorHelper error) {
        if (self == null)
            return null;
        if (self.GetValueKind() != JsonValueKind.Object)
            throw error.Create($"{objectPath} must be an Object");
        return self.AsObject();
    }

    public static string MustBeString(this JsonNode? self, string objectPath, ErrorHelper error) {
        if (self == null || self.GetValueKind() != JsonValueKind.String)
            throw error.Create($"{objectPath} must be a String");
        return self.GetValue<string>();
    }

    public static Identifier MustBeIdentifier(this JsonNode? self, string objectPath, ErrorHelper error) {
        if (self == null || self.GetValueKind() != JsonValueKind.String)
            throw error.Create($"{objectPath} must be a String Identifier (ex: 'namespace:path')");
        return new Identifier(self.GetValue<string>());
    }

    public static bool MustBeBool(this JsonNode? self, string objectPath, ErrorHelper error) {
        if (self?.GetValueKind() is not (JsonValueKind.True or JsonValueKind.False))
            throw error.Create($"{objectPath} must be a Boolean (values: {bool.TrueString} - {bool.FalseString})");
        return self.GetValue<bool>();
    }

    private static TNumber MustBeNumber<TNumber>(this JsonNode? self, string objectPath, ErrorHelper error, string typeName, object min, object max) {
        if (self?.GetValueKind() != JsonValueKind.Number)
            throw error.Create($"{objectPath} must be a {typeName} (values: {min} - {max})");
        return self.GetValue<TNumber>();
    }

    public static ushort MustBeUShort(this JsonNode? self, string objectPath, ErrorHelper helper) {
        return MustBeNumber<ushort>(self, objectPath, helper, "unsigned short", ushort.MinValue, ushort.MaxValue);
    }

    public static int MustBeInt(this JsonNode? self, string objectPath, ErrorHelper helper) {
        return MustBeNumber<int>(self, objectPath, helper, "int", int.MinValue, int.MaxValue);
    }

    public static int[] MustBeIntArray(this JsonNode? self, string objectPath, int? minLength, int? maxLength, ErrorHelper error) {
        if (self?.GetValueKind() is JsonValueKind.Array) {
            JsonArray arr = self.AsArray();
            int[] a = arr.Select((node, i) => node.MustBeInt($"{objectPath}[{i}]", error)).ToArray();
            if (minLength.HasValue && a.Length < minLength.Value) Error();
            if (maxLength.HasValue && a.Length > maxLength.Value) Error();
            return a;
        }
        Error();
        
        return null!;

        void Error() {
            string ext = "";
            if (minLength.HasValue && maxLength.HasValue && minLength == maxLength) {
                ext = $"Length must equal {minLength}";
            } else {
                if (minLength.HasValue) {
                    ext = $"Minumum length: {minLength.Value}";
                }
                if (maxLength.HasValue) {
                    if (minLength.HasValue) ext += ", ";
                    ext += $"Maximum length: {maxLength.Value}";
                }
            }

            if (ext.Length != 0) {
                ext = $" ({ext})";
            }
        
            throw error.Create($"{objectPath} must be an Integer Array{ext}");
        }
    }

    public static int[] MustBeIntArray(this JsonNode? self, string objectPath, int? length, ErrorHelper helper) => MustBeIntArray(self, objectPath, length, length, helper);
    public static int[] MustBeIntArray(this JsonNode? self, string objectPath, ErrorHelper helper) => MustBeIntArray(self, objectPath, null, null, helper);

}