using System;
using System.Diagnostics;
using GIGATMS.IO;
using GIGATMS.Protocol;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace GIGATMS
{
	[DebuggerNonUserCode]
	public class GNetPlusPackage : ByteBuffer, IPackage
	{
		public enum GNETPLUS_HEAD_CODE : byte
		{
			SOH = 1,
			STX
		}

		public enum GNETPLUS_RESPONSE_CODE : byte
		{
			ACK = 6,
			NAK = 21,
			EVN = 18,
			ECHO = 27
		}

		public enum GNETPLUS_PACKAGE_ITEM : byte
		{
			ITEM_HEAD = 1,
			ITEM_ADDR,
			ITEM_FUNCTION,
			ITEM_LENGTH,
			ITEM_DATA,
			ITEM_CRC
		}

		public enum GNET_TIME
		{
			SECOND,
			MINUTE,
			HOUR,
			DAYOFWEEK,
			DAY,
			MONTH,
			YEAR
		}

		private bool m_bReceived;

		private bool m_bCheckSum;

		private byte m_bHead;

		private byte m_bAddr;

		private byte m_bFunc;

		private byte m_bLength;

		private byte[] m_bDatas;

		private byte m_bCRCLow;

		private byte m_bCRCHi;

		private byte[] m_bPackageBuffer;

		private byte[] m_bOverSizeBuffer;

		private int m_nCurrentAddr;

		private bool m_bIsFullSize;

		private int m_iPackageMaxLength;

		private int PACKAGE_LENGTH_SIZE;

		private const int GNETPLUSPACKAGE_HEAD = 0;

		private const int GNETPLUSPACKAGE_ADDR = 1;

		private const int GNETPLUSPACKAGE_FUNCTION = 2;

		private const int GNETPLUSPACKAGE_LENGTH = 3;

		private const int GNETPLUSPACKAGE_DATA = 4;

		public int PackageMaxLength
		{
			get
			{
				return m_iPackageMaxLength;
			}
			set
			{
				m_iPackageMaxLength = checked((int)(value & (long)Math.Round(Math.Pow(256.0, PACKAGE_LENGTH_SIZE) - 1.0)));
			}
		}

		public bool bCheckSum => m_bCheckSum;

		public bool bReceived => m_bReceived;

		public byte DataLength => m_bLength;

		public byte[] Datas
		{
			get
			{
				return m_bDatas;
			}
			set
			{
				m_bDatas = value;
				createHead();
				checked
				{
					DumpRevBuffer(UBound - 3);
					if (value == null)
					{
						m_bLength = 0;
					}
					else if (value.Length <= m_iPackageMaxLength)
					{
						m_bLength = (byte)value.Length;
						Buffer[3] = m_bLength;
						Append(ref value);
					}
					else
					{
						m_bLength = 0;
					}
					createCRC16Code();
				}
			}
		}

		public bool IsFullSize => m_bIsFullSize;

		public byte Address
		{
			get
			{
				return m_bAddr;
			}
			set
			{
				m_bAddr = value;
				createHead();
				Buffer[1] = value;
				createCRC16Code();
			}
		}

		public byte QueryFunc
		{
			get
			{
				return m_bFunc;
			}
			set
			{
				m_bFunc = value;
				createHead();
				Buffer[2] = value;
				createCRC16Code();
			}
		}

		public byte CRCLow => m_bCRCLow;

		public byte CRCHi => m_bCRCHi;

		private void createHead()
		{
			if (UBound < 3)
			{
				AllocMem(4);
			}
			Buffer[0] = 1;
			m_bCheckSum = false;
		}

		public void createCRC16Code()
		{
			checked
			{
				byte[] bBuffer = new byte[m_bLength + 2 + 1];
				byte[] Value = new byte[1];
				if (UBound >= 1 + m_bLength + 2)
				{
					Array.Copy(Buffer, 1, bBuffer, 0, m_bLength + 3);
					int num = GNetCRC16(ref bBuffer);
					m_bCRCLow = (byte)(num >> 8);
					m_bCRCHi = (byte)(num & 0xFF);
					DumpRevBuffer(UBound - (4 + m_bLength - 1));
					Value[0] = m_bCRCLow;
					Append(ref Value);
					Value[0] = m_bCRCHi;
					Append(ref Value);
					m_bCheckSum = true;
				}
				else
				{
					m_bCheckSum = false;
				}
			}
		}

		[DebuggerNonUserCode]
		public GNetPlusPackage()
		{
			PACKAGE_LENGTH_SIZE = 1;
			m_bHead = 0;
			m_bAddr = 0;
			m_bFunc = 0;
			m_bLength = 0;
			m_bDatas = null;
			m_bCRCLow = 0;
			m_bCRCHi = 0;
			m_nCurrentAddr = -1;
			m_bIsFullSize = false;
			m_bReceived = false;
			m_bCheckSum = false;
			AllocMem(4);
			Buffer[0] = m_bHead;
			m_iPackageMaxLength = 128;
		}

		public static int GNetCRC16(ref byte[] bBuffer, int iLength = -1, int iStart = 0)
		{
			int num = 65535;
			int num2 = checked(bBuffer.Length - iStart);
			if (iLength == -1 || iLength > num2)
			{
				iLength = num2;
			}
			checked
			{
				int num3 = iStart + iLength - 1;
				for (int i = iStart; i <= num3; i++)
				{
					num ^= bBuffer[i];
					int num4 = 0;
					do
					{
						num = (((num & 1) == 0) ? (num >> 1) : ((num >> 1) ^ 0xA001));
						num4++;
					}
					while (num4 <= 7);
				}
				return num;
			}
		}

		public static byte[] GNetDateConvert(ref DateTime dtValue)
		{
			byte[] array = new byte[7];
			checked
			{
				if ((DateAndTime.Year(dtValue) >= 2000) & (DateAndTime.Year(dtValue) <= 2099))
				{
					array[6] = (byte)Math.Round(Conversion.Val("&H" + Conversions.ToString(DateAndTime.Year(dtValue) - 2000)));
					array[5] = (byte)Math.Round(Conversion.Val("&H" + Conversions.ToString(DateAndTime.Month(dtValue))));
					array[4] = (byte)Math.Round(Conversion.Val("&H" + Conversions.ToString(DateAndTime.Day(dtValue))));
					array[3] = (byte)Math.Round(Conversion.Val("&H" + Conversions.ToString(DateAndTime.Weekday(dtValue))));
					array[2] = (byte)Math.Round(Conversion.Val("&H" + Conversions.ToString(DateAndTime.Hour(dtValue))));
					array[1] = (byte)Math.Round(Conversion.Val("&H" + Conversions.ToString(DateAndTime.Minute(dtValue))));
					array[0] = (byte)Math.Round(Conversion.Val("&H" + Conversions.ToString(DateAndTime.Second(dtValue))));
				}
				return array;
			}
		}

		public void ClearPackage()
		{
			AllocMem(0);
			m_bHead = 0;
			m_bAddr = 0;
			m_bFunc = 0;
			m_bLength = 0;
			m_bDatas = null;
			m_bCRCLow = 0;
			m_bCRCHi = 0;
			m_nCurrentAddr = -1;
			m_bIsFullSize = false;
			m_bCheckSum = false;
			m_bReceived = false;
		}

		void IPackage.ClearPackage()
		{
			//ILSpy generated this explicit interface implementation from .override directive in ClearPackage
			this.ClearPackage();
		}

		public byte[] getBuffer()
		{
			m_bPackageBuffer = null;
			m_bPackageBuffer = new byte[checked(m_bLength + 5 + 1)];
			byte[] result = new byte[0];
			try
			{
				bool flag = CopyTo(ref m_bPackageBuffer, 0, m_bPackageBuffer.Length);
				if (m_bPackageBuffer[0] != 0)
				{
					result = m_bPackageBuffer;
				}
			}
			catch (Exception ex)
			{
				ProjectData.SetProjectError(ex);
				Exception ex2 = ex;
				ProjectData.ClearProjectError();
			}
			return result;
		}

		byte[] IPackage.getBuffer()
		{
			//ILSpy generated this explicit interface implementation from .override directive in getBuffer
			return this.getBuffer();
		}

		public bool getData(ref bool Value)
		{
			bool result = false;
			if (m_bLength > 0)
			{
				Value = m_bDatas[0] != 0;
				result = true;
			}
			return result;
		}

		bool IPackage.getData(ref bool Value)
		{
			//ILSpy generated this explicit interface implementation from .override directive in getData
			return this.getData(ref Value);
		}

		public bool getData(ref byte Value)
		{
			bool result = false;
			if (m_bLength > 0)
			{
				Value = m_bDatas[0];
				result = true;
			}
			return result;
		}

		bool IPackage.getData(ref byte Value)
		{
			//ILSpy generated this explicit interface implementation from .override directive in getData
			return this.getData(ref Value);
		}

		public bool getData(ref byte[] Value)
		{
			bool result = false;
			if (m_bLength > 0)
			{
				Value = m_bDatas;
				result = true;
			}
			return result;
		}

		bool IPackage.getData(ref byte[] Value)
		{
			//ILSpy generated this explicit interface implementation from .override directive in getData
			return this.getData(ref Value);
		}

		public bool getData(ref DateTime Value)
		{
			bool result = false;
			if (m_bLength == 7)
			{
				string text = Conversion.Hex(checked(8192 + m_bDatas[6])) + "/" + Conversion.Hex(m_bDatas[5]) + "/" + Conversion.Hex(m_bDatas[4]) + " " + Conversion.Hex(m_bDatas[2]) + ":" + Conversion.Hex(m_bDatas[1]) + ":" + Conversion.Hex(m_bDatas[0]);
				if (Information.IsDate(text))
				{
					Value = Convert.ToDateTime(text);
					result = true;
				}
				else
				{
					result = false;
				}
			}
			return result;
		}

		bool IPackage.getData(ref DateTime Value)
		{
			//ILSpy generated this explicit interface implementation from .override directive in getData
			return this.getData(ref Value);
		}

		public bool getData(ref int Value, bool bIsReverse = true)
		{
			bool result = false;
			byte[] array = new byte[4];
			if (m_bLength > 0)
			{
				int num = m_bLength;
				if (num > 4)
				{
					num = 4;
				}
				Array.Copy(m_bDatas, array, num);
				if (bIsReverse)
				{
					Array.Reverse(array, 0, num);
				}
				Value = BitConverter.ToInt32(array, 0);
				result = true;
			}
			return result;
		}

		bool IPackage.getData(ref int Value, bool bIsReverse = true)
		{
			//ILSpy generated this explicit interface implementation from .override directive in getData
			return this.getData(ref Value, bIsReverse);
		}

		public bool getData(ref long Value, bool bIsReverse = true)
		{
			bool result = false;
			byte[] array = new byte[8];
			if (m_bLength > 0)
			{
				int num = m_bLength;
				if (num > 8)
				{
					num = 8;
				}
				Array.Copy(m_bDatas, array, num);
				if (bIsReverse)
				{
					Array.Reverse(array);
				}
				Value = BitConverter.ToInt64(array, 0);
				result = true;
			}
			return result;
		}

		bool IPackage.getData(ref long Value, bool bIsReverse = true)
		{
			//ILSpy generated this explicit interface implementation from .override directive in getData
			return this.getData(ref Value, bIsReverse);
		}

		public bool getData(ref short Value, bool bIsReverse = true)
		{
			bool result = false;
			byte[] array = new byte[2];
			if (m_bLength > 0)
			{
				int num = m_bLength;
				if (num > 2)
				{
					num = 2;
				}
				Array.Copy(m_bDatas, array, num);
				if (bIsReverse)
				{
					Array.Reverse(array);
				}
				Value = BitConverter.ToInt16(array, 0);
				result = true;
			}
			return result;
		}

		bool IPackage.getData(ref short Value, bool bIsReverse = true)
		{
			//ILSpy generated this explicit interface implementation from .override directive in getData
			return this.getData(ref Value, bIsReverse);
		}

		public bool getData(ref string Value)
		{
			bool result = false;
			if (m_bLength > 0)
			{
				Value = m_oEncode.GetString(m_bDatas, 0, m_bLength);
				result = true;
			}
			return result;
		}

		bool IPackage.getData(ref string Value)
		{
			//ILSpy generated this explicit interface implementation from .override directive in getData
			return this.getData(ref Value);
		}

		public string getItemName(int Value)
		{
			string result = "";
			switch (Value)
			{
			case 1:
				result = "HEAD";
				break;
			case 2:
				result = "ADDRESS";
				break;
			case 3:
				result = "FUNCTION";
				break;
			case 4:
				result = "LENGTH";
				break;
			case 5:
				result = "DATA";
				break;
			case 6:
				result = "CRC";
				break;
			}
			return result;
		}

		string IPackage.getItemName(int Value)
		{
			//ILSpy generated this explicit interface implementation from .override directive in getItemName
			return this.getItemName(Value);
		}

		public byte[] getItemValue(GNETPLUS_PACKAGE_ITEM Index)
		{
			return getItemValue((int)Index);
		}

		public byte[] getItemValue(int Index)
		{
			byte[] array = new byte[1];
			switch (Index)
			{
			case 1:
				array[0] = m_bHead;
				break;
			case 2:
				array[0] = Address;
				break;
			case 3:
				array[0] = QueryFunc;
				break;
			case 4:
				array[0] = DataLength;
				break;
			case 5:
				array = Datas;
				break;
			case 6:
				array = new byte[2] { CRCLow, CRCHi };
				break;
			}
			return array;
		}

		byte[] IPackage.getItemValue(int Index)
		{
			//ILSpy generated this explicit interface implementation from .override directive in getItemValue
			return this.getItemValue(Index);
		}

		public byte[] PackageTaken()
		{
			byte[] array = null;
			array = new byte[0];
			byte[] bOverSizeBuffer = m_bOverSizeBuffer;
			ClearPackage();
			checked
			{
				if (bOverSizeBuffer != null)
				{
					int num = bOverSizeBuffer.Length - 1;
					for (int i = 0; i <= num; i++)
					{
						if (m_bReceived)
						{
							PackageAppend(ref bOverSizeBuffer[i]);
						}
						else if (bOverSizeBuffer[i] == 1)
						{
							array = new byte[i - 1 + 1];
							Array.Copy(bOverSizeBuffer, array, i);
							PackageAppend(ref bOverSizeBuffer[i]);
						}
					}
				}
				if (!m_bReceived && bOverSizeBuffer != null)
				{
					array = bOverSizeBuffer;
				}
				if (bOverSizeBuffer.Length == 0)
				{
					m_bReceived = false;
				}
				return array;
			}
		}

		public byte[] PackageDiscardHeader()
		{
			byte[] array = null;
			byte[] buffer = getBuffer();
			ClearPackage();
			m_bReceived = false;
			checked
			{
				int num = buffer.Length - 1;
				for (int i = 1; i <= num; i++)
				{
					if (m_bReceived)
					{
						PackageAppend(ref buffer[i]);
					}
					else if (buffer[i] == 1)
					{
						array = new byte[i - 1 + 1];
						Array.Copy(buffer, array, i);
						PackageAppend(ref buffer[i]);
					}
				}
				if (!m_bReceived)
				{
					array = buffer;
				}
				return array;
			}
		}

		public bool PackageAppend(ref byte Value)
		{
			byte[] Value2 = new byte[1];
			bool result = false;
			if (m_nCurrentAddr < 0 && Value == 1)
			{
				ClearPackage();
				m_bCheckSum = false;
				m_bReceived = true;
			}
			checked
			{
				m_nCurrentAddr++;
				if (!m_bIsFullSize)
				{
					Value2[0] = Value;
					Append(ref Value2);
				}
				int nCurrentAddr = m_nCurrentAddr;
				switch (nCurrentAddr)
				{
				case 0:
					if (Value == 1)
					{
						m_bHead = 1;
					}
					else
					{
						m_nCurrentAddr = -1;
					}
					break;
				case 1:
					m_bAddr = Value;
					break;
				case 2:
					m_bFunc = Value;
					break;
				case 3:
					m_bLength = Value;
					break;
				default:
					if (nCurrentAddr == 4 + m_bLength)
					{
						m_bDatas = new byte[m_bLength - 1 + 1];
						Array.Copy(Buffer, 4, m_bDatas, 0, m_bLength);
						m_bCRCLow = Value;
					}
					else if (nCurrentAddr == 4 + m_bLength + 1)
					{
						m_bIsFullSize = true;
						m_bCRCHi = Value;
						byte[] Value3 = getBuffer();
						result = (m_bCheckSum = PackageFill(ref Value3, isProveState: true));
						m_bOverSizeBuffer = null;
						m_bOverSizeBuffer = new byte[0];
					}
					else if (nCurrentAddr > 4 + m_bLength + 1)
					{
						m_bIsFullSize = true;
						m_bOverSizeBuffer = (byte[])Utils.CopyArray(m_bOverSizeBuffer, new byte[m_bOverSizeBuffer.Length + 1]);
						m_bOverSizeBuffer[m_bOverSizeBuffer.Length - 1] = Value;
					}
					break;
				}
				return result;
			}
		}

		bool IPackage.PackageAppend(ref byte Value)
		{
			//ILSpy generated this explicit interface implementation from .override directive in PackageAppend
			return this.PackageAppend(ref Value);
		}

		public bool PackageFill(ref byte[] Value, bool isProveState = false)
		{
			bool result = false;
			byte b = 0;
			checked
			{
				if (Value.Length >= 6 && Value[0] == 1 && Value[3] == Value.Length - 6)
				{
					int num = GNetCRC16(ref Value, Value.Length - 3, 1);
					byte b2 = (byte)(num >> 8);
					byte b3 = (byte)(num & 0xFF);
					if ((Value[4 + Value[3]] == b2) & (Value[4 + Value[3] + 1] == b3))
					{
						result = true;
						m_bReceived = true;
						if (!isProveState)
						{
							ClearPackage();
							m_bReceived = true;
							Append(ref Value);
							m_bAddr = Value[1];
							m_bFunc = Value[2];
							m_bLength = Value[3];
							m_bDatas = null;
							m_bDatas = new byte[m_bLength - 1 + 1];
							Array.Copy(Value, 4, m_bDatas, 0, m_bLength);
							m_bCRCLow = b2;
							m_bCRCHi = b3;
						}
						m_bCheckSum = true;
					}
				}
				return result;
			}
		}

		bool IPackage.PackageFill(ref byte[] Value, bool isProveState = false)
		{
			//ILSpy generated this explicit interface implementation from .override directive in PackageFill
			return this.PackageFill(ref Value, isProveState);
		}

		public byte[] getOverSizeBuffer()
		{
			return m_bOverSizeBuffer;
		}
	}
}
