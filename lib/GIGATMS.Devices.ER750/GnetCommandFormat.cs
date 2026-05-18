using System;
using System.Collections.Generic;
using Microsoft.VisualBasic.CompilerServices;

namespace GIGATMS.Devices.ER750
{
	public class GnetCommandFormat
	{
		private const byte GNet_SOH = 1;

		private const byte GNet_STX = 2;

		private const byte GNet_CR = 13;

		private const byte GNet_ACK = 6;

		private const byte GNet_NAK = 21;

		private const byte GNet_EVN = 18;

		private const int GNETPACKAGE_ADDR = 0;

		private const int GNETPACKAGE_FUNCTION = 1;

		private const int GNETPACKAGE_LENGTH = 2;

		private const int GNETPACKAGE_DATA = 3;

		private const int GNETPACKAGE_HEADER_SIZE = 5;

		private byte _replyCode;

		private byte _replyDataSize;

		private byte[] _replyDataByteArray;

		private E_GnetErrorCodes _errorCode;

		public byte ReplyCode => _replyCode;

		public byte ReplyDataSize => _replyDataSize;

		public byte[] ReplyDataByteArray => _replyDataByteArray;

		public E_GnetErrorCodes ErrorCode
		{
			get
			{
				switch (_replyCode)
				{
				case 6:
					_errorCode = E_GnetErrorCodes.E00_Success;
					break;
				case 21:
					_errorCode = E_GnetErrorCodes.E01_Failed;
					break;
				default:
					_errorCode = E_GnetErrorCodes.E02_Unknown;
					break;
				}
				return _errorCode;
			}
		}

		private int GNetCRC16(ref byte[] bBuffer, int iLength = -1, int iStart = 0)
		{
			int iCRC = 65535;
			int iLen = checked(bBuffer.Length - iStart);
			if (iLength == -1 || iLength > iLen)
			{
				iLength = iLen;
			}
			checked
			{
				int num = iStart + iLength - 1;
				for (int I = iStart; I <= num; I++)
				{
					iCRC ^= bBuffer[I];
					int J = 0;
					do
					{
						iCRC = (((iCRC & 1) == 0) ? (iCRC >> 1) : ((iCRC >> 1) ^ 0xA001));
						J++;
					}
					while (J <= 7);
				}
				return iCRC;
			}
		}

		public void ParseGnetCommand(byte[] replyParameterByteArray)
		{
			try
			{
				_replyCode = replyParameterByteArray[2];
				_replyDataSize = replyParameterByteArray[3];
				if (_replyDataSize > 0)
				{
					_replyDataByteArray = new byte[checked(_replyDataSize + 1)];
					Array.Copy(replyParameterByteArray, 4, _replyDataByteArray, 0, _replyDataSize);
				}
			}
			catch (Exception ex)
			{
				ProjectData.SetProjectError(ex);
				Exception ex2 = ex;
				throw new Exception("Invalid parameter");
			}
		}

		public byte[] GetPackageCommand(byte address, byte commandCode, byte[] parameterByteArray)
		{
			List<byte> cmd = new List<byte>();
			byte[] crcByteArray = new byte[2];
			cmd.Add(1);
			cmd.Add(address);
			cmd.Add(commandCode);
			checked
			{
				if (parameterByteArray == null)
				{
					cmd.Add(0);
				}
				else
				{
					cmd.Add((byte)parameterByteArray.Length);
					cmd.AddRange(parameterByteArray);
				}
				byte[] bBuffer = cmd.ToArray();
				int crc = GNetCRC16(ref bBuffer, cmd.ToArray().Length - 1, 1);
				crcByteArray[0] = (byte)(crc >> 8);
				crcByteArray[1] = (byte)(crc & 0xFF);
				cmd.AddRange(crcByteArray);
				return cmd.ToArray();
			}
		}
	}
}
