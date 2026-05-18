using System;
using System.Diagnostics;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace GIGATMS.Devices.ER750.Parameters
{
	public class DeviceStatusFormat : ParameterFormat
	{
		public enum E_FieldStartIndex
		{
			E00_Command = 2,
			E01_BoardType = 3,
			E02_BoardID = 4,
			E03_IpAddress = 5,
			E04_MacAddress = 9,
			E05_FirmwareVersion = 15,
			E06_DeviceName = 19,
			E07_Checksum = 83
		}

		public enum E_FieldLength
		{
			E00_Command = 1,
			E01_BoardType = 1,
			E02_BoardID = 1,
			E03_IpAddress = 4,
			E04_MacAddress = 6,
			E05_FirmwareVersion = 4,
			E06_DeviceName = 64,
			E07_Checksum = 1
		}

		private byte _commandCode;

		private byte _boardType;

		private byte _boardID;

		private string _ipAddress;

		private string _macAddress;

		private string _firmwareVersion;

		private string _deviceName;

		public string DeviceName => _deviceName;

		public string FirmwareVersion => _firmwareVersion;

		public string MacAddress => _macAddress;

		public string IpAddress => _ipAddress;

		public byte CommandCode => _commandCode;

		public byte BoardType => _boardType;

		public byte BoardID => _boardID;

		public override void ParseParameter(ref byte[] parameterByteArray)
		{
			_errorCode = -1L;
			if (parameterByteArray.Length != 84 && parameterByteArray.Length != 95)
			{
				throw new Exception("Device status bytes must be 84. Getting length is " + Conversions.ToString(parameterByteArray.Length));
			}
			if (parameterByteArray[0] != 254)
			{
				throw new Exception("Invalid [Type] value (0xFE). Getting value is " + Conversions.ToString(parameterByteArray[0]));
			}
			if (parameterByteArray[1] != 84 && parameterByteArray[1] != 95)
			{
				throw new Exception("Invalid [Length] value (0x54). Getting value is " + Conversions.ToString(parameterByteArray[0]));
			}
			int stepIndex = 0;
			string errorMessage = "";
			while (true)
			{
				switch (stepIndex)
				{
				case 0:
					_commandCode = parameterByteArray[2];
					break;
				case 1:
					_boardType = parameterByteArray[3];
					break;
				case 2:
					_boardID = parameterByteArray[4];
					break;
				case 3:
					_ipAddress = Conversions.ToString(parameterByteArray[5]) + "." + Conversions.ToString(parameterByteArray[6]) + "." + Conversions.ToString(parameterByteArray[7]) + "." + Conversions.ToString(parameterByteArray[8]);
					break;
				case 4:
					_macAddress = Conversion.Hex(parameterByteArray[14]).PadLeft(2, '0') + "." + Conversion.Hex(parameterByteArray[13]).PadLeft(2, '0') + "." + Conversion.Hex(parameterByteArray[12]).PadLeft(2, '0') + "." + Conversion.Hex(parameterByteArray[11]).PadLeft(2, '0') + "." + Conversion.Hex(parameterByteArray[10]).PadLeft(2, '0') + "." + Conversion.Hex(parameterByteArray[9]).PadLeft(2, '0');
					break;
				case 5:
					_firmwareVersion = parameterByteArray[18] + "." + parameterByteArray[17] + "." + parameterByteArray[16] + "." + parameterByteArray[15];
					break;
				case 6:
					_deviceName = Conversions.ToString(MyLib.Encoding.GetString(parameterByteArray, 19, 64));
					break;
				case 7:
					_errorCode = 0L;
					return;
				default:
					Debugger.Break();
					return;
				}
				stepIndex = checked(stepIndex + 1);
			}
		}

		public override void ParseParameter(ref byte[] parameterByteArray, int startIndex, int length)
		{
			ParseParameter(ref parameterByteArray, 0, parameterByteArray.Length);
		}
	}
}
