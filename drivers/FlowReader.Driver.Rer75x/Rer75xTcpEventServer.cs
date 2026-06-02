using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using FlowReader.Driver.Abstractions;

namespace FlowReader.Driver.Rer75x;

internal sealed class Rer75xTcpEventServer : IAsyncDisposable
{
    private readonly string _readerId;
    private readonly string _localIp;
    private readonly int _listenPort;

    private TcpListener? _listener;
    private CancellationTokenSource? _cts;
    private Task? _acceptTask;

    private readonly ConcurrentDictionary<TcpClient, byte> _activeClients = new();
    private readonly ConcurrentDictionary<Task, byte> _clientTasks = new();

    public Action<RawReaderEvent>? OnReaderEvent { get; set; }
    public Action<DriverStatusEvent>? OnStatusChanged { get; set; }

    public Rer75xTcpEventServer(string readerId, string localIp, int listenPort)
    {
        _readerId = readerId;
        _localIp = localIp;
        _listenPort = listenPort;
    }

    public Task StartAsync(CancellationToken ct)
    {
        if (_listener != null)
            return Task.CompletedTask;

        var ip = IPAddress.Parse(_localIp);
        _listener = new TcpListener(ip, _listenPort);
        _listener.Start();

        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        _acceptTask = Task.Run(() => AcceptLoopAsync(_cts.Token), _cts.Token);

        EmitStatus("listening", null);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken ct)
    {
        _cts?.Cancel();

        try { _listener?.Stop(); }
        catch { /* ignore stop race */ }

        // Close all active clients
        foreach (var client in _activeClients.Keys)
        {
            try { client.Close(); } catch { }
        }
        _activeClients.Clear();

        if (_acceptTask != null)
        {
            try { await _acceptTask.WaitAsync(ct); }
            catch { /* ignore shutdown race */ }
        }

        // Wait for client tasks to finish
        var tasks = _clientTasks.Keys.ToArray();
        if (tasks.Length > 0)
        {
            try { await Task.WhenAll(tasks).WaitAsync(ct); }
            catch { /* ignore shutdown race */ }
        }
        _clientTasks.Clear();

        _listener = null;
        EmitStatus("disconnected", null);
    }

    private async Task AcceptLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested && _listener != null)
        {
            TcpClient client;

            try
            {
                client = await _listener.AcceptTcpClientAsync(ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                EmitStatus("error", ex.Message);
                continue;
            }

            _activeClients.TryAdd(client, 0);
            var task = Task.Run(() => ClientLoopAsync(client, ct), ct);
            _clientTasks.TryAdd(task, 0);

            // Cleanup task from dictionary when it completes
            _ = task.ContinueWith(t => _clientTasks.TryRemove(t, out _), CancellationToken.None);
        }
    }

    private async Task ClientLoopAsync(TcpClient client, CancellationToken ct)
    {
        string sourceIp = "";
        int sourcePort = 0;

        try
        {
            if (client.Client.RemoteEndPoint is IPEndPoint remote)
            {
                sourceIp = remote.Address.ToString();
                sourcePort = remote.Port;
            }

            EmitStatus("push_connected", null);

            await using var stream = client.GetStream();

            var buffer = new byte[2048];
            var pending = new List<byte>(Rer75xProtocol.EventFrameLength * 2);

            while (!ct.IsCancellationRequested)
            {
                int read = await stream.ReadAsync(buffer, ct);
                if (read <= 0)
                    break;

                for (int i = 0; i < read; i++)
                    pending.Add(buffer[i]);

                while (pending.Count >= Rer75xProtocol.EventFrameLength)
                {
                    var frame = pending.GetRange(0, Rer75xProtocol.EventFrameLength).ToArray();
                    pending.RemoveRange(0, Rer75xProtocol.EventFrameLength);

                    if (!Rer75xProtocol.TryParseEventFrame(frame, out var parsed))
                    {
                        EmitStatus("error", "Invalid RER75x event frame.");
                        continue;
                    }

                    OnReaderEvent?.Invoke(new RawReaderEvent(
                        ReaderId: _readerId,
                        SourceIp: sourceIp,
                        SourcePort: sourcePort,
                        Protocol: "rer75x.tcp.event",
                        DataType: parsed.DataType,
                        Data: parsed.Data,
                        DataHex: parsed.DataHex,
                        DeviceName: parsed.DeviceName,
                        Xid: parsed.Xid,
                        TimestampUtcMs: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    ));
                }
            }
        }
        catch (OperationCanceledException)
        {
            // normal shutdown
        }
        catch (Exception ex)
        {
            EmitStatus("error", ex.Message);
        }
        finally
        {
            _activeClients.TryRemove(client, out _);
            try { client.Close(); } catch { }
            EmitStatus("push_disconnected", null);
        }
    }

    private void EmitStatus(string status, string? error)
    {
        OnStatusChanged?.Invoke(new DriverStatusEvent(
            ReaderId: _readerId,
            Status: status,
            ErrorMessage: error,
            TimestampUtcMs: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        ));
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync(CancellationToken.None);
        _cts?.Dispose();
    }
}
