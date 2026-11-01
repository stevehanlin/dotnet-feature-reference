namespace HttpClients.Api;

/// <summary>What the (imaginary) upstream quotes service returns.</summary>
public sealed record Quote(int Id, string Text);

/// <summary>What our endpoints return: the quote plus which approach fetched it.</summary>
public sealed record QuoteResult(string Approach, Quote Quote);

public sealed class QuotesOptions
{
    public string BaseUrl { get; set; } = "https://quotes.example.test";
    public string ApiKey { get; set; } = "demo-key";
}

public static class ClientNames
{
    public const string Named = "quotes";
    public const string Keyed = "quotes-keyed";
}
