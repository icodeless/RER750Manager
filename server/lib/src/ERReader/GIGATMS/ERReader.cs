using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using GIGATMS.IO;
using GIGATMS.Protocol;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Microsoft.Win32;

namespace GIGATMS
{
	[DebuggerNonUserCode]
	public class ERReader : GNetPlus
	{
		public delegate void OnMessageEventHandler(long dwMessage, string szMessage);

		public delegate void OnResultEventHandler(string szSource, int iResult, string szResult);

		[DebuggerNonUserCode]
		public class PollingReply
		{
			public enum StateEnum : byte
			{
				ACT_ENROLL_ADD = 80,
				ACT_ENROLL_DEL = 81,
				ACT_LAST_RESULT = 10,
				ACT_NORMAL = 0,
				ACT_READ_CARD = 1,
				ACT_VERIFY_FINGER = 5,
				ACT_IDENTIFY_FINGER = 6,
				ACT_OUTPUT = 7,
				ACT_WAIT_EXT_LED = 8,
				ACT_ADD_FINGER = 160,
				ACT_DEL_FINGER = 208,
				ACT_MANAGER_ADD_USER = 20,
				ACT_MANAGER_DEL_USER = 21,
				ACT_PASS = 224,
				ACT_ERROR = 225,
				ACT_END_OF_ACTION = 240,
				POLLING_NG = byte.MaxValue
			}

			private byte[] bReplyData;

			public StateEnum ReaderState
			{
				[DebuggerHidden]
				get
				{
					return (StateEnum)bReplyData[0];
				}
			}

			public byte ProtocolSize
			{
				[DebuggerHidden]
				get
				{
					return bReplyData[1];
				}
			}

			public byte BufferCount
			{
				[DebuggerHidden]
				get
				{
					return bReplyData[2];
				}
			}

			public byte[] ReplyData
			{
				[DebuggerHidden]
				get
				{
					return bReplyData;
				}
				[DebuggerHidden]
				set
				{
					if (value != null && value.Length == bReplyData.Length)
					{
						Array.Copy(value, bReplyData, bReplyData.Length);
					}
				}
			}

			[DebuggerNonUserCode]
			public PollingReply()
			{
				bReplyData = new byte[3];
				bReplyData[0] = byte.MaxValue;
			}

			[DebuggerHidden]
			public override string ToString()
			{
				string text = "";
				if (ReaderState == StateEnum.ACT_NORMAL)
				{
					return "Normal";
				}
				return "Busy";
			}
		}

		public enum ReaderModal
		{
			Any = -1,
			ER750 = 1
		}

		public enum MachineTypeConstants
		{
			Any = -1,
			Reader,
			Programer
		}

		public enum ModalNo
		{
			None = -1,
			ER750,
			ER755,
			ER750_10
		}

		private int bLastFNO;

		private byte[] bLastUserData;

		private int m_iBusy;

		private bool m_bCancel;

		private string m_szBaudrate;

		private ReaderModal m_iMachineModal;

		private MachineTypeConstants m_iMachineType;

		private ModalNo m_iModalNo;

		private bool m_bAutoMode;

		private bool m_bAlwaysAutoMode;

		private string m_szLastCommPort;

		private string m_strLastUsedCommportInfo;

		private string m_szLastStoreUserData;

		private long m_lBIOTimeout;

		private long m_lMax_Template_Size;

		private long m_lMax_Template_Card;

		private long m_lMax_Template_Reader;

		public int BIOTimeout
		{
			[DebuggerHidden]
			get
			{
				return checked((int)m_lBIOTimeout);
			}
		}

		public int MaxTemplateSize
		{
			[DebuggerHidden]
			get
			{
				return checked((int)m_lMax_Template_Size);
			}
		}

		public int MaxTemplateOnCard
		{
			[DebuggerHidden]
			get
			{
				return checked((int)m_lMax_Template_Card);
			}
		}

		public int MaxTemplateOnReader
		{
			[DebuggerHidden]
			get
			{
				return checked((int)m_lMax_Template_Reader);
			}
		}

		public int MachineModal
		{
			[DebuggerHidden]
			get
			{
				return (int)m_iMachineModal;
			}
		}

