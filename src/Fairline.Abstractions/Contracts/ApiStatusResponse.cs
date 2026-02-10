namespace Fairline.Abstractions.Contracts;

public sealed record ApiStatusResponse(
    string Version,
    bool DatabaseConnected,
    DateTimeOffset Timestamp);
