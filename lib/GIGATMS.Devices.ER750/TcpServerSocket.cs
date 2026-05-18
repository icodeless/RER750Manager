#define DEBUG
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Microsoft.VisualBasic.CompilerServices;

namespace GIGATMS.Devices.ER750
{
	public class TcpServerSocket
	{
		private class SocketState
		{
			public TcpListener ListenerSocket;

			public TcpClient ClientSocket;

			public byte[] ReceivedBytes;

			public SocketState()
			{
				ReceivedBytes = new byte[2049];
			}
		}

		public delegate void ConnectedEventHandler(string remoteIP, int remotePort);

		public delegate void DisconnectedEventHandler(string remoteIP, int remotePort);

		public delegate void ErrorOccurredEventHandler(string clientIP, int remotePort, string errorMessage);

		public delegate void DataReceivedEventHandler(string clientIP, int remotePort, ref byte[] dataByte);

		public delegate void SendCompleteEventHandler(string clientIP, int remotePort, int bytesSent);

		private TcpListener m_listenerSocket;

		private IPEndPoint m_listenEndPoint;

		private List<SocketState> m_clientSocketList;

		private AsyncCallback m_endAcceptTcpClientCallback;

		private AsyncCallback m_endReceiveCallback;

		private AsyncCallback m_endSendCallback;

		private string m_findClientIP;

		private int m_findClientPort;

		private string m_lockString;

		private bool m_isListening;

		public event ConnectedEventHandler Connected;

		public event DisconnectedEventHandler Disconnected;

		public event ErrorOccurredEventHandler ErrorOccurred;

		public event DataReceivedEventHandler DataReceived;

		public event SendCompleteEventHandler SendComplete;

		public TcpServerSocket()
		{
			m_clientSocketList = new List<SocketState>();
			m_endAcceptTcpClientCallback = EndAcceptTcpClientCallback;
			m_endReceiveCallback = EndReceiveCallback;
			m_endSendCallback = EndSendCallback;
			m_lockString = "LockMe";
			m_isListening = false;
		}

