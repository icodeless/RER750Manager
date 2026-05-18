using FlowReader.Driver.Abstractions;
using FlowReader.Driver.Rer75x;

Console.WriteLine("Rer75x Dev Harness Starting...");

var discovery = new Rer75xDiscoveryProvider();

Console.WriteLine("Starting discovery...");
var found = await discovery.DiscoverAsync(
    new ReaderDiscoveryRequest(BindLocalIp: null),
    CancellationToken.None);

Console.WriteLine($"Found {found.Count} devices.");
foreach (var device in found)
{
    Console.WriteLine($"{device.DeviceName} {device.IpAddress} {device.MacAddress}");
}

if (found.Count == 0)
{
    Console.WriteLine("No devices found. Exiting.");
    return;
}

var driver = new Rer75xDriver(
    readerId: "rer75x-dev-1",
    options: new ReaderConnectionOptions(
        Address: found.First().IpAddress,
        Extra: new Dictionary<string, string>
        {
            ["localListenIp"] = "0.0.0.0",
            ["eventPort"] = "2168",
            ["commandPort"] = "2167"
        }));

driver.OnStatusChanged = s =>
    Console.WriteLine($"STATUS {s.ReaderId} {s.Status} {s.ErrorMessage}");

driver.OnReaderEvent = e =>
    Console.WriteLine($"EVENT {e.SourceIp} type={e.DataType} data={e.DataHex} name={e.DeviceName} xid={e.Xid}");

await driver.ConnectAsync(CancellationToken.None);
await driver.StartListenAsync(CancellationToken.None);

// Test relay
Console.WriteLine("Testing relay pulse (3 seconds)...");
await driver.PulseRelayAsync(durationSeconds: 3, CancellationToken.None);

Console.WriteLine("Listening. Press Enter to stop.");
// In a non-interactive environment, we might want to wait a bit then exit
// But for the skeleton, we follow the provided example.
// Console.ReadLine();
await Task.Delay(5000); // Wait 5 seconds instead of ReadLine for automated testing

await driver.StopListenAsync(CancellationToken.None);
await driver.DisconnectAsync(CancellationToken.None);
driver.Dispose();

Console.WriteLine("Dev Harness Finished.");
