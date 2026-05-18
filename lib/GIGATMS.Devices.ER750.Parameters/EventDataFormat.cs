using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace GIGATMS.Devices.ER750.Parameters
{
	public class EventDataFormat : ParameterFormat
	{
		public enum E_FieldStartIndex
		{
			E00_Reserved = 0,
			E01_DataType = 4,
			E02_DataSize = 5,
			E03_DataBuffer = 6,
			E04_Name = 22,
			E05_XID = 38
		}

		public enum E_FieldLength
		{
			E00_Reserved = 4,
			E01_DataType = 1,
			E02_DataSize = 1,
			E03_DataBuffer = 16,
			E04_Name = 16,
			E05_XID = 4
		}

		private byte[] _reservedByteArray;

		private byte _dataType;

		private byte _dataSize;

		private byte[] _dataBufferByteArray;

		private string _dataBufferHexString;

		private byte[] _deviceNameByteArray;

		private string _deviceName;

		private byte[] _xidByteArray;

		private string _xidHexString;

		public byte DataType => _dataType;

		public byte[] DataByteArray => _dataBufferByteArray;

		public string DataHexString => MyLib.Encoding.ByteArrayToHexString(_dataBufferByteArray);

		public string DeviceName => Conversions.ToString(MyLib.Encoding.GetString(_deviceNameByteArray));

		public string XID => Conversion.Hex(_xidByteArray);

        private Dictionary<string, string> _deviceNameCache = new Dictionary<string, string>(); //Halim

        public EventDataFormat()
		{
			_reservedByteArray = new byte[4];
			_deviceNameByteArray = new byte[16];
			_xidByteArray = new byte[4];
		}

		public override void ParseParameter(ref byte[] parameterByteArray)
		{
			_errorCode = -1L;
			if (parameterByteArray.Length != 42)
			{
				throw new Exception("Event data bytes must be 42. Getting length is " + Conversions.ToString(parameterByteArray.Length));
			}
			int stepIndex = 0;
			string errorMessage = "";
			checked
			{
				while (true)
				{
					switch (stepIndex)
					{
					case 0:
						Array.Copy(parameterByteArray, 0, _reservedByteArray, 0, 4);
						break;
					case 1:
						_dataType = parameterByteArray[4];
						break;
					case 2:
						_dataSize = parameterByteArray[5];
						break;
					case 3:
						_dataBufferByteArray = new byte[_dataSize - 1 + 1];
						Array.Copy(parameterByteArray, 6, _dataBufferByteArray, 0, _dataSize);
						break;
					case 4:
						Array.Copy(parameterByteArray, 22, _deviceNameByteArray, 0, 16);
						break;
					case 5:
						Array.Copy(parameterByteArray, 38, _xidByteArray, 0, 4);
						break;
					case 6:
						_errorCode = 0L;
						return;
					default:
						Debugger.Break();
						return;
					}
					stepIndex++;
				}
			}
		}

		public override void ParseParameter(ref byte[] parameterByteArray, int startIndex, int length)
		{
			ParseParameter(ref parameterByteArray);
		}
	}
}
