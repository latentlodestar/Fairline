using System.Text.Json.Serialization;

namespace Fairline.Abstractions.Contracts;

public sealed record OddsApiSport(
    [property: JsonPropertyName("key")] string Key,
    [property: JsonPropertyName("group")] string Group,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("active")] bool Active,
    [property: JsonPropertyName("has_outrights")] bool HasOutrights);

public sealed record OddsApiOddsEvent(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("sport_key")] string SportKey,
    [property: JsonPropertyName("sport_title")] string SportTitle,
    [property: JsonPropertyName("commence_time")] string CommenceTime,
    [property: JsonPropertyName("home_team")] string HomeTeam,
    [property: JsonPropertyName("away_team")] string AwayTeam,
    [property: JsonPropertyName("bookmakers")] IReadOnlyList<OddsApiBookmaker> Bookmakers);

public sealed record OddsApiBookmaker(
    [property: JsonPropertyName("key")] string Key,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("last_update")] string? LastUpdate,
    [property: JsonPropertyName("markets")] IReadOnlyList<OddsApiMarket> Markets);

public sealed record OddsApiMarket(
    [property: JsonPropertyName("key")] string Key,
    [property: JsonPropertyName("last_update")] string? LastUpdate,
    [property: JsonPropertyName("outcomes")] IReadOnlyList<OddsApiOutcome> Outcomes);

public sealed record OddsApiOutcome(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("price")] decimal Price,
    [property: JsonPropertyName("point")] decimal? Point,
    [property: JsonPropertyName("description")] string? Description);

public sealed record OddsRequestOptions(
    IReadOnlyList<string> Markets,
    IReadOnlyList<string>? Regions,
    IReadOnlyList<string>? Bookmakers);
