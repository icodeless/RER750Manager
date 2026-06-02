namespace FlowReader.Driver.Rer75x;

public static class Rer75xCapabilities
{
    public const string Hf1356EventRead = "hf.13_56.event.read";
    public const string Hf1356UidRead = "hf.13_56.uid.read";
    public const string TcpPushListen = "hf.push.tcp.listen";
    public const string UdpDiscovery = "hf.discovery.udp_broadcast";
    public const string RelayOut = "device.relay.out";
    public const string LedOut = "device.led.out";
    public const string BuzzerOut = "device.buzzer.out";
    public const string NetworkTcpCommand = "network.tcp.command";
    public const string NetworkTcpListen = "network.tcp.listen";

    public const string Rer75xCommandPort = "rer75x.command.port";
    public const string Rer75xEventPort = "rer75x.event.port";
    public const string Rer75xEventFrameBytes = "rer75x.event.frame.bytes";
}
