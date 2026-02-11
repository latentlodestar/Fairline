using Fairline.Abstractions.Contracts;

namespace Fairline.Application.Ingest;

public static class OddsFlattener
{
    public static IReadOnlyList<OddsSnapshotEntry> Flatten(
        Guid eventId,
        IReadOnlyList<OddsApiBookmaker> bookmakers,
        DateTimeOffset capturedAtUtc)
    {
        var snapshots = new List<OddsSnapshotEntry>();

        foreach (var book in bookmakers)
        {
            DateTimeOffset? lastUpdate = null;
            if (DateTimeOffset.TryParse(book.LastUpdate, out var parsed))
                lastUpdate = parsed;

            foreach (var market in book.Markets)
            {
                foreach (var outcome in market.Outcomes)
                {
                    snapshots.Add(new OddsSnapshotEntry(
                        EventId: eventId,
                        BookmakerKey: book.Key,
                        BookmakerTitle: book.Title,
                        MarketKey: market.Key,
                        OutcomeName: outcome.Name,
                        Price: outcome.Price,
                        Point: outcome.Point,
                        ProviderLastUpdate: lastUpdate,
                        CapturedAtUtc: capturedAtUtc));
                }
            }
        }

        return snapshots;
    }
}
