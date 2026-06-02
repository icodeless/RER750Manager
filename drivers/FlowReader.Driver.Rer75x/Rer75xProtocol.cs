using System.Buffers;
using System.Net;
using System.Text;

namespace FlowReader.Driver.Rer75x;

internal static class Rer75xProtocol
{
    public const int DefaultDiscoveryPort = 23;
    public const int DefaultCommandPort = 2167;
    public const int DefaultEventPort = 2168;

    public const int EventFrameLength = 42;
    public static readonly byte[] DiscoveryProbe = { 0xFF, 0x04, 0x02, 0xFB };

    public static bool TryParseEventFrame(
        ReadOnlySpan<byte> frame,
        out Rer75xEventFrame parsed)
    {
        parsed = default;

        if (frame.Length != EventFrameLength)
            return false;

        byte dataType = frame[4];
        byte dataSize = frame[5];

        if (dataSize > 16)
            return false;

        var data = frame.Slice(6, dataSize).ToArray();

        string deviceName = ReadAsciiTrimmed(frame.Slice(22, 16));
        string xid = ToHex(frame.Slice(38, 4));

        parsed = new Rer75xEventFrame(
            DataType: dataType,
            Data: data,
            DataHex: ToHex(data),
            DeviceName: string.IsNullOrWhiteSpace(deviceName) ? null : deviceName,
            Xid: xid
        );

        return true;
    }

    public static bool TryParseDeviceStatus(
        ReadOnlySpan<byte> buffer,
        out Rer75xDeviceStatus status)
    {
        status = default;

        if (buffer.Length != 84 && buffer.Length != 95)
            return false;

        if (buffer[0] != 0xFE)
            return false;

        if (buffer[1] != 84 && buffer[1] != 95)
            return false;

        string ip = $"{buffer[5]}.{buffer[6]}.{buffer[7]}.{buffer[8]}";

        string mac =
            $"{buffer[14]:X2}:{buffer[13]:X2}:{buffer[12]:X2}:{buffer[11]:X2}:{buffer[10]:X2}:{buffer[9]:X2}";

        string firmware = $"{buffer[18]}.{buffer[17]}.{buffer[16]}.{buffer[15]}";
        string name = ReadAsciiTrimmed(buffer.Slice(19, 64));

        status = new Rer75xDeviceStatus(
            IpAddress: ip,
            MacAddress: mac,
            FirmwareVersion: firmware,
            DeviceName: string.IsNullOrWhiteSpace(name) ? null : name,
            CommandCode: buffer[2],
            BoardType: buffer[3],
            BoardId: buffer[4]
        );

        return true;
    }

    public static byte[] BuildRelayPulseCommand(byte durationSeconds)
    {
        // Legacy ER750Lib: GetPackageCommand(0, 17, new byte[] { 0, duration })
        return BuildGnetCommand(address: 0x00, commandCode: 0x11, parameter: new byte[] { 0x00, durationSeconds });
    }

    public static byte[] BuildIndicatorCommand(byte functionCode)
    {
        // Legacy ER750Lib: 02 4A (48 + selection) 0D 00
        return new byte[]
        {
            0x02,
            0x4A,
            checked((byte)(48 + functionCode)),
            0x0D,
            0x00
        };
    }

    private static byte[] BuildGnetCommand(byte address, byte commandCode, byte[]? parameter)
    {
        var length = parameter?.Length ?? 0;
        var cmd = new byte[4 + length + 2];

        cmd[0] = 0x01;
        cmd[1] = address;
        cmd[2] = commandCode;
        cmd[3] = checked((byte)length);

        if (parameter != null)
            Buffer.BlockCopy(parameter, 0, cmd, 4, parameter.Length);

        ushort crc = ComputeGnetCrc16(cmd.AsSpan(1, 3 + length));

        // Match legacy order: high byte then low byte.
        cmd[4 + length] = (byte)(crc >> 8);
        cmd[5 + length] = (byte)(crc & 0xFF);

        return cmd;
    }

    private static ushort ComputeGnetCrc16(ReadOnlySpan<byte> bytes)
    {
        ushort crc = 0xFFFF;

        for (int i = 0; i < bytes.Length; i++)
        {
            crc ^= bytes[i];

            for (int bit = 0; bit < 8; bit++)
            {
                if ((crc & 1) == 0)
                    crc >>= 1;
                else
                    crc = (ushort)((crc >> 1) ^ 0xA001);
            }
        }

        return crc;
    }

    private static string ReadAsciiTrimmed(ReadOnlySpan<byte> bytes)
    {
        int len = bytes.Length;
        while (len > 0 && (bytes[len - 1] == 0 || bytes[len - 1] == 32))
            len--;

        return Encoding.ASCII.GetString(bytes.Slice(0, len)).Trim();
    }

    private static string ToHex(ReadOnlySpan<byte> bytes)
    {
        return Convert.ToHexString(bytes);
    }
}

internal readonly record struct Rer75xEventFrame(
    byte DataType,
    byte[] Data,
    string DataHex,
    string? DeviceName,
    string? Xid
);

internal readonly record struct Rer75xDeviceStatus(
    string IpAddress,
    string MacAddress,
    string FirmwareVersion,
    string? DeviceName,
    byte CommandCode,
    byte BoardType,
    byte BoardId
);