		public string MachineModalStr
		{
			[DebuggerHidden]
			get
			{
				string text = "Unknow";
				if (iModalNo == -2)
				{
					iModalNo = (int)m_iModalNo;
				}
				try
				{
					text = ((ModalNo)iModalNo).ToString();
				}
				catch (Exception ex)
				{
					ProjectData.SetProjectError(ex);
					Exception ex2 = ex;
					text = "Unknow";
					ProjectData.ClearProjectError();
				}
				return text;
			}
		}

		public int MachineType
		{
			[DebuggerHidden]
			get
			{
				return (int)m_iMachineType;
			}
		}

		[method: DebuggerNonUserCode]
		public event OnMessageEventHandler OnMessage;

		[method: DebuggerNonUserCode]
		public event OnResultEventHandler OnResult;

		[DebuggerNonUserCode]
		public ERReader()
		{
			ISerialPort oSerialPort = new MSComm();
			base._002Ector(ref oSerialPort);
			base.OnMonitor += ERReader_OnMonitor;
			string ServerIP = "";
			oSerialPort = new GSocket(ref ServerIP, 0);
			AddSerialPort(ref oSerialPort);
			RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("Software\\VB and VBA Program Settings\\SmaFinger\\SF500x");
			m_szLastCommPort = registryKey.GetValue("LastCommPort", "COM1").ToString();
			m_strLastUsedCommportInfo = registryKey.GetValue("LastPCommPort", "COM1").ToString();
			m_lMax_Template_Card = 2L;
			m_lMax_Template_Reader = 10L;
			m_iMachineType = MachineTypeConstants.Programer;
		}

		[DebuggerNonUserCode]
		public ERReader(string szAddress, int port)
		{
			ISerialPort oSerialPort = new GSocket(ref szAddress, port);
			base._002Ector(ref oSerialPort);
			base.OnMonitor += ERReader_OnMonitor;
			m_lMax_Template_Card = 2L;
			m_lMax_Template_Reader = 10L;
			m_iMachineType = MachineTypeConstants.Programer;
		}

		[DebuggerHidden]
		public PollingReply PollingEx(int iAddr = 0)
		{
			PollingReply oPollingReply = new PollingReply();
			Polling(ref oPollingReply, iAddr);
			return oPollingReply;
		}

		[DebuggerHidden]
		public bool Polling(ref PollingReply oPollingReply, int iAddr = 0)
		{
			byte[] bValue = null;
			bool flag = Polling(ref bValue, iAddr);
			if (flag)
			{
				oPollingReply.ReplyData = bValue;
			}
			return flag;
		}

		[DebuggerHidden]
		public bool Polling(ref byte bState, ref byte bProtocolSize, ref byte bBufferCount, int iAddr = 0)
		{
			byte[] bValue = null;
			bool flag = Polling(ref bValue, iAddr);
			if (flag)
			{
				flag = false;
				if (bValue != null && bValue.Length >= 3)
				{
					bState = bValue[0];
					bProtocolSize = bValue[1];
					bBufferCount = bValue[2];
					flag = true;
				}
			}
			return flag;
		}

		[DebuggerHidden]
		private void ERReader_OnMonitor(ref ISerialPort oSender, ISerialPort.CommMonitorEventConstants iEvent, ref byte[] bBuffer)
		{
			if (iEvent == ISerialPort.CommMonitorEventConstants.comEvPortBeforeClose)
			{
				Query(4);
			}
		}

		[DebuggerHidden]
		public bool setRelay(byte bytPeriodTime)
		{
			byte b = default(byte);
			byte[] Params = new byte[2] { b, bytPeriodTime };
			int iAddress = -1;
			return Query(17, ref Params, -1, 0, -1, ref iAddress);
		}

		[DebuggerHidden]
		public bool setLedBuzzer(string sCommandNumber)
		{
			if (PortOpen)
			{
				Output = "\u0002J" + Strings.Trim(sCommandNumber) + "\r";
				return true;
			}
			bool result = default(bool);
			return result;
		}

		[DebuggerHidden]
		public bool getCardStatus([Optional][DefaultParameterValue(0)] ref int iResponse)
		{
			bool flag = Query(226);
			if (flag)
			{
				byte[] Values = new byte[1];
				if (m_oReply.GetReplyValue(ref Values))
				{
					iResponse = checked((int)Math.Round(Conversion.Val("&H" + Strings.Replace(BitConverter.ToString(Values), "-", ""))));
				}
			}
			return flag;
		}

