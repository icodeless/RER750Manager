using System.Net;
using System.Net.Sockets;
using FlowReader.Driver.Abstractions;

namespace FlowReader.Driver.Rer75x;

public sealed class Rer75xDiscoveryProvider : IReaderDiscoveryProvider
{
    public async Task<IReadOnlyList<ReaderDiscoveryResult>> DiscoverAsync(
        ReaderDiscoveryRequest request,
        CancellationToken ct)
    {
        var results = new Dictionary<string, ReaderDiscoveryResult>(StringComparer.OrdinalIgnoreCase);

        using var udp = new UdpClient();

        udp.EnableBroadcast = true;

        if (!string.IsNullOrWhiteSpace(request.BindLocalIp))
        {
            udp.Client.Bind(new IPEndPoint(IPAddress.Parse(request.BindLocalIp), 0));
        }
        else
        {
            udp.Client.Bind(new IPEndPoint(IPAddress.Any, 0));
        }

        var target = new IPEndPoint(IPAddress.Broadcast, request.BroadcastPort);

        int attempts = Math.Max(1, request.Attempts);
        for (int i = 0; i < attempts; i++)
        {
            await udp.SendAsync(Rer75xProtocol.DiscoveryProbe, target, ct);
        }

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(Math.Max(250, request.TimeoutMs));

        while (!timeoutCts.IsCancellationRequested)
        {
            UdpReceiveResult receive;
            try
            {
                receive = await udp.ReceiveAsync(timeoutCts.Token);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            if (!Rer75xProtocol.TryParseDeviceStatus(receive.Buffer, out var status))
                continue;

            var key = status.IpAddress;

            results[key] = new ReaderDiscoveryResult(
                DeviceKey: $"rer75x-{status.MacAddress.Replace(":", "").ToLowerInvariant()}",
                IpAddress: status.IpAddress,
                MacAddress: status.MacAddress,
                FirmwareVersion: status.FirmwareVersion,
                DeviceName: status.DeviceName,
                Brand: "rer75x",
                Model: "RER75x",
                Transport: "udp-discovery+tcp-push"
            );
        }

        return results.Values.ToList();
    }
}
