using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace GIGATMS.Devices.ER750
{
	public class UdpSocket
	{
		private class SocketState
		{
			public IPEndPoint RemoteIPEndPoint;

			public UdpClient clientSocket;

			public byte[] ReceivedBytes;

			public SocketState()
			{
				ReceivedBytes = new byte[2049];
			}
		}

		public delegate void ErrorOccurredEventHandler(string remoteIP, int remotePort, string errorMessage);

		public delegate void DataReceivedEventHandler(string remoteIP, int remotePort, ref byte[] dataByteArray);

		public delegate void SendCompleteEventHandler(string remoteIP, int remotePort, int bytesSent);

		private SocketState _socketState;

		private string _remoteIP;

		private int _remotePort;

		private int _localPort;

		private bool _socketBound;

		private AsyncCallback _endReceiveCallback;

		private AsyncCallback _endSendCallback;

		public string RemoteIP
		{
			get
			{
				return _remoteIP;
			}
			set
			{
				_remoteIP = value;
			}
		}

		public int RemotePort
		{
			get
			{
				return _remotePort;
			}
			set
			{
				_remotePort = value;
			}
		}

		public int LocalPort
		{
			get
			{
				return _localPort;
			}
			set
			{
				_localPort = value;
			}
		}

		public event ErrorOccurredEventHandler ErrorOccurred;

		public event DataReceivedEventHandler DataReceived;

		public event SendCompleteEventHandler SendComplete;

		public UdpSocket()
		{
			_socketBound = false;
			_endReceiveCallback = EndReceiveCallback;
			_endSendCallback = EndSendCallback;
		}

		public void GetLocalIP(ref string[] localIPs)
		{
			int ipCount = Dns.GetHostEntry(Dns.GetHostName()).AddressList.Length;
			ipCount = 0;
			IPAddress[] addressList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
			checked
			{
				foreach (IPAddress ip in addressList)
				{
					if (ip.ToString().IndexOf(":") + 1 == 0)
					{
						localIPs = (string[])Utils.CopyArray(localIPs, new string[ipCount + 1]);
						localIPs[ipCount] = ip.ToString();
						ipCount++;
					}
				}
			}
		}

		public static string GetEndPointIP(EndPoint endPointx)
		{
			return ((IPEndPoint)endPointx).Address.ToString();
		}

		public static int GetEndPointPort(EndPoint endPointx)
		{
			return ((IPEndPoint)endPointx).Port;
		}

		public void CloseConnection()
		{
			_socketBound = false;
			try
			{
				_socketState.clientSocket.Client.Close();
				_socketState.clientSocket.Close();
			}
			catch (Exception ex)
			{
				ProjectData.SetProjectError(ex);
				Exception ex2 = ex;
				throw ex2;
			}
		}

		private void EndReceiveCallback(IAsyncResult ar)
		{
			if (!_socketBound)
			{
				return;
			}
			string procedureName = MethodBase.GetCurrentMethod().Name;
			string remoteIP = "";
			SocketState ss = (SocketState)ar.AsyncState;
			int remotePort = default(int);
			try
			{
				byte[] buf = ss.clientSocket.EndReceive(ar, ref ss.RemoteIPEndPoint);
				remoteIP = GetEndPointIP(ss.RemoteIPEndPoint);
				remotePort = GetEndPointPort(ss.RemoteIPEndPoint);
				try
				{
					DataReceived?.Invoke(remoteIP, remotePort, ref buf);
				}
				catch (Exception ex)
				{
					ProjectData.SetProjectError(ex);
					Exception ex2 = ex;
					ErrorOccurred?.Invoke(remoteIP, remotePort, procedureName + ": " + ex2.Message);
					ProjectData.ClearProjectError();
				}
			}
			catch (SocketException ex3)
			{
				ProjectData.SetProjectError(ex3);
				SocketException ex4 = ex3;
				ErrorOccurred?.Invoke(remoteIP, remotePort, procedureName + ": (Error/Message) " + Conversions.ToString(ex4.ErrorCode) + " / " + ex4.Message);
				ProjectData.ClearProjectError();
			}
			catch (Exception ex5)
			{
				ProjectData.SetProjectError(ex5);
				Exception ex6 = ex5;
				ErrorOccurred?.Invoke(remoteIP, remotePort, procedureName + ": " + ex6.Message);
				ProjectData.ClearProjectError();
			}
			try
			{
				ss.clientSocket.BeginReceive(_endReceiveCallback, _socketState);
			}
			catch (SocketException ex7)
			{
				ProjectData.SetProjectError(ex7);
				SocketException ex8 = ex7;
				ErrorOccurred?.Invoke(remoteIP, remotePort, procedureName + ": (Error/Message) " + Conversions.ToString(ex8.ErrorCode) + " / " + ex8.Message);
				ProjectData.ClearProjectError();
			}
			catch (Exception ex9)
			{
				ProjectData.SetProjectError(ex9);
				Exception ex10 = ex9;
				ErrorOccurred?.Invoke(remoteIP, remotePort, procedureName + ": " + ex10.Message);
				ProjectData.ClearProjectError();
			}
		}

		public void NewSocketState(EndPoint ep)
		{
			_socketState = new SocketState();
			_socketState.clientSocket = new UdpClient((IPEndPoint)ep);
		}

		public void Bind(string localIP, int localPort)
		{
			try
			{
				IPEndPoint ep = ((Operators.CompareString(localIP, "", TextCompare: false) != 0) ? new IPEndPoint(IPAddress.Parse(localIP), localPort) : new IPEndPoint(IPAddress.Any, localPort));
				if (_socketState == null)
				{
					_socketState = new SocketState();
					_socketState.clientSocket = new UdpClient();
				}
				else
				{
					_socketState.clientSocket.Close();
					_socketState.clientSocket = new UdpClient();
				}
				_socketState.clientSocket.Client.Bind(ep);
				_localPort = GetEndPointPort(_socketState.clientSocket.Client.LocalEndPoint);
				_socketState.clientSocket.BeginReceive(_endReceiveCallback, _socketState);
				_socketBound = true;
			}
			catch (Exception ex)
			{
				ProjectData.SetProjectError(ex);
				Exception ex2 = ex;
				throw ex2;
			}
		}

		private void EndSendCallback(IAsyncResult ar)
		{
			string procedureName = MethodBase.GetCurrentMethod().Name;
			try
			{
				SocketState ss = (SocketState)ar.AsyncState;
				int bytesSent = ss.clientSocket.Client.EndSend(ar);
				int remotePort = GetEndPointPort(ss.RemoteIPEndPoint);
				string remoteIP = GetEndPointIP(ss.RemoteIPEndPoint);
				SendComplete?.Invoke(remoteIP, remotePort, bytesSent);
			}
			catch (Exception ex)
			{
				ProjectData.SetProjectError(ex);
				Exception ex2 = ex;
				ErrorOccurred?.Invoke(_remoteIP, _remotePort, procedureName + ": " + ex2.Message);
				ProjectData.ClearProjectError();
			}
		}

		public void Send(byte[] dataBytes)
		{
			try
			{
				if (_socketState == null)
				{
					_socketState = new SocketState();
					_socketState.clientSocket = new UdpClient();
				}
				_socketState.clientSocket.BeginSend(dataBytes, dataBytes.Length, _socketState.RemoteIPEndPoint, _endSendCallback, _socketState);
				_socketBound = true;
				_socketState.clientSocket.BeginReceive(_endReceiveCallback, _socketState);
			}
			catch (Exception ex)
			{
				ProjectData.SetProjectError(ex);
				Exception ex2 = ex;
				throw ex2;
			}
		}

		public void Send(string remoteIP, int remotePort, ref byte[] dataBytes)
		{
			string procedureName = MethodBase.GetCurrentMethod().Name;
			_remoteIP = remoteIP;
			_remotePort = remotePort;
			IPEndPoint ep = new IPEndPoint(IPAddress.Parse(remoteIP), remotePort);
			try
			{
				if (_socketState == null)
				{
					_socketState = new SocketState();
					_socketState.clientSocket = new UdpClient();
				}
				_socketState.RemoteIPEndPoint = ep;
				_socketState.clientSocket.BeginSend(dataBytes, dataBytes.Length, ep, _endSendCallback, _socketState);
				_socketBound = true;
				_socketState.clientSocket.BeginReceive(_endReceiveCallback, _socketState);
			}
			catch (Exception ex)
			{
				ProjectData.SetProjectError(ex);
				Exception ex2 = ex;
				throw ex2;
			}
		}
	}
}
