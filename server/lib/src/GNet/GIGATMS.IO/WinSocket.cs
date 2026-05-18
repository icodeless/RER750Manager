using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using GIGATMS.Windows;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace GIGATMS.IO
{
	[DebuggerNonUserCode]
	public class WinSocket : ISerialPort
	{
		private delegate void FireCommEventDelegate(ref ISerialPort.CommEventConstants iCommEvent);

		[DebuggerNonUserCode]
		public class StateObject
		{
			public TcpClient workSocket;

			public const int BufferSize = 256;

			public byte[] buffer;

			public ByteBuffer sb;

			public StateObject()
			{
				workSocket = null;
				buffer = new byte[257];
				sb = new ByteBuffer();
			}
		}

		private const string DEFAULT_IP = "127.0.0.1";

		private const int DEFAULT_PORT = 80;

		[AccessedThroughProperty("m_oNetSocket")]
		private TcpClient _m_oNetSocket;

		[AccessedThroughProperty("m_oReceiveTimer")]
		private System.Windows.Forms.Timer _m_oReceiveTimer;

		[AccessedThroughProperty("m_oWndMsg")]
		private WindowMessage _m_oWndMsg;

		private ISerialPort.CommEventConstants m_iCommEvent;

		private int iPostEventConstant;

		private ByteBuffer m_oRxbuffer;

		private FireCommEventDelegate m_oPostCommEventDelegate;

		private Exception m_oLastError;

		private ISerialPort.CommErrorConstants m_iLastError;

		private bool m_bIsAccesError;

		private bool m_bIsBackgroundMode;

		private bool m_bIsOnDataReceive;

		private bool m_bIsMonitor;

		private bool m_bIsPortOpen;

		private IPEndPoint m_oCurrEndPoint;

		private string m_szServerIP;

		private int m_iPort;

		private int m_iInputLen;

		private ISerialPort.InputModeConstants m_iInputMode;

		private static ManualResetEvent connectDone = new ManualResetEvent(initialState: false);

		private static ManualResetEvent sendDone = new ManualResetEvent(initialState: false);

		private static ManualResetEvent receiveDone = new ManualResetEvent(initialState: false);

		private static ManualResetEvent receiveCheck = new ManualResetEvent(initialState: false);

		private Thread m_oRecieveThread;

		private bool m_bIsDestroyClass;

		private bool m_bIsReading;

		private StateObject m_oReadState;

		private virtual TcpClient m_oNetSocket
		{
			[DebuggerNonUserCode]
			get
			{
				return _m_oNetSocket;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			[DebuggerNonUserCode]
			set
			{
				_m_oNetSocket = value;
			}
		}

		private virtual System.Windows.Forms.Timer m_oReceiveTimer
		{
			[DebuggerNonUserCode]
			get
			{
				return _m_oReceiveTimer;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			[DebuggerNonUserCode]
			set
			{
				if (_m_oReceiveTimer != null)
				{
					_m_oReceiveTimer.Tick -= m_oReceiveTimer_Tick;
				}
				_m_oReceiveTimer = value;
				if (_m_oReceiveTimer != null)
				{
					_m_oReceiveTimer.Tick += m_oReceiveTimer_Tick;
				}
			}
		}

		private virtual WindowMessage m_oWndMsg
		{
			[DebuggerNonUserCode]
			get
			{
				return _m_oWndMsg;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			[DebuggerNonUserCode]
			set
			{
				if (_m_oWndMsg != null)
				{
					_m_oWndMsg.WindowProc -= m_oWndMsg_WindowProc;
					_m_oWndMsg.DeviceRemoveComplete -= m_oDeviceMsg_DeviceRemoveComplete;
					_m_oWndMsg.DeviceArrivalChange -= m_oDeviceMsg_DeviceArrivalChange;
				}
				_m_oWndMsg = value;
				if (_m_oWndMsg != null)
				{
					_m_oWndMsg.WindowProc += m_oWndMsg_WindowProc;
					_m_oWndMsg.DeviceRemoveComplete += m_oDeviceMsg_DeviceRemoveComplete;
					_m_oWndMsg.DeviceArrivalChange += m_oDeviceMsg_DeviceArrivalChange;
				}
			}
		}

		public virtual object Input
		{
			get
			{
				object result = null;
				m_iLastError = ISerialPort.CommErrorConstants.Success;
				m_oLastError = null;
				int num = 0;
				try
				{
					lock (m_oNetSocket)
					{
						TcpClient oNetSocket = m_oNetSocket;
						if (oNetSocket.Connected)
						{
							num = oNetSocket.Available;
						}
						oNetSocket = null;
					}
				}
				catch (Exception ex)
				{
					ProjectData.SetProjectError(ex);
					Exception oError = ex;
					ProcessErrorCode(ISerialPort.CommErrorConstants.Unknown, ref oError);
					ProjectData.ClearProjectError();
				}
				if (num > 0)
				{
					OnDataReceive();
				}
				try
				{
					if (m_iInputMode == ISerialPort.InputModeConstants.comInputModeBinary)
					{
						byte[] Value = null;
						if (m_iInputLen == 0)
						{
							m_oRxbuffer.Take(ref Value);
						}
						else
						{
							m_oRxbuffer.Take(ref Value, 0, m_iInputLen);
						}
						result = Value;
					}
					else
					{
						string Value2 = null;
						if (m_iInputLen == 0)
						{
							m_oRxbuffer.Take(ref Value2);
						}
						else
						{
							m_oRxbuffer.Take(ref Value2, m_iInputLen);
						}
						result = Value2;
					}
				}
				catch (Exception ex2)
				{
					ProjectData.SetProjectError(ex2);
					Exception oError2 = ex2;
					ProcessErrorCode(ISerialPort.CommErrorConstants.Unknown, ref oError2);
					ProjectData.ClearProjectError();
				}
				return result;
			}
		}

		public virtual object Output
		{
			set
			{
				byte[] bBuffer = null;
				bool flag = true;
				m_oLastError = null;
				if (value is byte[])
				{
					bBuffer = (byte[])value;
				}
				else if (value is string)
				{
					bBuffer = Encoding.Default.GetBytes(Conversions.ToString(value));
				}
				else if (value is byte)
				{
					bBuffer = new byte[1] { Conversions.ToByte(value) };
				}
				else
				{
					flag = false;
				}
				if (!flag)
				{
					return;
				}
				try
				{
					lock (m_oNetSocket)
					{
						TcpClient oNetSocket = m_oNetSocket;
						if (oNetSocket.Connected)
						{
							oNetSocket.GetStream().Write(bBuffer, 0, bBuffer.Length);
						}
						else
						{
							flag = false;
						}
						oNetSocket = null;
					}
					if (m_bIsMonitor)
					{
						ISerialPort.OnMonitorHandler onMonitorEvent = OnMonitor;
						if (onMonitorEvent != null)
						{
							ISerialPort oSender = this;
							onMonitorEvent(ref oSender, ISerialPort.CommMonitorEventConstants.comEvOutput, ref bBuffer);
						}
					}
				}
				catch (Exception ex)
				{
					ProjectData.SetProjectError(ex);
					Exception oError = ex;
					ProcessErrorCode(ISerialPort.CommErrorConstants.Unknown, ref oError);
					ProjectData.ClearProjectError();
				}
			}
		}

		public virtual int InBufferCount
		{
			get
			{
				m_iLastError = ISerialPort.CommErrorConstants.Success;
				m_oLastError = null;
				try
				{
					int num;
					lock (m_oNetSocket)
					{
						TcpClient oNetSocket = m_oNetSocket;
						num = (oNetSocket.Connected ? oNetSocket.Available : 0);
						oNetSocket = null;
					}
					if (num > 0)
					{
						OnDataReceive();
					}
				}
				catch (Exception ex)
				{
					ProjectData.SetProjectError(ex);
					Exception oError = ex;
					ProcessErrorCode(ISerialPort.CommErrorConstants.Unknown, ref oError);
					ProjectData.ClearProjectError();
				}
				return m_oRxbuffer.GetSize();
			}
			set
			{
				if (value == 0)
				{
					m_iLastError = ISerialPort.CommErrorConstants.Success;
					m_oLastError = null;
					try
					{
						m_oNetSocket.GetStream().EndRead(null);
						m_oRxbuffer.Clear();
					}
					catch (Exception ex)
					{
						ProjectData.SetProjectError(ex);
						Exception oError = ex;
						ProcessErrorCode(ISerialPort.CommErrorConstants.Unknown, ref oError);
						ProjectData.ClearProjectError();
					}
				}
			}
		}

		public object BasePort => m_oNetSocket;

		public bool Break
		{
			get
			{
				bool result = false;
				m_iLastError = ISerialPort.CommErrorConstants.Success;
				m_oLastError = null;
				try
				{
				}
				catch (Exception ex)
				{
					ProjectData.SetProjectError(ex);
					Exception oError = ex;
					ProcessErrorCode(ISerialPort.CommErrorConstants.Unknown, ref oError);
					ProjectData.ClearProjectError();
				}
				return result;
			}
			set
			{
				m_iLastError = ISerialPort.CommErrorConstants.Success;
				m_oLastError = null;
				try
				{
				}
				catch (Exception ex)
				{
					ProjectData.SetProjectError(ex);
					Exception oError = ex;
					ProcessErrorCode(ISerialPort.CommErrorConstants.Unknown, ref oError);
					ProjectData.ClearProjectError();
				}
			}
		}

		public bool CDHolding
		{
			get
			{
				bool result = default(bool);
				return result;
			}
		}

		public ISerialPort.CommEventConstants CommEvent => m_iCommEvent;

		public int CommPort
		{
			get
			{
				int result = -1;
				m_iLastError = ISerialPort.CommErrorConstants.Success;
				m_oLastError = null;
				try
				{
					string text = m_oNetSocket.Client.RemoteEndPoint.ToString();
				}
				catch (Exception ex)
				{
					ProjectData.SetProjectError(ex);
					Exception oError = ex;
					ProcessErrorCode(ISerialPort.CommErrorConstants.Unknown, ref oError);
					ProjectData.ClearProjectError();
				}
				return result;
			}
			set
			{
				bool flag = true;
				m_iLastError = ISerialPort.CommErrorConstants.Success;
				m_oLastError = null;
				try
				{
					lock (m_oNetSocket)
					{
						TcpClient oNetSocket = m_oNetSocket;
						if (oNetSocket.Connected && oNetSocket.Connected)
						{
							flag = false;
						}
						if (flag)
						{
							m_bIsAccesError = false;
						}
						oNetSocket = null;
					}
				}
				catch (Exception ex)
				{
					ProjectData.SetProjectError(ex);
					Exception oError = ex;
					ProcessErrorCode(ISerialPort.CommErrorConstants.Unknown, ref oError);
					ProjectData.ClearProjectError();
				}
			}
		}

		public bool CTSHolding
		{
			get
			{
				bool result = default(bool);
				return result;
			}
		}

		public bool DSRHolding
		{
			get
			{
				bool result = default(bool);
				return result;
			}
		}

		public bool DTREnable
		{
			get
			{
				bool result = default(bool);
				return result;
			}
			set
			{
			}
		}

		public ISerialPort.HandshakeConstants Handshaking
		{
			get
			{
				ISerialPort.HandshakeConstants result = default(ISerialPort.HandshakeConstants);
				return result;
			}
			set
			{
			}
		}

		public int InBufferSize
		{
			get
			{
				m_iLastError = ISerialPort.CommErrorConstants.Success;
				m_oLastError = null;
				int receiveBufferSize = default(int);
				try
				{
					receiveBufferSize = m_oNetSocket.ReceiveBufferSize;
					return receiveBufferSize;
				}
				catch (Exception ex)
				{
					ProjectData.SetProjectError(ex);
					Exception oError = ex;
					ProcessErrorCode(ISerialPort.CommErrorConstants.Unknown, ref oError);
					ProjectData.ClearProjectError();
				}
				return receiveBufferSize;
			}
			set
			{
				m_iLastError = ISerialPort.CommErrorConstants.Success;
				m_oLastError = null;
				try
				{
					TcpClient oNetSocket = m_oNetSocket;
					if (oNetSocket.Connected)
					{
						oNetSocket.ReceiveBufferSize = value;
						oNetSocket.Connect(m_oCurrEndPoint);
					}
					else
					{
						oNetSocket.ReceiveBufferSize = value;
					}
					oNetSocket = null;
				}
				catch (Exception ex)
				{
					ProjectData.SetProjectError(ex);
					Exception oError = ex;
					ProcessErrorCode(ISerialPort.CommErrorConstants.Unknown, ref oError);
					ProjectData.ClearProjectError();
				}
			}
		}

		public int InputLen
		{
			get
			{
				return m_iInputLen;
			}
			set
			{
				m_iInputLen = value;
			}
		}

		public ISerialPort.InputModeConstants InputMode
		{
			get
			{
				return m_iInputMode;
			}
			set
			{
				m_iInputMode = value;
			}
		}

		public bool IsBackgroundMode
		{
			get
			{
				return m_bIsBackgroundMode;
			}
			set
			{
				m_bIsBackgroundMode = value;
			}
		}

		public bool IsMonitorIO
		{
			get
			{
				return m_bIsMonitor;
			}
			set
			{
				m_bIsMonitor = value;
			}
		}

		public bool NullDiscard
		{
			get
			{
				bool result = default(bool);
				return result;
			}
			set
			{
			}
		}

		public int OutBufferCount
		{
			get
			{
				m_iLastError = ISerialPort.CommErrorConstants.Success;
				m_oLastError = null;
				try
				{
				}
				catch (Exception ex)
				{
					ProjectData.SetProjectError(ex);
					Exception oError = ex;
					ProcessErrorCode(ISerialPort.CommErrorConstants.Unknown, ref oError);
					ProjectData.ClearProjectError();
				}
				int result = default(int);
				return result;
			}
			set
			{
				if (value == 0)
				{
					m_iLastError = ISerialPort.CommErrorConstants.Success;
					m_oLastError = null;
					try
					{
						m_oNetSocket.GetStream().EndWrite(null);
					}
					catch (Exception ex)
					{
						ProjectData.SetProjectError(ex);
						Exception oError = ex;
						ProcessErrorCode(ISerialPort.CommErrorConstants.Unknown, ref oError);
						ProjectData.ClearProjectError();
					}
				}
			}
		}

		public int OutBufferSize
		{
			get
			{
				m_iLastError = ISerialPort.CommErrorConstants.Success;
				m_oLastError = null;
				int sendBufferSize = default(int);
				try
				{
					sendBufferSize = m_oNetSocket.SendBufferSize;
					return sendBufferSize;
				}
				catch (Exception ex)
				{
					ProjectData.SetProjectError(ex);
					Exception oError = ex;
					ProcessErrorCode(ISerialPort.CommErrorConstants.Unknown, ref oError);
					ProjectData.ClearProjectError();
				}
				return sendBufferSize;
			}
			set
			{
				m_iLastError = ISerialPort.CommErrorConstants.Success;
				m_oLastError = null;
				try
				{
					TcpClient oNetSocket = m_oNetSocket;
					if (oNetSocket.Connected)
					{
						oNetSocket.SendBufferSize = value;
						oNetSocket.Connect(m_oCurrEndPoint);
					}
					else
					{
						oNetSocket.SendBufferSize = value;
					}
					oNetSocket = null;
				}
				catch (Exception ex)
				{
					ProjectData.SetProjectError(ex);
					Exception oError = ex;
					ProcessErrorCode(ISerialPort.CommErrorConstants.Unknown, ref oError);
					ProjectData.ClearProjectError();
				}
			}
		}

		public char ParityReplace
		{
			get
			{
				char result = default(char);
				return result;
			}
			set
			{
			}
		}

		public virtual string PortName
		{
			get
			{
				string result = null;
				m_iLastError = ISerialPort.CommErrorConstants.Success;
				m_oLastError = null;
				try
				{
					result = "TCP" + m_oCurrEndPoint.ToString();
				}
				catch (Exception ex)
				{
					ProjectData.SetProjectError(ex);
					Exception oError = ex;
					ProcessErrorCode(ISerialPort.CommErrorConstants.Unknown, ref oError);
					ProjectData.ClearProjectError();
				}
				return result;
			}
			set
			{
				m_iLastError = ISerialPort.CommErrorConstants.Success;
				m_oLastError = null;
				int num = Strings.InStr(value, "TCP");
				checked
				{
					if (num > 0)
					{
						num = Strings.InStr(value, ":");
						if (num > 0)
						{
							string text = Strings.Mid(value, 4, num - 4);
							int num2 = (int)Math.Round(Conversion.Val(Strings.Mid(value, num + 1)));
							m_szServerIP = text;
							m_iPort = num2;
							IPAddress iPAddress = Dns.GetHostAddresses(text)[0];
							if (iPAddress != null)
							{
								m_oCurrEndPoint = new IPEndPoint(iPAddress, num2);
							}
						}
					}
					try
					{
						lock (m_oNetSocket)
						{
							TcpClient oNetSocket = m_oNetSocket;
							_ = oNetSocket.Connected;
							oNetSocket = null;
							m_bIsAccesError = false;
						}
					}
					catch (Exception ex)
					{
						ProjectData.SetProjectError(ex);
						Exception oError = ex;
						ProcessErrorCode(ISerialPort.CommErrorConstants.Unknown, ref oError);
						ProjectData.ClearProjectError();
					}
				}
			}
		}

		public bool PortOpen
		{
			get
			{
				bool connected = default(bool);
				try
				{
					connected = m_oNetSocket.Connected;
					return connected;
				}
				catch (Exception ex)
				{
					ProjectData.SetProjectError(ex);
					Exception oError = ex;
					ProcessErrorCode(ISerialPort.CommErrorConstants.Unknown, ref oError);
					ProjectData.ClearProjectError();
				}
				return connected;
			}
			set
			{
				m_iLastError = ISerialPort.CommErrorConstants.Success;
				m_oLastError = null;
				try
				{
					lock (m_oNetSocket)
					{
						if (m_oNetSocket.Client == null)
						{
							m_oNetSocket = new TcpClient();
						}
						TcpClient oNetSocket = m_oNetSocket;
						if (oNetSocket.Connected)
						{
							oNetSocket.Close();
						}
						else
						{
							_ = m_bIsPortOpen;
						}
						m_bIsPortOpen = false;
						if (value)
						{
							m_oCurrEndPoint = null;
							try
							{
								m_oNetSocket = new TcpClient(m_szServerIP, m_iPort);
								m_oCurrEndPoint = (IPEndPoint)m_oNetSocket.Client.RemoteEndPoint;
							}
							catch (Exception ex)
							{
								ProjectData.SetProjectError(ex);
								Exception ex2 = ex;
								ProjectData.ClearProjectError();
							}
							Thread.Sleep(200);
							if (oNetSocket.Connected)
							{
								m_oCurrEndPoint = (IPEndPoint)oNetSocket.Client.RemoteEndPoint;
								m_bIsPortOpen = true;
								m_bIsAccesError = false;
							}
						}
						oNetSocket = null;
					}
				}
				catch (Exception ex3)
				{
					ProjectData.SetProjectError(ex3);
					Exception oError = ex3;
					ProcessErrorCode(ISerialPort.CommErrorConstants.Unknown, ref oError);
					ProjectData.ClearProjectError();
				}
			}
		}

		public int RThreshold
		{
			get
			{
				int result = default(int);
				return result;
			}
			set
			{
			}
		}

		public bool RTSEnable
		{
			get
			{
				bool result = default(bool);
				return result;
			}
			set
			{
			}
		}

		public string Settings
		{
			get
			{
				string result = "";
				if (m_oCurrEndPoint != null)
				{
					result = m_oCurrEndPoint.Address.ToString();
				}
				return result;
			}
			set
			{
			}
		}

		public int SThreshold
		{
			get
			{
				int result = default(int);
				return result;
			}
			set
			{
			}
		}

		[method: DebuggerNonUserCode]
		public event ISerialPort.OnCommHandler OnComm;

		[method: DebuggerNonUserCode]
		public event ISerialPort.OnMonitorHandler OnMonitor;

		[method: DebuggerNonUserCode]
		public event ISerialPort.OnPortHandler OnPort;

		public virtual bool ReadBuffer(ref byte[] bBuffer, int iStart, int iLength)
		{
			m_iLastError = ISerialPort.CommErrorConstants.Success;
			m_oLastError = null;
			try
			{
				int num = 0;
				lock (m_oNetSocket)
				{
					TcpClient oNetSocket = m_oNetSocket;
					if (oNetSocket.Connected)
					{
						num = oNetSocket.Available;
					}
					oNetSocket = null;
				}
				if (num > 0)
				{
					OnDataReceive();
				}
			}
			catch (Exception ex)
			{
				ProjectData.SetProjectError(ex);
				Exception oError = ex;
				ProcessErrorCode(ISerialPort.CommErrorConstants.Unknown, ref oError);
				ProjectData.ClearProjectError();
			}
			return m_oRxbuffer.Take(ref bBuffer, iStart, iLength);
		}

		bool ISerialPort.ReadBuffer(ref byte[] bBuffer, int iStart, int iLength)
		{
			//ILSpy generated this explicit interface implementation from .override directive in ReadBuffer
			return this.ReadBuffer(ref bBuffer, iStart, iLength);
		}

		public virtual bool WriteBuffer(ref byte[] bBuffer, int iStart, int iLength)
		{
			object obj = null;
			m_iLastError = ISerialPort.CommErrorConstants.Success;
			m_oLastError = null;
			bool result = default(bool);
			try
			{
				lock (m_oNetSocket)
				{
					TcpClient oNetSocket = m_oNetSocket;
					if (oNetSocket.Connected)
					{
						oNetSocket.GetStream().Write(bBuffer, 0, bBuffer.Length);
						result = true;
					}
					oNetSocket = null;
				}
				if (m_bIsMonitor && iLength > 0)
				{
					byte[] bBuffer2 = new byte[checked(iLength - 1 + 1)];
					Array.Copy(bBuffer, iStart, bBuffer2, 0, iLength);
					ISerialPort.OnMonitorHandler onMonitorEvent = OnMonitor;
					if (onMonitorEvent != null)
					{
						ISerialPort oSender = this;
						onMonitorEvent(ref oSender, ISerialPort.CommMonitorEventConstants.comEvOutput, ref bBuffer2);
					}
					bBuffer2 = null;
				}
			}
			catch (Exception ex)
			{
				ProjectData.SetProjectError(ex);
				Exception oError = ex;
				ProcessErrorCode(ISerialPort.CommErrorConstants.Unknown, ref oError);
				result = false;
				ProjectData.ClearProjectError();
			}
			return result;
		}

		bool ISerialPort.WriteBuffer(ref byte[] bBuffer, int iStart, int iLength)
		{
			//ILSpy generated this explicit interface implementation from .override directive in WriteBuffer
			return this.WriteBuffer(ref bBuffer, iStart, iLength);
		}

		public virtual string CommPortClass(int CommPort)
		{
			return "TCP";
		}

		string ISerialPort.CommPortClass(int CommPort)
		{
			//ILSpy generated this explicit interface implementation from .override directive in CommPortClass
			return this.CommPortClass(CommPort);
		}

		public virtual string CommPortClass(string CommPort)
		{
			return "TCP";
		}

		string ISerialPort.CommPortClass(string CommPort)
		{
			//ILSpy generated this explicit interface implementation from .override directive in CommPortClass
			return this.CommPortClass(CommPort);
		}

		public virtual string CommPortInfo(int CommPort)
		{
			return "TCP";
		}

		string ISerialPort.CommPortInfo(int CommPort)
		{
			//ILSpy generated this explicit interface implementation from .override directive in CommPortInfo
			return this.CommPortInfo(CommPort);
		}

		public virtual string CommPortInfo(string CommPort)
		{
			return "TCP";
		}

		string ISerialPort.CommPortInfo(string CommPort)
		{
			//ILSpy generated this explicit interface implementation from .override directive in CommPortInfo
			return this.CommPortInfo(CommPort);
		}

		public virtual bool CommPortInfo(string CommPort, ref string szDeviceClass, ref string szManufacturer, ref string szHardwareID)
		{
			return Conversions.ToBoolean("TCP");
		}

		bool ISerialPort.CommPortInfo(string CommPort, ref string szDeviceClass, ref string szManufacturer, ref string szHardwareID)
		{
			//ILSpy generated this explicit interface implementation from .override directive in CommPortInfo
			return this.CommPortInfo(CommPort, ref szDeviceClass, ref szManufacturer, ref szHardwareID);
		}

		public void Dispose()
		{
			m_bIsDestroyClass = true;
		}

		void ISerialPort.Dispose()
		{
			//ILSpy generated this explicit interface implementation from .override directive in Dispose
			this.Dispose();
		}

		public virtual string EnumCommPort(int Index)
		{
			string result = "";
			if (m_oCurrEndPoint != null)
			{
				result = m_oCurrEndPoint.Address.ToString() + ":" + Conversions.ToString(m_oCurrEndPoint.Port);
			}
			return result;
		}

		string ISerialPort.EnumCommPort(int Index)
		{
			//ILSpy generated this explicit interface implementation from .override directive in EnumCommPort
			return this.EnumCommPort(Index);
		}

		public virtual string EnumCommPort(int Index, string PortFilter)
		{
			string result = "";
			if (m_oCurrEndPoint != null)
			{
				result = m_oCurrEndPoint.Address.ToString() + ":" + Conversions.ToString(m_oCurrEndPoint.Port);
			}
			return result;
		}

		string ISerialPort.EnumCommPort(int Index, string PortFilter)
		{
			//ILSpy generated this explicit interface implementation from .override directive in EnumCommPort
			return this.EnumCommPort(Index, PortFilter);
		}

		public int EnumCommPortNumber(int Index)
		{
			if (m_oCurrEndPoint != null)
			{
				return 1;
			}
			int result = default(int);
			return result;
		}

		int ISerialPort.EnumCommPortNumber(int Index)
		{
			//ILSpy generated this explicit interface implementation from .override directive in EnumCommPortNumber
			return this.EnumCommPortNumber(Index);
		}

		public int EnumCommPortNumber(int Index, string PortFilter)
		{
			if (m_oCurrEndPoint != null)
			{
				return 1;
			}
			int result = default(int);
			return result;
		}

		int ISerialPort.EnumCommPortNumber(int Index, string PortFilter)
		{
			//ILSpy generated this explicit interface implementation from .override directive in EnumCommPortNumber
			return this.EnumCommPortNumber(Index, PortFilter);
		}

		public ISerialPort.CommErrorConstants GetLastError()
		{
			return m_iLastError;
		}

		ISerialPort.CommErrorConstants ISerialPort.GetLastError()
		{
			//ILSpy generated this explicit interface implementation from .override directive in GetLastError
			return this.GetLastError();
		}

		public ISerialPort.CommErrorConstants GetLastError(ref string szError)
		{
			switch (m_iLastError)
			{
			case ISerialPort.CommErrorConstants.Success:
				szError = "Success";
				break;
			case ISerialPort.CommErrorConstants.PortIsClose:
				szError = "Port is close";
				break;
			case ISerialPort.CommErrorConstants.PortAlreadyOpen:
				szError = "Port is already open";
				break;
			default:
				if (m_oLastError == null)
				{
					szError = "Unknown";
				}
				else
				{
					szError = m_oLastError.ToString();
				}
				break;
			}
			return m_iLastError;
		}

		ISerialPort.CommErrorConstants ISerialPort.GetLastError(ref string szError)
		{
			//ILSpy generated this explicit interface implementation from .override directive in GetLastError
			return this.GetLastError(ref szError);
		}

		public ISerialPort.CommErrorConstants GetLastError(ref Exception oError)
		{
			oError = m_oLastError;
			return m_iLastError;
		}

		ISerialPort.CommErrorConstants ISerialPort.GetLastError(ref Exception oError)
		{
			//ILSpy generated this explicit interface implementation from .override directive in GetLastError
			return this.GetLastError(ref oError);
		}

		public virtual bool IsMyPort(ref string szPortName)
		{
			bool result = false;
			int num = Strings.InStr(szPortName, "TCP", CompareMethod.Text);
			checked
			{
				if (num > 0)
				{
					try
					{
						string text = Strings.Mid(szPortName, num + 3);
						num = Strings.InStr(text, ":", CompareMethod.Text);
						if (num > 0)
						{
							try
							{
								if (Conversions.ToInteger(Strings.Mid(text, num + 1)) != 0)
								{
									result = true;
								}
							}
							catch (Exception ex)
							{
								ProjectData.SetProjectError(ex);
								Exception oError = ex;
								ProcessErrorCode(ISerialPort.CommErrorConstants.Unknown, ref oError);
								ProjectData.ClearProjectError();
							}
						}
					}
					catch (Exception ex2)
					{
						ProjectData.SetProjectError(ex2);
						Exception oError2 = ex2;
						ProcessErrorCode(ISerialPort.CommErrorConstants.Unknown, ref oError2);
						ProjectData.ClearProjectError();
					}
				}
				return result;
			}
		}

		bool ISerialPort.IsMyPort(ref string szPortName)
		{
			//ILSpy generated this explicit interface implementation from .override directive in IsMyPort
			return this.IsMyPort(ref szPortName);
		}

		public virtual int NumberOfPorts()
		{
			int result = default(int);
			return result;
		}

		int ISerialPort.NumberOfPorts()
		{
			//ILSpy generated this explicit interface implementation from .override directive in NumberOfPorts
			return this.NumberOfPorts();
		}

		public virtual int NumberOfPorts(string PortFilter)
		{
			int result = default(int);
			return result;
		}

		int ISerialPort.NumberOfPorts(string PortFilter)
		{
			//ILSpy generated this explicit interface implementation from .override directive in NumberOfPorts
			return this.NumberOfPorts(PortFilter);
		}

		protected void RaiseMonitorEvent(ref ISerialPort oSender, ISerialPort.CommMonitorEventConstants iEvent, ref byte[] bBuffer)
		{
			OnMonitor?.Invoke(ref oSender, iEvent, ref bBuffer);
		}

		private void Class_Init(ref TcpClient oSocket, ref WindowMessage oWndMsg)
		{
			if (oWndMsg == null)
			{
				m_oWndMsg = new WindowMessage();
			}
			else
			{
				m_oWndMsg = oWndMsg;
			}
			if (oSocket == null)
			{
				oSocket = new TcpClient();
			}
			iPostEventConstant = m_oWndMsg.RegisterWindowMessage("PostEvent");
			m_oNetSocket = oSocket;
			TcpClient oNetSocket = m_oNetSocket;
			oNetSocket.ReceiveBufferSize = 1024;
			oNetSocket.SendBufferSize = 1024;
			oNetSocket.ReceiveTimeout = 1000;
			oNetSocket.SendTimeout = 1000;
			oNetSocket = null;
			m_oReceiveTimer = new System.Windows.Forms.Timer();
			System.Windows.Forms.Timer oReceiveTimer = m_oReceiveTimer;
			oReceiveTimer.Interval = 50;
			oReceiveTimer.Enabled = false;
			oReceiveTimer = null;
			if (oSocket.Connected)
			{
				m_oCurrEndPoint = (IPEndPoint)oSocket.Client.RemoteEndPoint;
			}
			Thread oRecieveThread = m_oRecieveThread;
			oRecieveThread.IsBackground = true;
			oRecieveThread.Start();
			oRecieveThread = null;
		}

		[DebuggerNonUserCode]
		public WinSocket()
		{
			m_iCommEvent = (ISerialPort.CommEventConstants)0;
			m_oRxbuffer = new ByteBuffer();
			m_oPostCommEventDelegate = FireCommEvent;
			m_bIsBackgroundMode = false;
			m_bIsMonitor = false;
			m_iInputLen = 0;
			m_iInputMode = ISerialPort.InputModeConstants.comInputModeBinary;
			m_oRecieveThread = new Thread(RecieveThread);
			m_bIsDestroyClass = false;
			m_bIsReading = false;
			m_oReadState = new StateObject();
			TcpClient oSocket = new TcpClient();
			WindowMessage oWndMsg = new WindowMessage();
			Class_Init(ref oSocket, ref oWndMsg);
		}

		[DebuggerNonUserCode]
		public WinSocket(ref string ServerIP, int Port)
		{
			m_iCommEvent = (ISerialPort.CommEventConstants)0;
			m_oRxbuffer = new ByteBuffer();
			m_oPostCommEventDelegate = FireCommEvent;
			m_bIsBackgroundMode = false;
			m_bIsMonitor = false;
			m_iInputLen = 0;
			m_iInputMode = ISerialPort.InputModeConstants.comInputModeBinary;
			m_oRecieveThread = new Thread(RecieveThread);
			m_bIsDestroyClass = false;
			m_bIsReading = false;
			m_oReadState = new StateObject();
			TcpClient oSocket = null;
			m_szServerIP = ServerIP;
			m_iPort = Port;
			try
			{
				oSocket = new TcpClient(ServerIP, Port);
			}
			catch (Exception ex)
			{
				ProjectData.SetProjectError(ex);
				Exception ex2 = ex;
				ProjectData.ClearProjectError();
			}
			WindowMessage oWndMsg = new WindowMessage();
			Class_Init(ref oSocket, ref oWndMsg);
		}

		[DebuggerNonUserCode]
		public WinSocket(ref TcpClient oSocket)
		{
			m_iCommEvent = (ISerialPort.CommEventConstants)0;
			m_oRxbuffer = new ByteBuffer();
			m_oPostCommEventDelegate = FireCommEvent;
			m_bIsBackgroundMode = false;
			m_bIsMonitor = false;
			m_iInputLen = 0;
			m_iInputMode = ISerialPort.InputModeConstants.comInputModeBinary;
			m_oRecieveThread = new Thread(RecieveThread);
			m_bIsDestroyClass = false;
			m_bIsReading = false;
			m_oReadState = new StateObject();
			WindowMessage oWndMsg = new WindowMessage();
			Class_Init(ref oSocket, ref oWndMsg);
		}

		[DebuggerNonUserCode]
		public WinSocket(ref WindowMessage oWndMsg)
		{
			m_iCommEvent = (ISerialPort.CommEventConstants)0;
			m_oRxbuffer = new ByteBuffer();
			m_oPostCommEventDelegate = FireCommEvent;
			m_bIsBackgroundMode = false;
			m_bIsMonitor = false;
			m_iInputLen = 0;
			m_iInputMode = ISerialPort.InputModeConstants.comInputModeBinary;
			m_oRecieveThread = new Thread(RecieveThread);
			m_bIsDestroyClass = false;
			m_bIsReading = false;
			m_oReadState = new StateObject();
			TcpClient oSocket = new TcpClient();
			Class_Init(ref oSocket, ref oWndMsg);
		}

		[DebuggerNonUserCode]
		public WinSocket(ref TcpClient oSocket, ref WindowMessage oWndMsg)
		{
			m_iCommEvent = (ISerialPort.CommEventConstants)0;
			m_oRxbuffer = new ByteBuffer();
			m_oPostCommEventDelegate = FireCommEvent;
			m_bIsBackgroundMode = false;
			m_bIsMonitor = false;
			m_iInputLen = 0;
			m_iInputMode = ISerialPort.InputModeConstants.comInputModeBinary;
			m_oRecieveThread = new Thread(RecieveThread);
			m_bIsDestroyClass = false;
			m_bIsReading = false;
			m_oReadState = new StateObject();
			Class_Init(ref oSocket, ref oWndMsg);
		}

		public static TcpClient getSocketFromIP(ref string ServerIP, int Port, [Optional][DefaultParameterValue(null)] ref IPEndPoint oEndPoint)
		{
			IPAddress iPAddress = IPAddress.Parse(ServerIP);
			oEndPoint = new IPEndPoint(iPAddress, Port);
			ServerIP = iPAddress.ToString();
			return new TcpClient(oEndPoint);
		}

		private void OnDataReceive()
		{
			byte[] Value = null;
			checked
			{
				if (!m_bIsOnDataReceive)
				{
					m_bIsOnDataReceive = true;
					m_iLastError = ISerialPort.CommErrorConstants.Success;
					try
					{
						int size = m_oRxbuffer.GetSize();
						int num = 0;
						lock (m_oNetSocket)
						{
							TcpClient oNetSocket = m_oNetSocket;
							try
							{
								if (oNetSocket.Connected && size < oNetSocket.ReceiveBufferSize)
								{
									num = oNetSocket.Available;
									if (num > 0)
									{
										try
										{
											Value = new byte[num - 1 + 1];
											oNetSocket.GetStream().Read(Value, 0, num);
										}
										catch (Exception ex)
										{
											ProjectData.SetProjectError(ex);
											Exception oError = ex;
											ProcessErrorCode(ISerialPort.CommErrorConstants.Unknown, ref oError);
											num = 0;
											ProjectData.ClearProjectError();
										}
									}
								}
							}
							catch (Exception ex2)
							{
								ProjectData.SetProjectError(ex2);
								Exception oError2 = ex2;
								ProcessErrorCode(ISerialPort.CommErrorConstants.Unknown, ref oError2);
								num = 0;
								ProjectData.ClearProjectError();
							}
							oNetSocket = null;
						}
						if (num > 0)
						{
							try
							{
								if (m_bIsMonitor)
								{
									byte[] bBuffer = new byte[num - 1 + 1];
									Array.Copy(Value, bBuffer, num);
									ISerialPort.OnMonitorHandler onMonitorEvent = OnMonitor;
									if (onMonitorEvent != null)
									{
										ISerialPort oSender = this;
										onMonitorEvent(ref oSender, ISerialPort.CommMonitorEventConstants.comEvInput, ref bBuffer);
									}
									bBuffer = null;
								}
							}
							catch (Exception ex3)
							{
								ProjectData.SetProjectError(ex3);
								Exception oError3 = ex3;
								ProcessErrorCode(ISerialPort.CommErrorConstants.Unknown, ref oError3);
								ProjectData.ClearProjectError();
							}
							m_oRxbuffer.Append(ref Value);
							size = m_oRxbuffer.GetSize();
						}
					}
					catch (Exception ex4)
					{
						ProjectData.SetProjectError(ex4);
						Exception oError4 = ex4;
						ProcessErrorCode(ISerialPort.CommErrorConstants.Unknown, ref oError4);
						ProjectData.ClearProjectError();
					}
					m_bIsOnDataReceive = false;
					return;
				}
				try
				{
					lock (m_oNetSocket)
					{
						TcpClient oNetSocket2 = m_oNetSocket;
						if (oNetSocket2.Connected && oNetSocket2.Available > 0)
						{
							System.Windows.Forms.Timer oReceiveTimer = m_oReceiveTimer;
							if (!oReceiveTimer.Enabled)
							{
								oReceiveTimer.Enabled = true;
							}
							oReceiveTimer = null;
						}
						oNetSocket2 = null;
					}
				}
				catch (Exception ex5)
				{
					ProjectData.SetProjectError(ex5);
					Exception oError5 = ex5;
					ProcessErrorCode(ISerialPort.CommErrorConstants.Unknown, ref oError5);
					ProjectData.ClearProjectError();
				}
			}
		}

		private void m_oReceiveTimer_Tick(object sender, EventArgs e)
		{
			try
			{
				if (!m_bIsOnDataReceive)
				{
					m_oReceiveTimer.Enabled = false;
					TcpClient oNetSocket = m_oNetSocket;
					if (oNetSocket.Connected && oNetSocket.GetStream().DataAvailable)
					{
						PostCommEvent(ISerialPort.CommEventConstants.comEvReceive);
					}
					oNetSocket = null;
				}
			}
			catch (Exception ex)
			{
				ProjectData.SetProjectError(ex);
				Exception oError = ex;
				ProcessErrorCode(ISerialPort.CommErrorConstants.Unknown, ref oError);
				ProjectData.ClearProjectError();
			}
		}

		private void FireCommEvent(ref ISerialPort.CommEventConstants iCommEvent)
		{
			m_iCommEvent = iCommEvent;
			if (iCommEvent == ISerialPort.CommEventConstants.comEvReceive)
			{
				try
				{
					TcpClient oNetSocket = m_oNetSocket;
					if (oNetSocket.Connected && oNetSocket.GetStream().DataAvailable)
					{
						OnDataReceive();
					}
					oNetSocket = null;
					return;
				}
				catch (Exception ex)
				{
					ProjectData.SetProjectError(ex);
					Exception oError = ex;
					ProcessErrorCode(ISerialPort.CommErrorConstants.Unknown, ref oError);
					ProjectData.ClearProjectError();
					return;
				}
			}
			if (iCommEvent == ISerialPort.CommEventConstants.comEventFrame)
			{
			}
			try
			{
				ISerialPort.OnCommHandler onCommEvent = OnComm;
				if (onCommEvent != null)
				{
					ISerialPort oSender = this;
					onCommEvent(ref oSender, iCommEvent);
				}
			}
			catch (Exception ex2)
			{
				ProjectData.SetProjectError(ex2);
				Exception oError2 = ex2;
				ProcessErrorCode(ISerialPort.CommErrorConstants.Unknown, ref oError2);
				ProjectData.ClearProjectError();
			}
		}

		private void PostCommEvent(ISerialPort.CommEventConstants iCommEvent)
		{
			if (m_bIsBackgroundMode)
			{
				FireCommEvent(ref iCommEvent);
			}
			else
			{
				m_oWndMsg.PostMessage(iPostEventConstant, (int)iCommEvent, 0);
			}
		}

		protected void ProcessErrorCode(ISerialPort.CommErrorConstants iErrorCode, ref Exception oError)
		{
			int try0000_dispatch = -1;
			int num3 = default(int);
			int num = default(int);
			int num2 = default(int);
			TcpClient tcpClient = default(TcpClient);
			while (true)
			{
				try
				{
					/*Note: ILSpy has introduced the following switch to emulate a goto from catch-block to try-block*/;
					switch (try0000_dispatch)
					{
					default:
						ProjectData.ClearProjectError();
						num3 = -2;
						goto IL_0008;
					case 136:
						{
							num = num2;
							switch ((num3 <= -2) ? 1 : num3)
							{
							case 1:
								break;
							default:
								goto end_IL_0000;
							}
							int num4 = num + 1;
							num = 0;
							switch (num4)
							{
							case 1:
								break;
							case 2:
								goto IL_0008;
							case 3:
								goto IL_001b;
							case 4:
								goto IL_0024;
							case 5:
								goto IL_0029;
							case 6:
								goto IL_0032;
							case 7:
							case 8:
								goto IL_003b;
							case 9:
							case 10:
								goto end_IL_0000_2;
							default:
								goto end_IL_0000;
							case 11:
								goto end_IL_0000_3;
							}
							goto default;
						}
						IL_0024:
						num2 = 4;
						iErrorCode = ISerialPort.CommErrorConstants.PortAccessFail;
						goto IL_0029;
						IL_0029:
						num2 = 5;
						tcpClient = m_oNetSocket;
						goto IL_0032;
						IL_001b:
						num2 = 3;
						m_bIsAccesError = true;
						goto IL_0024;
						IL_0032:
						num2 = 6;
						_ = tcpClient.Connected;
						goto IL_003b;
						IL_0008:
						num2 = 2;
						if (iErrorCode != ISerialPort.CommErrorConstants.Unknown || !(m_oLastError is UnauthorizedAccessException))
						{
							break;
						}
						goto IL_001b;
						IL_003b:
						tcpClient = null;
						break;
						end_IL_0000_2:
						break;
					}
					num2 = 10;
					m_iLastError = iErrorCode;
					break;
					end_IL_0000:;
				}
				catch (object obj) when (obj is Exception && num3 != 0 && num == 0)
				{
					ProjectData.SetProjectError((Exception)obj);
					try0000_dispatch = 136;
					continue;
				}
				throw ProjectData.CreateProjectError(-2146828237);
				continue;
				end_IL_0000_3:
				break;
			}
			if (num != 0)
			{
				ProjectData.ClearProjectError();
			}
		}

		private void m_oDeviceMsg_DeviceArrivalChange(string szDevice)
		{
			ISerialPort.OnPortHandler onPortEvent = OnPort;
			if (onPortEvent != null)
			{
				ISerialPort oSender = this;
				onPortEvent(ref oSender, ISerialPort.CommPortEventConstants.comEvPlugin, szDevice);
			}
		}

		private void m_oDeviceMsg_DeviceRemoveComplete(string szDevice)
		{
			if (PortOpen && Operators.CompareString(PortName, szDevice, TextCompare: false) == 0)
			{
				PortOpen = false;
				ISerialPort.OnPortHandler onPortEvent = OnPort;
				if (onPortEvent != null)
				{
					ISerialPort.OnPortHandler onPortHandler = onPortEvent;
					ISerialPort oSender = this;
					onPortHandler(ref oSender, ISerialPort.CommPortEventConstants.comEvRemoveClosed, szDevice);
				}
			}
			else
			{
				ISerialPort.OnPortHandler onPortEvent = OnPort;
				if (onPortEvent != null)
				{
					ISerialPort.OnPortHandler onPortHandler2 = onPortEvent;
					ISerialPort oSender = this;
					onPortHandler2(ref oSender, ISerialPort.CommPortEventConstants.comEvRemove, szDevice);
				}
			}
		}

		private void m_oWndMsg_WindowProc(ref int iResult, int hWnd, int uMsg, int wParam, ref IntPtr lParam)
		{
			if (uMsg == iPostEventConstant)
			{
				iResult = 1;
				ISerialPort.CommEventConstants iCommEvent = (ISerialPort.CommEventConstants)wParam;
				FireCommEvent(ref iCommEvent);
			}
		}

		private void m_oSocket_DataReceived()
		{
			PostCommEvent(ISerialPort.CommEventConstants.comEvReceive);
		}

		private void DataReceived(IAsyncResult ar)
		{
			StateObject stateObject = (StateObject)ar.AsyncState;
			TcpClient workSocket = stateObject.workSocket;
			int num = workSocket.GetStream().EndRead(ar);
			byte[] array = null;
			if (num <= 0)
			{
				return;
			}
			stateObject.sb.Append(ref stateObject.buffer, 0, num);
			m_bIsReading = false;
			if (stateObject.sb.GetSize() <= 1)
			{
				return;
			}
			int size = stateObject.sb.GetSize();
			checked
			{
				array = new byte[size - 1 + 1];
				stateObject.sb.Take(ref array);
				try
				{
					if (m_bIsMonitor)
					{
						byte[] bBuffer = new byte[size - 1 + 1];
						Array.Copy(array, bBuffer, size);
						ISerialPort.OnMonitorHandler onMonitorEvent = OnMonitor;
						if (onMonitorEvent != null)
						{
							ISerialPort oSender = this;
							onMonitorEvent(ref oSender, ISerialPort.CommMonitorEventConstants.comEvInput, ref bBuffer);
						}
						bBuffer = null;
					}
				}
				catch (Exception ex)
				{
					ProjectData.SetProjectError(ex);
					Exception oError = ex;
					ProcessErrorCode(ISerialPort.CommErrorConstants.Unknown, ref oError);
					ProjectData.ClearProjectError();
				}
				m_oRxbuffer.Append(ref array);
			}
		}

		private void m_oSocket_WriteCallBack(IAsyncResult ar)
		{
			TcpClient tcpClient = (TcpClient)ar.AsyncState;
			tcpClient.GetStream().EndWrite(ar);
			sendDone.Set();
		}

		private void readanswer()
		{
			OnDataReceive();
		}

		private void RecieveThread()
		{
			bool flag = false;
			m_bIsReading = false;
			while (!m_bIsDestroyClass)
			{
				flag = ((!m_oNetSocket.Connected) ? receiveCheck.WaitOne() : receiveCheck.WaitOne(1, exitContext: false));
				if (m_bIsDestroyClass)
				{
					break;
				}
				if (!flag && m_oNetSocket.Connected)
				{
					flag = true;
				}
				if (flag)
				{
					receiveCheck.Reset();
					if (m_oNetSocket.Available > 0)
					{
						m_oSocket_DataReceived();
					}
				}
			}
		}
	}
}
