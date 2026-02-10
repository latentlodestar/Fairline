using Fairline.Abstractions.Interfaces;

namespace Fairline.Infrastructure.Services;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