		public void Close()
		{
			PortOpen = false;
		}

		[DebuggerHidden]
		private bool checkPort(string szPort, ref MachineTypeConstants iType, ref ReaderModal iModal)
		{
			bool flag = false;
			byte[] array = null;
			MachineTypeConstants machineTypeConstants = MachineTypeConstants.Any;
			ReaderModal readerModal = ReaderModal.Any;
			ERReader eRReader = this;
			if (Operators.CompareString(szPort, "", TextCompare: false) != 0)
			{
				eRReader.PortName = szPort;
				eRReader.PortOpen = true;
				eRReader.GNetPlusTimeout = 200;
				if (eRReader.PortOpen && Polling() != -1)
				{
					string version = GetVersion();
					ModalNo iModalNo = default(ModalNo);
					switch (Strings.Left(version, checked(Strings.InStr(1, version + " ", " ") - 1)))
					{
					case "PGM-T1374":
						readerModal = ReaderModal.ER750;
						machineTypeConstants = MachineTypeConstants.Reader;
						iModalNo = ModalNo.ER750;
						break;
					case "PGM-T1379":
						readerModal = ReaderModal.ER750;
						machineTypeConstants = MachineTypeConstants.Reader;
						iModalNo = ModalNo.ER755;
						break;
					case "PGM-T1623":
						readerModal = ReaderModal.ER750;
						machineTypeConstants = MachineTypeConstants.Reader;
						iModalNo = ModalNo.ER750_10;
						break;
					}
					if (readerModal != ReaderModal.Any && (iType == MachineTypeConstants.Any || iType == machineTypeConstants) && (iModal == ReaderModal.Any || iModal == readerModal))
					{
						iType = machineTypeConstants;
						iModal = readerModal;
						m_iModalNo = iModalNo;
						flag = true;
					}
				}
				if (!flag)
				{
					eRReader.PortOpen = false;
				}
				eRReader = null;
				return flag;
			}
			bool result = default(bool);
			return result;
		}

		[DebuggerHidden]
		private bool checkReaderRegisted(string strPortInfo)
		{
			bool flag = false;
			byte[] array = null;
			if (Operators.CompareString(strPortInfo, "", TextCompare: false) != 0)
			{
				PortName = strPortInfo;
				GNetPlusTimeout = 200;
				PortOpen = true;
				if (PortOpen && Polling() != -1)
				{
					string version = GetVersion();
					switch (Strings.Left(version, checked(Strings.InStr(1, version + " ", " ") - 1)))
					{
					case "PGM-T1374":
						flag = true;
						break;
					case "PGM-T1379":
						flag = true;
						break;
					case "PGM-T1623":
						flag = true;
						break;
					}
				}
				if (!flag & PortOpen)
				{
					PortOpen = false;
				}
				return flag;
			}
			bool result = default(bool);
			return result;
		}

		[DebuggerHidden]
		private byte[] getEncryCode(string szCode)
		{
			string text = "";
			checked
			{
				int num = Strings.Len(szCode) - 1;
				int num2 = default(int);
				byte[] array = default(byte[]);
				for (int i = 1; i <= num; i++)
				{
					byte b = (byte)(Strings.Asc(Strings.Mid(szCode, i, 1)) ^ Strings.Asc(Strings.Mid(szCode, i + 1, 1)));
					text += Conversions.ToString(Strings.Chr(b));
					array = (byte[])Utils.CopyArray(array, new byte[num2 + 1]);
					array[num2] = b;
					num2++;
				}
				text += Strings.Right(szCode, 1);
				array = (byte[])Utils.CopyArray(array, new byte[num2 + 1]);
				array[num2] = (byte)Strings.Asc(Strings.Right(szCode, 1));
				return array;
			}
		}

