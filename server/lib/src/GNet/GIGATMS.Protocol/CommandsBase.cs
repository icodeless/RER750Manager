using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;
using GIGATMS.IO;
using GIGATMS.Windows;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace GIGATMS.Protocol
{
	[DebuggerNonUserCode]
	public class CommandsBase : ISerialPort
	{
		public enum iOnCommandFinishEventConstants
		{
			Disable,
			DisableIfCallBack,
			ErrorOnly,
			AllEvent
		}

		public enum iDoEventConstants
		{
			DoEventOnComm,
			DoEventOnMonitor,
			DoEventOnPort,
			DoEventOnEvent,
			DoEventOnCommandFinish,
			DoEventOnReceive
		}

		public delegate void OnEventEventHandler(ref CommandsBase oSender, ref IEvent oEvent);

		public delegate void OnCommandFinishEventHandler(ref CommandsBase oSender, ref ICommand oCommand);

		public delegate void OnReceiveEventHandler(ref CommandsBase oSender);

		[DebuggerNonUserCode]
		protected class DoEventData
		{
			public object oSender;

			public iDoEventConstants iEventType;

			public object[] oParams;

			[DebuggerNonUserCode]
			public DoEventData(ref ISerialPort oSender, ISerialPort.CommEventConstants iCommEvent)
			{
				iEventType = iDoEventConstants.DoEventOnComm;
				oSender = oSender;
				oParams = new object[1];
				oParams[0] = iCommEvent;
			}

			[DebuggerNonUserCode]
			public DoEventData(ref ISerialPort oSender, ISerialPort.CommMonitorEventConstants iEvent, ref byte[] bBuffer)
			{
				iEventType = iDoEventConstants.DoEventOnMonitor;
				oSender = oSender;
				oParams = new object[2];
				oParams[0] = iEvent;
				oParams[1] = bBuffer;
			}

			[DebuggerNonUserCode]
			public DoEventData(ref ISerialPort oSender, ISerialPort.CommPortEventConstants iEvent, string szPortName)
			{
				iEventType = iDoEventConstants.DoEventOnPort;
				oSender = oSender;
				oParams = new object[2];
				oParams[0] = iEvent;
				oParams[1] = szPortName;
			}

			[DebuggerNonUserCode]
			public DoEventData(ref CommandsBase oSender, ref IEvent oEvent)
			{
				iEventType = iDoEventConstants.DoEventOnEvent;
				oSender = oSender;
				oParams = new object[1];
				oParams[0] = oEvent;
			}

			[DebuggerNonUserCode]
			public DoEventData(ref CommandsBase oSender, ref ICommand oCommand)
			{
				iEventType = iDoEventConstants.DoEventOnCommandFinish;
				oSender = oSender;
				oParams = new object[1];
				oParams[0] = oCommand;
			}

			[DebuggerNonUserCode]
			public DoEventData(ref CommandsBase oSender)
			{
				iEventType = iDoEventConstants.DoEventOnReceive;
				oSender = oSender;
			}
		}

		private List<ICommand> m_oCommandList;

		private ICommand m_oCurrentCommand;

		private iOnCommandFinishEventConstants m_iCommandFinishEvent;

		private bool m_bIsDiscardCommands;

		[AccessedThroughProperty("m_oNullSerialPort")]
		private ISerialPort _m_oNullSerialPort;

		[AccessedThroughProperty("m_oNullReceiver")]
		private IReceiver _m_oNullReceiver;

		private ISerialPort m_oCurrentPort;

		private List<ISerialPort> m_oSerialPorts;

		private List<IReceiver> m_oReceivers;

		protected List<DoEventData> m_oEvents;

		protected int m_iDoEventsMsg;

		private bool m_bIsDataReceiving;

		private bool m_bIsFinalize;

		private bool m_bIsCheckAndSendCommandRunning;

		private object m_oLastSendCommand;

		private bool m_IsPortClosed;

		[AccessedThroughProperty("m_oWndMsg")]
		private WindowMessage _m_oWndMsg;

		private bool m_bIsCancel;

		private bool m_bIsOnTimer;

		private ByteBuffer m_oRxBuffer;

		private Exception m_oLastError;

		private ISerialPort.CommErrorConstants m_iLastError;

		private bool m_bIsDestroyClass;

		private ISerialPort.OnCommHandler m_oSerialPortOnCommAddress;

		private ISerialPort.OnMonitorHandler m_oSerialPortOnMonitorAddress;

		private ISerialPort.OnPortHandler m_oSerialPortOnPortAddress;

		private IReceiver.OnEventHandler m_oReceiverOnEventAddress;

		private IReceiver.OnReceiveToCommandHandler m_oReceiverOnReceiveToCommandAddress;

		private bool m_bIsBackgroundMode;

		private Thread m_oCommandThread;

		private EventWaitHandle m_hCommandThreadEvent;

		private Thread m_oEventThread;

		private EventWaitHandle m_hEventThreadEvent;

		private bool m_bIsCommandThreadAlive;

		private bool m_bIsEventThreadAlive;

		private int m_hMainThreadID;

		private virtual ISerialPort m_oNullSerialPort
		{
			[DebuggerNonUserCode]
			get
			{
				return _m_oNullSerialPort;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			[DebuggerNonUserCode]
			set
			{
				if (_m_oNullSerialPort != null)
				{
					_m_oNullSerialPort.OnPort -= m_oNullSerialPort_OnPort;
					_m_oNullSerialPort.OnMonitor -= m_oNullSerialPort_OnMonitor;
					_m_oNullSerialPort.OnComm -= m_oNullSerialPort_OnComm;
				}
				_m_oNullSerialPort = value;
				if (_m_oNullSerialPort != null)
				{
					_m_oNullSerialPort.OnPort += m_oNullSerialPort_OnPort;
					_m_oNullSerialPort.OnMonitor += m_oNullSerialPort_OnMonitor;
					_m_oNullSerialPort.OnComm += m_oNullSerialPort_OnComm;
				}
			}
		}

		private virtual IReceiver m_oNullReceiver
		{
			[DebuggerNonUserCode]
			get
			{
				return _m_oNullReceiver;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			[DebuggerNonUserCode]
			set
			{
				if (_m_oNullReceiver != null)
				{
					_m_oNullReceiver.OnReceiveToCommand -= m_oNullReceiver_OnReceiveToCommand;
					_m_oNullReceiver.OnEvent -= m_oNullReceiver_OnEvent;
				}
				_m_oNullReceiver = value;
				if (_m_oNullReceiver != null)
				{
					_m_oNullReceiver.OnReceiveToCommand += m_oNullReceiver_OnReceiveToCommand;
					_m_oNullReceiver.OnEvent += m_oNullReceiver_OnEvent;
				}
			}
		}

		protected virtual WindowMessage m_oWndMsg
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
				}
				_m_oWndMsg = value;
				if (_m_oWndMsg != null)
				{
					_m_oWndMsg.WindowProc += m_oWndMsg_WindowProc;
				}
			}
		}

		public object Busy
		{
			get
			{
				bool flag = false;
				if (m_oCommandList.Count > 0 || m_oCurrentCommand != null || m_oEvents.Count > 0)
				{
					flag = true;
				}
				return flag;
			}
		}

		public ICommand LastSendCommand => (ICommand)m_oLastSendCommand;

		public iOnCommandFinishEventConstants CommandFinishEvent
		{
			get
			{
				return m_iCommandFinishEvent;
			}
			set
			{
				m_iCommandFinishEvent = value;
			}
		}

		public object BasePort => m_oCurrentPort.BasePort;

		public bool CTSHolding => m_oCurrentPort.CTSHolding;

		public bool DSRHolding => m_oCurrentPort.DSRHolding;

		public bool DTREnable
		{
			get
			{
				return m_oCurrentPort.DTREnable;
			}
			set
			{
				m_oCurrentPort.DTREnable = value;
			}
		}

		public ISerialPort.HandshakeConstants Handshaking
		{
			get
			{
				return m_oCurrentPort.Handshaking;
			}
			set
			{
				m_oCurrentPort.Handshaking = value;
			}
		}

		public int InBufferCount
		{
			get
			{
				return m_oRxBuffer.GetSize();
			}
			set
			{
				if (value == 0)
				{
					m_oRxBuffer.Clear();
				}
			}
		}

		public int InBufferSize
		{
			get
			{
				return m_oCurrentPort.InBufferSize;
			}
			set
			{
				m_oCurrentPort.InBufferSize = value;
			}
		}

		public object Input
		{
			get
			{
				object result = null;
				m_oLastError = null;
				try
				{
					ISerialPort oCurrentPort = m_oCurrentPort;
					if (oCurrentPort.InputMode == ISerialPort.InputModeConstants.comInputModeBinary)
					{
						byte[] Value = null;
						if (oCurrentPort.InputLen == 0)
						{
							m_oRxBuffer.Take(ref Value);
						}
						else
						{
							m_oRxBuffer.Take(ref Value, 0, oCurrentPort.InputLen);
						}
						result = Value;
					}
					else
					{
						string Value2 = null;
						if (oCurrentPort.InputLen == 0)
						{
							m_oRxBuffer.Take(ref Value2);
						}
						else
						{
							m_oRxBuffer.Take(ref Value2, oCurrentPort.InputLen);
						}
						result = Value2;
					}
					oCurrentPort = null;
				}
				catch (Exception ex)
				{
					ProjectData.SetProjectError(ex);
					Exception oLastError = ex;
					m_iLastError = ISerialPort.CommErrorConstants.Unknown;
					m_oLastError = oLastError;
					ProjectData.ClearProjectError();
				}
				return result;
			}
		}

		public int InputLen
		{
			get
			{
				return m_oCurrentPort.InputLen;
			}
			set
			{
				m_oCurrentPort.InputLen = value;
			}
		}

		public ISerialPort.InputModeConstants InputMode
		{
			get
			{
				return m_oCurrentPort.InputMode;
			}
			set
			{
				m_oCurrentPort.InputMode = value;
			}
		}

		public bool NullDiscard
		{
			get
			{
				return m_oCurrentPort.NullDiscard;
			}
			set
			{
				m_oCurrentPort.NullDiscard = value;
			}
		}

		public int OutBufferCount
		{
			get
			{
				return m_oCurrentPort.OutBufferCount;
			}
			set
			{
				m_oCurrentPort.OutBufferCount = value;
			}
		}

		public int OutBufferSize
		{
			get
			{
				return m_oCurrentPort.OutBufferSize;
			}
			set
			{
				m_oCurrentPort.OutBufferSize = value;
			}
		}

		public object Output
		{
			set
			{
				m_oCurrentPort.Output = RuntimeHelpers.GetObjectValue(value);
			}
		}

		public char ParityReplace
		{
			get
			{
				return m_oCurrentPort.ParityReplace;
			}
			set
			{
				m_oCurrentPort.ParityReplace = value;
			}
		}

		public string PortName
		{
			get
			{
				return m_oCurrentPort.PortName;
			}
			set
			{
				if (m_oCurrentPort.IsMyPort(ref value))
				{
					m_oCurrentPort.PortName = value;
					return;
				}
				List<ISerialPort> oSerialPorts = m_oSerialPorts;
				checked
				{
					int num = oSerialPorts.Count - 1;
					for (int i = 0; i <= num; i++)
					{
						if (oSerialPorts[i].IsMyPort(ref value))
						{
							lock (m_oSerialPorts)
							{
								m_oCurrentPort = oSerialPorts[i];
								m_oCurrentPort.PortName = value;
							}
						}
					}
					oSerialPorts = null;
				}
			}
		}

		public bool PortOpen
		{
			get
			{
				return m_oCurrentPort.PortOpen;
			}
			set
			{
				byte[] bBuffer = new byte[1];
				ISerialPort oCurrentPort = m_oCurrentPort;
				ISerialPort oSender;
				if (!value)
				{
					ISerialPort.OnMonitorHandler onMonitorEvent = OnMonitor;
					if (onMonitorEvent != null)
					{
						oSender = this;
						onMonitorEvent(ref oSender, ISerialPort.CommMonitorEventConstants.comEvPortBeforeClose, ref bBuffer);
					}
				}
				oCurrentPort.PortOpen = value;
				oCurrentPort.IsBackgroundMode = true;
				if (oCurrentPort.PortOpen == value)
				{
					bBuffer[0] = 1;
				}
				oSender = this;
				AddEvent(ref oSender, (ISerialPort.CommMonitorEventConstants)Conversions.ToInteger(Interaction.IIf(value, ISerialPort.CommMonitorEventConstants.comEvPortOpen, ISerialPort.CommMonitorEventConstants.comEvPortClose)), ref bBuffer);
				oCurrentPort = null;
				m_hCommandThreadEvent.Set();
			}
		}

		public int RThreshold
		{
			get
			{
				return m_oCurrentPort.RThreshold;
			}
			set
			{
				m_oCurrentPort.RThreshold = value;
			}
		}

		public bool RTSEnable
		{
			get
			{
				return m_oCurrentPort.RTSEnable;
			}
			set
			{
				m_oCurrentPort.RTSEnable = value;
			}
		}

		public string Settings
		{
			get
			{
				return m_oCurrentPort.Settings;
			}
			set
			{
				m_oCurrentPort.Settings = value;
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

		public bool Break
		{
			get
			{
				return m_oCurrentPort.Break;
			}
			set
			{
				m_oCurrentPort.Break = value;
			}
		}

		public bool CDHolding => m_oCurrentPort.CDHolding;

		public ISerialPort.CommEventConstants CommEvent => m_oCurrentPort.CommEvent;

		public int CommPort
		{
			get
			{
				return m_oCurrentPort.CommPort;
			}
			set
			{
				m_oCurrentPort.CommPort = value;
			}
		}

		public bool IsMonitorIO
		{
			get
			{
				return m_oCurrentPort.IsMonitorIO;
			}
			set
			{
				m_oCurrentPort.IsMonitorIO = value;
			}
		}

		public int SThreshold
		{
			get
			{
				return m_oCurrentPort.SThreshold;
			}
			set
			{
				m_oCurrentPort.SThreshold = value;
			}
		}

		[method: DebuggerNonUserCode]
		public event ISerialPort.OnCommHandler OnComm;

		[method: DebuggerNonUserCode]
		public event ISerialPort.OnMonitorHandler OnMonitor;

		[method: DebuggerNonUserCode]
		public event ISerialPort.OnPortHandler OnPort;

		[method: DebuggerNonUserCode]
		public event OnEventEventHandler OnEvent;

		[method: DebuggerNonUserCode]
		public event OnCommandFinishEventHandler OnCommandFinish;

		[method: DebuggerNonUserCode]
		public event OnReceiveEventHandler OnReceive;

		public void Cancel()
		{
			m_bIsCancel = true;
			DiscardCommands();
		}

		private void m_oWndMsg_WindowProc(ref int iResult, int hWnd, int uMsg, int wParam, ref IntPtr lParam)
		{
			if (uMsg == m_iDoEventsMsg)
			{
				DoEventList();
			}
		}

		private void DoEventList()
		{
			if (m_oEvents.Count <= 0)
			{
				return;
			}
			DoEventData doEventData;
			lock (m_oEvents)
			{
				doEventData = m_oEvents[0];
				m_oEvents.Remove(doEventData);
			}
			CommandsBase oSender = null;
			DoEventData doEventData2 = doEventData;
			if (doEventData2.oSender != null)
			{
				oSender = (CommandsBase)doEventData2.oSender;
			}
			switch (doEventData2.iEventType)
			{
			case iDoEventConstants.DoEventOnComm:
			{
				ISerialPort.OnCommHandler onCommEvent = OnComm;
				if (onCommEvent != null)
				{
					ISerialPort oSender2 = oSender;
					onCommEvent(ref oSender2, (ISerialPort.CommEventConstants)Conversions.ToInteger(doEventData2.oParams[0]));
					oSender = (CommandsBase)oSender2;
				}
				break;
			}
			case iDoEventConstants.DoEventOnMonitor:
			{
				byte[] bBuffer = (byte[])doEventData2.oParams[1];
				ISerialPort.OnMonitorHandler onMonitorEvent = OnMonitor;
				if (onMonitorEvent != null)
				{
					ISerialPort oSender2 = oSender;
					onMonitorEvent(ref oSender2, (ISerialPort.CommMonitorEventConstants)Conversions.ToInteger(doEventData2.oParams[0]), ref bBuffer);
					oSender = (CommandsBase)oSender2;
				}
				break;
			}
			case iDoEventConstants.DoEventOnPort:
			{
				byte b = Conversions.ToByte(Conversions.ToString(doEventData2.oParams[1]));
				ISerialPort.OnPortHandler onPortEvent = OnPort;
				if (onPortEvent != null)
				{
					ISerialPort oSender2 = oSender;
					onPortEvent(ref oSender2, (ISerialPort.CommPortEventConstants)Conversions.ToInteger(doEventData2.oParams[0]), Conversions.ToString(b));
					oSender = (CommandsBase)oSender2;
				}
				break;
			}
			case iDoEventConstants.DoEventOnEvent:
			{
				OnEventEventHandler onEventEvent = OnEvent;
				if (onEventEvent != null)
				{
					IEvent oEvent = (IEvent)doEventData2.oParams[0];
					onEventEvent(ref oSender, ref oEvent);
				}
				break;
			}
			case iDoEventConstants.DoEventOnCommandFinish:
			{
				ICommand oCommand = (ICommand)doEventData2.oParams[0];
				oCommand.Finish();
				OnCommandFinish?.Invoke(ref oSender, ref oCommand);
				break;
			}
			case iDoEventConstants.DoEventOnReceive:
				OnReceive?.Invoke(ref oSender);
				break;
			}
			doEventData2 = null;
		}

		public void AddEvent(ref ISerialPort oSender, ISerialPort.CommEventConstants iCommEvent)
		{
			lock (m_oEvents)
			{
				m_oEvents.Add(new DoEventData(ref oSender, iCommEvent));
			}
			m_hEventThreadEvent.Set();
		}

		public void AddEvent(ref ISerialPort oSender, ISerialPort.CommMonitorEventConstants iEvent, ref byte[] bBuffer)
		{
			lock (m_oEvents)
			{
				m_oEvents.Add(new DoEventData(ref oSender, iEvent, ref bBuffer));
			}
			m_hEventThreadEvent.Set();
		}

		public void AddEvent(ref ISerialPort oSender, ISerialPort.CommPortEventConstants iEvent, string szPortName)
		{
			lock (m_oEvents)
			{
				m_oEvents.Add(new DoEventData(ref oSender, iEvent, szPortName));
			}
			m_hEventThreadEvent.Set();
		}

		public void AddEvent(ref CommandsBase oSender, ref IEvent oEvent)
		{
			lock (m_oEvents)
			{
				m_oEvents.Add(new DoEventData(ref oSender, ref oEvent));
			}
			m_hEventThreadEvent.Set();
		}

		public void AddEvent(ref CommandsBase oSender, ref ICommand oCommand)
		{
			lock (m_oEvents)
			{
				m_oEvents.Add(new DoEventData(ref oSender, ref oCommand));
			}
			m_hEventThreadEvent.Set();
		}

		public void AddEvent(ref CommandsBase oSender)
		{
			lock (m_oEvents)
			{
				m_oEvents.Add(new DoEventData(ref oSender));
			}
			m_hEventThreadEvent.Set();
		}

		[DebuggerNonUserCode]
		public CommandsBase()
		{
			m_oCommandList = new List<ICommand>();
			m_oCurrentCommand = null;
			m_oSerialPorts = new List<ISerialPort>();
			m_oReceivers = new List<IReceiver>();
			m_oEvents = new List<DoEventData>();
			m_oLastSendCommand = null;
			m_oWndMsg = new WindowMessage();
			m_oRxBuffer = new ByteBuffer();
			m_bIsDestroyClass = false;
			m_oSerialPortOnCommAddress = m_oNullSerialPort_OnComm;
			m_oSerialPortOnMonitorAddress = m_oNullSerialPort_OnMonitor;
			m_oSerialPortOnPortAddress = m_oNullSerialPort_OnPort;
			m_oReceiverOnEventAddress = m_oNullReceiver_OnEvent;
			m_oReceiverOnReceiveToCommandAddress = m_oNullReceiver_OnReceiveToCommand;
			m_bIsBackgroundMode = false;
			m_oCommandThread = new Thread(CommandThread);
			m_hCommandThreadEvent = new ManualResetEvent(initialState: false);
			m_oEventThread = new Thread(EventThread);
			m_hEventThreadEvent = new ManualResetEvent(initialState: false);
			m_bIsCommandThreadAlive = false;
			m_bIsEventThreadAlive = false;
			IReceiver oReceiver = null;
			WindowMessage oWndMsg = m_oWndMsg;
			MSComm mSComm = new MSComm(ref oWndMsg);
			m_oWndMsg = oWndMsg;
			ISerialPort oSerialPort = mSComm;
			Init(ref oReceiver, ref oSerialPort);
		}

		[DebuggerNonUserCode]
		public CommandsBase(ref IReceiver oReceiver)
		{
			m_oCommandList = new List<ICommand>();
			m_oCurrentCommand = null;
			m_oSerialPorts = new List<ISerialPort>();
			m_oReceivers = new List<IReceiver>();
			m_oEvents = new List<DoEventData>();
			m_oLastSendCommand = null;
			m_oWndMsg = new WindowMessage();
			m_oRxBuffer = new ByteBuffer();
			m_bIsDestroyClass = false;
			m_oSerialPortOnCommAddress = m_oNullSerialPort_OnComm;
			m_oSerialPortOnMonitorAddress = m_oNullSerialPort_OnMonitor;
			m_oSerialPortOnPortAddress = m_oNullSerialPort_OnPort;
			m_oReceiverOnEventAddress = m_oNullReceiver_OnEvent;
			m_oReceiverOnReceiveToCommandAddress = m_oNullReceiver_OnReceiveToCommand;
			m_bIsBackgroundMode = false;
			m_oCommandThread = new Thread(CommandThread);
			m_hCommandThreadEvent = new ManualResetEvent(initialState: false);
			m_oEventThread = new Thread(EventThread);
			m_hEventThreadEvent = new ManualResetEvent(initialState: false);
			m_bIsCommandThreadAlive = false;
			m_bIsEventThreadAlive = false;
			WindowMessage oWndMsg = m_oWndMsg;
			MSComm mSComm = new MSComm(ref oWndMsg);
			m_oWndMsg = oWndMsg;
			ISerialPort oSerialPort = mSComm;
			Init(ref oReceiver, ref oSerialPort);
		}

		[DebuggerNonUserCode]
		public CommandsBase(ref IReceiver oReceiver, ref ISerialPort oSerialPort)
		{
			m_oCommandList = new List<ICommand>();
			m_oCurrentCommand = null;
			m_oSerialPorts = new List<ISerialPort>();
			m_oReceivers = new List<IReceiver>();
			m_oEvents = new List<DoEventData>();
			m_oLastSendCommand = null;
			m_oWndMsg = new WindowMessage();
			m_oRxBuffer = new ByteBuffer();
			m_bIsDestroyClass = false;
			m_oSerialPortOnCommAddress = m_oNullSerialPort_OnComm;
			m_oSerialPortOnMonitorAddress = m_oNullSerialPort_OnMonitor;
			m_oSerialPortOnPortAddress = m_oNullSerialPort_OnPort;
			m_oReceiverOnEventAddress = m_oNullReceiver_OnEvent;
			m_oReceiverOnReceiveToCommandAddress = m_oNullReceiver_OnReceiveToCommand;
			m_bIsBackgroundMode = false;
			m_oCommandThread = new Thread(CommandThread);
			m_hCommandThreadEvent = new ManualResetEvent(initialState: false);
			m_oEventThread = new Thread(EventThread);
			m_hEventThreadEvent = new ManualResetEvent(initialState: false);
			m_bIsCommandThreadAlive = false;
			m_bIsEventThreadAlive = false;
			Init(ref oReceiver, ref oSerialPort);
		}

		private void Init(ref IReceiver oReceiver, ref ISerialPort oSerialPort)
		{
			m_hMainThreadID = Thread.CurrentThread.ManagedThreadId;
			if (oSerialPort != null)
			{
				AddSerialPort(ref oSerialPort);
				m_oCurrentPort = oSerialPort;
			}
			if (oReceiver != null)
			{
				AddReceiver(ref oReceiver);
			}
			m_bIsFinalize = false;
			m_iDoEventsMsg = m_oWndMsg.RegisterWindowMessage("DoEvents Message");
			Thread oCommandThread = m_oCommandThread;
			oCommandThread.IsBackground = true;
			oCommandThread.Start();
			oCommandThread = null;
			Thread oEventThread = m_oEventThread;
			oEventThread.IsBackground = true;
			oEventThread.Start();
			oEventThread = null;
		}

		protected virtual void Finalize()
		{
			base.Finalize();
			Dispose();
		}

		private void m_oNullSerialPort_OnComm(ref ISerialPort oSender, ISerialPort.CommEventConstants iCommEvent)
		{
			if (m_oCurrentPort.CommEvent == ISerialPort.CommEventConstants.comEvReceive)
			{
				m_hCommandThreadEvent.Set();
				return;
			}
			ISerialPort oSender2 = this;
			AddEvent(ref oSender2, iCommEvent);
		}

		private void m_oNullSerialPort_OnMonitor(ref ISerialPort oSender, ISerialPort.CommMonitorEventConstants iEvent, ref byte[] bBuffer)
		{
			ISerialPort oSender2 = this;
			AddEvent(ref oSender2, iEvent, ref bBuffer);
		}

		private void m_oNullSerialPort_OnPort(ref ISerialPort oSender, ISerialPort.CommPortEventConstants iEvent, string szPortName)
		{
			ISerialPort.OnPortHandler onPortEvent = OnPort;
			if (onPortEvent != null)
			{
				ISerialPort oSender2 = this;
				onPortEvent(ref oSender2, iEvent, szPortName);
			}
		}

		protected void AddSerialPort(ref ISerialPort oSerialPort, bool bIsAddEventOnly = false)
		{
			if (oSerialPort != null)
			{
				oSerialPort.OnComm += m_oSerialPortOnCommAddress;
				oSerialPort.OnMonitor += m_oSerialPortOnMonitorAddress;
				oSerialPort.OnPort += m_oSerialPortOnPortAddress;
				if (!bIsAddEventOnly)
				{
					m_oSerialPorts.Add(oSerialPort);
				}
			}
		}

		private void m_oNullReceiver_OnEvent(ref IReceiver oSender, ref IEvent oEvent)
		{
			CommandsBase oSender2 = this;
			AddEvent(ref oSender2, ref oEvent);
			m_hEventThreadEvent.Set();
		}

		private void m_oNullReceiver_OnReceiveToCommand(ref IReceiver oSender, ref ICommand oCommand)
		{
			oCommand = m_oCurrentCommand;
		}

		protected void AddReceiver(ref IReceiver oReceiver, bool bIsAddEventOnly = false)
		{
			if (oReceiver != null)
			{
				oReceiver.OnEvent += m_oReceiverOnEventAddress;
				oReceiver.OnReceiveToCommand += m_oReceiverOnReceiveToCommandAddress;
				if (!bIsAddEventOnly)
				{
					m_oReceivers.Add(oReceiver);
				}
			}
		}

		public ICommand AsyncSendCommand(ref ICommand oCommand)
		{
			m_bIsCancel = false;
			lock (m_oCommandList)
			{
				m_oCommandList.Insert(0, oCommand);
			}
			m_hCommandThreadEvent.Set();
			return oCommand;
		}

		public bool SendCommand(ref ICommand oCommand, ref IReply oReply)
		{
			AsyncSendCommand(ref oCommand);
			if (WaitCommand(ref oCommand))
			{
				oReply = oCommand.ReplyValue;
				return true;
			}
			bool result = default(bool);
			return result;
		}

		public ICommand AsyncSendCommand(ref ICommand oCommand, bool bIsRealTime)
		{
			m_bIsCancel = false;
			lock (m_oCommandList)
			{
				if (bIsRealTime)
				{
					m_oCommandList.Insert(0, oCommand);
				}
				else
				{
					m_oCommandList.Add(oCommand);
				}
			}
			m_hCommandThreadEvent.Set();
			return oCommand;
		}

		public bool SendCommand(ref ICommand oCommand, bool bIsRealTime, ref IReply oReply)
		{
			AsyncSendCommand(ref oCommand);
			if (WaitCommand(ref oCommand))
			{
				oReply = oCommand.ReplyValue;
				return true;
			}
			bool result = default(bool);
			return result;
		}

		public void DiscardCommands()
		{
			ICommand command;
			lock (m_oCommandList)
			{
				while (m_oCommandList.Count > 0)
				{
					command = m_oCommandList[0];
					try
					{
						command.AddReplyValue(ICommand.CommandStateConstants.Cancel);
						m_oCommandList.Remove(command);
					}
					catch (Exception ex)
					{
						ProjectData.SetProjectError(ex);
						Exception oLastError = ex;
						m_iLastError = ISerialPort.CommErrorConstants.Unknown;
						m_oLastError = oLastError;
						ProjectData.ClearProjectError();
					}
				}
			}
			command = null;
			m_bIsDiscardCommands = true;
		}

		public bool WaitCommand(ref ICommand oCommand)
		{
			bool result = false;
			bool flag = ((m_hMainThreadID == Thread.CurrentThread.ManagedThreadId) ? true : false);
			if (oCommand != null)
			{
				int num = 0;
				while (!m_bIsCancel)
				{
					if (m_IsPortClosed)
					{
						m_iLastError = ISerialPort.CommErrorConstants.PortIsClose;
						m_oLastError = null;
						result = false;
						break;
					}
					if (!oCommand.WaitHandle.WaitOne(1, exitContext: false) && !m_bIsBackgroundMode && flag)
					{
						Application.DoEvents();
					}
					switch (oCommand.State)
					{
					case ICommand.CommandStateConstants.ACK:
						result = true;
						break;
					case ICommand.CommandStateConstants.WaitToSend:
					case ICommand.CommandStateConstants.Receiving:
						continue;
					}
					break;
				}
			}
			return result;
		}

		private void CommandThread()
		{
			byte[] bBuffer = null;
			int num = 1024;
			m_bIsCommandThreadAlive = true;
			m_IsPortClosed = false;
			checked
			{
				int num2 = default(int);
				while (!m_bIsDestroyClass)
				{
					m_hCommandThreadEvent.Reset();
					bool flag = ((!m_IsPortClosed || m_oCurrentCommand != null || m_oCommandList.Count != 0) ? m_hCommandThreadEvent.WaitOne(1, exitContext: false) : m_hCommandThreadEvent.WaitOne());
					if (m_bIsDestroyClass)
					{
						break;
					}
					try
					{
						lock (m_oSerialPorts)
						{
							ISerialPort oCurrentPort = m_oCurrentPort;
							if (!flag)
							{
								if (!oCurrentPort.PortOpen)
								{
									m_IsPortClosed = true;
								}
								else
								{
									m_IsPortClosed = false;
									if (oCurrentPort.InBufferCount > 0)
									{
										flag = true;
									}
								}
							}
							if (m_oCurrentPort.GetLastError() == ISerialPort.CommErrorConstants.PortAccessFail)
							{
								m_IsPortClosed = true;
							}
							oCurrentPort = null;
						}
					}
					catch (Exception ex)
					{
						ProjectData.SetProjectError(ex);
						Exception ex2 = ex;
						ProjectData.ClearProjectError();
					}
					if (flag)
					{
						m_IsPortClosed = false;
						m_hCommandThreadEvent.Reset();
						if (m_bIsDiscardCommands)
						{
							m_bIsDiscardCommands = false;
							m_oCurrentCommand = null;
						}
						try
						{
							lock (m_oSerialPorts)
							{
								ISerialPort oCurrentPort2 = m_oCurrentPort;
								num = oCurrentPort2.InBufferSize;
								num2 = oCurrentPort2.InBufferCount;
								if (num2 > 0)
								{
									bBuffer = new byte[num2 - 1 + 1];
									if (!oCurrentPort2.ReadBuffer(ref bBuffer, 0, num2))
									{
										num2 = 0;
										bBuffer = null;
									}
								}
								oCurrentPort2 = null;
							}
						}
						catch (Exception ex3)
						{
							ProjectData.SetProjectError(ex3);
							Exception ex4 = ex3;
							ProjectData.ClearProjectError();
						}
						if (num2 > 0 && bBuffer != null)
						{
							if (m_oCurrentCommand != null)
							{
								IReceiver receiver = m_oCurrentCommand.Receiver;
								if (receiver.AppendReceiveData(ref bBuffer, 0, num2))
								{
									bBuffer = null;
									num2 = 0;
									if (receiver.DiscardData(ref bBuffer))
									{
										num2 = bBuffer.Length;
									}
								}
								receiver = null;
							}
							int num3 = m_oReceivers.Count - 1;
							for (int i = 0; i <= num3; i++)
							{
								if (num2 == 0)
								{
									break;
								}
								IReceiver receiver2 = m_oReceivers[i];
								if (receiver2.AppendReceiveData(ref bBuffer, 0, num2))
								{
									bBuffer = null;
									num2 = 0;
									if (receiver2.DiscardData(ref bBuffer))
									{
										num2 = bBuffer.Length;
									}
								}
								receiver2 = null;
							}
							if (num2 > 0 && m_oRxBuffer.GetSize() < num)
							{
								m_oRxBuffer.Append(ref bBuffer);
								ISerialPort oSender = this;
								AddEvent(ref oSender, ISerialPort.CommEventConstants.comEvReceive);
							}
							if (m_oCurrentCommand != null)
							{
								m_oCurrentCommand.ResetTimeout();
							}
						}
					}
					else
					{
						if (m_oCurrentCommand != null)
						{
							m_oCurrentCommand.Receiver.CheckTimeout();
						}
						int num4 = m_oReceivers.Count - 1;
						for (int i = 0; i <= num4; i++)
						{
							m_oReceivers[i].CheckTimeout();
						}
					}
					if (m_oCurrentCommand != null)
					{
						if (m_IsPortClosed)
						{
							m_oCurrentCommand.AddReplyValue(ICommand.CommandStateConstants.PortIsClosed);
							m_oCurrentCommand.WaitHandle.Set();
							CommandsBase oSender2 = this;
							AddEvent(ref oSender2, ref m_oCurrentCommand);
							m_oCurrentCommand = null;
							m_hEventThreadEvent.Set();
						}
						else
						{
							switch (m_oCurrentCommand.State)
							{
							case ICommand.CommandStateConstants.WaitToSend:
								try
								{
									if (m_oCurrentPort != null)
									{
										lock (m_oSerialPorts)
										{
											m_oCurrentCommand.AsyncSendCommand(ref m_oCurrentPort);
										}
										m_oLastSendCommand = m_oCurrentCommand;
									}
								}
								catch (Exception ex5)
								{
									ProjectData.SetProjectError(ex5);
									Exception ex6 = ex5;
									ProjectData.ClearProjectError();
								}
								break;
							default:
							{
								m_oCurrentCommand.WaitHandle.Set();
								CommandsBase oSender2 = this;
								AddEvent(ref oSender2, ref m_oCurrentCommand);
								m_oCurrentCommand = null;
								m_hEventThreadEvent.Set();
								break;
							}
							case ICommand.CommandStateConstants.Receiving:
								break;
							}
						}
					}
					if (m_oCurrentCommand != null || m_bIsCancel || m_bIsDestroyClass)
					{
						continue;
					}
					if (m_IsPortClosed)
					{
						lock (m_oCommandList)
						{
							List<ICommand> oCommandList = m_oCommandList;
							int num5 = oCommandList.Count - 1;
							for (int i = 0; i <= num5; i++)
							{
								m_oCurrentCommand = m_oCommandList[0];
								m_oCommandList.Remove(m_oCurrentCommand);
								ICommand.CommandStateConstants state = m_oCurrentCommand.State;
								if (state == ICommand.CommandStateConstants.Receiving)
								{
									m_oCurrentCommand.AddReplyValue(ICommand.CommandStateConstants.PortIsClosed);
									m_oCurrentCommand.WaitHandle.Set();
								}
								CommandsBase oSender2 = this;
								AddEvent(ref oSender2, ref m_oCurrentCommand);
								m_oCurrentCommand = null;
								m_hEventThreadEvent.Set();
							}
							oCommandList = null;
						}
					}
					else
					{
						if (flag)
						{
							continue;
						}
						lock (m_oCommandList)
						{
							List<ICommand> oCommandList2 = m_oCommandList;
							int num6 = oCommandList2.Count - 1;
							for (int i = 0; i <= num6; i++)
							{
								m_oCurrentCommand = m_oCommandList[0];
								m_oCommandList.Remove(m_oCurrentCommand);
								if (m_oCurrentCommand.State == ICommand.CommandStateConstants.WaitToSend)
								{
									break;
								}
								m_oCurrentCommand = null;
							}
							oCommandList2 = null;
						}
					}
				}
				if (m_bIsDiscardCommands)
				{
					m_bIsDiscardCommands = false;
					m_oCurrentCommand = null;
				}
				m_bIsCommandThreadAlive = false;
			}
		}

		private void EventThread()
		{
			m_bIsEventThreadAlive = true;
			while (!m_bIsDestroyClass)
			{
				if (!m_hEventThreadEvent.WaitOne() || m_bIsDestroyClass)
				{
					continue;
				}
				m_hEventThreadEvent.Reset();
				while (m_oEvents.Count > 0)
				{
					if (m_bIsBackgroundMode)
					{
						DoEventList();
					}
					else
					{
						m_oWndMsg.SendMessage(m_iDoEventsMsg, 0, 0);
					}
				}
			}
			m_bIsEventThreadAlive = false;
		}

		public void Dispose()
		{
			int try0000_dispatch = -1;
			int num3 = default(int);
			int num = default(int);
			int num2 = default(int);
			List<ISerialPort> list = default(List<ISerialPort>);
			int num5 = default(int);
			ISerialPort serialPort = default(ISerialPort);
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
					case 335:
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
								goto IL_0012;
							case 4:
								goto IL_001c;
							case 5:
								goto IL_0026;
							case 8:
								goto IL_0031;
							case 9:
								goto IL_003c;
							case 10:
							case 11:
								goto IL_004c;
							case 12:
								goto IL_0058;
							case 13:
							case 14:
								goto IL_0068;
							case 6:
							case 7:
							case 15:
								goto IL_0072;
							case 16:
								goto IL_0084;
							case 17:
								goto IL_008f;
							case 20:
								goto IL_009c;
							case 21:
								goto IL_00a8;
							case 22:
								goto IL_00b4;
							case 23:
								goto IL_00bc;
							case 18:
							case 19:
							case 24:
								goto IL_00c6;
							case 25:
								goto end_IL_0000_2;
							default:
								goto end_IL_0000;
							case 26:
								goto end_IL_0000_3;
							}
							goto default;
						}
						IL_004c:
						num2 = 11;
						if (m_bIsEventThreadAlive)
						{
							goto IL_0058;
						}
						goto IL_0068;
						IL_0058:
						num2 = 12;
						m_hEventThreadEvent.Set();
						goto IL_0068;
						IL_003c:
						num2 = 9;
						m_hCommandThreadEvent.Set();
						goto IL_004c;
						IL_0068:
						num2 = 14;
						Thread.Sleep(5);
						goto IL_0072;
						IL_0008:
						num2 = 2;
						m_bIsDestroyClass = true;
						goto IL_0012;
						IL_0012:
						num2 = 3;
						m_bIsFinalize = true;
						goto IL_001c;
						IL_001c:
						num2 = 4;
						m_oLastSendCommand = null;
						goto IL_0026;
						IL_0026:
						num2 = 5;
						Cancel();
						goto IL_0072;
						IL_0072:
						num2 = 7;
						if (m_bIsCommandThreadAlive | m_bIsEventThreadAlive)
						{
							goto IL_0031;
						}
						goto IL_0084;
						IL_0084:
						num2 = 16;
						list = m_oSerialPorts;
						goto IL_008f;
						IL_008f:
						num2 = 17;
						num5 = list.Count;
						goto IL_00c6;
						IL_00c6:
						num2 = 19;
						if (num5 <= 0)
						{
							break;
						}
						goto IL_009c;
						IL_009c:
						num2 = 20;
						serialPort = list[0];
						goto IL_00a8;
						IL_00a8:
						num2 = 21;
						list.Remove(serialPort);
						goto IL_00b4;
						IL_00b4:
						num2 = 22;
						num5 = checked(num5 - 1);
						goto IL_00bc;
						IL_00bc:
						num2 = 23;
						serialPort.Dispose();
						goto IL_00c6;
						IL_0031:
						num2 = 8;
						if (m_bIsCommandThreadAlive)
						{
							goto IL_003c;
						}
						goto IL_004c;
						end_IL_0000_2:
						break;
					}
					list = null;
					break;
					end_IL_0000:;
				}
				catch (object obj) when (obj is Exception && num3 != 0 && num == 0)
				{
					ProjectData.SetProjectError((Exception)obj);
					try0000_dispatch = 335;
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

		void ISerialPort.Dispose()
		{
			//ILSpy generated this explicit interface implementation from .override directive in Dispose
			this.Dispose();
		}

		public bool WriteBuffer(ref byte[] bBuffer, int iStart, int iLength)
		{
			return m_oCurrentPort.WriteBuffer(ref bBuffer, iStart, iLength);
		}

		bool ISerialPort.WriteBuffer(ref byte[] bBuffer, int iStart, int iLength)
		{
			//ILSpy generated this explicit interface implementation from .override directive in WriteBuffer
			return this.WriteBuffer(ref bBuffer, iStart, iLength);
		}

		public bool ReadBuffer(ref byte[] bBuffer, int iStart, int iLength)
		{
			return m_oCurrentPort.ReadBuffer(ref bBuffer, iStart, iLength);
		}

		bool ISerialPort.ReadBuffer(ref byte[] bBuffer, int iStart, int iLength)
		{
			//ILSpy generated this explicit interface implementation from .override directive in ReadBuffer
			return this.ReadBuffer(ref bBuffer, iStart, iLength);
		}

		public bool IsMyPort(ref string szPortName)
		{
			return m_oCurrentPort.IsMyPort(ref szPortName);
		}

		bool ISerialPort.IsMyPort(ref string szPortName)
		{
			//ILSpy generated this explicit interface implementation from .override directive in IsMyPort
			return this.IsMyPort(ref szPortName);
		}

		public int NumberOfPorts()
		{
			List<ISerialPort> oSerialPorts = m_oSerialPorts;
			checked
			{
				int num = oSerialPorts.Count - 1;
				int num2 = default(int);
				for (int i = 0; i <= num; i++)
				{
					num2 += oSerialPorts[i].NumberOfPorts();
				}
				oSerialPorts = null;
				return num2;
			}
		}

		int ISerialPort.NumberOfPorts()
		{
			//ILSpy generated this explicit interface implementation from .override directive in NumberOfPorts
			return this.NumberOfPorts();
		}

		public int NumberOfPorts(string PortFilter)
		{
			List<ISerialPort> oSerialPorts = m_oSerialPorts;
			checked
			{
				int num = oSerialPorts.Count - 1;
				int num2 = default(int);
				for (int i = 0; i <= num; i++)
				{
					num2 += oSerialPorts[i].NumberOfPorts(PortFilter);
				}
				oSerialPorts = null;
				return num2;
			}
		}

		int ISerialPort.NumberOfPorts(string PortFilter)
		{
			//ILSpy generated this explicit interface implementation from .override directive in NumberOfPorts
			return this.NumberOfPorts(PortFilter);
		}

		public string[] EnumCommPortEx()
		{
			string[] array = new string[0];
			int num = 0;
			List<ISerialPort> oSerialPorts = m_oSerialPorts;
			checked
			{
				int num2 = oSerialPorts.Count - 1;
				for (int i = 0; i <= num2; i++)
				{
					int num3 = oSerialPorts[i].NumberOfPorts();
					int num4 = num3 - 1;
					for (int j = 0; j <= num4; j++)
					{
						try
						{
							array = (string[])Utils.CopyArray(array, new string[num + 1]);
							array[num] = oSerialPorts[i].EnumCommPort(j);
							num++;
						}
						catch (Exception ex)
						{
							ProjectData.SetProjectError(ex);
							Exception ex2 = ex;
							ProjectData.ClearProjectError();
						}
					}
				}
				oSerialPorts = null;
				return array;
			}
		}

		public string EnumCommPort(int Index)
		{
			return m_oCurrentPort.EnumCommPort(Index);
		}

		string ISerialPort.EnumCommPort(int Index)
		{
			//ILSpy generated this explicit interface implementation from .override directive in EnumCommPort
			return this.EnumCommPort(Index);
		}

		public string EnumCommPort(int Index, string PortFilter)
		{
			return m_oCurrentPort.EnumCommPort(Index);
		}

		string ISerialPort.EnumCommPort(int Index, string PortFilter)
		{
			//ILSpy generated this explicit interface implementation from .override directive in EnumCommPort
			return this.EnumCommPort(Index, PortFilter);
		}

		public int EnumCommPortNumber(int Index)
		{
			return m_oCurrentPort.EnumCommPortNumber(Index);
		}

		int ISerialPort.EnumCommPortNumber(int Index)
		{
			//ILSpy generated this explicit interface implementation from .override directive in EnumCommPortNumber
			return this.EnumCommPortNumber(Index);
		}

		public int EnumCommPortNumber(int Index, string PortFilter)
		{
			return m_oCurrentPort.EnumCommPortNumber(Index);
		}

		int ISerialPort.EnumCommPortNumber(int Index, string PortFilter)
		{
			//ILSpy generated this explicit interface implementation from .override directive in EnumCommPortNumber
			return this.EnumCommPortNumber(Index, PortFilter);
		}

		public string CommPortClass(int CommPort)
		{
			return m_oCurrentPort.CommPortClass(CommPort);
		}

		string ISerialPort.CommPortClass(int CommPort)
		{
			//ILSpy generated this explicit interface implementation from .override directive in CommPortClass
			return this.CommPortClass(CommPort);
		}

		public string CommPortClass(string CommPort)
		{
			return m_oCurrentPort.CommPortClass(CommPort);
		}

		string ISerialPort.CommPortClass(string CommPort)
		{
			//ILSpy generated this explicit interface implementation from .override directive in CommPortClass
			return this.CommPortClass(CommPort);
		}

		public string CommPortInfo(int CommPort)
		{
			return m_oCurrentPort.CommPortInfo(CommPort);
		}

		string ISerialPort.CommPortInfo(int CommPort)
		{
			//ILSpy generated this explicit interface implementation from .override directive in CommPortInfo
			return this.CommPortInfo(CommPort);
		}

		public string CommPortInfo(string CommPort)
		{
			return m_oCurrentPort.CommPortInfo(CommPort);
		}

		string ISerialPort.CommPortInfo(string CommPort)
		{
			//ILSpy generated this explicit interface implementation from .override directive in CommPortInfo
			return this.CommPortInfo(CommPort);
		}

		public bool CommPortInfo(string CommPort, ref string szDeviceClass, ref string szManufacturer, ref string szHardwareID)
		{
			return m_oCurrentPort.CommPortInfo(CommPort, ref szDeviceClass, ref szManufacturer, ref szHardwareID);
		}

		bool ISerialPort.CommPortInfo(string CommPort, ref string szDeviceClass, ref string szManufacturer, ref string szHardwareID)
		{
			//ILSpy generated this explicit interface implementation from .override directive in CommPortInfo
			return this.CommPortInfo(CommPort, ref szDeviceClass, ref szManufacturer, ref szHardwareID);
		}

		public ISerialPort.CommErrorConstants GetLastError()
		{
			return m_oCurrentPort.GetLastError();
		}

		ISerialPort.CommErrorConstants ISerialPort.GetLastError()
		{
			//ILSpy generated this explicit interface implementation from .override directive in GetLastError
			return this.GetLastError();
		}

		public ISerialPort.CommErrorConstants GetLastError(ref string szError)
		{
			return m_oCurrentPort.GetLastError(ref szError);
		}

		ISerialPort.CommErrorConstants ISerialPort.GetLastError(ref string szError)
		{
			//ILSpy generated this explicit interface implementation from .override directive in GetLastError
			return this.GetLastError(ref szError);
		}

		public ISerialPort.CommErrorConstants GetLastError(ref Exception oError)
		{
			return m_oCurrentPort.GetLastError(ref oError);
		}

		ISerialPort.CommErrorConstants ISerialPort.GetLastError(ref Exception oError)
		{
			//ILSpy generated this explicit interface implementation from .override directive in GetLastError
			return this.GetLastError(ref oError);
		}
	}
}
