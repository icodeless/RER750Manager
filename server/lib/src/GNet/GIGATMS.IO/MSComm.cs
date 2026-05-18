using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using GIGATMS.Windows;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Microsoft.Win32;

namespace GIGATMS.IO
{
	[DebuggerNonUserCode]
	public class MSComm : ISerialPort
	{
		[DebuggerNonUserCode]
		private class clsPortInfo
		{
			public int iPortNumber;

			public string szPortName;

			public string szManufacturer;

			public string szClass;

			public string szHardwareID;

			[DebuggerNonUserCode]
			public clsPortInfo()
			{
			}
		}

		private delegate void FireCommEventDelegate(ref ISerialPort.CommEventConstants iCommEvent);

		private Exception m_oLastError;

		private ISerialPort.CommErrorConstants m_iLastError;

		private bool m_bIsAccesError;

		[AccessedThroughProperty("m_oReceiveTimer")]
		private System.Windows.Forms.Timer _m_oReceiveTimer;

		private ISerialPort.CommEventConstants m_iCommEvent;

		private bool m_bCDHolding;

		private bool m_bCTSHolding;

		private bool m_bDSRHolding;

		private bool m_bRingHolding;

		private int m_iEnterDataReceived;

		private int m_iInputLen;

		private ISerialPort.InputModeConstants m_iInputMode;

		private int m_iRThreshold;

		private int m_iSThreshold;

		private bool m_bSkipListPorts;

		private Collection m_oPortInfos;

		private ByteBuffer m_oRxbuffer;

		private bool m_bIsOnDataReceive;

		private bool m_bIsMonitor;

		private bool m_bIsBackgroundMode;

		private FireCommEventDelegate m_oPostCommEventDelegate;

		private int iPostEventConstant;

		[AccessedThroughProperty("m_oWndMsg")]
		private WindowMessage _m_oWndMsg;

		[AccessedThroughProperty("m_oSerialPort")]
		private SerialPort _m_oSerialPort;

		private bool m_bIsPortOpen;

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

