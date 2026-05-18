using System.Net.Sockets;

namespace FlowReader.Driver.Rer75x;

internal sealed class Rer75xCommandClient
{
    private readonly string _readerIp;
    private readonly int _commandPort;
    private readonly int _timeoutMs;

    public Rer75xCommandClient(string readerIp, int commandPort, int timeoutMs = 1500)
    {
        _readerIp = readerIp;
        _commandPort = commandPort;
        _timeoutMs = timeoutMs;
    }

    public Task PulseRelayAsync(byte durationSeconds, CancellationToken ct)
    {
        var command = Rer75xProtocol.BuildRelayPulseCommand(durationSeconds);
        return SendCommandAsync(command, ct);
    }

    public Task ControlIndicatorAsync(byte functionCode, CancellationToken ct)
    {
        var command = Rer75xProtocol.BuildIndicatorCommand(functionCode);
        return SendCommandAsync(command, ct);
    }

    private async Task SendCommandAsync(byte[] command, CancellationToken ct)
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(_timeoutMs);

        using var client = new TcpClient();
        await client.ConnectAsync(_readerIp, _commandPort, timeoutCts.Token);

        await using var stream = client.GetStream();
        await stream.WriteAsync(command, timeoutCts.Token);
        await stream.FlushAsync(timeoutCts.Token);

        // Optional ACK read. Some firmware may close quickly after command.
        var buffer = new byte[64];

        try
        {
            if (stream.CanRead)
            {
                using var ackCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token);
                ackCts.CancelAfter(300);
                _ = await stream.ReadAsync(buffer, ackCts.Token);
            }
        }
        catch (OperationCanceledException)
        {
            // Do not fail command solely because ACK was not received in skeleton.
        }
    }
}
