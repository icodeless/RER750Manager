using System;

namespace GIGATMS.IO
{
	public interface ISerialPort
	{
		public enum InputModeConstants
		{
			comInputModeText,
			comInputModeBinary
		}

		public enum CommPortEventConstants
		{
			comEvRemove,
			comEvPlugin,
			comEvRemoveClosed
		}

		public enum CommEventConstants
		{
			comEventBreak = 1001,
			comEventFrame = 1004,
			comEventOverrun = 1006,
			comEventRxOver = 1008,
			comEventRxParity = 1009,
			comEventTxFull = 1010,
			comEvSend = 1,
			comEvReceive = 2,
			comEvCTS = 3,
			comEvDSR = 4,
			comEvCD = 5,
			comEvRing = 6,
			comEvEOF = 7,
			comEvPower = 256
		}

		public enum HandshakeConstants
		{
			comNone,
			comXonXoff,
			comRtsCts,
			comBoth
		}

		public enum CommMonitorEventConstants
		{
			comEvOutput,
			comEvInput,
			comEvPortOpen,
			comEvPortBeforeClose,
			comEvPortClose
		}

		public enum CommErrorConstants
		{
			Success,
			PortIsClose,
			PortAlreadyOpen,
			PortAccessFail,
			Unknown
		}

		public delegate void OnCommHandler(ref ISerialPort oSender, CommEventConstants iCommEvent);

		public delegate void OnMonitorHandler(ref ISerialPort oSender, CommMonitorEventConstants iEvent, ref byte[] bBuffer);

		public delegate void OnPortHandler(ref ISerialPort oSender, CommPortEventConstants iEvent, string szPortName);

		object BasePort { get; }

		bool IsBackgroundMode { get; set; }

		bool Break { get; set; }

		bool CDHolding { get; }

		CommEventConstants CommEvent { get; }

		int CommPort { get; set; }

		string PortName { get; set; }

		bool CTSHolding { get; }

		bool DSRHolding { get; }

		bool DTREnable { get; set; }

		InputModeConstants InputMode { get; set; }

		int RThreshold { get; set; }

		HandshakeConstants Handshaking { get; set; }

		int InBufferCount { get; set; }

		int InBufferSize { get; set; }

		object Input { get; }

		int InputLen { get; set; }

		bool NullDiscard { get; set; }

		int OutBufferCount { get; set; }

		int OutBufferSize { get; set; }

		object Output { set; }

		char ParityReplace { get; set; }

		bool PortOpen { get; set; }

		bool RTSEnable { get; set; }

		string Settings { get; set; }

		bool IsMonitorIO { get; set; }

		int SThreshold { get; set; }

		event OnCommHandler OnComm;

		event OnMonitorHandler OnMonitor;

		event OnPortHandler OnPort;

		bool ReadBuffer(ref byte[] bBuffer, int iStart, int iLength);

		bool WriteBuffer(ref byte[] bBuffer, int iStart, int iLength);

		bool IsMyPort(ref string szPortName);

		int NumberOfPorts();

		int NumberOfPorts(string PortFilter);

		int EnumCommPortNumber(int Index, string PortFilter);

		int EnumCommPortNumber(int Index);

		string EnumCommPort(int Index, string PortFilter);

		string EnumCommPort(int Index);

		string CommPortClass(int CommPort);

		string CommPortClass(string CommPort);

		string CommPortInfo(int CommPort);

		string CommPortInfo(string CommPort);

		bool CommPortInfo(string CommPort, ref string szDeviceClass, ref string szManufacturer, ref string szHardwareID);

		CommErrorConstants GetLastError();

		CommErrorConstants GetLastError(ref string szError);

		CommErrorConstants GetLastError(ref Exception oError);

		void Dispose();
	}
}
