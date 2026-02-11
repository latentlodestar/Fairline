namespace Fairline.Abstractions.Interfaces;

public interface IIngestEventSink
{
    void Publish(Guid runId, string eventType, string jsonData);
    void Complete(Guid runId);
}