		public void GetLocalIP(ref string[] localIPs)
		{
			int ipCount = Dns.GetHostEntry(Dns.GetHostName()).AddressList.Length;
			checked
			{
				localIPs = new string[ipCount - 1 + 1];
				int i = 0;
				IPAddress[] addressList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
				foreach (IPAddress ip in addressList)
				{
					localIPs[i] = ip.ToString();
					i++;
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

		private void EndReceiveCallback(IAsyncResult ar)
		{
			string procedureName = MethodBase.GetCurrentMethod().Name;
			if (!m_isListening)
			{
				return;
			}
			string clientIP = "";
			int clientPort = default(int);
			try
			{
				SocketState ss = (SocketState)ar.AsyncState;
				if (ss.ClientSocket.Client.Connected)
				{
					clientIP = GetEndPointIP(ss.ClientSocket.Client.RemoteEndPoint);
					clientPort = GetEndPointPort(ss.ClientSocket.Client.RemoteEndPoint);
					int receivedDataLength = ss.ClientSocket.Client.EndReceive(ar);
					if (receivedDataLength > 0)
					{
						byte[] buf = new byte[checked(receivedDataLength - 1 + 1)];
						Array.Copy(ss.ReceivedBytes, 0, buf, 0, receivedDataLength);
						try
						{
							DataReceived?.Invoke(clientIP, clientPort, ref buf);
							ss.ClientSocket.Client.BeginReceive(ss.ReceivedBytes, 0, ss.ReceivedBytes.Length, SocketFlags.None, m_endReceiveCallback, ss);
							return;
						}
						catch (Exception ex)
						{
							ProjectData.SetProjectError(ex);
							Exception ex2 = ex;
							ErrorOccurred?.Invoke(clientIP, clientPort, procedureName + ": " + ex2.Message);
							ProjectData.ClearProjectError();
							return;
						}
					}
					ErrorOccurred?.Invoke(clientIP, clientPort, procedureName + ": Abort Receive Data");
					CloseClientSocket(clientIP, clientPort);
				}
				else
				{
					ErrorOccurred?.Invoke(clientIP, clientPort, procedureName + ": Abort Receive Data");
					CloseClientSocket(clientIP, clientPort);
				}
			}
			catch (SocketException ex3)
			{
				ProjectData.SetProjectError(ex3);
				SocketException e = ex3;
				if (e.ErrorCode == 10053)
				{
					ErrorOccurred?.Invoke(clientIP, clientPort, procedureName + ": Client may close the connection. " + e.Message);
					CloseClientSocket(clientIP, clientPort);
				}
				else
				{
					ErrorOccurred?.Invoke(clientIP, clientPort, procedureName + ": Error Code: " + Conversions.ToString(e.ErrorCode) + " " + e.Message);
				}
				ProjectData.ClearProjectError();
			}
			catch (Exception ex4)
			{
				ProjectData.SetProjectError(ex4);
				Exception e2 = ex4;
				ErrorOccurred?.Invoke(clientIP, clientPort, procedureName + ": " + e2.Message);
				ProjectData.ClearProjectError();
			}
		}

		private void EndAcceptTcpClientCallback(IAsyncResult ar)
		{
			string procedureName = MethodBase.GetCurrentMethod().Name;
			if (!m_isListening)
			{
				return;
			}
			SocketState ss = new SocketState();
			string clientIP = "";
			int clientPort = default(int);
			try
			{
				ss.ListenerSocket = (TcpListener)ar.AsyncState;
				ss.ClientSocket = ss.ListenerSocket.EndAcceptTcpClient(ar);
				m_clientSocketList.Add(ss);
				clientIP = GetEndPointIP(ss.ClientSocket.Client.RemoteEndPoint);
				clientPort = GetEndPointPort(ss.ClientSocket.Client.RemoteEndPoint);
				Connected?.Invoke(clientIP, clientPort);
				try
				{
					ss.ClientSocket.Client.BeginReceive(ss.ReceivedBytes, 0, ss.ReceivedBytes.Length, SocketFlags.None, m_endReceiveCallback, ss);
				}
				catch (SocketException ex)
				{
					ProjectData.SetProjectError(ex);
					SocketException e = ex;
					ErrorOccurred?.Invoke(clientIP, clientPort, procedureName + ": " + e.Message);
					ProjectData.ClearProjectError();
				}
				catch (Exception ex2)
				{
					ProjectData.SetProjectError(ex2);
					Exception e2 = ex2;
					ErrorOccurred?.Invoke(clientIP, clientPort, procedureName + ": " + e2.Message);
					ProjectData.ClearProjectError();
				}
				try
				{
					m_listenerSocket.BeginAcceptTcpClient(m_endAcceptTcpClientCallback, m_listenerSocket);
				}
				catch (Exception ex3)
				{
					ProjectData.SetProjectError(ex3);
					Exception ex4 = ex3;
					ErrorOccurred?.Invoke(clientIP, clientPort, procedureName + ": " + ex4.Message);
					ProjectData.ClearProjectError();
				}
			}
			catch (SocketException ex5)
			{
				ProjectData.SetProjectError(ex5);
				SocketException e3 = ex5;
				if (!ss.ListenerSocket.Server.Connected)
				{
					ErrorOccurred?.Invoke(clientIP, clientPort, procedureName + ": Abort client request");
				}
				else
				{
					ErrorOccurred?.Invoke(clientIP, clientPort, procedureName + ": " + e3.Message);
				}
				ProjectData.ClearProjectError();
			}
			catch (Exception ex6)
			{
				ProjectData.SetProjectError(ex6);
				Exception e4 = ex6;
				if (!ss.ListenerSocket.Server.Connected)
				{
					ErrorOccurred?.Invoke(clientIP, clientPort, procedureName + ": Abort client request");
				}
				else
				{
					ErrorOccurred?.Invoke(clientIP, clientPort, procedureName + ": " + e4.Message);
				}
				ProjectData.ClearProjectError();
			}
		}

		public void StartListen(string ipString, int port)
		{
			IPAddress ip = null;
			m_isListening = false;
			if (IPAddress.TryParse(ipString, out ip))
			{
				Debug.Print(ip.ToString());
				m_listenEndPoint = new IPEndPoint(ip, port);
				bool bResult = false;
				try
				{
					if (m_listenerSocket != null)
					{
						m_listenerSocket.Stop();
					}
					m_listenerSocket = new TcpListener(m_listenEndPoint);
					m_listenerSocket.Start();
					m_listenerSocket.BeginAcceptTcpClient(m_endAcceptTcpClientCallback, m_listenerSocket);
					m_isListening = true;
					return;
				}
				catch (Exception ex)
				{
					ProjectData.SetProjectError(ex);
					Exception ex2 = ex;
					throw ex2;
				}
			}
			throw new Exception("Invalid IP format");
		}

		private bool RemoveAllClientSocketStateList(SocketState ss)
		{
			ss.ClientSocket.Client.Disconnect(reuseSocket: false);
			ss.ClientSocket.Client.Close();
			return true;
		}

		public void StopListen()
		{
			try
			{
				if (m_listenerSocket != null)
				{
					lock (m_lockString)
					{
						m_clientSocketList.RemoveAll(RemoveAllClientSocketStateList);
						m_listenerSocket.Stop();
						m_listenerSocket.Server.Close();
						m_isListening = false;
						return;
					}
				}
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
			if (m_isListening)
			{
				string clientIP = "";
				int clientPort = default(int);
				try
				{
					SocketState ss = (SocketState)ar.AsyncState;
					int bytesSent = ss.ClientSocket.Client.EndSend(ar);
					clientIP = GetEndPointIP(ss.ClientSocket.Client.RemoteEndPoint);
					clientPort = GetEndPointPort(ss.ClientSocket.Client.RemoteEndPoint);
					SendComplete?.Invoke(clientIP, clientPort, bytesSent);
				}
				catch (Exception ex)
				{
					ProjectData.SetProjectError(ex);
					Exception ex2 = ex;
					m_findClientIP = clientIP;
					m_clientSocketList.RemoveAll(RemoveSpecifiedClientSocketStateList);
					ErrorOccurred?.Invoke(clientIP, clientPort, procedureName + ": " + ex2.Message);
					ProjectData.ClearProjectError();
				}
			}
		}

		private bool FindSpecifiedClientSocketStateList(SocketState ss)
		{
			string ip = GetEndPointIP(ss.ClientSocket.Client.RemoteEndPoint);
			int port = GetEndPointPort(ss.ClientSocket.Client.RemoteEndPoint);
			if ((Operators.CompareString(ip, m_findClientIP, TextCompare: false) == 0) & ((m_findClientPort == 0) | (port == m_findClientPort)))
			{
				return true;
			}
			return false;
		}

		private bool RemoveSpecifiedClientSocketStateList(SocketState ss)
		{
			string clientIP = GetEndPointIP(ss.ClientSocket.Client.RemoteEndPoint);
			int clientPort = GetEndPointPort(ss.ClientSocket.Client.RemoteEndPoint);
			if ((Operators.CompareString(clientIP, m_findClientIP, TextCompare: false) == 0) & ((m_findClientPort == 0) | (clientPort == m_findClientPort)))
			{
				ss.ClientSocket.Client.Disconnect(reuseSocket: false);
				ss.ClientSocket.Client.Close();
				return true;
			}
			return false;
		}

		private void CloseClientSocket(string clientIP, int clientPort)
		{
			string procedureName = MethodBase.GetCurrentMethod().Name;
			m_findClientIP = clientIP;
			m_findClientPort = clientPort;
			int match = m_clientSocketList.RemoveAll(RemoveSpecifiedClientSocketStateList);
			try
			{
				if (match > 0)
				{
					Disconnected?.Invoke(clientIP, clientPort);
				}
			}
			catch (Exception ex)
			{
				ProjectData.SetProjectError(ex);
				Exception ex2 = ex;
				ErrorOccurred?.Invoke(clientIP, clientPort, procedureName + ": " + ex2.Message);
				ProjectData.ClearProjectError();
			}
		}

		private void CloseClientSocket(string clientIP)
		{
			CloseClientSocket(clientIP, 0);
		}

		public void Send(string clientIP, int clientPort, byte[] dataByte)
		{
			string procedureName = MethodBase.GetCurrentMethod().Name;
			try
			{
				m_findClientIP = clientIP;
				m_findClientPort = clientPort;
				List<SocketState> ssList = m_clientSocketList.FindAll(FindSpecifiedClientSocketStateList);
				if (ssList.Count > 0)
				{
					foreach (SocketState ss in ssList)
					{
						ss.ClientSocket.Client.BeginSend(dataByte, 0, dataByte.Length, SocketFlags.None, m_endSendCallback, ss);
					}
					return;
				}
				throw new Exception($"The selected client IP {clientIP} is not connected.");
			}
			catch (SocketException ex)
			{
				ProjectData.SetProjectError(ex);
				SocketException e = ex;
				if (e.ErrorCode == 10053)
				{
					CloseClientSocket(clientIP, clientPort);
				}
				else
				{
					ErrorOccurred?.Invoke(clientIP, clientPort, procedureName + ": Unknown socket exception: " + e.Message);
				}
				throw e;
			}
			catch (Exception ex2)
			{
				ProjectData.SetProjectError(ex2);
				Exception ex3 = ex2;
				CloseClientSocket(clientIP, clientPort);
				throw ex3;
			}
		}

		public void Send(string clientIP, byte[] dataByte)
		{
			Send(clientIP, 0, dataByte);
		}

		public void Close()
		{
			StopListen();
		}


	}
}
