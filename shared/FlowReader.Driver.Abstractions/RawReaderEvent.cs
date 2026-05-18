namespace FlowReader.Driver.Abstractions;

public sealed record RawReaderEvent(
    string ReaderId,
    string SourceIp,
    int SourcePort,
    string Protocol,
    byte DataType,
    byte[] Data,
    string DataHex,
    string? DeviceName,
    string? Xid,
    long TimestampUtcMs
);
