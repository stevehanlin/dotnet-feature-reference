using Microsoft.Extensions.Options;

namespace Options.Api;

/// <summary>
/// Cross-property rules that data annotations cannot express.
/// Registered as a singleton so it is picked up for every options lifetime.
/// </summary>
public sealed class FeatureOptionsValidator : IValidateOptions<FeatureOptions>
{
    public ValidateOptionsResult Validate(string? name, FeatureOptions options) =>
        options is { EnableBeta: true, BetaKey: null or "" }
            ? ValidateOptionsResult.Fail("Features:BetaKey is required when Features:EnableBeta is true.")
            : ValidateOptionsResult.Success;
}
