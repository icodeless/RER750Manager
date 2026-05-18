using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualBasic.CompilerServices;

namespace GIGATMS
{
	[DebuggerNonUserCode]
	public class MyTCPListener : Component
	{
		public delegate void OnServerStatusChangedHandler(ref object sender, bool bIsServerStarted);

		private IContainer components;

		private TcpListener m_oComm;

		private string m_szCommLock;

		private IPEndPoint m_oListenEndPoint;

		private bool m_bIsServerStarted;

		private List<MyTCPClient> m_oTcpClientList;

		private MyTCPClient.OnErrorHandler m_oClientOnError;

		private MyTCPClient.OnMonitorHandler m_oClientOnMonitor;

		private MyTCPClient.OnDataReceiveHandler m_oClientOnDataReceive;

		private MyTCPClient.OnConnectStatusChangedHandler m_oClientOnConnectStatusChanged;

		private AsyncCallback m_oEndAcceptTcpClient;

		private int m_iMaxConnectionCount;

		private TimerCallback m_oCheckIsConnectedDelegate;

		public int MaxConnectionCount
		{
			[DebuggerHidden]
			get
			{
				return m_iMaxConnectionCount;
			}
			set
			{
				m_iMaxConnectionCount = value;
			}
		}

		public string ListenAddress
		{
			[DebuggerHidden]
			get
			{
				return m_oListenEndPoint.Address.ToString() + ":" + Conversions.ToString(m_oListenEndPoint.Port);
			}
			set
			{
				IPAddress address = null;
				int result = default(int);
				try
				{
					result = 1001;
					int num = value.IndexOf(':');
					if (num > 0)
					{
						if (int.TryParse(value.Substring(checked(num + 1)), out result))
						{
							value = value.Substring(0, num);
						}
						else
						{
							result = 1001;
						}
					}
				}
				catch (Exception ex)
				{
					ProjectData.SetProjectError(ex);
					Exception e = ex;
					fireOnError("MyTCPListener.ListenAddress", e);
					ProjectData.ClearProjectError();
				}
				if (IPAddress.TryParse(value, out address) && (!m_oListenEndPoint.Address.Equals(address) || m_oListenEndPoint.Port != result) && m_oComm != null)
				{
					if (m_bIsServerStarted)
					{
						Close();
					}
					m_oComm.Stop();
					m_oListenEndPoint.Address = address;
					m_oListenEndPoint.Port = result;
					Listen();
				}
			}
		}

		public bool IsServerStarted
		{
			[DebuggerHidden]
			get
			{
				return m_bIsServerStarted;
			}
		}

		[method: DebuggerNonUserCode]
		public event MyTCPClient.OnErrorHandler OnError;

		[method: DebuggerNonUserCode]
		public event MyTCPClient.OnMonitorHandler OnMonitor;

		[method: DebuggerNonUserCode]
		public event MyTCPClient.OnDataReceiveHandler OnDataReceive;

		[method: DebuggerNonUserCode]
		public event MyTCPClient.OnConnectStatusChangedHandler OnConnectStatusChanged;

		[method: DebuggerNonUserCode]
		public event OnServerStatusChangedHandler OnServerStatusChanged;

