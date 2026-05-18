using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Microsoft.VisualBasic.CompilerServices;

namespace GIGATMS.Devices.ER750
{
	public class TCPClientSocket
	{
		public delegate void ConnectedEventHandler();

		public delegate void DisconnectedEventHandler();

		public delegate void ErrorOccurredEventHandler(string errorMessage);

		public delegate void DataReceivedEventHandler(ref byte[] dataByte);

		public delegate void SendCompleteEventHandler(int bytesSent);

		public TcpClient m_clientSocket;

		public byte[] m_dataByte;

		private AsyncCallback m_endConnectCallback;

		private AsyncCallback m_endDisconnectCallback;

		private AsyncCallback m_endReceiveCallback;

		private AsyncCallback m_endSendCallback;

		private string m_localHostName;

		private string m_lockString;

		private string _remoteIP;

		private int _remotePort;

		private bool _isConnected;

		public bool IsConnected => m_clientSocket.Client.Connected;

		public string RemoteIP => _remoteIP;

		public int RemotePort => _remotePort;

		public object LocalHostName => m_localHostName;

		public event ConnectedEventHandler Connected;

		public event DisconnectedEventHandler Disconnected;

		public event ErrorOccurredEventHandler ErrorOccurred;

		public event DataReceivedEventHandler DataReceived;

		public event SendCompleteEventHandler SendComplete;

		public TCPClientSocket()
		{
			m_clientSocket = new TcpClient();
			m_dataByte = new byte[2049];
			m_endConnectCallback = EndConnectCallback;
			m_endDisconnectCallback = EndDisconnectCallback;
			m_endReceiveCallback = EndReceiveCallback;
			m_endSendCallback = EndSendCallback;
			m_localHostName = "";
			m_lockString = "LockMe";
		}

		private void EndDisconnectCallback(IAsyncResult ar)
		{
			string procedureName = MethodBase.GetCurrentMethod().Name;
			try
			{
				m_clientSocket.Client.EndDisconnect(ar);
				if (m_clientSocket.Client.Connected)
				{
					m_clientSocket.Client.Disconnect(reuseSocket: true);
				}
				Disconnected?.Invoke();
			}
			catch (SocketException ex)
			{
				ProjectData.SetProjectError(ex);
				SocketException ex2 = ex;
				ErrorOccurred?.Invoke(procedureName + ": " + ex2.Message);
				ProjectData.ClearProjectError();
			}
			catch (Exception ex3)
			{
				ProjectData.SetProjectError(ex3);
				Exception ex4 = ex3;
				ErrorOccurred?.Invoke(procedureName + ": " + ex4.Message);
				ProjectData.ClearProjectError();
			}
		}

		public void CloseConnection()
		{
			string procedureName = MethodBase.GetCurrentMethod().Name;
			try
			{
				m_clientSocket.Client.BeginDisconnect(reuseSocket: true, m_endDisconnectCallback, null);
			}
			catch (Exception ex)
			{
				ProjectData.SetProjectError(ex);
				Exception ex2 = ex;
				ProjectData.ClearProjectError();
			}
		}

		private void EndSendCallback(IAsyncResult ar)
		{
			string procedureName = MethodBase.GetCurrentMethod().Name;
			try
			{
				int bytesSent = m_clientSocket.Client.EndSend(ar);
				SendComplete?.Invoke(bytesSent);
			}
			catch (Exception ex)
			{
				ProjectData.SetProjectError(ex);
				Exception ex2 = ex;
				CloseConnection();
				ErrorOccurred?.Invoke(procedureName + ": " + ex2.Message);
				ProjectData.ClearProjectError();
			}
		}

		public void Send(ref byte[] dataByte)
		{
			string procedureName = MethodBase.GetCurrentMethod().Name;
			try
			{
				m_clientSocket.Client.BeginSend(dataByte, 0, dataByte.Length, SocketFlags.None, m_endSendCallback, null);
			}
			catch (SocketException ex)
			{
				ProjectData.SetProjectError(ex);
				SocketException ex2 = ex;
				ErrorOccurred?.Invoke(procedureName + ": " + ex2.Message);
				CloseConnection();
				ProjectData.ClearProjectError();
			}
			catch (Exception ex3)
			{
				ProjectData.SetProjectError(ex3);
				Exception ex4 = ex3;
				ErrorOccurred?.Invoke(procedureName + ": " + ex4.Message);
				CloseConnection();
				ProjectData.ClearProjectError();
			}
		}

