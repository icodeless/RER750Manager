namespace FlowReader.Driver.Abstractions;

public sealed record ReaderConnectionOptions(
    string Address,
    IDictionary<string, string>? Extra = null
);