		[DebuggerNonUserCode]
		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing && components != null)
				{
					components.Dispose();
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		[DebuggerStepThrough]
		private void InitializeComponent()
		{
			components = new Container();
		}

		[DllImport("iphlpapi.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
		private static extern int SendARP(uint DestIP, uint SrcIP, byte[] pMacAddr, ref int PhyAddrLen);

		[DebuggerNonUserCode]
		public MyTCPListener(IContainer container)
			: this()
		{
			container?.Add(this);
		}

		[DebuggerNonUserCode]
		public MyTCPListener()
		{
			m_oComm = null;
			m_szCommLock = "Comm";
			m_oListenEndPoint = new IPEndPoint(IPAddress.Any, 1001);
			m_oTcpClientList = new List<MyTCPClient>();
			m_oClientOnError = OnClientError;
			m_oClientOnMonitor = OnClientMonitor;
			m_oClientOnDataReceive = OnClientDataReceive;
			m_oClientOnConnectStatusChanged = OnClientConnectStatusChanged;
			m_oEndAcceptTcpClient = EndAcceptTcpClient;
			m_iMaxConnectionCount = 0;
			m_oCheckIsConnectedDelegate = null;
			InitializeComponent();
		}

		[DebuggerHidden]
		protected virtual void fireOnError(string source, Exception e)
		{
			MyTCPClient.OnErrorHandler onErrorEvent = OnError;
			if (onErrorEvent != null)
			{
				object sender = this;
				onErrorEvent(ref sender, source, ref e);
			}
		}

		[DebuggerHidden]
		public void Close()
		{
			if (m_oComm == null)
			{
				return;
			}
			checked
			{
				lock (m_szCommLock)
				{
					m_oComm.Stop();
					m_oComm.Server.Close();
					List<MyTCPClient> oTcpClientList = m_oTcpClientList;
					int num = oTcpClientList.Count - 1;
					for (int i = 0; i <= num; i++)
					{
						MyTCPClient myTCPClient = oTcpClientList[i];
						myTCPClient.Close();
						myTCPClient = null;
					}
					oTcpClientList = null;
				}
				m_bIsServerStarted = false;
				OnServerStatusChangedHandler onServerStatusChangedEvent = OnServerStatusChanged;
				if (onServerStatusChangedEvent != null)
				{
					object sender = this;
					onServerStatusChangedEvent(ref sender, m_bIsServerStarted);
				}
			}
		}

		[DebuggerHidden]
		private bool doListen()
		{
			bool result = false;
			try
			{
				if (m_oComm != null)
				{
					m_oComm.Stop();
				}
				m_oComm = new TcpListener(m_oListenEndPoint);
				if (m_iMaxConnectionCount == 0)
				{
					m_oComm.Start();
				}
				else
				{
					m_oComm.Start(m_iMaxConnectionCount);
				}
				m_oComm.BeginAcceptTcpClient(m_oEndAcceptTcpClient, m_oComm);
				m_bIsServerStarted = true;
				result = true;
				OnServerStatusChangedHandler onServerStatusChangedEvent = OnServerStatusChanged;
				if (onServerStatusChangedEvent != null)
				{
					object sender = this;
					onServerStatusChangedEvent(ref sender, m_bIsServerStarted);
				}
			}
			catch (Exception ex)
			{
				ProjectData.SetProjectError(ex);
				Exception e = ex;
				fireOnError("MyTCPListener.doListen()", e);
				ProjectData.ClearProjectError();
			}
			return result;
		}

		[DebuggerHidden]
		public bool Listen()
		{
			lock (m_szCommLock)
			{
				return doListen();
			}
		}

		[DebuggerHidden]
		public bool Listen(int iPort)
		{
			lock (m_szCommLock)
			{
				IPEndPoint oListenEndPoint = m_oListenEndPoint;
				oListenEndPoint.Address = IPAddress.Any;
				oListenEndPoint.Port = iPort;
				oListenEndPoint = null;
				return doListen();
			}
		}

		[DebuggerHidden]
		public bool Listen(string szAddress, int iPort)
		{
			IPAddress address = null;
			lock (m_szCommLock)
			{
				IPEndPoint oListenEndPoint = m_oListenEndPoint;
				bool result = default(bool);
				if (IPAddress.TryParse(szAddress, out address))
				{
					oListenEndPoint.Address = address;
					oListenEndPoint.Port = iPort;
					result = doListen();
				}
				oListenEndPoint = null;
				return result;
			}
		}

		[DebuggerHidden]
		public bool Listen(string szAddress)
		{
			IPAddress address = null;
			int result = default(int);
			try
			{
				result = 1001;
				int num = szAddress.IndexOf(':');
				if (num > 0)
				{
					if (int.TryParse(szAddress.Substring(checked(num + 1)), out result))
					{
						szAddress = szAddress.Substring(0, num);
					}
					else
					{
						result = 1001;
					}
				}
			}
			catch (Exception ex)
			{
				ProjectData.SetProjectError(ex);
				Exception e = ex;
				fireOnError("MyTCPListener.Listen(Address)", e);
				ProjectData.ClearProjectError();
			}
			lock (m_szCommLock)
			{
				IPEndPoint oListenEndPoint = m_oListenEndPoint;
				bool result2 = default(bool);
				if (IPAddress.TryParse(szAddress, out address))
				{
					oListenEndPoint.Address = address;
					oListenEndPoint.Port = result;
					result2 = doListen();
				}
				oListenEndPoint = null;
				return result2;
			}
		}

		[DebuggerHidden]
		private void EndAcceptTcpClient(IAsyncResult ar)
		{
			try
			{
				TcpListener tcpListener = (TcpListener)ar.AsyncState;
				MyTCPClient myTCPClient = new MyTCPClient(tcpListener.EndAcceptTcpClient(ar));
				m_oTcpClientList.Add(myTCPClient);
				myTCPClient.OnError += m_oClientOnError;
				myTCPClient.OnMonitor += m_oClientOnMonitor;
				myTCPClient.OnDataReceive += m_oClientOnDataReceive;
				myTCPClient.OnConnectStatusChanged += m_oClientOnConnectStatusChanged;
				m_oComm.BeginAcceptTcpClient(m_oEndAcceptTcpClient, m_oComm);
			}
			catch (Exception ex)
			{
				ProjectData.SetProjectError(ex);
				Exception e = ex;
				fireOnError("MyTCPListener.EndAcceptTcpClient()", e);
				ProjectData.ClearProjectError();
			}
		}

		[DebuggerHidden]
		public void OnClientError(ref object sender, string source, ref Exception e)
		{
			OnError?.Invoke(ref sender, source, ref e);
		}

		[DebuggerHidden]
		public void OnClientMonitor(ref object sender, MyTCPClient.MonitorConstants iMonitorDataType, ref byte[] bDatas)
		{
			OnMonitor?.Invoke(ref sender, iMonitorDataType, ref bDatas);
		}

		[DebuggerHidden]
		public void OnClientDataReceive(ref object sender, int iBytesToReceive, ref byte[] bDatas)
		{
			OnDataReceive?.Invoke(ref sender, iBytesToReceive, ref bDatas);
		}

		[DebuggerHidden]
		public void OnClientConnectStatusChanged(ref object sender, MyTCPClient.ConnectStatusConstants iStatus, string szConnectTo)
		{
			OnConnectStatusChanged?.Invoke(ref sender, iStatus, szConnectTo);
			try
			{
				if (iStatus == MyTCPClient.ConnectStatusConstants.Disconnected)
				{
					m_oTcpClientList.Remove((MyTCPClient)sender);
				}
			}
			catch (Exception ex)
			{
				ProjectData.SetProjectError(ex);
				Exception e = ex;
				fireOnError("MyTCPListener.OnClientConnectStatusChanged()", e);
				ProjectData.ClearProjectError();
			}
		}

		[DebuggerHidden]
		public bool AsyncSend(ref byte[] bBuffer, int iOffset, int iLength)
		{
			bool result = false;
			checked
			{
				lock (m_szCommLock)
				{
					List<MyTCPClient> oTcpClientList = m_oTcpClientList;
					if (oTcpClientList.Count > 0)
					{
						result = true;
					}
					int num = oTcpClientList.Count - 1;
					for (int i = 0; i <= num; i++)
					{
						MyTCPClient myTCPClient = oTcpClientList[i];
						if (!myTCPClient.AsyncSend(ref bBuffer, iOffset, iLength))
						{
							result = false;
						}
						myTCPClient = null;
					}
					oTcpClientList = null;
					return result;
				}
			}
		}

		[DebuggerHidden]
		public string getListenerIpAddress()
		{
			string hostName = Dns.GetHostName();
			IPHostEntry hostEntry = Dns.GetHostEntry(hostName);
			IPAddress[] addressList = hostEntry.AddressList;
			return addressList[0].ToString();
		}

		[DebuggerHidden]
		public string getListenerMacAddress(string IPAddress)
		{
			byte[] array = new byte[7];
			int PhyAddrLen = array.Length;
			uint destIP = BitConverter.ToUInt32(System.Net.IPAddress.Parse(IPAddress).GetAddressBytes(), 0);
			SendARP(destIP, 0u, array, ref PhyAddrLen);
			return BitConverter.ToString(array, 0, PhyAddrLen);
		}

		[DebuggerHidden]
		public string getListenerIpAddress_IPv4()
		{
			string result = "";
			string hostName = Dns.GetHostName();
			IPAddress[] hostAddresses = Dns.GetHostAddresses(hostName);
			foreach (IPAddress iPAddress in hostAddresses)
			{
				if (iPAddress.IsIPv6LinkLocal)
				{
					result = "IPv6: " + iPAddress.ToString();
				}
				else if (iPAddress.AddressFamily == AddressFamily.InterNetwork)
				{
					result = iPAddress.ToString();
					break;
				}
			}
			return result;
		}
	}
}
