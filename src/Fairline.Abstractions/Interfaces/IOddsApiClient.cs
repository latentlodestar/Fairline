using Fairline.Abstractions.Contracts;

namespace Fairline.Abstractions.Interfaces;

public interface IOddsApiClient
{
    Task<(IReadOnlyList<OddsApiSport> Sports, string RawJson)> GetSportsAsync(CancellationToken ct = default);

    Task<IReadOnlyList<OddsApiOddsEvent>> GetOddsAsync(
        string sportKey,
        OddsRequestOptions options,
        CancellationToken ct = default);
}
