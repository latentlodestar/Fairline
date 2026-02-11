using System.Collections.Concurrent;
using System.Threading.Channels;
using Fairline.Abstractions.Interfaces;

namespace Fairline.Infrastructure.Services;

public sealed record SseEvent(string EventType, string Data);

public sealed class IngestRunNotifier : IIngestEventSink
{
    private readonly ConcurrentDictionary<Guid, Channel<SseEvent>> _channels = new();

    public Channel<SseEvent> CreateChannel(Guid runId)
    {
        var channel = Channel.CreateUnbounded<SseEvent>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });
        _channels[runId] = channel;
        return channel;
    }

    public ChannelReader<SseEvent>? TryGetReader(Guid runId)
    {
        return _channels.TryGetValue(runId, out var channel) ? channel.Reader : null;
    }

    public void Publish(Guid runId, string eventType, string jsonData)
    {
        if (_channels.TryGetValue(runId, out var channel))
        {
            channel.Writer.TryWrite(new SseEvent(eventType, jsonData));
        }
    }

    public void Complete(Guid runId)
    {
        if (_channels.TryRemove(runId, out var channel))
        {
            channel.Writer.TryComplete();
        }
    }
}
