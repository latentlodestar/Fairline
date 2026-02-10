namespace Fairline.Abstractions.Contracts;

public sealed record ProviderInfo(
    Guid Id,
    string Name,
    string Slug,
    bool IsActive);
