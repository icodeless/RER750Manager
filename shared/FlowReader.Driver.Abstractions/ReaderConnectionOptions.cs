namespace FlowReader.Driver.Abstractions;

public sealed record ReaderConnectionOptions(
    string Address,
    IReadOnlyDictionary<string, string>? Extra = null
);
