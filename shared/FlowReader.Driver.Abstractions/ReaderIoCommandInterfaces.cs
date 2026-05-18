namespace FlowReader.Driver.Abstractions;

public interface IRelayOutputDriver
{
    Task PulseRelayAsync(byte durationSeconds, CancellationToken ct);
}

public interface IIndicatorOutputDriver
{
    Task ControlIndicatorAsync(byte functionCode, CancellationToken ct);
}
