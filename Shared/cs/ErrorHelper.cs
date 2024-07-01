namespace Shared;

public record ErrorHelper(string Name, string ErrorTargetName) {
    public Exception Create(string reason) {
        string ext = "";
        if (Path != null) {
            ext = $" (Path: \"{Path}\")";
        }
        return new Exception($"Invalid {Name}: '{ErrorTargetName}', {reason}{ext}");
    }

    public string? Path { get; set; }

    public static Exception Create(ErrorHelper? self, string reason) {
        return self?.Create(reason) ?? new Exception($"Invalid format: {reason}");
    }

    public ErrorHelper WithTarget(string target, string path) => new(Name, target) { Path = path };
    public ErrorHelper WithTarget(string target) => this with {
        ErrorTargetName = target
    };
}

public static class ErrorHelperExtensions {
    public static Exception Create(this ErrorHelper? self, string reason) {
        return self?.Create(reason) ?? new Exception($"Invalid format: {reason}");
    }
}