		private virtual SerialPort m_oSerialPort
		{
			[DebuggerNonUserCode]
			get
			{
				return _m_oSerialPort;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			[DebuggerNonUserCode]
			set
			{
				if (_m_oSerialPort != null)
				{
					_m_oSerialPort.PinChanged -= m_oSerialPort_PinChanged;
					_m_oSerialPort.ErrorReceived -= m_oSerialPort_ErrorReceived;
					_m_oSerialPort.DataReceived -= m_oSerialPort_DataReceived;
				}
				_m_oSerialPort = value;
				if (_m_oSerialPort != null)
				{
					_m_oSerialPort.PinChanged += m_oSerialPort_PinChanged;
					_m_oSerialPort.ErrorReceived += m_oSerialPort_ErrorReceived;
					_m_oSerialPort.DataReceived += m_oSerialPort_DataReceived;
				}
			}
		}

		public object BasePort => m_oSerialPort;

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

		public bool Break
		{
			get
			{
				bool result = false;
				m_iLastError = ISerialPort.CommErrorConstants.Success;
				m_oLastError = null;
				try
				{
					result = m_oSerialPort.BreakState;
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
					m_oSerialPort.BreakState = value;
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

		public bool CDHolding => m_bCDHolding;

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
					string portName = m_oSerialPort.PortName;
					if (Operators.CompareString(portName.Substring(0, 3), "COM", TextCompare: false) == 0)
					{
						portName = portName.Replace(":", "");
						result = Convert.ToInt16(portName.Substring(3));
					}
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
					lock (m_oSerialPort)
					{
						SerialPort oSerialPort = m_oSerialPort;
						if (oSerialPort.IsOpen)
						{
							oSerialPort.Close();
							Thread.Sleep(150);
							if (oSerialPort.IsOpen)
							{
								flag = false;
							}
						}
						if (flag)
						{
							oSerialPort.PortName = Strings.Format(value, "COM0");
							m_bIsAccesError = false;
						}
						oSerialPort = null;
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

		public bool CTSHolding => m_bCTSHolding;

		public bool DSRHolding => m_bDSRHolding;

		public bool DTREnable
		{
			get
			{
				bool result = false;
				m_iLastError = ISerialPort.CommErrorConstants.Success;
				m_oLastError = null;
				try
				{
					result = m_oSerialPort.DtrEnable;
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
					m_oSerialPort.DtrEnable = value;
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

		public ISerialPort.HandshakeConstants Handshaking
		{
			get
			{
				ISerialPort.HandshakeConstants result = ISerialPort.HandshakeConstants.comNone;
				m_iLastError = ISerialPort.CommErrorConstants.Success;
				m_oLastError = null;
				try
				{
					result = (ISerialPort.HandshakeConstants)m_oSerialPort.Handshake;
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
					m_oSerialPort.Handshake = (Handshake)value;
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

		public int InBufferCount
		{
			get
			{
				m_iLastError = ISerialPort.CommErrorConstants.Success;
				m_oLastError = null;
				try
				{
					int num;
					lock (m_oSerialPort)
					{
						SerialPort oSerialPort = m_oSerialPort;
						num = (oSerialPort.IsOpen ? oSerialPort.BytesToRead : 0);
						oSerialPort = null;
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
						m_oSerialPort.DiscardInBuffer();
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

		public int InBufferSize
		{
			get
			{
				m_iLastError = ISerialPort.CommErrorConstants.Success;
				m_oLastError = null;
				int readBufferSize = default(int);
				try
				{
					readBufferSize = m_oSerialPort.ReadBufferSize;
					return readBufferSize;
				}
				catch (Exception ex)
				{
					ProjectData.SetProjectError(ex);
					Exception oError = ex;
					ProcessErrorCode(ISerialPort.CommErrorConstants.Unknown, ref oError);
					ProjectData.ClearProjectError();
				}
				return readBufferSize;
			}
			set
			{
				m_iLastError = ISerialPort.CommErrorConstants.Success;
				m_oLastError = null;
				try
				{
					SerialPort oSerialPort = m_oSerialPort;
					if (oSerialPort.IsOpen)
					{
						oSerialPort.Close();
						Thread.Sleep(150);
						oSerialPort.ReadBufferSize = value;
						oSerialPort.Open();
					}
					else
					{
						oSerialPort.ReadBufferSize = value;
					}
					oSerialPort = null;
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

		public object Input
		{
			get
			{
				object result = null;
				m_iLastError = ISerialPort.CommErrorConstants.Success;
				m_oLastError = null;
				int num = 0;
				try
				{
					lock (m_oSerialPort)
					{
						SerialPort oSerialPort = m_oSerialPort;
						if (oSerialPort.IsOpen)
						{
							num = oSerialPort.BytesToRead;
						}
						oSerialPort = null;
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

		public bool NullDiscard
		{
			get
			{
				bool result = false;
				m_iLastError = ISerialPort.CommErrorConstants.Success;
				m_oLastError = null;
				try
				{
					result = m_oSerialPort.DiscardNull;
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
					m_oSerialPort.DiscardNull = value;
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

		public int OutBufferCount
		{
			get
			{
				m_iLastError = ISerialPort.CommErrorConstants.Success;
				m_oLastError = null;
				int bytesToWrite = default(int);
				try
				{
					bytesToWrite = m_oSerialPort.BytesToWrite;
					return bytesToWrite;
				}
				catch (Exception ex)
				{
					ProjectData.SetProjectError(ex);
					Exception oError = ex;
					ProcessErrorCode(ISerialPort.CommErrorConstants.Unknown, ref oError);
					ProjectData.ClearProjectError();
				}
				return bytesToWrite;
			}
			set
			{
				if (value == 0)
				{
					m_iLastError = ISerialPort.CommErrorConstants.Success;
					m_oLastError = null;
					try
					{
						m_oSerialPort.DiscardOutBuffer();
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
				int writeBufferSize = default(int);
				try
				{
					writeBufferSize = m_oSerialPort.WriteBufferSize;
					return writeBufferSize;
				}
				catch (Exception ex)
				{
					ProjectData.SetProjectError(ex);
					Exception oError = ex;
					ProcessErrorCode(ISerialPort.CommErrorConstants.Unknown, ref oError);
					ProjectData.ClearProjectError();
				}
				return writeBufferSize;
			}
			set
			{
				m_iLastError = ISerialPort.CommErrorConstants.Success;
				m_oLastError = null;
				try
				{
					SerialPort oSerialPort = m_oSerialPort;
					if (oSerialPort.IsOpen)
					{
						oSerialPort.Close();
						Thread.Sleep(150);
						oSerialPort.WriteBufferSize = value;
						oSerialPort.Open();
					}
					else
					{
						oSerialPort.WriteBufferSize = value;
					}
					oSerialPort = null;
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

		public object Output
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
					lock (m_oSerialPort)
					{
						SerialPort oSerialPort = m_oSerialPort;
						if (oSerialPort.IsOpen)
						{
							oSerialPort.Write(bBuffer, 0, bBuffer.Length);
						}
						else
						{
							flag = false;
						}
						oSerialPort = null;
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

		public char ParityReplace
		{
			get
			{
				m_iLastError = ISerialPort.CommErrorConstants.Success;
				m_oLastError = null;
				char result = default(char);
				try
				{
					result = Conversions.ToChar(m_oSerialPort.ParityReplace.ToString());
					return result;
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
					m_oSerialPort.ParityReplace = Conversions.ToByte(value.ToString());
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

		public string PortName
		{
			get
			{
				string result = null;
				m_iLastError = ISerialPort.CommErrorConstants.Success;
				m_oLastError = null;
				try
				{
					result = m_oSerialPort.PortName;
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
				int num = Strings.InStr(value, ":");
				if (num > 0)
				{
					value = Strings.Left(value, checked(num - 1));
				}
				try
				{
					lock (m_oSerialPort)
					{
						SerialPort oSerialPort = m_oSerialPort;
						if (oSerialPort.IsOpen)
						{
							oSerialPort.Close();
							Thread.Sleep(150);
						}
						oSerialPort.PortName = value;
						oSerialPort = null;
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

		public bool PortOpen
		{
			get
			{
				bool isOpen = default(bool);
				try
				{
					isOpen = m_oSerialPort.IsOpen;
				}
				catch (Exception ex)
				{
					ProjectData.SetProjectError(ex);
					Exception oError = ex;
					ProcessErrorCode(ISerialPort.CommErrorConstants.Unknown, ref oError);
					ProjectData.ClearProjectError();
				}
				if (!isOpen && m_bIsPortOpen)
				{
					SerialPort oSerialPort = m_oSerialPort;
					TerminalSerialPort(ref oSerialPort);
					m_oSerialPort = oSerialPort;
				}
				return isOpen;
			}
			set
			{
				m_iLastError = ISerialPort.CommErrorConstants.Success;
				m_oLastError = null;
				try
				{
					lock (m_oSerialPort)
					{
						SerialPort oSerialPort = m_oSerialPort;
						if (oSerialPort.IsOpen)
						{
							oSerialPort.Close();
							Thread.Sleep(150);
						}
						else if (m_bIsPortOpen)
						{
							SerialPort oSerialPort2 = m_oSerialPort;
							TerminalSerialPort(ref oSerialPort2);
							m_oSerialPort = oSerialPort2;
						}
						m_bIsPortOpen = false;
						if (value)
						{
							m_iEnterDataReceived = 0;
							oSerialPort.ReceivedBytesThreshold = 1;
							oSerialPort.Open();
							Thread.Sleep(200);
							if (oSerialPort.IsOpen)
							{
								m_bIsPortOpen = true;
								m_bIsAccesError = false;
							}
						}
						oSerialPort = null;
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

		public int RThreshold
		{
			get
			{
				m_iLastError = ISerialPort.CommErrorConstants.Success;
				m_oLastError = null;
				try
				{
					m_iRThreshold = m_oSerialPort.ReceivedBytesThreshold;
				}
				catch (Exception ex)
				{
					ProjectData.SetProjectError(ex);
					Exception oError = ex;
					ProcessErrorCode(ISerialPort.CommErrorConstants.Unknown, ref oError);
					ProjectData.ClearProjectError();
				}
				return m_iRThreshold;
			}
			set
			{
				m_iLastError = ISerialPort.CommErrorConstants.Success;
				m_oLastError = null;
				m_iRThreshold = value;
				try
				{
					m_oSerialPort.ReceivedBytesThreshold = value;
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

		public bool RTSEnable
		{
			get
			{
				bool result = false;
				m_iLastError = ISerialPort.CommErrorConstants.Success;
				m_oLastError = null;
				try
				{
					result = m_oSerialPort.RtsEnable;
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
					m_oSerialPort.RtsEnable = value;
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

		public string Settings
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				bool flag = false;
				m_iLastError = ISerialPort.CommErrorConstants.Success;
				m_oLastError = null;
				try
				{
					SerialPort oSerialPort = m_oSerialPort;
					stringBuilder.Append(oSerialPort.BaudRate);
					switch (oSerialPort.Parity)
					{
					case Parity.Even:
						stringBuilder.Append(",E,");
						break;
					case Parity.Mark:
						stringBuilder.Append(",M,");
						break;
					case Parity.None:
						stringBuilder = stringBuilder.Append(",N,");
						break;
					case Parity.Odd:
						stringBuilder = stringBuilder.Append(",O,");
						break;
					case Parity.Space:
						stringBuilder = stringBuilder.Append(",S,");
						break;
					default:
						flag = true;
						break;
					}
					stringBuilder.Append(oSerialPort.DataBits);
					switch (oSerialPort.StopBits)
					{
					case StopBits.One:
						stringBuilder.Append(",1");
						break;
					case StopBits.OnePointFive:
						stringBuilder.Append(",1.5");
						break;
					case StopBits.Two:
						stringBuilder.Append(",2");
						break;
					default:
						flag = true;
						break;
					}
					oSerialPort = null;
				}
				catch (Exception ex)
				{
					ProjectData.SetProjectError(ex);
					Exception oError = ex;
					ProcessErrorCode(ISerialPort.CommErrorConstants.Unknown, ref oError);
					ProjectData.ClearProjectError();
				}
				return stringBuilder.ToString();
			}
			set
			{
				if (value == null || value.Length <= 0)
				{
					return;
				}
				m_iLastError = ISerialPort.CommErrorConstants.Success;
				string[] array = Strings.Split(value, ",");
				checked
				{
					try
					{
						lock (m_oSerialPort)
						{
							SerialPort oSerialPort = m_oSerialPort;
							int num = array.Length - 1;
							for (int i = 0; i <= num; i++)
							{
								if (Versioned.IsNumeric(array[i]))
								{
									double num2 = Convert.ToDouble(array[i]);
									double num3 = num2;
									if (num3 == 1.0)
									{
										oSerialPort.StopBits = StopBits.One;
									}
									else if (num3 == 1.5)
									{
										oSerialPort.StopBits = StopBits.OnePointFive;
									}
									else if (num3 == 2.0)
									{
										oSerialPort.StopBits = StopBits.Two;
									}
									else if (num3 >= 4.0 && num3 <= 8.0)
									{
										oSerialPort.DataBits = (byte)Math.Round(num2);
									}
									else
									{
										oSerialPort.BaudRate = (int)Math.Round(num2);
									}
									continue;
								}
								switch (Strings.Left(array[i].ToUpper(), 1))
								{
								case "E":
									oSerialPort.Parity = Parity.Even;
									break;
								case "M":
									oSerialPort.Parity = Parity.Mark;
									break;
								case "N":
									oSerialPort.Parity = Parity.None;
									break;
								case "O":
									oSerialPort.Parity = Parity.Odd;
									break;
								case "S":
									oSerialPort.Parity = Parity.Space;
									break;
								}
							}
							oSerialPort = null;
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

		public int SThreshold
		{
			get
			{
				return m_iSThreshold;
			}
			set
			{
				m_iSThreshold = value;
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

		[method: DebuggerNonUserCode]
		public event ISerialPort.OnCommHandler OnComm;

		[method: DebuggerNonUserCode]
		public event ISerialPort.OnMonitorHandler OnMonitor;

		[method: DebuggerNonUserCode]
		public event ISerialPort.OnPortHandler OnPort;

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
						lock (m_oSerialPort)
						{
							SerialPort oSerialPort = m_oSerialPort;
							try
							{
								if (oSerialPort.IsOpen && size < oSerialPort.ReadBufferSize)
								{
									num = oSerialPort.BytesToRead;
									if (num > 0)
									{
										try
										{
											Value = new byte[num - 1 + 1];
											oSerialPort.Read(Value, 0, num);
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
							oSerialPort = null;
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
					lock (m_oSerialPort)
					{
						SerialPort oSerialPort2 = m_oSerialPort;
						if (oSerialPort2.IsOpen && oSerialPort2.BytesToRead > 0)
						{
							System.Windows.Forms.Timer oReceiveTimer = m_oReceiveTimer;
							if (!oReceiveTimer.Enabled)
							{
								oReceiveTimer.Enabled = true;
							}
							oReceiveTimer = null;
						}
						oSerialPort2 = null;
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
					SerialPort oSerialPort = m_oSerialPort;
					if (oSerialPort.IsOpen && oSerialPort.BytesToRead > 0)
					{
						PostCommEvent(ISerialPort.CommEventConstants.comEvReceive);
					}
					oSerialPort = null;
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
					SerialPort oSerialPort = m_oSerialPort;
					if (oSerialPort.IsOpen && oSerialPort.BytesToRead > 0)
					{
						OnDataReceive();
					}
					oSerialPort = null;
					if (m_iRThreshold > 0 && m_oRxbuffer.GetSize() >= m_iRThreshold)
					{
						ISerialPort.OnCommHandler onCommEvent = OnComm;
						if (onCommEvent != null)
						{
							ISerialPort.OnCommHandler onCommHandler = onCommEvent;
							ISerialPort oSender = this;
							onCommHandler(ref oSender, iCommEvent);
						}
					}
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
				PortOpen = false;
				Thread.Sleep(150);
				PortOpen = true;
			}
			try
			{
				ISerialPort.OnCommHandler onCommEvent = OnComm;
				if (onCommEvent != null)
				{
					ISerialPort.OnCommHandler onCommHandler2 = onCommEvent;
					ISerialPort oSender = this;
					onCommHandler2(ref oSender, iCommEvent);
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

		private void TerminalSerialPort(ref SerialPort oSerialPort)
		{
			int try0000_dispatch = -1;
			int num3 = default(int);
			int num = default(int);
			int num2 = default(int);
			object instance = default(object);
			FieldInfo field = default(FieldInfo);
			object objectValue = default(object);
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
					case 238:
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
								goto IL_001f;
							case 4:
								goto IL_0025;
							case 5:
								goto IL_0036;
							case 6:
								goto IL_003c;
							case 7:
								goto IL_0041;
							case 8:
								goto IL_005b;
							case 9:
								goto IL_0075;
							case 10:
								goto IL_0090;
							case 11:
								goto end_IL_0000_2;
							default:
								goto end_IL_0000;
							case 12:
							case 13:
							case 14:
								goto end_IL_0000_3;
							}
							goto default;
						}
						IL_005b:
						num2 = 8;
						NewLateBinding.LateCall(instance, null, "Close", new object[0], null, null, null, IgnoreReturn: true);
						goto IL_0075;
						IL_0075:
						num2 = 9;
						NewLateBinding.LateCall(instance, null, "Dispose", new object[0], null, null, null, IgnoreReturn: true);
						goto IL_0090;
						IL_0041:
						num2 = 7;
						NewLateBinding.LateCall(instance, null, "Flush", new object[0], null, null, null, IgnoreReturn: true);
						goto IL_005b;
						IL_0090:
						instance = null;
						break;
						IL_0008:
						num2 = 2;
						field = oSerialPort.GetType().GetField("internalSerialStream", BindingFlags.Instance | BindingFlags.NonPublic);
						goto IL_001f;
						IL_001f:
						num2 = 3;
						if (field == null)
						{
							goto end_IL_0000_3;
						}
						goto IL_0025;
						IL_0025:
						num2 = 4;
						objectValue = RuntimeHelpers.GetObjectValue(field.GetValue(oSerialPort));
						goto IL_0036;
						IL_0036:
						num2 = 5;
						if (objectValue == null)
						{
							goto end_IL_0000_3;
						}
						goto IL_003c;
						IL_003c:
						num2 = 6;
						instance = objectValue;
						goto IL_0041;
						end_IL_0000_2:
						break;
					}
					num2 = 11;
					field.SetValue(oSerialPort, null);
					break;
					end_IL_0000:;
				}
				catch (object obj) when (obj is Exception && num3 != 0 && num == 0)
				{
					ProjectData.SetProjectError((Exception)obj);
					try0000_dispatch = 238;
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

		private void ProcessErrorCode(ISerialPort.CommErrorConstants iErrorCode, ref Exception oError)
		{
			int try0000_dispatch = -1;
			int num3 = default(int);
			int num = default(int);
			int num2 = default(int);
			SerialPort serialPort = default(SerialPort);
			while (true)
			{
				try
				{
					/*Note: ILSpy has introduced the following switch to emulate a goto from catch-block to try-block*/;
					SerialPort oSerialPort;
					switch (try0000_dispatch)
					{
					default:
						ProjectData.ClearProjectError();
						num3 = -2;
						goto IL_0008;
					case 251:
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
								goto IL_001c;
							case 4:
								goto IL_0026;
							case 5:
								goto IL_002c;
							case 6:
								goto IL_0036;
							case 7:
								goto IL_0041;
							case 8:
							case 9:
								goto IL_004a;
							case 10:
								goto IL_004c;
							case 12:
								goto IL_0068;
							case 13:
								goto IL_0081;
							case 11:
							case 14:
							case 15:
								goto end_IL_0000_2;
							default:
								goto end_IL_0000;
							case 16:
								goto end_IL_0000_3;
							}
							goto default;
						}
						IL_004a:
						serialPort = null;
						goto IL_004c;
						IL_004c:
						num2 = 10;
						oSerialPort = m_oSerialPort;
						TerminalSerialPort(ref oSerialPort);
						m_oSerialPort = oSerialPort;
						break;
						IL_0041:
						num2 = 7;
						serialPort.Close();
						goto IL_004a;
						IL_0068:
						num2 = 12;
						if (m_oSerialPort.IsOpen || !m_bIsPortOpen)
						{
							break;
						}
						goto IL_0081;
						IL_0008:
						num2 = 2;
						if (iErrorCode == ISerialPort.CommErrorConstants.Unknown && m_oLastError is UnauthorizedAccessException)
						{
							goto IL_001c;
						}
						goto IL_0068;
						IL_0081:
						num2 = 13;
						oSerialPort = m_oSerialPort;
						TerminalSerialPort(ref oSerialPort);
						m_oSerialPort = oSerialPort;
						break;
						IL_001c:
						num2 = 3;
						m_bIsAccesError = true;
						goto IL_0026;
						IL_0026:
						num2 = 4;
						iErrorCode = ISerialPort.CommErrorConstants.PortAccessFail;
						goto IL_002c;
						IL_002c:
						num2 = 5;
						serialPort = m_oSerialPort;
						goto IL_0036;
						IL_0036:
						num2 = 6;
						if (serialPort.IsOpen)
						{
							goto IL_0041;
						}
						goto IL_004a;
						end_IL_0000_2:
						break;
					}
					num2 = 15;
					m_iLastError = iErrorCode;
					break;
					end_IL_0000:;
				}
				catch (object obj) when (obj is Exception && num3 != 0 && num == 0)
				{
					ProjectData.SetProjectError((Exception)obj);
					try0000_dispatch = 251;
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

		public bool ReadBuffer(ref byte[] bBuffer, int iStart, int iLength)
		{
			m_iLastError = ISerialPort.CommErrorConstants.Success;
			m_oLastError = null;
			try
			{
				int num = 0;
				lock (m_oSerialPort)
				{
					SerialPort oSerialPort = m_oSerialPort;
					if (oSerialPort.IsOpen)
					{
						num = oSerialPort.BytesToRead;
					}
					oSerialPort = null;
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

		public bool WriteBuffer(ref byte[] bBuffer, int iStart, int iLength)
		{
			m_iLastError = ISerialPort.CommErrorConstants.Success;
			m_oLastError = null;
			checked
			{
				bool result = default(bool);
				try
				{
					lock (m_oSerialPort)
					{
						SerialPort oSerialPort = m_oSerialPort;
						if (oSerialPort.IsOpen)
						{
							oSerialPort.WriteTimeout = (int)Math.Round((double)(iLength * 8000) / (double)oSerialPort.BaudRate * 10.0 + 5.0);
							oSerialPort.Write(bBuffer, iStart, iLength);
							result = true;
						}
						oSerialPort = null;
					}
					if (m_bIsMonitor && iLength > 0)
					{
						byte[] bBuffer2 = new byte[iLength - 1 + 1];
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
		}

		bool ISerialPort.WriteBuffer(ref byte[] bBuffer, int iStart, int iLength)
		{
			//ILSpy generated this explicit interface implementation from .override directive in WriteBuffer
			return this.WriteBuffer(ref bBuffer, iStart, iLength);
		}

		public string CommPortClass(int CommPort)
		{
			string result = null;
			int count = m_oPortInfos.Count;
			for (int i = 1; i <= count; i = checked(i + 1))
			{
				clsPortInfo clsPortInfo = (clsPortInfo)m_oPortInfos[i];
				if (clsPortInfo.iPortNumber == CommPort)
				{
					result = clsPortInfo.szClass;
				}
				clsPortInfo = null;
			}
			return result;
		}

		string ISerialPort.CommPortClass(int CommPort)
		{
			//ILSpy generated this explicit interface implementation from .override directive in CommPortClass
			return this.CommPortClass(CommPort);
		}

		public string CommPortClass(string CommPort)
		{
			string result = null;
			int count = m_oPortInfos.Count;
			for (int i = 1; i <= count; i = checked(i + 1))
			{
				clsPortInfo clsPortInfo = (clsPortInfo)m_oPortInfos[i];
				if (Strings.StrComp(clsPortInfo.szPortName, CommPort, CompareMethod.Text) == 0)
				{
					result = clsPortInfo.szClass;
					break;
				}
				clsPortInfo = null;
			}
			return result;
		}

		string ISerialPort.CommPortClass(string CommPort)
		{
			//ILSpy generated this explicit interface implementation from .override directive in CommPortClass
			return this.CommPortClass(CommPort);
		}

		public string CommPortInfo(int CommPort)
		{
			StringBuilder stringBuilder = new StringBuilder();
			int count = m_oPortInfos.Count;
			for (int i = 1; i <= count; i = checked(i + 1))
			{
				clsPortInfo clsPortInfo = (clsPortInfo)m_oPortInfos[i];
				if (clsPortInfo.iPortNumber == CommPort)
				{
					stringBuilder.Append("Class=" + clsPortInfo.szClass + "\r\n");
					stringBuilder.Append("Manufacturer=" + clsPortInfo.szManufacturer + "\r\n");
					stringBuilder.Append("PortName=" + clsPortInfo.szPortName + "\r\n");
					stringBuilder.Append("HardwareID=" + clsPortInfo.szHardwareID + "\r\n");
					break;
				}
				clsPortInfo = null;
			}
			return stringBuilder.ToString();
		}

		string ISerialPort.CommPortInfo(int CommPort)
		{
			//ILSpy generated this explicit interface implementation from .override directive in CommPortInfo
			return this.CommPortInfo(CommPort);
		}

		public string CommPortInfo(string CommPort)
		{
			StringBuilder stringBuilder = new StringBuilder();
			int count = m_oPortInfos.Count;
			for (int i = 1; i <= count; i = checked(i + 1))
			{
				clsPortInfo clsPortInfo = (clsPortInfo)m_oPortInfos[i];
				if (Strings.StrComp(clsPortInfo.szPortName, CommPort, CompareMethod.Text) == 0)
				{
					stringBuilder.Append("Class=" + clsPortInfo.szClass + "\r\n");
					stringBuilder.Append("Manufacturer=" + clsPortInfo.szManufacturer + "\r\n");
					stringBuilder.Append("PortName=" + clsPortInfo.szPortName + "\r\n");
					stringBuilder.Append("HardwareID=" + clsPortInfo.szHardwareID + "\r\n");
					break;
				}
				clsPortInfo = null;
			}
			return stringBuilder.ToString();
		}

		string ISerialPort.CommPortInfo(string CommPort)
		{
			//ILSpy generated this explicit interface implementation from .override directive in CommPortInfo
			return this.CommPortInfo(CommPort);
		}

		public bool CommPortInfo(string CommPort, ref string szDeviceClass, ref string szManufacturer, ref string szHardwareID)
		{
			bool result = false;
			int count = m_oPortInfos.Count;
			for (int i = 1; i <= count; i = checked(i + 1))
			{
				clsPortInfo clsPortInfo = (clsPortInfo)m_oPortInfos[i];
				if (Strings.StrComp(clsPortInfo.szPortName, CommPort, CompareMethod.Text) == 0)
				{
					szDeviceClass = clsPortInfo.szClass;
					szManufacturer = clsPortInfo.szManufacturer;
					szHardwareID = clsPortInfo.szHardwareID;
					result = true;
					break;
				}
				clsPortInfo = null;
			}
			return result;
		}

		bool ISerialPort.CommPortInfo(string CommPort, ref string szDeviceClass, ref string szManufacturer, ref string szHardwareID)
		{
			//ILSpy generated this explicit interface implementation from .override directive in CommPortInfo
			return this.CommPortInfo(CommPort, ref szDeviceClass, ref szManufacturer, ref szHardwareID);
		}

		public string EnumCommPort(int Index)
		{
			string text = null;
			m_iLastError = ISerialPort.CommErrorConstants.Success;
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
				text = clsPortInfo.szPortName;
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

		string ISerialPort.EnumCommPort(int Index)
		{
			//ILSpy generated this explicit interface implementation from .override directive in EnumCommPort
			return this.EnumCommPort(Index);
		}

		public string EnumCommPort(int Index, string PortFilter)
		{
			string text = null;
			m_iLastError = ISerialPort.CommErrorConstants.Success;
			if (Index == 0)
			{
				if (!m_bSkipListPorts)
				{
					NumberOfPorts(PortFilter);
				}
				m_bSkipListPorts = false;
			}
			try
			{
				clsPortInfo clsPortInfo = (clsPortInfo)m_oPortInfos[checked(Index + 1)];
				text = clsPortInfo.szPortName;
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

		string ISerialPort.EnumCommPort(int Index, string PortFilter)
		{
			//ILSpy generated this explicit interface implementation from .override directive in EnumCommPort
			return this.EnumCommPort(Index, PortFilter);
		}

		public int EnumCommPortNumber(int Index)
		{
			short num = 0;
			m_iLastError = ISerialPort.CommErrorConstants.Success;
			if (Index == 0)
			{
				if (!m_bSkipListPorts)
				{
					NumberOfPorts();
				}
				m_bSkipListPorts = false;
			}
			checked
			{
				try
				{
					clsPortInfo clsPortInfo = (clsPortInfo)m_oPortInfos[Index + 1];
					num = (short)clsPortInfo.iPortNumber;
					clsPortInfo = null;
				}
				catch (Exception ex)
				{
					ProjectData.SetProjectError(ex);
					Exception oError = ex;
					ProcessErrorCode(ISerialPort.CommErrorConstants.Unknown, ref oError);
					num = 0;
					ProjectData.ClearProjectError();
				}
				return num;
			}
		}

		int ISerialPort.EnumCommPortNumber(int Index)
		{
			//ILSpy generated this explicit interface implementation from .override directive in EnumCommPortNumber
			return this.EnumCommPortNumber(Index);
		}

		public int EnumCommPortNumber(int Index, string PortFilter)
		{
			short num = 0;
			m_iLastError = ISerialPort.CommErrorConstants.Success;
			if (Index == 0)
			{
				NumberOfPorts(PortFilter);
				m_bSkipListPorts = false;
			}
			checked
			{
				try
				{
					clsPortInfo clsPortInfo = (clsPortInfo)m_oPortInfos[Index + 1];
					num = (short)clsPortInfo.iPortNumber;
					clsPortInfo = null;
				}
				catch (Exception ex)
				{
					ProjectData.SetProjectError(ex);
					Exception oError = ex;
					ProcessErrorCode(ISerialPort.CommErrorConstants.Unknown, ref oError);
					num = 0;
					ProjectData.ClearProjectError();
				}
				return num;
			}
		}

		int ISerialPort.EnumCommPortNumber(int Index, string PortFilter)
		{
			//ILSpy generated this explicit interface implementation from .override directive in EnumCommPortNumber
			return this.EnumCommPortNumber(Index, PortFilter);
		}

		public int NumberOfPorts()
		{
			RegistryKey registryKey = null;
			RegistryKey registryKey2 = null;
			StringBuilder stringBuilder = new StringBuilder();
			string[] array = null;
			checked
			{
				lock (m_oPortInfos)
				{
					m_oPortInfos.Clear();
					m_iLastError = ISerialPort.CommErrorConstants.Success;
					switch (Environment.OSVersion.Platform)
					{
					case PlatformID.Win32NT:
					{
						List<string> list = new List<string>();
						registryKey = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Services");
						if (registryKey != null)
						{
							try
							{
								array = registryKey.GetSubKeyNames();
							}
							catch (Exception ex5)
							{
								ProjectData.SetProjectError(ex5);
								Exception oError5 = ex5;
								ProcessErrorCode(ISerialPort.CommErrorConstants.Unknown, ref oError5);
								ProjectData.ClearProjectError();
							}
						}
						if (array == null)
						{
							array = Strings.Split("serenum Ser2pl64 BTHMODEM");
						}
						int num6 = Information.UBound(array);
						for (int k = 0; k <= num6; k++)
						{
							registryKey = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Services\\" + array[k] + "\\Enum");
							int num7;
							if (registryKey != null)
							{
								try
								{
									num7 = Conversions.ToInteger(registryKey.GetValue("Count", 0));
								}
								catch (Exception ex6)
								{
									ProjectData.SetProjectError(ex6);
									Exception oError6 = ex6;
									ProcessErrorCode(ISerialPort.CommErrorConstants.Unknown, ref oError6);
									num7 = 0;
									ProjectData.ClearProjectError();
								}
							}
							else
							{
								num7 = 0;
							}
							string text5 = null;
							int num8 = num7 - 1;
							for (int i = 0; i <= num8; i++)
							{
								string text = Conversions.ToString(registryKey.GetValue(Conversions.ToString(i)));
								if (registryKey2 != null)
								{
									registryKey2 = null;
								}
								if (list.Count == 0 || list.IndexOf(text) == -1)
								{
									list.Add(text);
									registryKey2 = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Enum\\" + text);
								}
								bool flag = false;
								string szManufacturer = null;
								string text3 = null;
								string text4 = null;
								if (registryKey2 != null)
								{
									try
									{
										if (Strings.InStr(Conversions.ToString(registryKey2.GetValue("Service")), "Serial", CompareMethod.Text) > 0 || Strings.InStr(Conversions.ToString(registryKey2.GetValue("Class")), "Ports", CompareMethod.Text) > 0)
										{
											szManufacturer = Conversions.ToString(registryKey2.GetValue("Mfg"));
											if (Environment.OSVersion.Version.Major == 6)
											{
												int num3 = Strings.InStr(szManufacturer, ";");
												if (num3 > 0 && Operators.CompareString(Strings.Left(szManufacturer, 1), "@", TextCompare: false) == 0)
												{
													szManufacturer = Strings.Mid(szManufacturer, num3 + 1);
												}
											}
											text3 = Strings.Join((string[])registryKey2.GetValue("HardwareID"), "\0");
											text5 = Strings.Trim(Conversions.ToString(registryKey2.GetValue("Service")));
											registryKey2 = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Enum\\" + text + "\\Device Parameters");
											text4 = Conversions.ToString(registryKey2.GetValue("PortName"));
											if (text4 != null && text4.StartsWith("COM"))
											{
												flag = true;
											}
											if (registryKey2 != null)
											{
												registryKey2 = null;
											}
										}
									}
									catch (Exception ex7)
									{
										ProjectData.SetProjectError(ex7);
										Exception oError7 = ex7;
										ProcessErrorCode(ISerialPort.CommErrorConstants.Unknown, ref oError7);
										flag = false;
										ProjectData.ClearProjectError();
									}
								}
								if (!flag)
								{
									continue;
								}
								int j;
								if (k == 0 || text5 == null || Strings.Len(text5) == 0)
								{
									int num3 = Strings.InStr(text3, "\\");
									if (num3 > 1)
									{
										text5 = Strings.Left(text3, num3 - 1);
									}
									else
									{
										num3 = Strings.InStr(text, "\\");
										text5 = ((num3 <= 1 || Strings.InStr(text, "ROOT\\", CompareMethod.Text) != 0) ? text3 : Strings.Left(text, num3 - 1));
									}
									registryKey2 = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Services\\" + text5 + "\\Enum");
									int num9;
									if (registryKey2 != null)
									{
										try
										{
											num9 = Conversions.ToInteger(registryKey2.GetValue("Count", 0));
										}
										catch (Exception ex8)
										{
											ProjectData.SetProjectError(ex8);
											Exception oError8 = ex8;
											ProcessErrorCode(ISerialPort.CommErrorConstants.Unknown, ref oError8);
											num9 = 0;
											ProjectData.ClearProjectError();
										}
									}
									else
									{
										num9 = 0;
									}
									flag = false;
									string text6 = null;
									int num10 = num9 - 1;
									for (j = 0; j <= num10; j++)
									{
										string sDest = Conversions.ToString(registryKey2.GetValue(Conversions.ToString(j)));
										num3 = Strings.InStr(sDest, "\\");
										if (num3 > 1)
										{
											text6 = Strings.Left(sDest, num3 - 1);
											sDest = Strings.Mid(sDest, num3 + 1);
											if (Strings.InStr(text, sDest, CompareMethod.Text) > 0)
											{
												flag = true;
											}
											else
											{
												int num4 = Strings.InStr(sDest, "\\");
												num3 = Strings.InStr(sDest, "&");
												if (num4 > 0)
												{
													StringType.MidStmtStr(ref sDest, num4, int.MaxValue, "\0");
													if (unchecked(num3 > 0 && num3 > num4))
													{
														num3 = 0;
													}
												}
												if (num3 > 0)
												{
													StringType.MidStmtStr(ref sDest, num3, int.MaxValue, "\0");
												}
												string[] subKeyNames = Strings.Split(sDest, "\0");
												if (subKeyNames.Length > 0)
												{
													flag = true;
													int num11 = subKeyNames.Length - 1;
													for (num3 = 0; num3 <= num11; num3++)
													{
														if (Strings.InStr(text, subKeyNames[num3], CompareMethod.Text) == 0)
														{
															flag = false;
															break;
														}
													}
												}
												subKeyNames = null;
											}
										}
										if (flag && text6.Length > 0)
										{
											if (Operators.CompareString(text6, text5, TextCompare: false) != 0)
											{
												text5 = text6 + "." + text5;
											}
											break;
										}
									}
								}
								if (registryKey2 != null)
								{
									registryKey2 = null;
								}
								clsPortInfo clsPortInfo = new clsPortInfo();
								int num5 = ((text4 != null && Operators.CompareString(Strings.Left(text4, 3), "COM", TextCompare: false) == 0) ? Conversions.ToInteger(Strings.Mid(text4, 4)) : 0);
								clsPortInfo clsPortInfo4 = clsPortInfo;
								clsPortInfo4.szClass = text5;
								clsPortInfo4.szPortName = text4;
								clsPortInfo4.iPortNumber = num5;
								clsPortInfo4.szManufacturer = szManufacturer;
								clsPortInfo4.szHardwareID = text3;
								clsPortInfo4 = null;
								num5 = Conversions.ToInteger(Strings.Mid(text4, 4));
								bool flag3 = false;
								int count2 = m_oPortInfos.Count;
								for (j = 1; j <= count2; j++)
								{
									clsPortInfo clsPortInfo5 = (clsPortInfo)m_oPortInfos[j];
									if (clsPortInfo5.iPortNumber > 0 && num5 < clsPortInfo5.iPortNumber)
									{
										m_oPortInfos.Add(clsPortInfo, null, j);
										flag3 = true;
										break;
									}
									if (Operators.CompareString(text4, clsPortInfo5.szPortName, TextCompare: false) < 0)
									{
										m_oPortInfos.Add(clsPortInfo, null, j);
										flag3 = true;
										break;
									}
									clsPortInfo5 = null;
								}
								if (!flag3)
								{
									m_oPortInfos.Add(clsPortInfo, null, j);
								}
								clsPortInfo = null;
							}
						}
						array = null;
						break;
					}
					case PlatformID.Win32S:
					case PlatformID.Win32Windows:
					{
						registryKey = Registry.DynData.OpenSubKey("Config Manager\\Enum");
						string[] subKeyNames = registryKey.GetSubKeyNames();
						registryKey = null;
						int num = subKeyNames.Length - 1;
						for (int i = 0; i <= num; i++)
						{
							registryKey = Registry.DynData.OpenSubKey("Config Manager\\Enum\\" + subKeyNames[i]);
							try
							{
								string text = Conversions.ToString(registryKey.GetValue("HardWareKey"));
								if (Strings.InStr(text, "USB\\", CompareMethod.Text) > 0)
								{
									stringBuilder.Append("\0" + text);
								}
								registryKey = null;
							}
							catch (Exception ex)
							{
								ProjectData.SetProjectError(ex);
								Exception oError = ex;
								ProcessErrorCode(ISerialPort.CommErrorConstants.Unknown, ref oError);
								ProjectData.ClearProjectError();
							}
						}
						string text2 = stringBuilder?.ToString();
						stringBuilder = null;
						int num2 = subKeyNames.Length - 1;
						for (int i = 0; i <= num2; i++)
						{
							string text;
							try
							{
								registryKey = Registry.DynData.OpenSubKey("Config Manager\\Enum\\" + subKeyNames[i]);
								text = Conversions.ToString(registryKey.GetValue("HardWareKey"));
							}
							catch (Exception ex2)
							{
								ProjectData.SetProjectError(ex2);
								Exception oError2 = ex2;
								ProcessErrorCode(ISerialPort.CommErrorConstants.Unknown, ref oError2);
								text = null;
								ProjectData.ClearProjectError();
							}
							registryKey = null;
							bool flag = false;
							string szManufacturer = null;
							string text3 = null;
							string text4 = null;
							if (text != null && text.Length > 0)
							{
								registryKey = Registry.LocalMachine.OpenSubKey("Enum\\" + text);
								if (registryKey != null && Strings.InStr(Conversions.ToString(registryKey.GetValue("Class")), "Ports", CompareMethod.Text) > 0)
								{
									bool flag2 = false;
									try
									{
										text4 = Conversions.ToString(registryKey.GetValue("PortName"));
										szManufacturer = Conversions.ToString(registryKey.GetValue("Mfg"));
									}
									catch (Exception ex3)
									{
										ProjectData.SetProjectError(ex3);
										Exception oError3 = ex3;
										ProcessErrorCode(ISerialPort.CommErrorConstants.Unknown, ref oError3);
										ProjectData.ClearProjectError();
									}
									try
									{
										text3 = Conversions.ToString(registryKey.GetValue("HardwareID"));
									}
									catch (Exception ex4)
									{
										ProjectData.SetProjectError(ex4);
										Exception oError4 = ex4;
										ProcessErrorCode(ISerialPort.CommErrorConstants.Unknown, ref oError4);
										flag2 = true;
										ProjectData.ClearProjectError();
									}
									if (flag2)
									{
										text3 = ((Strings.InStr(text, "ROOT\\", CompareMethod.Text) != 1) ? text : Strings.Mid(text, 6));
									}
									if (Strings.InStr(text4, "COM") > 0)
									{
										flag = true;
									}
								}
							}
							registryKey = null;
							if (!flag)
							{
								continue;
							}
							int num3 = Strings.InStr(text3, "\\");
							string text5;
							if (num3 > 1)
							{
								text5 = Strings.Left(text3, num3 - 1);
							}
							else
							{
								num3 = Strings.InStr(text, "\\");
								text5 = ((num3 <= 1) ? text3 : Strings.Left(text, num3 - 1));
							}
							if (text5 != null || Strings.StrComp(text5, "USB", CompareMethod.Text) != 0)
							{
								num3 = Strings.InStr(text, "\\");
								string sDest = null;
								if (num3 > 0)
								{
									int num4 = Strings.InStr(text, "PID_", CompareMethod.Text);
									if (num4 > 0)
									{
										num4 += 8;
									}
									if (num4 > num3)
									{
										sDest = Strings.Mid(text, num3 + 1, num4 - num3 - 1);
										num3 = Strings.InStr(sDest, "+");
										if (num3 > 0)
										{
											StringType.MidStmtStr(ref sDest, num3, 1, "&");
										}
									}
								}
								if (sDest != null && sDest.Length > 0 && text2 != null && Strings.InStr(text2, sDest, CompareMethod.Text) > 0)
								{
									text5 = "USB." + text5;
								}
							}
							clsPortInfo clsPortInfo = new clsPortInfo();
							int num5 = ((text4 != null && Operators.CompareString(Strings.Left(text4, 3), "COM", TextCompare: false) == 0) ? Conversions.ToInteger(Strings.Mid(text4, 4)) : 0);
							clsPortInfo clsPortInfo2 = clsPortInfo;
							clsPortInfo2.szClass = text5;
							clsPortInfo2.szPortName = text4;
							clsPortInfo2.iPortNumber = num5;
							clsPortInfo2.szManufacturer = szManufacturer;
							clsPortInfo2.szHardwareID = text3;
							clsPortInfo2 = null;
							bool flag3 = false;
							int count = m_oPortInfos.Count;
							int j;
							for (j = 1; j <= count; j++)
							{
								clsPortInfo clsPortInfo3 = (clsPortInfo)m_oPortInfos[j];
								if (clsPortInfo3.iPortNumber > 0 && num5 < clsPortInfo3.iPortNumber)
								{
									m_oPortInfos.Add(clsPortInfo, null, j);
									flag3 = true;
									break;
								}
								if (Operators.CompareString(text4, clsPortInfo3.szPortName, TextCompare: false) < 0)
								{
									m_oPortInfos.Add(clsPortInfo, null, j);
									flag3 = true;
									break;
								}
								clsPortInfo3 = null;
							}
							if (!flag3)
							{
								m_oPortInfos.Add(clsPortInfo, null, j);
							}
							clsPortInfo = null;
						}
						subKeyNames = null;
						break;
					}
					}
				}
				m_bSkipListPorts = true;
				return m_oPortInfos.Count;
			}
		}

		int ISerialPort.NumberOfPorts()
		{
			//ILSpy generated this explicit interface implementation from .override directive in NumberOfPorts
			return this.NumberOfPorts();
		}

		public int NumberOfPorts(string PortFilter)
		{
			PortFilter = ";" + PortFilter + ";";
			bool flag = ((Strings.InStr(PortFilter, "!") > 0) ? true : false);
			NumberOfPorts();
			checked
			{
				lock (m_oPortInfos)
				{
					for (int i = m_oPortInfos.Count; i >= 1; i += -1)
					{
						bool flag2 = flag;
						clsPortInfo clsPortInfo = (clsPortInfo)m_oPortInfos[i];
						int num = Strings.InStr(PortFilter, clsPortInfo.szClass);
						if (num > 0)
						{
							string left = Strings.Mid(PortFilter, num - 1);
							if (Operators.CompareString(left, ";", TextCompare: false) == 0)
							{
								flag2 = false;
							}
							else if (Operators.CompareString(left, "!", TextCompare: false) == 0)
							{
								flag2 = true;
							}
						}
						if (flag2)
						{
							m_oPortInfos.Remove(i);
						}
						clsPortInfo = null;
					}
				}
				return m_oPortInfos.Count;
			}
		}

		int ISerialPort.NumberOfPorts(string PortFilter)
		{
			//ILSpy generated this explicit interface implementation from .override directive in NumberOfPorts
			return this.NumberOfPorts(PortFilter);
		}

		public bool IsMyPort(ref string szPortName)
		{
			bool result = false;
			int num = Strings.InStr(szPortName, "COM", CompareMethod.Text);
			if (num > 0)
			{
				try
				{
					int num2 = Conversions.ToInteger(Strings.Mid(szPortName, checked(num + 3)));
					int num3 = num2;
					if (num3 >= 1 && num3 <= 256)
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
			return result;
		}

		bool ISerialPort.IsMyPort(ref string szPortName)
		{
			//ILSpy generated this explicit interface implementation from .override directive in IsMyPort
			return this.IsMyPort(ref szPortName);
		}

		public void Dispose()
		{
			if (m_oSerialPort != null)
			{
				try
				{
					lock (m_oSerialPort)
					{
						SerialPort oSerialPort = m_oSerialPort;
						TerminalSerialPort(ref oSerialPort);
						m_oSerialPort = oSerialPort;
					}
					m_oSerialPort.Dispose();
				}
				catch (Exception ex)
				{
					ProjectData.SetProjectError(ex);
					Exception ex2 = ex;
					ProjectData.ClearProjectError();
				}
				m_oSerialPort = null;
			}
			m_oWndMsg = null;
		}

		void ISerialPort.Dispose()
		{
			//ILSpy generated this explicit interface implementation from .override directive in Dispose
			this.Dispose();
		}

		private void Class_Init(ref SerialPort oSerialPort, ref WindowMessage oWndMsg)
		{
			if (oWndMsg == null)
			{
				m_oWndMsg = new WindowMessage();
			}
			else
			{
				m_oWndMsg = oWndMsg;
			}
			iPostEventConstant = m_oWndMsg.RegisterWindowMessage("PostEvent");
			m_oSerialPort = oSerialPort;
			SerialPort oSerialPort2 = m_oSerialPort;
			oSerialPort2.ReadBufferSize = 1024;
			oSerialPort2.WriteBufferSize = 1024;
			oSerialPort2.ReadTimeout = 50;
			oSerialPort2.WriteTimeout = 50;
			oSerialPort2.ReceivedBytesThreshold = 1;
			m_iRThreshold = oSerialPort2.ReceivedBytesThreshold;
			oSerialPort2 = null;
			m_oReceiveTimer = new System.Windows.Forms.Timer();
			System.Windows.Forms.Timer oReceiveTimer = m_oReceiveTimer;
			oReceiveTimer.Interval = 50;
			oReceiveTimer.Enabled = false;
			oReceiveTimer = null;
		}

		[DebuggerNonUserCode]
		public MSComm()
		{
			m_iCommEvent = (ISerialPort.CommEventConstants)0;
			m_bCDHolding = false;
			m_bCTSHolding = false;
			m_bDSRHolding = false;
			m_bRingHolding = false;
			m_iEnterDataReceived = 0;
			m_iInputLen = 0;
			m_iInputMode = ISerialPort.InputModeConstants.comInputModeBinary;
			m_iSThreshold = 0;
			m_oPortInfos = new Collection();
			m_oRxbuffer = new ByteBuffer();
			m_bIsMonitor = false;
			m_bIsBackgroundMode = false;
			m_oPostCommEventDelegate = FireCommEvent;
			SerialPort oSerialPort = new SerialPort();
			WindowMessage oWndMsg = new WindowMessage();
			Class_Init(ref oSerialPort, ref oWndMsg);
		}

		[DebuggerNonUserCode]
		public MSComm(ref SerialPort oSerialPort)
		{
			m_iCommEvent = (ISerialPort.CommEventConstants)0;
			m_bCDHolding = false;
			m_bCTSHolding = false;
			m_bDSRHolding = false;
			m_bRingHolding = false;
			m_iEnterDataReceived = 0;
			m_iInputLen = 0;
			m_iInputMode = ISerialPort.InputModeConstants.comInputModeBinary;
			m_iSThreshold = 0;
			m_oPortInfos = new Collection();
			m_oRxbuffer = new ByteBuffer();
			m_bIsMonitor = false;
			m_bIsBackgroundMode = false;
			m_oPostCommEventDelegate = FireCommEvent;
			WindowMessage oWndMsg = new WindowMessage();
			Class_Init(ref oSerialPort, ref oWndMsg);
		}

		[DebuggerNonUserCode]
		public MSComm(ref WindowMessage oWndMsg)
		{
			m_iCommEvent = (ISerialPort.CommEventConstants)0;
			m_bCDHolding = false;
			m_bCTSHolding = false;
			m_bDSRHolding = false;
			m_bRingHolding = false;
			m_iEnterDataReceived = 0;
			m_iInputLen = 0;
			m_iInputMode = ISerialPort.InputModeConstants.comInputModeBinary;
			m_iSThreshold = 0;
			m_oPortInfos = new Collection();
			m_oRxbuffer = new ByteBuffer();
			m_bIsMonitor = false;
			m_bIsBackgroundMode = false;
			m_oPostCommEventDelegate = FireCommEvent;
			SerialPort oSerialPort = new SerialPort();
			Class_Init(ref oSerialPort, ref oWndMsg);
		}

		[DebuggerNonUserCode]
		public MSComm(ref SerialPort oSerialPort, ref WindowMessage oWndMsg)
		{
			m_iCommEvent = (ISerialPort.CommEventConstants)0;
			m_bCDHolding = false;
			m_bCTSHolding = false;
			m_bDSRHolding = false;
			m_bRingHolding = false;
			m_iEnterDataReceived = 0;
			m_iInputLen = 0;
			m_iInputMode = ISerialPort.InputModeConstants.comInputModeBinary;
			m_iSThreshold = 0;
			m_oPortInfos = new Collection();
			m_oRxbuffer = new ByteBuffer();
			m_bIsMonitor = false;
			m_bIsBackgroundMode = false;
			m_oPostCommEventDelegate = FireCommEvent;
			Class_Init(ref oSerialPort, ref oWndMsg);
		}

		protected virtual void Finalize()
		{
			base.Finalize();
			Dispose();
		}

		private void m_oSerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
		{
			PostCommEvent(ISerialPort.CommEventConstants.comEvReceive);
		}

		private void m_oSerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
		{
			switch (e.EventType)
			{
			case SerialError.Frame:
				PostCommEvent(ISerialPort.CommEventConstants.comEventFrame);
				break;
			case SerialError.Overrun:
				PostCommEvent(ISerialPort.CommEventConstants.comEventOverrun);
				break;
			case SerialError.RXOver:
				PostCommEvent(ISerialPort.CommEventConstants.comEventRxOver);
				break;
			case SerialError.RXParity:
				PostCommEvent(ISerialPort.CommEventConstants.comEventRxParity);
				break;
			case SerialError.TXFull:
				PostCommEvent(ISerialPort.CommEventConstants.comEventTxFull);
				break;
			}
		}

		private void m_oSerialPort_PinChanged(object sender, SerialPinChangedEventArgs e)
		{
			switch (e.EventType)
			{
			case SerialPinChange.CDChanged:
				PostCommEvent(ISerialPort.CommEventConstants.comEvCD);
				break;
			case SerialPinChange.CtsChanged:
				PostCommEvent(ISerialPort.CommEventConstants.comEvCTS);
				break;
			case SerialPinChange.DsrChanged:
				PostCommEvent(ISerialPort.CommEventConstants.comEvDSR);
				break;
			case SerialPinChange.Ring:
				m_bRingHolding = !m_bRingHolding;
				PostCommEvent(ISerialPort.CommEventConstants.comEvRing);
				break;
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
	}
}