		public void GetLocalIP(ref string[] localIP)
		{
			int ipCount = Dns.GetHostEntry(Dns.GetHostName()).AddressList.Length;
			checked
			{
				localIP = new string[ipCount - 1 + 1];
				int i = 0;
				IPAddress[] addressList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
				foreach (IPAddress ip in addressList)
				{
					localIP[i] = ip.ToString();
					i++;
				}
			}
		}

		private void EndReceiveCallback(IAsyncResult ar)
		{
			string procedureName = MethodBase.GetCurrentMethod().Name;
			try
			{
				int bytesReceived = m_clientSocket.Client.EndReceive(ar);
				if (bytesReceived > 0)
				{
					byte[] dataByte = new byte[checked(bytesReceived - 1 + 1)];
					Array.Copy(m_dataByte, 0, dataByte, 0, bytesReceived);
					m_clientSocket.Client.BeginReceive(m_dataByte, 0, m_dataByte.Length, SocketFlags.None, m_endReceiveCallback, null);
					DataReceived?.Invoke(ref dataByte);
					return;
				}
				if (m_clientSocket.Client.Connected)
				{
					m_clientSocket.Client.Disconnect(reuseSocket: true);
				}
				ErrorOccurred?.Invoke(procedureName + ": The connection may be closed by remote host.");
				Disconnected?.Invoke();
			}
			catch (SocketException ex)
			{
				ProjectData.SetProjectError(ex);
				SocketException e = ex;
				CloseConnection();
				if (e.ErrorCode != 10057)
				{
					ErrorOccurred?.Invoke(procedureName + ": " + e.Message);
				}
				ProjectData.ClearProjectError();
			}
			catch (Exception ex2)
			{
				ProjectData.SetProjectError(ex2);
				Exception e2 = ex2;
				CloseConnection();
				ErrorOccurred?.Invoke(procedureName + ": " + e2.Message);
				ProjectData.ClearProjectError();
			}
		}

		private void EndConnectCallback(IAsyncResult ar)
		{
			string procedureName = MethodBase.GetCurrentMethod().Name;
			try
			{
				m_clientSocket.EndConnect(ar);
				m_clientSocket.Client.BeginReceive(m_dataByte, 0, m_dataByte.Length, SocketFlags.None, m_endReceiveCallback, null);
				Connected?.Invoke();
			}
			catch (SocketException ex)
			{
				ProjectData.SetProjectError(ex);
				SocketException e = ex;
				ErrorOccurred?.Invoke(procedureName + ": " + e.Message);
				ProjectData.ClearProjectError();
			}
			catch (Exception ex2)
			{
				ProjectData.SetProjectError(ex2);
				Exception e2 = ex2;
				ErrorOccurred?.Invoke(procedureName + ": " + e2.Message);
				ProjectData.ClearProjectError();
			}
		}

		public void Connect(string remoteHostIP, int remotePort)
		{
			string procedureName = MethodBase.GetCurrentMethod().Name;
			IPAddress hostIPAddress = null;
			_remoteIP = remoteHostIP;
			_remotePort = _remotePort;
			if (IPAddress.TryParse(remoteHostIP, out hostIPAddress))
			{
				try
				{
					lock (m_lockString)
					{
						if (!m_clientSocket.Client.Connected)
						{
							m_clientSocket.Close();
							m_clientSocket = new TcpClient();
							m_clientSocket.BeginConnect(remoteHostIP, remotePort, m_endConnectCallback, null);
							return;
						}
						throw new Exception("Already connects to host.");
					}
				}
				catch (Exception ex)
				{
					ProjectData.SetProjectError(ex);
					Exception ex2 = ex;
					throw new Exception(procedureName + ": " + ex2.Message);
				}
			}
			throw new Exception(procedureName + ": Invalid IP format");
		}
	}
}
