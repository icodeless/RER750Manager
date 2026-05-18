using System;
using System.Diagnostics;
using System.Text;
using GIGATMS.IO;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace GIGATMS.Protocol
{
	[DebuggerNonUserCode]
	public class GNet : CommandsBase
	{
		public enum iConnectStateConstants
		{
			Disconnect,
			Logout,
			Logon
		}

		private int m_iTimeout;

		private byte[] m_bRegisters;

		private bool m_bIsReadRegisters;

		private Timeout m_oReadRegistersTimeout;

		private byte m_bStringPadValue;

		protected IReceiver m_oGNetReceiver;

		protected IReceiver m_oRAWReceiver;

		[DebuggerNonUserCode]
		public GNet()
		{
			IReceiver oReceiver = new GNetReceiver();
			base._002Ector(ref oReceiver);
			m_iTimeout = 3000;
			m_oReadRegistersTimeout = new Timeout(500);
			m_bStringPadValue = 0;
			m_oGNetReceiver = new GNetReceiver();
			m_oRAWReceiver = new RAWReceiver();
			AddReceiver(ref m_oGNetReceiver);
			AddReceiver(ref m_oRAWReceiver, bIsAddEventOnly: true);
		}

		[DebuggerNonUserCode]
		public GNet(ref ISerialPort oSerialPort)
		{
			IReceiver oReceiver = null;
			base._002Ector(ref oReceiver, ref oSerialPort);
			m_iTimeout = 3000;
			m_oReadRegistersTimeout = new Timeout(500);
			m_bStringPadValue = 0;
			m_oGNetReceiver = new GNetReceiver();
			m_oRAWReceiver = new RAWReceiver();
			AddReceiver(ref m_oGNetReceiver);
			AddReceiver(ref m_oRAWReceiver, bIsAddEventOnly: true);
		}

		public bool SendGNetCMD(ref string szResult, ref string szCMD, bool bRxChkSum = false, bool bTxChkSum = false, int iTimeout = -1)
		{
			IReply oReply = null;
			if (iTimeout == -1)
			{
				iTimeout = m_iTimeout;
			}
			ICommand oCommand = new GNetCommand(ref m_oGNetReceiver, szCMD, bTxChkSum, bRxChkSum, iTimeout);
			bool flag = SendCommand(ref oCommand, ref oReply);
			if (flag)
			{
				oReply.GetReplyValue(ref szResult);
				Timeout oReadRegistersTimeout = m_oReadRegistersTimeout;
				if (!oReadRegistersTimeout.IsTimeout)
				{
					oReadRegistersTimeout.Reset();
				}
				oReadRegistersTimeout = null;
			}
			return flag;
		}

		public bool SendGNetCMD(ref string szCMD, bool bRxChkSum = false, bool bTxChkSum = false, int iTimeout = -1)
		{
			IReply oReply = null;
			if (iTimeout == -1)
			{
				iTimeout = m_iTimeout;
			}
			ICommand oCommand = new GNetCommand(ref m_oGNetReceiver, szCMD, bTxChkSum, bRxChkSum, iTimeout);
			bool flag = SendCommand(ref oCommand, ref oReply);
			if (flag)
			{
				Timeout oReadRegistersTimeout = m_oReadRegistersTimeout;
				if (!oReadRegistersTimeout.IsTimeout)
				{
					oReadRegistersTimeout.Reset();
				}
				oReadRegistersTimeout = null;
			}
			return flag;
		}

		public bool SendGNetCMD(int iReTryCount, ref string szResult, ref string szCMD, bool bRxChkSum = false, bool bTxChkSum = false, int iTimeout = -1)
		{
			if (iReTryCount < 1)
			{
				iReTryCount = 1;
			}
			else if (iReTryCount > 5)
			{
				iReTryCount = 5;
			}
			int num = iReTryCount;
			bool flag = default(bool);
			for (int i = 1; i <= num; i = checked(i + 1))
			{
				szResult = null;
				flag = SendGNetCMD(ref szResult, ref szCMD, bRxChkSum, bTxChkSum, iTimeout);
				if (flag)
				{
					break;
				}
				if (szResult != null)
				{
					switch (szResult)
					{
					default:
						continue;
					case "00":
					case "D":
					case "I":
						break;
					}
					break;
				}
			}
			return flag;
		}

		public bool DumpRegisters(int iRegistersSize, ref byte[] bBuffer)
		{
			byte[] bCommandBytes = new byte[3] { 2, 89, 13 };
			byte[] Values = null;
			byte[] Values2 = null;
			IReply oReply = null;
			bool flag;
			if (PortOpen)
			{
				ICommand oCommand = new RAWCommand(ref m_oRAWReceiver, ref bCommandBytes, 1500);
				flag = SendCommand(ref oCommand, ref oReply);
				if (flag)
				{
					oReply.GetReplyValue(ref Values);
					oReply = null;
					oCommand = new RAWCommand(ref m_oRAWReceiver, ref bCommandBytes, 1500);
					flag = SendCommand(ref oCommand, ref oReply);
					if (flag)
					{
						oReply.GetReplyValue(ref Values2);
					}
				}
			}
			else
			{
				flag = false;
			}
			checked
			{
				if (flag)
				{
					if (Values != null && Values2 != null)
					{
						if (Values.Length == Values2.Length && (iRegistersSize == 0 || Values.Length >= iRegistersSize))
						{
							int num = Values.Length & -256;
							int num2 = num - 1;
							for (int i = 0; i <= num2; i++)
							{
								if (Values[i] != Values2[i])
								{
									flag = false;
									break;
								}
							}
							if (flag)
							{
								if (num > iRegistersSize && iRegistersSize > 0)
								{
									num = iRegistersSize;
								}
								if (bBuffer == null || bBuffer.Length != num)
								{
									bBuffer = new byte[num - 1 + 1];
								}
								Array.Copy(Values, bBuffer, num);
							}
						}
						else
						{
							flag = false;
						}
					}
					else
					{
						flag = false;
					}
				}
				return flag;
			}
		}

		public string GetVersion()
		{
			string szResult = null;
			string szCMD = "F";
			SendGNetCMD(ref szResult, ref szCMD);
			return szResult;
		}

		public bool GetVersion(ref string szVersion)
		{
			string szCMD = "F";
			return SendGNetCMD(ref szVersion, ref szCMD);
		}

		public bool Login(ref string szPassword)
		{
			string szCMD = "L" + szPassword;
			return SendGNetCMD(ref szCMD);
		}

		public bool Login(ref string szPassword, ref string szDecodeInfo)
		{
			szDecodeInfo = null;
			string szCMD = "L" + szPassword;
			return SendGNetCMD(ref szDecodeInfo, ref szCMD);
		}

		public bool Logout()
		{
			string szCMD = "O";
			return SendGNetCMD(ref szCMD);
		}

		public iConnectStateConstants ConnectState()
		{
			Timeout timeout = new Timeout();
			string szResult = null;
			iConnectStateConstants result = iConnectStateConstants.Disconnect;
			if (PortOpen)
			{
				try
				{
					int num = 1;
					do
					{
						string szCMD = "B00";
						if (SendGNetCMD(1, ref szResult, ref szCMD, bRxChkSum: false, bTxChkSum: false, 200))
						{
							result = iConnectStateConstants.Logon;
							break;
						}
						if (szResult != null && Strings.Len(szResult) > 0)
						{
							result = iConnectStateConstants.Logout;
							break;
						}
						num = checked(num + 1);
					}
					while (num <= 2);
				}
				catch (Exception ex)
				{
					ProjectData.SetProjectError(ex);
					Exception ex2 = ex;
					ProjectData.ClearProjectError();
				}
			}
			return result;
		}

		public bool SetPassword(ref string szPassword)
		{
			string szCMD = "P" + szPassword;
			return SendGNetCMD(ref szCMD, bRxChkSum: false, bTxChkSum: true);
		}

		public bool SetPassword(ref string szPassword, bool bIsNoCheckSum)
		{
			string szCMD;
			if (bIsNoCheckSum)
			{
				szCMD = "P" + szPassword;
				return SendGNetCMD(ref szCMD);
			}
			szCMD = "P" + szPassword;
			return SendGNetCMD(ref szCMD, bRxChkSum: false, bTxChkSum: true);
		}

		public bool GetTime(ref DateTime dtDateTime)
		{
			string szResult = null;
			string szCMD = "T";
			bool flag = SendGNetCMD(ref szResult, ref szCMD);
			if (flag)
			{
				try
				{
					dtDateTime = DateAndTime.Now;
					dtDateTime = new DateTime(Conversions.ToInteger(Strings.Mid(szResult, 1, 4)), Conversions.ToInteger(Strings.Mid(szResult, 5, 2)), Conversions.ToInteger(Strings.Mid(szResult, 7, 2)), Conversions.ToInteger(Strings.Mid(szResult, 9, 2)), Conversions.ToInteger(Strings.Mid(szResult, 11, 2)), Conversions.ToInteger(Strings.Mid(szResult, 13, 2)));
				}
				catch (Exception ex)
				{
					ProjectData.SetProjectError(ex);
					Exception ex2 = ex;
					flag = false;
					ProjectData.ClearProjectError();
				}
			}
			return flag;
		}

		public bool SetTime(DateTime dtDateTime)
		{
			string szCMD = "S" + Strings.Format(dtDateTime, "yyyyMMddHHmmss");
			return SendGNetCMD(ref szCMD);
		}

		public bool SetTime(DateTime dtDateTime, bool bIsWithWeek)
		{
			string szCMD;
			if (bIsWithWeek)
			{
				szCMD = "S" + Strings.Format(dtDateTime, "yyyyMMddHHmmss") + Conversions.ToString((int)checked(dtDateTime.DayOfWeek + 1));
				return SendGNetCMD(ref szCMD);
			}
			szCMD = "S" + Strings.Format(dtDateTime, "yyyyMMddHHmmss");
			return SendGNetCMD(ref szCMD);
		}

		public int GetRecordCount()
		{
			int result = -1;
			string szResult = null;
			string szCMD = "N";
			if (SendGNetCMD(ref szResult, ref szCMD))
			{
				try
				{
					result = Conversions.ToInteger("&H" + Strings.Mid(szResult, 2));
				}
				catch (Exception ex)
				{
					ProjectData.SetProjectError(ex);
					Exception ex2 = ex;
					ProjectData.ClearProjectError();
				}
			}
			return result;
		}

		public bool GetRegister(int iRegister, ref byte bRegisterValue)
		{
			return GetRegister(iRegister, ref bRegisterValue, bIsCache: true);
		}

		public bool GetRegister(int iRegister, ref byte bRegisterValue, bool bIsCache)
		{
			string szResult = null;
			if (bIsCache)
			{
				if (!m_bIsReadRegisters || m_oReadRegistersTimeout.IsTimeout)
				{
					m_bIsReadRegisters = DumpRegisters(0, ref m_bRegisters);
					if (m_bIsReadRegisters)
					{
						m_oReadRegistersTimeout.Reset();
					}
				}
				if (!m_bIsReadRegisters || m_oReadRegistersTimeout.IsTimeout || iRegister > Information.UBound(m_bRegisters) || iRegister < 0)
				{
					bIsCache = false;
				}
			}
			bool flag;
			if (bIsCache)
			{
				bRegisterValue = m_bRegisters[iRegister];
				flag = true;
			}
			else
			{
				int totalWidth = ((iRegister >= 256) ? 4 : 2);
				string szCMD = "B" + iRegister.ToString("X").PadLeft(totalWidth, '0');
				flag = SendGNetCMD(ref szResult, ref szCMD, bRxChkSum: true);
				if (flag)
				{
					try
					{
						bRegisterValue = Conversions.ToByte("&H" + Strings.Mid(szResult, 2, 2));
					}
					catch (Exception ex)
					{
						ProjectData.SetProjectError(ex);
						Exception ex2 = ex;
						flag = false;
						ProjectData.ClearProjectError();
					}
				}
			}
			return flag;
		}

		public bool GetRegister(int iRegister, int iRegisterLength, ref byte[] bRegisterValues)
		{
			checked
			{
				bRegisterValues = new byte[iRegisterLength - 1 + 1];
				int num = iRegisterLength - 1;
				bool register = default(bool);
				for (int i = 0; i <= num; i++)
				{
					register = GetRegister(iRegister + i, ref bRegisterValues[i]);
					if (!register)
					{
						break;
					}
				}
				return register;
			}
		}

		public bool GetRegister(int iRegister, int iRegisterLength, ref string szString, bool bIsAlignmentRight)
		{
			checked
			{
				byte[] array = new byte[iRegisterLength - 1 + 1];
				int num;
				int num2;
				int num3;
				if (bIsAlignmentRight)
				{
					num = iRegisterLength - 1;
					num2 = 0;
					num3 = -1;
				}
				else
				{
					num = 0;
					num2 = iRegisterLength - 1;
					num3 = 1;
				}
				int count = iRegisterLength;
				int num4 = num;
				int num5 = num2;
				int num6 = num3;
				int num7 = num5;
				int i;
				bool register = default(bool);
				for (i = num4; ((num6 >> 31) ^ i) <= ((num6 >> 31) ^ num7); i += num6)
				{
					register = GetRegister(iRegister + i, ref array[i]);
					if (!register)
					{
						break;
					}
					if (array[i] == byte.MaxValue || array[i] == 0)
					{
						count = ((!bIsAlignmentRight) ? ((i != 0) ? i : 0) : (iRegisterLength - i - 1));
						break;
					}
				}
				if (bIsAlignmentRight)
				{
					szString = Encoding.Default.GetString(array, i + 1, count);
				}
				else
				{
					szString = Encoding.Default.GetString(array, 0, count);
				}
				array = null;
				return register;
			}
		}

		public bool SetRegister(int iRegister, byte bRegisterValue)
		{
			string szResult = null;
			if (!m_bIsReadRegisters || m_oReadRegistersTimeout.IsTimeout)
			{
				m_bIsReadRegisters = DumpRegisters(0, ref m_bRegisters);
				if (m_bIsReadRegisters)
				{
					m_oReadRegistersTimeout.Reset();
				}
			}
			bool flag;
			if (m_bIsReadRegisters && !m_oReadRegistersTimeout.IsTimeout && iRegister <= Information.UBound(m_bRegisters) && m_bRegisters[iRegister] == bRegisterValue)
			{
				flag = true;
			}
			else
			{
				int totalWidth = ((iRegister >= 256) ? 4 : 2);
				string szCMD = "C" + iRegister.ToString("X").PadLeft(totalWidth, '0') + "," + bRegisterValue.ToString("X").PadLeft(2, '0');
				flag = SendGNetCMD(3, ref szResult, ref szCMD, bRxChkSum: false, bTxChkSum: true);
				if (flag && m_bIsReadRegisters)
				{
					m_oReadRegistersTimeout.Reset();
				}
			}
			return flag;
		}

		public bool SetRegister(int iRegister, int iRegisterLength, byte[] bRegisterValues)
		{
			checked
			{
				int num = iRegisterLength - 1;
				bool flag = default(bool);
				for (int i = 0; i <= num; i++)
				{
					flag = SetRegister(iRegister + i, bRegisterValues[i]);
					if (!flag)
					{
						break;
					}
				}
				return flag;
			}
		}

		public bool SetRegister(int iRegister, int iRegisterLength, string szString, bool bIsAlignmentRight)
		{
			checked
			{
				bool flag = default(bool);
				if (szString != null)
				{
					byte[] bytes = Encoding.Default.GetBytes(szString);
					int num = (bIsAlignmentRight ? (iRegisterLength - bytes.Length) : 0);
					int num2 = default(int);
					for (int i = 0; i <= iRegisterLength; i++)
					{
						if (i < num)
						{
							flag = SetRegister(iRegister + i, m_bStringPadValue);
						}
						else
						{
							if (num2 > Information.UBound(bytes))
							{
								flag = SetRegister(iRegister + i, byte.MaxValue);
								break;
							}
							flag = SetRegister(iRegister + i, bytes[num2]);
							num2++;
						}
						if (!flag)
						{
							break;
						}
					}
				}
				else
				{
					flag = false;
				}
				return flag;
			}
		}

		public bool EraseRecords()
		{
			string szCMD = "E";
			return SendGNetCMD(ref szCMD, bRxChkSum: false, bTxChkSum: false, 60000);
		}

		public bool Recovery()
		{
			string szCMD = "M";
			return SendGNetCMD(ref szCMD, bRxChkSum: false, bTxChkSum: false, 60000);
		}
	}
}
