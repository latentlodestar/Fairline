using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Fairline.Abstractions.Contracts;
using Fairline.Abstractions.Interfaces;

namespace Fairline.Infrastructure.Providers;

public sealed class OddsApiClient : IOddsApiClient
{
    private readonly HttpClient _http;
    private readonly OddsApiOptions _options;
    private readonly SemaphoreSlim _throttle;
    private readonly ILogger<OddsApiClient> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public OddsApiClient(HttpClient http, IOptions<OddsApiOptions> options, ILogger<OddsApiClient> logger)
    {
        _http = http;
        _options = options.Value;
        _logger = logger;
        _throttle = new SemaphoreSlim(_options.MaxConcurrentRequests, _options.MaxConcurrentRequests);

        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            _logger.LogWarning("OddsApi:ApiKey is not configured. API calls will fail.");

        _http.BaseAddress = new Uri(_options.BaseUrl);
    }

    public async Task<(IReadOnlyList<OddsApiSport> Sports, string RawJson)> GetSportsAsync(CancellationToken ct)
    {
        var url = $"/v4/sports/?apiKey={_options.ApiKey}&all=true";
        var rawJson = await ExecuteWithRetryAsync(url, ct);
        var sports = JsonSerializer.Deserialize<List<OddsApiSport>>(rawJson, JsonOptions) ?? [];
        return (sports, rawJson);
    }

    public async Task<IReadOnlyList<OddsApiOddsEvent>> GetOddsAsync(string sportKey, OddsRequestOptions options, CancellationToken ct)
    {
        var standardMarkets = options.Markets.Where(m => m != "outrights").ToList();
        var hasOutrights = options.Markets.Any(m => m == "outrights");

        var regionParams = BuildRegionParams(options);
        var results = new List<OddsApiOddsEvent>();

        if (standardMarkets.Count > 0)
        {
            var url = $"/v4/sports/{sportKey}/odds/?apiKey={_options.ApiKey}&markets={string.Join(",", standardMarkets)}&oddsFormat=decimal&dateFormat=iso{regionParams}";
            var rawJson = await ExecuteWithRetryAsync(url, ct);
            results.AddRange(JsonSerializer.Deserialize<List<OddsApiOddsEvent>>(rawJson, JsonOptions) ?? []);
        }

        if (hasOutrights)
        {
            var url = $"/v4/sports/{sportKey}/odds/?apiKey={_options.ApiKey}&markets=outrights&oddsFormat=decimal&dateFormat=iso{regionParams}";
            try
            {
                var rawJson = await ExecuteWithRetryAsync(url, ct);
                var outrightEvents = JsonSerializer.Deserialize<List<OddsApiOddsEvent>>(rawJson, JsonOptions) ?? [];
                results = MergeEvents(results, outrightEvents);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.UnprocessableEntity)
            {
                _logger.LogInformation("Outrights market not supported for {SportKey}, skipping", sportKey);
            }
        }

        return results;
    }

    private static string BuildRegionParams(OddsRequestOptions options)
    {
        if (options.Bookmakers is { Count: > 0 })
            return $"&bookmakers={string.Join(",", options.Bookmakers)}";
        if (options.Regions is { Count: > 0 })
            return $"&regions={string.Join(",", options.Regions)}";
        return "&regions=us";
    }

    private static List<OddsApiOddsEvent> MergeEvents(List<OddsApiOddsEvent> primary, List<OddsApiOddsEvent> secondary)
    {
        var byId = primary.ToDictionary(e => e.Id);
        foreach (var evt in secondary)
        {
            if (byId.TryGetValue(evt.Id, out var existing))
            {
                // Merge bookmakers from the outrights response into the existing event
                var mergedBookmakers = existing.Bookmakers.ToList();
                mergedBookmakers.AddRange(evt.Bookmakers);
                byId[evt.Id] = existing with { Bookmakers = mergedBookmakers };
            }
            else
            {
                byId[evt.Id] = evt;
            }
        }
        return [.. byId.Values];
    }

    private async Task<string> ExecuteWithRetryAsync(string url, CancellationToken ct)
    {
        await _throttle.WaitAsync(ct);
        try
        {
            for (var attempt = 0; attempt <= _options.RetryCount; attempt++)
            {
                try
                {
                    var response = await _http.GetAsync(url, ct);

                    if (response.StatusCode == HttpStatusCode.TooManyRequests)
                    {
                        var retryAfter = response.Headers.RetryAfter?.Delta
                            ?? TimeSpan.FromMilliseconds(_options.RetryBaseDelayMs * Math.Pow(2, attempt));
                        _logger.LogWarning("Rate limited (429). Retrying after {Delay}ms", retryAfter.TotalMilliseconds);
                        await Task.Delay(retryAfter, ct);
                        continue;
                    }

                    if ((int)response.StatusCode >= 500 && attempt < _options.RetryCount)
                    {
                        var delay = TimeSpan.FromMilliseconds(_options.RetryBaseDelayMs * Math.Pow(2, attempt));
                        _logger.LogWarning("Server error {StatusCode}. Retrying after {Delay}ms", (int)response.StatusCode, delay.TotalMilliseconds);
                        await Task.Delay(delay, ct);
                        continue;
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        var body = await response.Content.ReadAsStringAsync(ct);
                        _logger.LogWarning("API returned {StatusCode} for {Url}: {Body}", (int)response.StatusCode, url, body);
                        response.EnsureSuccessStatusCode();
                    }

                    return await response.Content.ReadAsStringAsync(ct);
                }
                catch (HttpRequestException) when (attempt < _options.RetryCount)
                {
                    var delay = TimeSpan.FromMilliseconds(_options.RetryBaseDelayMs * Math.Pow(2, attempt));
                    _logger.LogWarning("Request failed. Retrying after {Delay}ms", delay.TotalMilliseconds);
                    await Task.Delay(delay, ct);
                }
            }

            throw new InvalidOperationException($"All {_options.RetryCount + 1} attempts failed for {url}");
        }
        finally
        {
            _throttle.Release();
        }
    }
}
