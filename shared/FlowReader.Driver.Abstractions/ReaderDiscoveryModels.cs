namespace FlowReader.Driver.Abstractions;

public sealed record ReaderDiscoveryRequest(
    string? BindLocalIp = null,
    int BroadcastPort = 23,
    int TimeoutMs = 1500,
    int Attempts = 6
);

public sealed record ReaderDiscoveryResult(
    string DeviceKey,
    string IpAddress,
    string? MacAddress,
    string? FirmwareVersion,
    string? DeviceName,
    string Brand,
    string Model,
    string Transport
);

public interface IReaderDiscoveryProvider
{
    Task<IReadOnlyList<ReaderDiscoveryResult>> DiscoverAsync(
        ReaderDiscoveryRequest request,
        CancellationToken ct);
}
