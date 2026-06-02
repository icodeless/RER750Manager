namespace FlowReader.Driver.Abstractions;

public interface IReaderPushEventDriver : IDisposable
{
    string ReaderId { get; }

    Action<RawReaderEvent>? OnReaderEvent { get; set; }
    Action<DriverStatusEvent>? OnStatusChanged { get; set; }

    Task ConnectAsync(CancellationToken ct);
    Task DisconnectAsync(CancellationToken ct);

    Task StartListenAsync(CancellationToken ct);
    Task StopListenAsync(CancellationToken ct);

    Task<string?> GetSerialNumberAsync(CancellationToken ct);
}
