using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using GIGATMS.Windows;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace GIGATMS.IO
{
	[DebuggerNonUserCode]
	public class GSocket : WinSocket
	{
		[DebuggerNonUserCode]
		private class clsRecieveAR
		{
			public UdpClient Client;

			public clsPortInfo oPortInfo;

			public clsRecieveAR()
			{
				Client = null;
				oPortInfo = null;
			}
		}

		[DebuggerNonUserCode]
		private class clsPortInfo
		{
			public string szServerIP;

			public int iPort;

			public byte bCommand;

			public byte bBoardType;

			public byte bBoardID;

			public byte[] bMacAddress;

			public string szFirmwareVersion;

			public string szApplicationTitle;

			public byte[] bOverBuffer;

			public bool isCheckSum;

			public byte[] Buffer;

			public string DeviceName => Strings.Split(szApplicationTitle + ":", ":")[0];

			[DebuggerNonUserCode]
			public clsPortInfo()
			{
				bMacAddress = new byte[6];
				bOverBuffer = null;
				isCheckSum = false;
			}

			[DebuggerNonUserCode]
			public clsPortInfo(byte[] bData)
			{
				bMacAddress = new byte[6];
				bOverBuffer = null;
				isCheckSum = fillData(bData);
			}

			public bool fillData(byte[] bData, Collection oCompares = null)
			{
				bool flag = true;
				bOverBuffer = null;
				if (oCompares != null)
				{
					foreach (clsPortInfo oCompare in oCompares)
					{
						if (Operators.CompareString(BitConverter.ToString(bData), BitConverter.ToString(oCompare.Buffer), TextCompare: false) == 0)
						{
							flag = false;
							break;
						}
					}
				}
				checked
				{
					if (flag)
					{
						flag = false;
						bool flag2 = false;
						if (flag2 != (bData != null) && flag2 != bData.Length >= 4 && flag2 != (bData[0] == 254) && flag2 != bData[1] <= bData.Length)
						{
							byte[] array = new byte[bData[1] - 1 + 1];
							Array.Copy(bData, array, array.Length);
							bOverBuffer = new byte[bData.Length - array.Length - 1 + 1];
							Array.Copy(bData, array.Length, bOverBuffer, 0, bOverBuffer.Length);
							if (array[array.Length - 1] == getBoardCheckSum(array))
							{
								Buffer = array;
								bCommand = array[2];
								bBoardType = array[3];
								bBoardID = array[4];
								szServerIP = Conversions.ToString(array[5]) + "." + Conversions.ToString(array[6]) + "." + Conversions.ToString(array[7]) + "." + Conversions.ToString(array[8]);
								Array.Copy(array, 9, bMacAddress, 0, bMacAddress.Length);
								szFirmwareVersion = "V" + Conversions.ToString(array[18]) + "." + Conversions.ToString(array[17]) + "." + Conversions.ToString(array[16]) + "." + Conversions.ToString(array[15]);
								szApplicationTitle = Encoding.ASCII.GetString(array, 19, 64);
								iPort = (int)Math.Round(Conversion.Val(Strings.Mid(szApplicationTitle, Strings.InStr(szApplicationTitle, ":") + 1)));
								flag = true;
							}
						}
					}
					isCheckSum = flag;
					return flag;
				}
			}

			public static byte getBoardCheckSum(byte[] bSend)
			{
				checked
				{
					int num = bSend.Length - 2;
					long num2 = default(long);
					for (int i = 0; i <= num; i++)
					{
						num2 = (num2 - bSend[i]) & 0xFF;
					}
					return (byte)(num2 & 0xFF);
				}
			}

			public override string ToString()
			{
				string text = "TCP:" + szServerIP + "\r\n";
				text = "Command = " + Conversions.ToString(bCommand) + "\r\n";
				text = "BoardType = " + Conversions.ToString(bBoardType) + "\r\n";
				text = "BoardID = " + Conversions.ToString(bBoardID) + "\r\n";
				byte[] array = bMacAddress;
				Array.Reverse(array);
				text = "MACAddress=" + Strings.Replace(BitConverter.ToString(array), "-", ":") + "\r\n";
				text = "Version=" + szFirmwareVersion + "\r\n";
				return "Title=" + szApplicationTitle;
			}
		}

		private UdpClient m_oBoardSocket;

		private Collection m_oPortInfos;

		private const int BOARD_PORT = 23;

		private const byte TAG_CMD = byte.MaxValue;

		private const byte TAG_STATUS = 254;

		private const byte CMD_DISCOVER_TARGET = 2;

		private static ManualResetEvent receiveBoard = new ManualResetEvent(initialState: false);

		private bool m_bSkipListPorts;

		public override string PortName
		{
			get
			{
				clsPortInfo clsPortInfo = CommportInfoEx(base.PortName);
				string text = "TCP" + clsPortInfo.szServerIP;
				text = text + ":" + Conversions.ToString(clsPortInfo.iPort);
				text = text + "\r\n" + clsPortInfo.DeviceName;
				clsPortInfo = null;
				return text;
			}
			set
			{
				string portName = Strings.Split(value + "\r\n", "\r\n")[0];
				base.PortName = portName;
			}
		}

		[DebuggerNonUserCode]
		public GSocket()
		{
			m_oBoardSocket = new UdpClient();
			m_oPortInfos = new Collection();
		}

		[DebuggerNonUserCode]
		public GSocket(ref string ServerIP, int Port)
			: base(ref ServerIP, Port)
		{
			m_oBoardSocket = new UdpClient();
			m_oPortInfos = new Collection();
		}

		[DebuggerNonUserCode]
		public GSocket(ref TcpClient oSocket)
			: base(ref oSocket)
		{
			m_oBoardSocket = new UdpClient();
			m_oPortInfos = new Collection();
		}

		[DebuggerNonUserCode]
		public GSocket(ref WindowMessage oWndMsg)
			: base(ref oWndMsg)
		{
			m_oBoardSocket = new UdpClient();
			m_oPortInfos = new Collection();
		}

		[DebuggerNonUserCode]
		public GSocket(ref TcpClient oSocket, ref WindowMessage oWndMsg)
			: base(ref oSocket, ref oWndMsg)
		{
			m_oBoardSocket = new UdpClient();
			m_oPortInfos = new Collection();
		}

		public override string EnumCommPort(int Index)
		{
			string text = null;
			if (Index == 0)
			{
				if (!m_bSkipListPorts)
				{
					NumberOfPorts();
				}
				m_bSkipListPorts = false;
			}
			try
			{
				clsPortInfo clsPortInfo = (clsPortInfo)m_oPortInfos[checked(Index + 1)];
				text = "TCP" + clsPortInfo.szServerIP + ":" + Conversions.ToString(clsPortInfo.iPort) + "\r\n" + clsPortInfo.DeviceName;
				clsPortInfo = null;
			}
			catch (Exception ex)
			{
				ProjectData.SetProjectError(ex);
				Exception oError = ex;
				ProcessErrorCode(ISerialPort.CommErrorConstants.Unknown, ref oError);
				text = null;
				ProjectData.ClearProjectError();
			}
			return text;
		}

		public override int NumberOfPorts()
		{
			IPEndPoint endPoint = new IPEndPoint(IPAddress.Broadcast, 23);
			byte[] array = new byte[4];
			clsRecieveAR clsRecieveAR;
			lock (m_oPortInfos)
			{
				m_oPortInfos.Clear();
				m_oBoardSocket.EnableBroadcast = true;
				array[0] = byte.MaxValue;
				array[1] = 4;
				array[2] = 2;
				array[3] = clsPortInfo.getBoardCheckSum(array);
				clsRecieveAR = new clsRecieveAR();
				clsRecieveAR.Client = m_oBoardSocket;
			}
			m_oBoardSocket.Send(array, array.Length, endPoint);
			int num = 0;
			do
			{
				receiveBoard.WaitOne(50, exitContext: true);
				if (m_oBoardSocket.Available != 0)
				{
					doRecieveBoard(clsRecieveAR);
					num = 0;
				}
				num = checked(num + 1);
			}
			while (num <= 20);
			m_bSkipListPorts = true;
			return m_oPortInfos.Count;
		}

		public override int NumberOfPorts(string PortFilter)
		{
			int result = default(int);
			return result;
		}

		public override string CommPortClass(int CommPort)
		{
			return "TCP";
		}

		public override string CommPortClass(string CommPort)
		{
			return "TCP";
		}

		public override string CommPortInfo(int CommPort)
		{
			string text = null;
			if (CommPort == 0)
			{
				if (!m_bSkipListPorts)
				{
					NumberOfPorts();
				}
				m_bSkipListPorts = false;
			}
			try
			{
				clsPortInfo clsPortInfo = (clsPortInfo)m_oPortInfos[checked(CommPort + 1)];
				text = clsPortInfo.ToString();
				clsPortInfo = null;
			}
			catch (Exception ex)
			{
				ProjectData.SetProjectError(ex);
				Exception oError = ex;
				ProcessErrorCode(ISerialPort.CommErrorConstants.Unknown, ref oError);
				text = null;
				ProjectData.ClearProjectError();
			}
			return text;
		}

		public override string CommPortInfo(string CommPort)
		{
			string text = null;
			return CommportInfoEx(CommPort).ToString();
		}

		private clsPortInfo CommportInfoEx(string CommPort)
		{
			clsPortInfo result = null;
			string left = Strings.Split(CommPort + "\r\n", "\r\n")[0];
			int count = m_oPortInfos.Count;
			for (int i = 1; i <= count; i = checked(i + 1))
			{
				clsPortInfo clsPortInfo = (clsPortInfo)m_oPortInfos[i];
				if (Operators.CompareString(left, "TCP" + clsPortInfo.szServerIP + ":" + Conversions.ToString(clsPortInfo.iPort), TextCompare: false) == 0)
				{
					result = (clsPortInfo)m_oPortInfos[i];
					break;
				}
				clsPortInfo = null;
			}
			return result;
		}

		public override bool IsMyPort(ref string szPortName)
		{
			string szPortName2 = Strings.Split(szPortName + "\r\n", "\r\n")[0];
			return base.IsMyPort(ref szPortName2);
		}

		private void doRecieveBoard(clsRecieveAR oPortInfo)
		{
			UdpClient client = oPortInfo.Client;
			IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
			byte[] bBuffer = client.Receive(ref remoteEP);
			ISerialPort oSender = this;
			RaiseMonitorEvent(ref oSender, ISerialPort.CommMonitorEventConstants.comEvInput, ref bBuffer);
			lock (m_oPortInfos)
			{
				while (bBuffer != null && bBuffer.Length != 0)
				{
					oPortInfo.oPortInfo = new clsPortInfo();
					if (oPortInfo.oPortInfo.fillData(bBuffer, m_oPortInfos))
					{
						m_oPortInfos.Add(oPortInfo.oPortInfo);
					}
					bBuffer = oPortInfo.oPortInfo.bOverBuffer;
					oPortInfo.oPortInfo = null;
				}
			}
		}

		private void onRecieveBoard(IAsyncResult ar)
		{
			clsRecieveAR clsRecieveAR = (clsRecieveAR)ar.AsyncState;
			UdpClient client = clsRecieveAR.Client;
			IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
			byte[] bBuffer = client.EndReceive(ar, ref remoteEP);
			ISerialPort oSender = this;
			RaiseMonitorEvent(ref oSender, ISerialPort.CommMonitorEventConstants.comEvInput, ref bBuffer);
			receiveBoard.Reset();
			lock (m_oPortInfos)
			{
				while (bBuffer != null && bBuffer.Length != 0)
				{
					clsRecieveAR.oPortInfo = new clsPortInfo(bBuffer);
					if (clsRecieveAR.oPortInfo.isCheckSum)
					{
						m_oPortInfos.Add(clsRecieveAR.oPortInfo);
					}
					bBuffer = clsRecieveAR.oPortInfo.bOverBuffer;
					clsRecieveAR.oPortInfo = null;
				}
			}
			client.BeginReceive(onRecieveBoard, clsRecieveAR);
		}
	}
}
