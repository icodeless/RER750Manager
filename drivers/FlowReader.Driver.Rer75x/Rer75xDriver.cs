using FlowReader.Driver.Abstractions;

namespace FlowReader.Driver.Rer75x;

public sealed class Rer75xDriver :
    IReaderPushEventDriver,
    IRelayOutputDriver,
    IIndicatorOutputDriver
{
    private readonly ReaderConnectionOptions _options;
    private readonly Rer75xCommandClient _commandClient;
    private readonly Rer75xTcpEventServer _eventServer;

    private bool _disposed;

    public string ReaderId { get; }

    public Action<RawReaderEvent>? OnReaderEvent { get; set; }
    public Action<DriverStatusEvent>? OnStatusChanged { get; set; }

    public Rer75xDriver(
        string readerId,
        ReaderConnectionOptions options)
    {
        ReaderId = readerId;
        _options = options;

        string readerIp = options.Address;

        int commandPort = GetIntOption(options, "commandPort", Rer75xProtocol.DefaultCommandPort);
        int eventPort = GetIntOption(options, "eventPort", Rer75xProtocol.DefaultEventPort);
        string localListenIp = GetStringOption(options, "localListenIp", "0.0.0.0");

        _commandClient = new Rer75xCommandClient(readerIp, commandPort);
        _eventServer = new Rer75xTcpEventServer(readerId, localListenIp, eventPort);

        _eventServer.OnReaderEvent = e => OnReaderEvent?.Invoke(e);
        _eventServer.OnStatusChanged = e => OnStatusChanged?.Invoke(e);
    }

    public Task ConnectAsync(CancellationToken ct)
    {
        EmitStatus("connected", null);
        return Task.CompletedTask;
    }

    public async Task DisconnectAsync(CancellationToken ct)
    {
        await StopListenAsync(ct);
        EmitStatus("disconnected", null);
    }

    public Task StartListenAsync(CancellationToken ct)
    {
        return _eventServer.StartAsync(ct);
    }

    public Task StopListenAsync(CancellationToken ct)
    {
        return _eventServer.StopAsync(ct);
    }

    public Task PulseRelayAsync(byte durationSeconds, CancellationToken ct)
    {
        if (durationSeconds == 0)
            throw new ArgumentOutOfRangeException(nameof(durationSeconds), "Relay duration must be > 0.");

        return _commandClient.PulseRelayAsync(durationSeconds, ct);
    }

    public Task ControlIndicatorAsync(byte functionCode, CancellationToken ct)
    {
        return _commandClient.ControlIndicatorAsync(functionCode, ct);
    }

    public Task<string?> GetSerialNumberAsync(CancellationToken ct)
    {
        return Task.FromResult<string?>(null);
    }

    private void EmitStatus(string status, string? error)
    {
        OnStatusChanged?.Invoke(new DriverStatusEvent(
            ReaderId,
            status,
            error,
            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        ));
    }

    private static int GetIntOption(
        ReaderConnectionOptions options,
        string key,
        int fallback)
    {
        if (options.Extra != null &&
            options.Extra.TryGetValue(key, out var raw) &&
            int.TryParse(raw, out var value))
        {
            return value;
        }

        return fallback;
    }

    private static string GetStringOption(
        ReaderConnectionOptions options,
        string key,
        string fallback)
    {
        if (options.Extra != null &&
            options.Extra.TryGetValue(key, out var raw) &&
            !string.IsNullOrWhiteSpace(raw))
        {
            return raw;
        }

        return fallback;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        try
        {
            _eventServer.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }
        catch
        {
            // Dispose must not throw in skeleton.
        }
    }
}
