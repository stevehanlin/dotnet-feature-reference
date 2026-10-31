using System.ComponentModel.DataAnnotations;

namespace Options.Api;

public sealed class GreetingOptions
{
    public const string SectionName = "Greeting";

    [Required, MinLength(2)]
    public string Message { get; init; } = "";

    [Range(1, 10)]
    public int Repeat { get; init; } = 1;
}

public sealed class FeatureOptions
{
    public const string SectionName = "Features";

    public bool EnableBeta { get; init; }
    public string? BetaKey { get; init; }
}
