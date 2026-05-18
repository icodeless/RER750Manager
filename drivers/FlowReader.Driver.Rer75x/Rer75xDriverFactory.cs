using FlowReader.Driver.Abstractions;

namespace FlowReader.Driver.Rer75x;

public sealed class Rer75xDriverFactory
{
    public string Brand => "rer75x";

    public Rer75xDriver Create(
        string readerId,
        ReaderConnectionOptions options)
    {
        return new Rer75xDriver(readerId, options);
    }
}
