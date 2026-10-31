namespace ExtensionMethodsDi.Api;

public interface IGreeter { string Greet(string name); }

public sealed class GreeterOptions { public string Greeting { get; set; } = "Hi"; }

public sealed class Greeter(Microsoft.Extensions.Options.IOptions<GreeterOptions> options, TimeProvider clock) : IGreeter
{
    public string Greet(string name) => $"{options.Value.Greeting}, {name}! ({clock.GetUtcNow():HH:mm} UTC)";
}

public sealed class AuditLogOptions { public string Prefix { get; set; } = "AUDIT"; }

public interface IAuditLog { string Format(string message); }

public sealed class AuditLog(Microsoft.Extensions.Options.IOptions<AuditLogOptions> options) : IAuditLog
{
    public string Format(string message) => $"[{options.Value.Prefix}] {message}";
}
