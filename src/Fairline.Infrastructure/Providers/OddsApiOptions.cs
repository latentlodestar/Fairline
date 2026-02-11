namespace Fairline.Infrastructure.Providers;

public sealed class OddsApiOptions
{
    public const string SectionName = "OddsApi";

    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.the-odds-api.com";
    public int MaxConcurrentRequests { get; set; } = 3;
    public int RetryCount { get; set; } = 3;
    public int RetryBaseDelayMs { get; set; } = 1000;
}
