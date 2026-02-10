namespace Fairline.Abstractions.Interfaces;

public interface IDbContextChecker
{
    Task<bool> CanConnectAsync(CancellationToken ct = default);
}
