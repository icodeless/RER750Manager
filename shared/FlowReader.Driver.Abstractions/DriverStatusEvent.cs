namespace FlowReader.Driver.Abstractions;

public sealed record DriverStatusEvent(
    string ReaderId,
    string Status,
    string? ErrorMessage,
    long TimestampUtcMs
);