		[DebuggerHidden]
		public bool AutoScan([Optional][DefaultParameterValue(-1)] ref MachineTypeConstants iMType, [Optional][DefaultParameterValue(-1)] ref ReaderModal iReadModal, string szSetPort = "", bool bOnlySetPort = false)
		{
			bool flag = false;
			m_bCancel = false;
			string text = "";
			string text2 = "";
			m_bAlwaysAutoMode = true;
			if (Operators.CompareString(szSetPort, "", TextCompare: false) != 0)
			{
				text2 = "\0" + szSetPort;
			}
			switch (iMType)
			{
			case MachineTypeConstants.Programer:
				if (Operators.CompareString(Strings.Left(m_strLastUsedCommportInfo, 3), "COM", TextCompare: false) == 0)
				{
					text2 = text2 + "\0" + m_strLastUsedCommportInfo;
				}
				break;
			case MachineTypeConstants.Reader:
				if (Operators.CompareString(Strings.Left(m_szLastCommPort, 3), "COM", TextCompare: false) == 0)
				{
					text2 = text2 + "\0" + m_strLastUsedCommportInfo;
				}
				break;
			case MachineTypeConstants.Any:
				if (Operators.CompareString(Strings.Left(m_strLastUsedCommportInfo, 3), "COM", TextCompare: false) == 0)
				{
					text2 = text2 + "\0" + m_strLastUsedCommportInfo;
				}
				if (Operators.CompareString(text2, "", TextCompare: false) == 0 && Operators.CompareString(Strings.Left(m_szLastCommPort, 3), "COM", TextCompare: false) == 0)
				{
					text2 = text2 + "\0" + m_strLastUsedCommportInfo;
				}
				break;
			}
			string[] array = EnumCommPortEx();
			long num = 0L;
			checked
			{
				long num2 = array.Length - 1;
				for (long num3 = num; num3 <= num2; num3++)
				{
					string text3 = array[(int)num3];
					if (Operators.CompareString("\0" + text3, text2, TextCompare: false) != 0 && Operators.CompareString(text3, "", TextCompare: false) != 0)
					{
						text2 = text2 + "\0" + text3;
					}
				}
				text2 = Strings.Mid(text2, 2);
				string[] array2 = Strings.Split(text2, "\0");
				string[] array3 = Strings.Split("38400", ";");
				long num4 = GNetPlusTimeout;
				GNetPlusTimeout = 200;
				long num5 = 0L;
				long num6 = Information.UBound(array2);
				for (long num3 = num5; num3 <= num6; num3++)
				{
					long num7 = 0L;
					long num8 = Information.UBound(array3);
					for (long num9 = num7; num9 <= num8; num9++)
					{
						Settings = array3[(int)num9];
						if (checkPort(array2[(int)num3], ref iMType, ref iReadModal))
						{
							text = array2[(int)num3];
							m_iMachineType = iMType;
							m_iMachineModal = iReadModal;
							m_szBaudrate = array3[(int)num9];
							flag = true;
							if (flag)
							{
								break;
							}
						}
					}
					if (unchecked((PortOpen | m_bCancel) || bOnlySetPort))
					{
						break;
					}
				}
				GNetPlusTimeout = (int)num4;
				if (flag)
				{
					switch (iMType)
					{
					case MachineTypeConstants.Programer:
						m_strLastUsedCommportInfo = text;
						break;
					case MachineTypeConstants.Reader:
						m_szLastCommPort = text;
						break;
					}
					OnResult?.Invoke("AutoScan", 0, "Found on " + PortName);
				}
				else
				{
					OnResult?.Invoke("AutoScan", 0, "Not found");
				}
				return flag;
			}
		}

		[DebuggerHidden]
		public bool ConnectToReader(string szPortInfo)
		{
			bool flag = false;
			string strLastUsedCommportInfo = "";
			if (Operators.CompareString(szPortInfo, "", TextCompare: false) == 0)
			{
				return flag;
			}
			string[] array = Strings.Split("38400", ";");
			int gNetPlusTimeout = GNetPlusTimeout;
			GNetPlusTimeout = 200;
			int num = Information.UBound(array);
			for (int i = 0; i <= num; i = checked(i + 1))
			{
				Settings = array[i];
				if (checkReaderRegisted(szPortInfo))
				{
					strLastUsedCommportInfo = szPortInfo;
					m_szBaudrate = array[i];
					flag = true;
					if (flag)
					{
						break;
					}
				}
			}
			if (flag)
			{
				m_strLastUsedCommportInfo = strLastUsedCommportInfo;
				OnResult?.Invoke("ConnectToReader", 0, "Found on " + PortName);
			}
			else
			{
				OnResult?.Invoke("ConnectToReader", 0, "Not found");
			}
			GNetPlusTimeout = gNetPlusTimeout;
			return flag;
		}
	}
}
