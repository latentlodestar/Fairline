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
        var markets = string.Join(",", options.Markets);
        var url = $"/v4/sports/{sportKey}/odds/?apiKey={_options.ApiKey}&markets={markets}&oddsFormat=decimal&dateFormat=iso";

        if (options.Bookmakers is { Count: > 0 })
        {
            url += $"&bookmakers={string.Join(",", options.Bookmakers)}";
        }
        else if (options.Regions is { Count: > 0 })
        {
            url += $"&regions={string.Join(",", options.Regions)}";
        }
        else
        {
            url += "&regions=us";
        }

        var rawJson = await ExecuteWithRetryAsync(url, ct);
        return JsonSerializer.Deserialize<List<OddsApiOddsEvent>>(rawJson, JsonOptions) ?? [];
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

                    response.EnsureSuccessStatusCode();
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
