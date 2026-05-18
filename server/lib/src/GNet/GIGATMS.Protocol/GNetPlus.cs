using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using GIGATMS.IO;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace GIGATMS.Protocol
{
	[DebuggerNonUserCode]
	public class GNetPlus : CommandsBase
	{
		private int m_iTimeout;

		private byte[] m_bRegisters;

		private bool m_bIsReadRegisters;

		private Timeout m_oReadRegistersTimeout;

		private byte m_bStringPadValue;

		protected IReceiver m_oGNetPlusReceiver;

		protected IReceiver m_oRAWReceiver;

		protected IReply m_oReply;

		protected ICommand m_oLastCommand;

		private int m_bCurAddr;

		private int m_iGNetPlusBufferSize;

		public int GNetPlusTimeout
		{
			get
			{
				return m_iTimeout;
			}
			set
			{
				m_iTimeout = value;
			}
		}

		public int ActiveAddress
		{
			get
			{
				return m_bCurAddr;
			}
			set
			{
				m_bCurAddr = value & 0xFF;
			}
		}

		public int GNetPlusBufferSize
		{
			get
			{
				return m_iGNetPlusBufferSize;
			}
			set
			{
				if (value >= 1 && value <= 255)
				{
					m_iGNetPlusBufferSize = value;
				}
				else
				{
					m_iGNetPlusBufferSize = 128;
				}
			}
		}

		[DebuggerNonUserCode]
		public GNetPlus()
		{
			IReceiver oReceiver = new GNetPlusReceiver();
			base._002Ector(ref oReceiver);
			m_iTimeout = 3000;
			m_oReadRegistersTimeout = new Timeout(500);
			m_bStringPadValue = 0;
			m_oGNetPlusReceiver = new GNetPlusReceiver();
			m_oRAWReceiver = new RAWReceiver();
			m_iGNetPlusBufferSize = 128;
			AddReceiver(ref m_oGNetPlusReceiver);
			AddReceiver(ref m_oRAWReceiver, bIsAddEventOnly: true);
		}

		[DebuggerNonUserCode]
		public GNetPlus(ref ISerialPort oSerialPort)
		{
			IReceiver oReceiver = null;
			base._002Ector(ref oReceiver, ref oSerialPort);
			m_iTimeout = 3000;
			m_oReadRegistersTimeout = new Timeout(500);
			m_bStringPadValue = 0;
			m_oGNetPlusReceiver = new GNetPlusReceiver();
			m_oRAWReceiver = new RAWReceiver();
			m_iGNetPlusBufferSize = 128;
			AddReceiver(ref m_oGNetPlusReceiver);
			AddReceiver(ref m_oRAWReceiver, bIsAddEventOnly: true);
		}

		public bool Query(ref int iAddress, byte bFunction)
		{
			byte[] Params = null;
			int iLength = iAddress;
			int iAddress2 = -1;
			return Query(bFunction, ref Params, iLength, 0, -1, ref iAddress2);
		}

		public bool Query(byte bFunction)
		{
			byte[] Params = null;
			int iAddress = -1;
			return Query(bFunction, ref Params, -1, 0, -1, ref iAddress);
		}

		public bool Query(byte bFunction, byte Param, [Optional][DefaultParameterValue(-1)] ref int iAddress)
		{
			byte[] Params = new byte[1] { Param };
			return Query(bFunction, ref Params, -1, 0, -1, ref iAddress);
		}

		public bool Query(byte bFunction, short Param, [Optional][DefaultParameterValue(-1)] ref int iAddress)
		{
			byte[] Params = BitConverter.GetBytes(Param);
			return Query(bFunction, ref Params, -1, 0, -1, ref iAddress);
		}

		public bool Query(byte bFunction, int Param, [Optional][DefaultParameterValue(-1)] ref int iAddress)
		{
			byte[] Params = BitConverter.GetBytes(Param);
			return Query(bFunction, ref Params, -1, 0, -1, ref iAddress);
		}

		public bool Query(byte bFunction, long Param, [Optional][DefaultParameterValue(-1)] ref int iAddress)
		{
			byte[] Params = BitConverter.GetBytes(Param);
			return Query(bFunction, ref Params, -1, 0, -1, ref iAddress);
		}

		public bool Query(byte bFunction, string Param, [Optional][DefaultParameterValue(-1)] ref int iAddress)
		{
			byte[] Params = Encoding.Default.GetBytes(Param);
			return Query(bFunction, ref Params, -1, 0, -1, ref iAddress);
		}

		public bool Query(byte bFunction, ref byte[] Params, int iLength = -1, int iStart = 0, int iTimeout = -1, [Optional][DefaultParameterValue(-1)] ref int iAddress)
		{
			bool flag = false;
			IReply oReply = null;
			if (iTimeout == -1)
			{
				iTimeout = m_iTimeout;
			}
			if (iAddress == -1)
			{
				iAddress = m_bCurAddr;
			}
			checked
			{
				byte[] array;
				if (iLength > 0)
				{
					array = new byte[iLength - 1 + 1];
					Array.Copy(Params, iStart, array, 0, iLength);
				}
				else
				{
					array = Params;
				}
				m_oLastCommand = new GNetPlusCommand(ref m_oGNetPlusReceiver, bFunction, array, (byte)iAddress, iTimeout);
				flag = SendCommand(ref m_oLastCommand, ref oReply);
				if (flag)
				{
					m_oReply = oReply;
					Timeout oReadRegistersTimeout = m_oReadRegistersTimeout;
					if (!oReadRegistersTimeout.IsTimeout)
					{
						oReadRegistersTimeout.Reset();
					}
					oReadRegistersTimeout = null;
				}
				return flag;
			}
		}

		public bool Polling(ref byte[] bValue, int iAddr = 0)
		{
			byte[] Values = null;
			ActiveAddress = iAddr;
			bool flag = default(bool);
			if (Query(0))
			{
				flag = false;
				m_oReply.GetReplyValue(ref Values);
				flag = Values != null;
				if (flag)
				{
					bValue = Values;
				}
			}
			return flag;
		}

		public short Polling()
		{
			short result = -1;
			byte[] Values = null;
			if (Query(0))
			{
				result = -1;
				m_oReply.GetReplyValue(ref Values);
				result = (short)((Values != null) ? Values[0] : (-1));
			}
			return result;
		}

		public short Polling(int intAddr)
		{
			short result = -1;
			byte[] Values = null;
			byte param = checked((byte)(intAddr & 0xFF));
			int iAddress = -1;
			if (Query(0, param, ref iAddress))
			{
				result = -1;
				m_oReply.GetReplyValue(ref Values);
				result = (short)((Values != null) ? Values[0] : (-1));
			}
			return result;
		}

		public string GetVersion()
		{
			string Value = null;
			checked
			{
				if (Query(1))
				{
					m_oReply.GetReplyValue(ref Value);
					Value += "";
					try
					{
						if (Value.Length > 0 && Conversions.ToChar(Value.Substring(Value.Length - 1, 1)) == '\0')
						{
							Value = Value.Remove(Value.Length - 1, 1);
						}
					}
					catch (Exception ex)
					{
						ProjectData.SetProjectError(ex);
						Exception ex2 = ex;
						ProjectData.ClearProjectError();
					}
				}
				return Value;
			}
		}

		public bool SetSlaveAddr(byte Addr)
		{
			bool result = false;
			int iAddress = -1;
			if (Query(2, Addr, ref iAddress) && m_bCurAddr == Addr)
			{
				result = true;
			}
			return result;
		}

		public bool SetSlaveAddr(byte Addr, byte[] bXIDNo)
		{
			bool result = false;
			byte[] Params = new byte[5];
			if (bXIDNo != null && bXIDNo.Length == 4)
			{
				Params[0] = Addr;
				Array.Copy(bXIDNo, 0, Params, 1, 4);
				m_bCurAddr = 0;
				int iAddress = -1;
				if (Query(2, ref Params, -1, 0, -1, ref iAddress) && m_bCurAddr == Addr)
				{
					result = true;
				}
			}
			return result;
		}

		public int getSlaveAddr()
		{
			return m_bCurAddr;
		}

		public bool SetDateTime(ref DateTime dtValue)
		{
			bool result = false;
			byte[] array = new byte[7];
			int num = DateAndTime.Year(dtValue);
			if (num >= 2000 && num <= 2099)
			{
				byte[] Params = GNetPlusPackage.GNetDateConvert(ref dtValue);
				int iAddress = -1;
				if (Query(7, ref Params, -1, 0, -1, ref iAddress))
				{
					result = true;
				}
			}
			return result;
		}

		public bool GetDateTime(ref DateTime dtValue)
		{
			bool result = false;
			if (Query(8))
			{
				byte[] array = new byte[7];
				IReply oReply = m_oReply;
				string Value = Conversions.ToString(dtValue);
				bool replyValue = oReply.GetReplyValue(ref Value);
				dtValue = Conversions.ToDate(Value);
				result = replyValue;
			}
			return result;
		}

		public bool GetRegister(int iAddr, ref byte Value)
		{
			bool flag = false;
			byte[] Value2 = new byte[1];
			flag = GetRegister(iAddr, ref Value2, 1);
			if (flag)
			{
				Value = Value2[0];
			}
			return flag;
		}

		public bool GetRegister(int iAddr, ref short Value)
		{
			bool flag = false;
			byte[] Value2 = new byte[2];
			flag = GetRegister(iAddr, ref Value2, 2);
			if (flag)
			{
				Value = BitConverter.ToInt16(Value2, 0);
			}
			return flag;
		}

		public bool GetRegister(int iAddr, ref int Value)
		{
			bool flag = false;
			byte[] Value2 = new byte[4];
			flag = GetRegister(iAddr, ref Value2, 4);
			if (flag)
			{
				Value = BitConverter.ToInt32(Value2, 0);
			}
			return flag;
		}

		public bool GetRegister(int iAddr, ref long Value)
		{
			bool flag = false;
			byte[] Value2 = new byte[8];
			flag = GetRegister(iAddr, ref Value2, 8);
			if (flag)
			{
				Value = BitConverter.ToInt64(Value2, 0);
			}
			return flag;
		}

		public bool GetRegister(int iAddr, ref string Value)
		{
			bool flag = false;
			byte[] Value2 = new byte[checked(Value.Length - 1 + 1)];
			int iRegLen = ((Value != null) ? Value.Length : 0);
			flag = GetRegister(iAddr, ref Value2, iRegLen);
			if (flag)
			{
				Value = Encoding.Default.GetString(Value2, 0, Value2.Length);
			}
			return flag;
		}

		public bool GetRegister(int iAddr, ref string Value, int iLength)
		{
			bool flag = false;
			byte[] Value2 = new byte[checked(iLength - 1 + 1)];
			flag = GetRegister(iAddr, ref Value2, iLength);
			if (flag)
			{
				Value = Encoding.Default.GetString(Value2, 0, Value2.Length);
			}
			return flag;
		}

		public bool GetRegister(int iAddr, ref byte[] Value, int iRegLen = -1, int iStart = 0)
		{
			GNetPlusPackage gNetPlusPackage = new GNetPlusPackage();
			byte[] Values = null;
			byte[] Params = new byte[3];
			bool flag = false;
			if ((iRegLen == -1) & (Value != null))
			{
				iRegLen = Value.Length;
			}
			checked
			{
				if (iRegLen > 0)
				{
					gNetPlusPackage.AllocMem(iRegLen);
					flag = true;
					int num = iRegLen - 1;
					int iGNetPlusBufferSize = m_iGNetPlusBufferSize;
					int num2 = num;
					for (int i = 0; ((iGNetPlusBufferSize >> 31) ^ i) <= ((iGNetPlusBufferSize >> 31) ^ num2); i += iGNetPlusBufferSize)
					{
						int num3 = iRegLen - i;
						if (num3 > m_iGNetPlusBufferSize)
						{
							num3 = m_iGNetPlusBufferSize;
						}
						int num4 = iAddr + i;
						Params[0] = (byte)((num4 >> 8) & 0xFF);
						Params[1] = (byte)(num4 & 0xFF);
						Params[2] = (byte)(num3 & 0xFF);
						int iAddress = -1;
						if (Query(9, ref Params, -1, 0, -1, ref iAddress))
						{
							if (m_oReply.GetReplyValue(ref Values))
							{
								Array.Copy(Values, 0, gNetPlusPackage.Buffer, i, num3);
								continue;
							}
							flag = false;
							break;
						}
						flag = false;
						break;
					}
					if (flag && Value != null)
					{
						gNetPlusPackage.CopyTo(ref Value, iStart, iRegLen);
					}
				}
				return flag;
			}
		}

		public bool SetRegister(int iAddr, byte Value)
		{
			byte[] Value2 = BitConverter.GetBytes(Value);
			return SetRegister(iAddr, ref Value2);
		}

		public bool SetRegister(int iAddr, short Value)
		{
			byte[] Value2 = BitConverter.GetBytes(Value);
			return SetRegister(iAddr, ref Value2);
		}

		public bool SetRegister(int iAddr, int Value)
		{
			byte[] Value2 = BitConverter.GetBytes(Value);
			return SetRegister(iAddr, ref Value2);
		}

		public bool SetRegister(int iAddr, long Value)
		{
			byte[] Value2 = BitConverter.GetBytes(Value);
			return SetRegister(iAddr, ref Value2);
		}

		public bool SetRegister(int iAddr, string Value)
		{
			byte[] Value2 = Encoding.Default.GetBytes(Value);
			return SetRegister(iAddr, ref Value2);
		}

		public bool SetRegister(int iAddr, ref byte[] Value, int iRegLen = -1, int iStart = 0)
		{
			GNetPlusPackage gNetPlusPackage = new GNetPlusPackage();
			byte[] Value2 = new byte[2];
			bool flag = false;
			if ((iRegLen == -1) & (Value != null))
			{
				iRegLen = Value.Length;
			}
			checked
			{
				if (iRegLen > 0)
				{
					gNetPlusPackage.AllocMem(0);
					gNetPlusPackage.Append(ref Value2);
					gNetPlusPackage.Append(ref Value, iStart, iRegLen);
					int num = iRegLen - 1;
					int num2 = m_iGNetPlusBufferSize - 2;
					int num3 = num;
					for (int i = 0; ((num2 >> 31) ^ i) <= ((num2 >> 31) ^ num3); i += num2)
					{
						int num4 = iRegLen - i;
						if (num4 > m_iGNetPlusBufferSize - 2)
						{
							num4 = m_iGNetPlusBufferSize - 2;
						}
						int num5 = iAddr + i;
						if (gNetPlusPackage.GetSize() == 0)
						{
							flag = false;
							break;
						}
						int lBound = gNetPlusPackage.LBound;
						gNetPlusPackage.Buffer[lBound] = (byte)((num5 >> 8) & 0xFF);
						gNetPlusPackage.Buffer[lBound + 1] = (byte)(num5 & 0xFF);
						ref byte[] buffer = ref gNetPlusPackage.Buffer;
						int iLength = num4 + 2;
						int iAddress = -1;
						flag = Query(10, ref buffer, iLength, lBound, -1, ref iAddress);
						if (!flag)
						{
							break;
						}
						gNetPlusPackage.DumpBuffer(m_iGNetPlusBufferSize - 2);
					}
				}
				return flag;
			}
		}

		public int GetRecordCount()
		{
			int Value = -1;
			if (Query(11))
			{
				m_oReply.GetReplyValue(ref Value);
			}
			return Value;
		}

		public bool GetFirstRecord()
		{
			return Query(12);
		}

		public bool GetFirstRecord(ref string Value)
		{
			bool flag = Query(12);
			if (flag)
			{
				m_oReply.GetReplyValue(ref Value);
			}
			return flag;
		}

		public bool GetFirstRecord(ref byte[] Value)
		{
			bool flag = Query(12);
			if (flag)
			{
				m_oReply.GetReplyValue(ref Value);
			}
			return flag;
		}

		public bool GetNextRecord()
		{
			return Query(13);
		}

		public bool GetNextRecord(ref string Value)
		{
			bool flag = Query(13);
			if (flag)
			{
				m_oReply.GetReplyValue(ref Value);
			}
			return flag;
		}

		public bool GetNextRecord(ref byte[] Value)
		{
			bool flag = Query(13);
			if (flag)
			{
				m_oReply.GetReplyValue(ref Value);
			}
			return flag;
		}

		public bool EraseRecords()
		{
			return Query(14);
		}

		public bool AddRecord(string Value)
		{
			byte[] Value2 = Encoding.Default.GetBytes(Value);
			return AddRecord(ref Value2);
		}

		public bool AddRecord(ref byte[] Value, int iLength = -1, int iStart = 0)
		{
			bool result = false;
			if ((iLength == -1) & (Value != null))
			{
				iLength = Value.Length;
			}
			if (iLength > 0)
			{
				int iLength2 = iLength;
				int iAddress = -1;
				result = Query(15, ref Value, iLength2, iStart, -1, ref iAddress);
			}
			return result;
		}

		public bool RecoverRecords()
		{
			return Query(16);
		}

		public short Thermometer(bool isFloat = false)
		{
			short Value = -1;
			bool flag;
			if (isFloat)
			{
				int iAddress = -1;
				flag = Query((byte)20, (byte)1, ref iAddress);
			}
			else
			{
				flag = Query(20);
			}
			if (flag)
			{
				m_oReply.GetReplyValue(ref Value);
				if (isFloat)
				{
					Value = checked((short)Math.Round((double)Value / 10.0));
				}
			}
			return Value;
		}

		public bool GetBufferData(ref byte[] Value)
		{
			bool flag = Query(21);
			if (flag)
			{
				m_oReply.GetReplyValue(ref Value);
			}
			return flag;
		}

		public bool GetXIDNo(ref byte[] Value, int iAddr = -1)
		{
			if (iAddr == -1)
			{
				iAddr = m_bCurAddr;
			}
			byte param = checked((byte)iAddr);
			int iAddress = -1;
			bool flag = Query(22, param, ref iAddress);
			if (flag)
			{
				m_oReply.GetReplyValue(ref Value);
			}
			return flag;
		}

		public bool SetDevicesSlient(bool bOn, byte bExceptAddr = 0)
		{
			byte[] Params = new byte[2];
			byte b = Conversions.ToByte(Interaction.IIf(bOn, 1, 0));
			int iAddress;
			if (bExceptAddr == 0)
			{
				iAddress = -1;
				return Query(23, b, ref iAddress);
			}
			Params[0] = b;
			Params[1] = bExceptAddr;
			iAddress = -1;
			return Query(23, ref Params, -1, 0, -1, ref iAddress);
		}

		public bool Reset()
		{
			return Query(30);
		}

		public bool GoToISP()
		{
			return Query(31);
		}
	}
}
