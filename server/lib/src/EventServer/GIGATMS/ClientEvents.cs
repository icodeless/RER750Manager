using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace GIGATMS
{
	public class ClientEvents
	{
		private const int EVENT_DATE_TIME = 0;

		private const int EVENT_DATE_TIME_LENGTH = 4;

		private const int EVENT_DATA_TYPE = 4;

		private const int EVENT_DATA_TYPE_LENGTH = 1;

		private const int EVENT_DATA_SIZE = 5;

		private const int EVENT_DATA_SIZE_LENGTH = 1;

		private const int EVENT_DATA_BUFFER = 6;

		private const int EVENT_DATA_BUFFER_LENGTH = 16;

		private const int EVENT_NAME = 22;

		private const int EVENT_NAME_LENGTH = 16;

		private const int EVENT_XID = 38;

		private const int EVENT_XID_LENGTH = 4;

		private const int OLD_PACKET_SIZE = 22;

		private const int NEW_PACKET_SIZE = 38;

		private const int NEW_PACKET1_SIZE = 42;

		private DateTime m_dtDateTime;

		private byte m_bytDataType;

		private int m_iDataSize;

		private byte[] m_bytDataBuffer;

		private string m_szName;

		private string m_szXID;

		private string m_szData;

		private const int DEVICE_UTC_TIME_OFFSET_DAYS = 2;

		public DateTime DateTime => m_dtDateTime;

		public byte DataType => m_bytDataType;

		public int DataSize => m_iDataSize;

		public byte[] DataBuffer => m_bytDataBuffer;

		public string Name => m_szName;

		public string XID => m_szXID;

		public string Data => m_szData;

		[DebuggerNonUserCode]
		public ClientEvents(DateTime dtDateTime, byte bytDataType, int iDataSize, ref byte[] bytDataBuffer, string szName, string szXID)
		{
			m_dtDateTime = dtDateTime;
			m_bytDataType = bytDataType;
			m_bytDataBuffer = bytDataBuffer;
			if (bytDataBuffer != null && bytDataBuffer.Length > 0)
			{
				m_szData = BitConverter.ToString(bytDataBuffer, 0, bytDataBuffer.Length).Replace("-", "");
			}
			else
			{
				m_szData = null;
			}
			m_szName = szName;
			m_szXID = szXID;
		}

		public static List<ClientEvents> ToClientEventsList(bool bIsEnablePCTime, bool bIsUID_MSB_first, ref byte[] bytBuffer, int iLength, TimeSpan oTimeZoneOffsetHour)
		{
			List<ClientEvents> list = new List<ClientEvents>();
			int num = 0;
			if (iLength % 38 == 0)
			{
				num = 38;
			}
			else if (iLength % 42 == 0)
			{
				num = 42;
			}
			else if (iLength % 22 == 0)
			{
				num = 22;
			}
			checked
			{
				if (num > 0)
				{
					int num2 = iLength - 1;
					int num3 = num;
					int num4 = num2;
					for (int i = 0; ((num3 >> 31) ^ i) <= ((num3 >> 31) ^ num4); i += num3)
					{
						long num5 = BitConverter.ToInt64(bytBuffer, i + 0);
						num5 &= 0xFFFFFFFFu;
						DateTime dtDateTime = (bIsEnablePCTime ? DateTime.FromOADate(DateAndTime.Now.ToOADate()) : (DateTime.FromOADate((double)num5 / 86400.0 + 2.0) + oTimeZoneOffsetHour));
						byte[] bytDataBuffer = null;
						int num6 = bytBuffer[i + 5];
						if (num6 > 0)
						{
							if (num6 > 16)
							{
								break;
							}
							bytDataBuffer = new byte[num6 - 1 + 1];
							Array.Copy(bytBuffer, i + 6, bytDataBuffer, 0, num6);
						}
						if (bIsUID_MSB_first)
						{
							Array.Reverse(bytDataBuffer);
						}
						byte b = bytBuffer[i + 4];
						bool flag = Conversions.ToBoolean(Interaction.IIf(b == 224, true, false));
						string szName = null;
						if (num >= 38)
						{
							int count = 16;
							int num7 = i + 22;
							int num8 = i + 22 + 16 - 1;
							for (int j = num7; j <= num8; j++)
							{
								if (bytBuffer[j] == 0)
								{
									count = j - i - 22;
									break;
								}
							}
							szName = Encoding.Default.GetString(bytBuffer, i + 22, count);
						}
						string text = null;
						if (num >= 42)
						{
							int count = 4;
							int num9 = 0;
							int num10 = 0;
							int num11 = count - 1;
							for (int k = 0; k <= num11; k++)
							{
								num9 = bytBuffer[k + 38];
								text = Conversion.Hex(num9) + text;
							}
						}
						text = text;
						list.Add(new ClientEvents(dtDateTime, b, num6, ref bytDataBuffer, szName, text));
					}
				}
				return list;
			}
		}

		[DebuggerNonUserCode]
		public static string Hex2Dec(string num, int iLen = 0)
		{
			string text = "";
			string text2 = "";
			checked
			{
				long num4 = default(long);
				while (Strings.Len(num) > 0)
				{
					int num2 = Strings.Len(num);
					int num3 = num2 - 1;
					for (int i = 0; i <= num3; i++)
					{
						num4 = (long)Math.Round((double)(num4 * 16) + Conversion.Val("&h" + Strings.Mid(num, i + 1, 1)));
						int num5 = (int)unchecked(num4 / 10);
						unchecked
						{
							num4 %= 10;
							if (Strings.Len(text2) > 0 || num5 > 0)
							{
								text2 += Conversion.Hex(num5);
							}
						}
					}
					num = text2;
					text2 = "";
					text = Conversions.ToString(Strings.Chr((int)(num4 + 48))) + text;
					num4 = 0L;
				}
				if (iLen > 0)
				{
					text = Strings.Right(text.PadLeft(iLen, '0'), iLen);
				}
				return text;
			}
		}

		public override string ToString()
		{
			string text = null;
			return Strings.Format(m_dtDateTime, "yyyy/MM/dd hh:mm:ss") + "\t" + m_bytDataType.ToString("X").PadLeft(2, '0') + "\t" + m_szData + "\t" + m_szName;
		}
	}
}
