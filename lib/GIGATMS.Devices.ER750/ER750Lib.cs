using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using GIGATMS.Devices.ER750.Parameters;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace GIGATMS.Devices.ER750
{
	public class ER750Lib : Device
	{
		public delegate void EventMessageEventHandler(string source, string eventMessage);

		public delegate void ReceivedDataEventHandler(string remoteIpAddress, int remotePort, byte[] dataByteArray);

		public delegate void ReceivedDeviceStatusEventHandler(ref DeviceStatusFormat deviceStatus);

		public delegate void ReceivedEventDataEventHandler(string deviceIP, ref EventDataFormat eventData);

		public delegate void ReceivedReplyCommandEventHandler(string remoteIpaddress, E_ErrorCodes result);

		public delegate void ErrorOccuredEventHandler(string source, string errorMessage);


		[DebuggerBrowsable(DebuggerBrowsableState.Never)]

		private UdpSocket __udp;


		[DebuggerBrowsable(DebuggerBrowsableState.Never)]

		private TcpServerSocket __tcpServer;


		[DebuggerBrowsable(DebuggerBrowsableState.Never)]

		private TCPClientSocket __tcpClient;

		private Queue<JobFormat> _batchJobs;

		private System.Collections.Generic.List<string> _broadcastDeviceIPCollection;

		protected virtual UdpSocket _udp
		{

			get
			{
				return __udp;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]

			set
			{
				UdpSocket.DataReceivedEventHandler obj = _udp_DataReceived;
				UdpSocket udpSocket = __udp;
				if (udpSocket != null)
				{
					udpSocket.DataReceived -= obj;
				}
				__udp = value;
				udpSocket = __udp;
				if (udpSocket != null)
				{
					udpSocket.DataReceived += obj;
				}
			}
		}

		protected virtual TcpServerSocket _tcpServer
		{

			get
			{
				return __tcpServer;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]

			set
			{
				TcpServerSocket.ConnectedEventHandler obj = _tcpServer_Connected;
				TcpServerSocket.DataReceivedEventHandler obj2 = _tcpServer_DataReceived;
				TcpServerSocket.DisconnectedEventHandler obj3 = _tcpServer_Disconnected;
				TcpServerSocket tcpServerSocket = __tcpServer;
				if (tcpServerSocket != null)
				{
					tcpServerSocket.Connected -= obj;
					tcpServerSocket.DataReceived -= obj2;
					tcpServerSocket.Disconnected -= obj3;
				}
				__tcpServer = value;
				tcpServerSocket = __tcpServer;
				if (tcpServerSocket != null)
				{
					tcpServerSocket.Connected += obj;
					tcpServerSocket.DataReceived += obj2;
					tcpServerSocket.Disconnected += obj3;
				}
			}
		}

		protected virtual TCPClientSocket _tcpClient
		{

			get
			{
				return __tcpClient;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]

			set
			{
				TCPClientSocket.ConnectedEventHandler obj = _tcpClient_Connected;
				TCPClientSocket.DataReceivedEventHandler obj2 = _tcpClient_DataReceived;
				TCPClientSocket.DisconnectedEventHandler obj3 = _tcpClient_Disconnected;
				TCPClientSocket.ErrorOccurredEventHandler obj4 = _tcpClient_ErrorOccurred;
				TCPClientSocket.SendCompleteEventHandler obj5 = _tcpClient_SendComplete;
				TCPClientSocket tCPClientSocket = __tcpClient;
				if (tCPClientSocket != null)
				{
					tCPClientSocket.Connected -= obj;
					tCPClientSocket.DataReceived -= obj2;
					tCPClientSocket.Disconnected -= obj3;
					tCPClientSocket.ErrorOccurred -= obj4;
					tCPClientSocket.SendComplete -= obj5;
				}
				__tcpClient = value;
				tCPClientSocket = __tcpClient;
				if (tCPClientSocket != null)
				{
					tCPClientSocket.Connected += obj;
					tCPClientSocket.DataReceived += obj2;
					tCPClientSocket.Disconnected += obj3;
					tCPClientSocket.ErrorOccurred += obj4;
					tCPClientSocket.SendComplete += obj5;
				}
			}
		}

		public override int Retries
		{
			get
			{
				return _retries;
			}
			set
			{
				_retries = value;
			}
		}

		public override int Timeout
		{
			get
			{
				return _timeout;
			}
			set
			{
				_timeout = value;
			}
		}

		public event EventMessageEventHandler EventMessage;

		public event ReceivedDataEventHandler ReceivedData;

		public event ReceivedDeviceStatusEventHandler ReceivedDeviceStatus;

		public event ReceivedEventDataEventHandler ReceivedEventData;

		public event ReceivedReplyCommandEventHandler ReceivedReplyCommand;

		public event ErrorOccuredEventHandler ErrorOccured;

		public ER750Lib()
		{
			_tcpServer = new TcpServerSocket();
			_tcpClient = new TCPClientSocket();
			_batchJobs = new Queue<JobFormat>();
			_broadcastDeviceIPCollection = new System.Collections.Generic.List<string>();
		}

		public void Broadcast()
		{
			byte[] dataBytes = new byte[4] { 255, 4, 2, 251 };
			try
			{
				if (_udp != null)
				{
					_udp = null;
				}
				_udp = new UdpSocket();
				_broadcastDeviceIPCollection.Clear();
				_udp.Bind("", 0);
				int i = 0;
				do
				{
					_udp.Send("255.255.255.255", 23, ref dataBytes);
					i = checked(i + 1);
				}
				while (i <= 5);
			}
			catch (Exception ex)
			{
				ProjectData.SetProjectError(ex);
				Exception ex2 = ex;
				EventMessage?.Invoke("Broascast", ex2.Message);
				ProjectData.ClearProjectError();
			}
		}

		public void Broadcast(string bindLocalIP)
		{
			byte[] dataBytes = new byte[4] { 255, 4, 2, 251 };
			try
			{
				if (_udp != null)
				{
					_udp = null;
				}
				_udp = new UdpSocket();
				_broadcastDeviceIPCollection.Clear();
				_udp.Bind(bindLocalIP, 0);
				int i = 0;
				do
				{
					_udp.Send("255.255.255.255", 23, ref dataBytes);
					i = checked(i + 1);
				}
				while (i <= 5);
			}
			catch (Exception ex)
			{
				ProjectData.SetProjectError(ex);
				Exception ex2 = ex;
				EventMessage?.Invoke("Broascast", ex2.Message);
				ProjectData.ClearProjectError();
			}
		}

		public void StopListen()
		{
			_tcpServer.StopListen();
		}

		public void StartListen(string localIP, int localPort)
		{
			_tcpServer.StartListen(localIP, localPort);
		}

		public void StartListen(string localIP)
		{
			_tcpServer.StartListen(localIP, 2168);
		}

		public override void Close()
		{
			_udp.CloseConnection();
		}

		public override void Open()
		{
		}

		public override void Open(string connectionString)
		{
		}

		private void _udp_DataReceived(string remoteIP, int remotePort, ref byte[] dataByte)
		{
			ReceivedData?.Invoke(remoteIP, remotePort, dataByte);
			DeviceStatusFormat deviceState = new DeviceStatusFormat();
			try
			{
				deviceState.ParseParameter(ref dataByte);
			}
			catch (Exception ex)
			{
				EventMessage?.Invoke(remoteIP, $"Cannot parse device status: {ex.Message}");
				return;
			}

			if (deviceState.ErrorCode == 0)
			{
				try
				{
					if (string.IsNullOrWhiteSpace(deviceState.IpAddress) ||
						!System.Net.IPAddress.TryParse(deviceState.IpAddress, out _))
					{
						deviceState.IpAddress = remoteIP;
						EventMessage?.Invoke(remoteIP, "Device status IP fallback applied");
					}

					if (!_broadcastDeviceIPCollection.Contains(deviceState.IpAddress))
					{
						_broadcastDeviceIPCollection.Add(deviceState.IpAddress);
						ReceivedDeviceStatus?.Invoke(ref deviceState);
					}
					return;
				}
				catch (Exception ex)
				{
					ProjectData.SetProjectError(ex);
					ProjectData.ClearProjectError();
					return;
				}
			}
			EventMessage?.Invoke(remoteIP, "Cannot parse device status");
		}

		public void GetLocalIP(ref int localIpCount, ref string[] localIPs)
		{
			if (_udp == null)
			{
				_udp = new UdpSocket();
			}
			_udp.GetLocalIP(ref localIPs);
			if (localIPs == null)
			{
				localIpCount = 0;
			}
			else
			{
				localIpCount = localIPs.Length;
			}
		}

		private void _tcpServer_Connected(string remoteIP, int remotePort)
		{
			EventMessage?.Invoke(remoteIP, "Connected (Server Socket)");
		}

		private void _tcpServer_DataReceived(string clientIP, int remotePort, ref byte[] dataByte)
		{
			EventDataFormat eventData = new EventDataFormat();
			try
			{
				eventData.ParseParameter(ref dataByte);
				ReceivedEventData?.Invoke(clientIP, ref eventData);
			}
			catch (Exception ex)
			{
				ProjectData.SetProjectError(ex);
				Exception ex2 = ex;
				EventMessage?.Invoke(clientIP, "Cannot parse Event Data");
				ProjectData.ClearProjectError();
			}
		}

		public void ConnectToReader(string remoteIP, int remotePort)
		{
			_tcpClient.Connect(remoteIP, remotePort);
		}

		public void ConnectToReader(string remoteIP)
		{
			_tcpClient.Connect(remoteIP, 2167);
		}

		public void SendOpenDoorCommand(byte duration)
		{
			GnetCommandFormat gnet = new GnetCommandFormat();
			byte[] commandByteArray = gnet.GetPackageCommand(0, 17, new byte[2] { 0, duration });
			try
			{
				_tcpClient.Send(ref commandByteArray);
			}
			catch (Exception ex)
			{
				ProjectData.SetProjectError(ex);
				Exception ex2 = ex;
				Console.WriteLine(ex2.Message);
				ProjectData.ClearProjectError();
			}
		}

		private void SendControlLedBuzzerCommand(byte controlLedBuzzerSelection)
		{
			byte[] commandByteArray = new byte[5]
			{
				2,
				74,
				checked((byte)(48 + controlLedBuzzerSelection)),
				13,
				0
			};
			try
			{
				_tcpClient.Send(ref commandByteArray);
			}
			catch (Exception ex)
			{
				ProjectData.SetProjectError(ex);
				Exception ex2 = ex;
				Console.WriteLine(ex2.Message);
				ProjectData.ClearProjectError();
			}
		}

		private void DoNextJobs()
		{
			checked
			{
				if (_batchJobs.Count > 0)
				{
					JobFormat job = _batchJobs.Dequeue();
					switch (job.Job)
					{
					case E_Jobs.E00_ConnectToReader:
					{
						string[] col = Conversions.ToString(job.Parameter).Split(new string[] { ":" }, StringSplitOptions.None);
						ConnectToReader(col[0], (int)Math.Round(Convert.ToDouble(col[1])));
						break;
					}
					case E_Jobs.E01_SendOpenDoorCommand:
					{
						int integerParameter = Conversions.ToInteger(job.Parameter);
						SendOpenDoorCommand((byte)integerParameter);
						break;
					}
					case E_Jobs.E02_SendControlLedBuzzerCommand:
					{
						int integerParameter = Conversions.ToInteger(job.Parameter);
						SendControlLedBuzzerCommand((byte)integerParameter);
						break;
					}
					case E_Jobs.E98_Disconnect:
						_tcpClient.CloseConnection();
						break;
					case E_Jobs.E99_Complete:
						CompleteJob();
						break;
					default:
						EventMessage?.Invoke("DoNextJob", "Unknown job");
						break;
					}
				}
			}
		}

		public void CompleteJob()
		{
			_batchJobs.Clear();
			EventMessage?.Invoke(_tcpClient.RemoteIP, "Completed");
		}

		public void OpenDoor(string remoteIP, int remotePort, byte duration)
		{
			_batchJobs.Clear();
			if (_tcpClient.IsConnected)
			{
				_batchJobs.Enqueue(new JobFormat(E_Jobs.E98_Disconnect));
			}
			_batchJobs.Enqueue(new JobFormat(E_Jobs.E00_ConnectToReader, remoteIP + ":" + remotePort));
			_batchJobs.Enqueue(new JobFormat(E_Jobs.E01_SendOpenDoorCommand, duration));
			_batchJobs.Enqueue(new JobFormat(E_Jobs.E98_Disconnect));
			_batchJobs.Enqueue(new JobFormat(E_Jobs.E99_Complete));
			DoNextJobs();
		}

		public void OpenDoor(string remoteIP, int remotePort, byte duration, bool closeConnection)
		{
			_batchJobs.Clear();
			if (_tcpClient.IsConnected)
			{
				_batchJobs.Enqueue(new JobFormat(E_Jobs.E98_Disconnect));
			}
			_batchJobs.Enqueue(new JobFormat(E_Jobs.E00_ConnectToReader, remoteIP + ":" + remotePort));
			_batchJobs.Enqueue(new JobFormat(E_Jobs.E01_SendOpenDoorCommand, duration));
			if (closeConnection)
			{
				_batchJobs.Enqueue(new JobFormat(E_Jobs.E98_Disconnect));
			}
			_batchJobs.Enqueue(new JobFormat(E_Jobs.E99_Complete));
			DoNextJobs();
		}

		public void ControlLedBuzzer(string remoteIP, int remotePort, byte controlLedBuzzerSelection)
		{
			_batchJobs.Clear();
			if (_tcpClient.IsConnected)
			{
				_batchJobs.Enqueue(new JobFormat(E_Jobs.E98_Disconnect));
			}
			_batchJobs.Enqueue(new JobFormat(E_Jobs.E00_ConnectToReader, remoteIP + ":" + remotePort));
			_batchJobs.Enqueue(new JobFormat(E_Jobs.E02_SendControlLedBuzzerCommand, controlLedBuzzerSelection));
			_batchJobs.Enqueue(new JobFormat(E_Jobs.E98_Disconnect));
			_batchJobs.Enqueue(new JobFormat(E_Jobs.E99_Complete));
			DoNextJobs();
		}

		public void ControlLedBuzzer(string remoteIP, int remotePort, byte controlLedBuzzerSelection, bool establishConnection)
		{
			_batchJobs.Clear();
			if (establishConnection)
			{
				if (_tcpClient.IsConnected)
				{
					_batchJobs.Enqueue(new JobFormat(E_Jobs.E98_Disconnect));
				}
				_batchJobs.Enqueue(new JobFormat(E_Jobs.E00_ConnectToReader, remoteIP + ":" + remotePort));
			}
			_batchJobs.Enqueue(new JobFormat(E_Jobs.E02_SendControlLedBuzzerCommand, controlLedBuzzerSelection));
			_batchJobs.Enqueue(new JobFormat(E_Jobs.E98_Disconnect));
			_batchJobs.Enqueue(new JobFormat(E_Jobs.E99_Complete));
			DoNextJobs();
		}

		private void _tcpClient_Connected()
		{
			EventMessage?.Invoke(_tcpClient.RemoteIP, "Connected (Client Socket)");
			DoNextJobs();
		}

		private void _tcpClient_DataReceived(ref byte[] dataByte)
		{
			GnetCommandFormat cmd = new GnetCommandFormat();
			try
			{
				if (dataByte.Length == 1)
				{
					if (dataByte[0] == 6)
					{
						EventMessage?.Invoke(_tcpClient.RemoteIP, "Received: E00_Success");
						ReceivedReplyCommand?.Invoke(_tcpClient.RemoteIP, E_ErrorCodes.E00_Success);
					}
					else
					{
						ErrorOccured?.Invoke(_tcpClient.RemoteIP, "_tcpClient_DataReceived: Unknown reply command");
						ReceivedReplyCommand?.Invoke(_tcpClient.RemoteIP, E_ErrorCodes.E01_Failed);
					}
				}
				else
				{
					cmd.ParseGnetCommand(dataByte);
					EventMessage?.Invoke(_tcpClient.RemoteIP, "Received: " + cmd.ErrorCode);
					if (cmd.ErrorCode == E_GnetErrorCodes.E00_Success)
					{
						ReceivedReplyCommand?.Invoke(_tcpClient.RemoteIP, E_ErrorCodes.E00_Success);
					}
					else
					{
						ErrorOccured?.Invoke(_tcpClient.RemoteIP, "_tcpClient_DataReceived: " + cmd.ErrorCode);
						ReceivedReplyCommand?.Invoke(_tcpClient.RemoteIP, E_ErrorCodes.E01_Failed);
					}
				}
				DoNextJobs();
			}
			catch (Exception ex)
			{
				ProjectData.SetProjectError(ex);
				Exception ex2 = ex;
				EventMessage?.Invoke(_tcpClient.RemoteIP, ex2.Message);
				_batchJobs.Clear();
				ErrorOccured?.Invoke(_tcpClient.RemoteIP, ex2.Message);
				ProjectData.ClearProjectError();
			}
		}

		private void _tcpClient_Disconnected()
		{
			EventMessage?.Invoke(_tcpClient.RemoteIP, "Disconnected (Client Socket)");
			DoNextJobs();
		}

		private void _tcpServer_Disconnected(string remoteIP, int remotePort)
		{
			EventMessage?.Invoke(remoteIP, "Disconnected (Server Socket)");
		}

		private void _tcpClient_ErrorOccurred(string errorMessage)
		{
			ErrorOccured?.Invoke(_tcpClient.RemoteIP, errorMessage);
		}

		private void _tcpClient_SendComplete(int bytesSent)
		{
			EventMessage?.Invoke(_tcpClient.RemoteIP, "Command sent");
		}
	}
}
