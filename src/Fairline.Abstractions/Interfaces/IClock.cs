namespace Fairline.Abstractions.Interfaces;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
