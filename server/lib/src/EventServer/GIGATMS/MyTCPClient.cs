using System;
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
	public class MyTCPClient : Component
	{
		public delegate void OnErrorHandler(ref object sender, string source, ref Exception e);

		public enum MonitorConstants
		{
			Output,
			Input
		}

		public delegate void OnMonitorHandler(ref object sender, MonitorConstants iMonitorDataType, ref byte[] bDatas);

		public delegate void OnDataReceiveHandler(ref object sender, int iBytesToReceive, ref byte[] bDatas);

		public enum ConnectStatusConstants
		{
			Disconnected,
			Connecting,
			Connected
		}

		public delegate void OnConnectStatusChangedHandler(ref object sender, ConnectStatusConstants iStatus, string szConnectTo);

		private IContainer components;

		private ConnectStatusConstants m_iConnectStatus;

		private TcpClient m_oComm;

		private string m_szCommLock;

		private string m_szConnectTo;

		private AsyncCallback m_oEndConnect;

		private AsyncCallback m_oEndDisconnect;

		private AsyncCallback m_oEndSend;

		private AsyncCallback m_oEndReceive;

		private Timer m_oKeepAliveTimer;

		private TimerCallback m_oCheckIsConnectedDelegate;

		private Timer m_oDelayDoConnectTimer;

		private TimerCallback m_oDelayDoConnectDelegate;

		private string m_szRxBufferLock;

		private byte[] m_bRxBuffer;

		private int m_iRxBufferCount;

		private byte[] m_bSocketInBuffer;

		public bool IsConnected
		{
			[DebuggerHidden]
			get
			{
				bool result = false;
				try
				{
					if (m_oComm != null)
					{
						result = m_oComm.Connected;
					}
				}
				catch (SocketException ex)
				{
					ProjectData.SetProjectError(ex);
					SocketException e = ex;
					fireOnError("MyTCPClient.IsConnected", e);
					Close();
					ProjectData.ClearProjectError();
				}
				catch (Exception ex2)
				{
					ProjectData.SetProjectError(ex2);
					Exception e2 = ex2;
					fireOnError("MyTCPClient.IsConnected", e2);
					ProjectData.ClearProjectError();
				}
				return result;
			}
		}

		public string ConnectTo
		{
			[DebuggerHidden]
			get
			{
				return m_szConnectTo;
			}
		}

		[method: DebuggerNonUserCode]
		public event OnErrorHandler OnError;

		[method: DebuggerNonUserCode]
		public event OnMonitorHandler OnMonitor;

		[method: DebuggerNonUserCode]
		public event OnDataReceiveHandler OnDataReceive;

		[method: DebuggerNonUserCode]
		public event OnConnectStatusChangedHandler OnConnectStatusChanged;

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
		public MyTCPClient(IContainer container)
			: this()
		{
			container?.Add(this);
		}

		public MyTCPClient(TcpClient oTcpClient)
			: this()
		{
			m_oComm = oTcpClient;
			EndPoint remoteEndPoint = oTcpClient.Client.RemoteEndPoint;
			m_szConnectTo = remoteEndPoint.ToString();
			remoteEndPoint = null;
			m_oDelayDoConnectTimer = new Timer(m_oDelayDoConnectDelegate, null, 0, 100);
		}

		[DebuggerNonUserCode]
		public MyTCPClient()
		{
			m_iConnectStatus = ConnectStatusConstants.Disconnected;
			m_oComm = null;
			m_szCommLock = "Comm";
			m_szConnectTo = null;
			m_oEndConnect = null;
			m_oEndDisconnect = null;
			m_oEndSend = null;
			m_oEndReceive = null;
			m_oKeepAliveTimer = null;
			m_oCheckIsConnectedDelegate = CheckIsConnected;
			m_oDelayDoConnectTimer = null;
			m_oDelayDoConnectDelegate = DelayDoConnect;
			m_szRxBufferLock = "ReceiveLock";
			m_bRxBuffer = new byte[1024];
			m_iRxBufferCount = 0;
			m_bSocketInBuffer = new byte[5210];
			InitializeComponent();
			m_oEndConnect = EndAsyncConnect;
			m_oEndDisconnect = EndAsyncDisconnect;
			m_oEndSend = EndAsyncSend;
			m_oEndReceive = EndAsyncReceive;
		}

		[DebuggerHidden]
		protected virtual void fireOnError(string source, Exception e)
		{
			OnErrorHandler onErrorEvent = OnError;
			if (onErrorEvent != null)
			{
				object sender = this;
				onErrorEvent(ref sender, source, ref e);
			}
		}

		[DebuggerHidden]
		public void Close()
		{
			try
			{
				if (m_oKeepAliveTimer != null)
				{
					m_oKeepAliveTimer.Dispose();
				}
				m_oKeepAliveTimer = null;
			}
			catch (Exception ex)
			{
				ProjectData.SetProjectError(ex);
				Exception e = ex;
				fireOnError("MyTCPClient.Close()", e);
				ProjectData.ClearProjectError();
			}
			if (m_oComm == null)
			{
				return;
			}
			lock (m_szCommLock)
			{
				try
				{
					if (m_oComm.Client != null)
					{
						Socket client = m_oComm.Client;
						client.Shutdown(SocketShutdown.Both);
						client.Close();
						client = null;
					}
				}
				catch (Exception ex2)
				{
					ProjectData.SetProjectError(ex2);
					Exception e2 = ex2;
					fireOnError("MyTCPClient.Close()", e2);
					ProjectData.ClearProjectError();
				}
				try
				{
					m_oComm.Close();
					m_oComm = null;
					Thread.Sleep(200);
					GC.Collect();
					fireOnConnectStatusChanged(ConnectStatusConstants.Disconnected);
				}
				catch (Exception ex3)
				{
					ProjectData.SetProjectError(ex3);
					Exception e3 = ex3;
					fireOnError("MyTCPClient.Close()", e3);
					ProjectData.ClearProjectError();
				}
			}
		}

		[DebuggerHidden]
		protected virtual void fireOnConnectStatusChanged(ConnectStatusConstants iStatus)
		{
			if (m_iConnectStatus == iStatus)
			{
				return;
			}
			m_iConnectStatus = iStatus;
			try
			{
				OnConnectStatusChangedHandler onConnectStatusChangedEvent = OnConnectStatusChanged;
				if (onConnectStatusChangedEvent != null)
				{
					object sender = this;
					onConnectStatusChangedEvent(ref sender, iStatus, m_szConnectTo);
				}
			}
			catch (Exception ex)
			{
				ProjectData.SetProjectError(ex);
				Exception e = ex;
				fireOnError("MyTCPClient.fireOnConnectStatusChanged()", e);
				ProjectData.ClearProjectError();
			}
		}

		[DebuggerHidden]
		public void AsyncConnect(string szAddress)
		{
			try
			{
				int result = 1001;
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
				Close();
				lock (m_szCommLock)
				{
					m_oComm = new TcpClient();
				}
				m_szConnectTo = szAddress + ":" + Conversions.ToString(result);
				fireOnConnectStatusChanged(ConnectStatusConstants.Connecting);
				m_iRxBufferCount = 0;
				m_oComm.BeginConnect(szAddress, result, m_oEndConnect, null);
			}
			catch (Exception ex)
			{
				ProjectData.SetProjectError(ex);
				Exception e = ex;
				fireOnError("MyTCPClient.AsyncOpen:Connect()", e);
				ProjectData.ClearProjectError();
			}
		}

		[DebuggerHidden]
		protected void DelayDoConnect(object oState)
		{
			doConnect();
			try
			{
				m_oDelayDoConnectTimer.Dispose();
			}
			catch (Exception ex)
			{
				ProjectData.SetProjectError(ex);
				Exception ex2 = ex;
				ProjectData.ClearProjectError();
			}
		}

		[DebuggerHidden]
		private void doConnect()
		{
			try
			{
				if (m_oKeepAliveTimer != null)
				{
					m_oKeepAliveTimer.Dispose();
				}
				m_oKeepAliveTimer = new Timer(m_oCheckIsConnectedDelegate, null, 0, 200);
				beginRead();
				if (m_oComm.Connected)
				{
					fireOnConnectStatusChanged(ConnectStatusConstants.Connected);
				}
				else
				{
					fireOnConnectStatusChanged(ConnectStatusConstants.Disconnected);
				}
				fireOnConnected();
			}
			catch (Exception ex)
			{
				ProjectData.SetProjectError(ex);
				Exception e = ex;
				fireOnError("MyTCPClient.doConnect()", e);
				ProjectData.ClearProjectError();
			}
		}

		[DebuggerHidden]
		private void EndAsyncConnect(IAsyncResult ar)
		{
			try
			{
				m_oComm.EndConnect(ar);
			}
			catch (Exception ex)
			{
				ProjectData.SetProjectError(ex);
				Exception e = ex;
				fireOnError("MyTCPClient.EndAsyncConnect:EndConnect()", e);
				ProjectData.ClearProjectError();
			}
			doConnect();
		}

		[DebuggerHidden]
		protected virtual void fireOnConnected()
		{
		}

		[DebuggerHidden]
		public void AsyncDisconnect()
		{
			try
			{
				if (m_oComm != null)
				{
					m_oComm.Client.BeginDisconnect(reuseSocket: true, m_oEndDisconnect, null);
				}
			}
			catch (Exception ex)
			{
				ProjectData.SetProjectError(ex);
				Exception e = ex;
				fireOnError("MyTCPClient.AsyncDisconnect:BeginDisconnect()", e);
				ProjectData.ClearProjectError();
			}
		}

		[DebuggerHidden]
		protected void CheckIsConnected(object oState)
		{
			byte[] buffer = new byte[1];
			bool flag = false;
			if (m_oComm == null)
			{
				return;
			}
			try
			{
				if (m_oComm.Client != null && m_oComm.Connected && m_oComm.Client.Poll(0, SelectMode.SelectRead) && m_oComm.Client.Receive(buffer, SocketFlags.Peek) == 0)
				{
					flag = true;
				}
			}
			catch (SocketException ex)
			{
				ProjectData.SetProjectError(ex);
				SocketException e = ex;
				flag = true;
				fireOnError("MyTCPClient.CheckIsConnected()", e);
				ProjectData.ClearProjectError();
			}
			if (flag)
			{
				Close();
			}
		}

		[DebuggerHidden]
		private void EndAsyncDisconnect(IAsyncResult ar)
		{
			try
			{
				m_oComm.Client.EndDisconnect(ar);
				if (m_oComm.Connected)
				{
					fireOnConnectStatusChanged(ConnectStatusConstants.Connected);
				}
				else
				{
					fireOnConnectStatusChanged(ConnectStatusConstants.Disconnected);
				}
			}
			catch (Exception ex)
			{
				ProjectData.SetProjectError(ex);
				Exception e = ex;
				fireOnError("MyTCPClient.EndAsyncConnect:EndDisConnect()", e);
				ProjectData.ClearProjectError();
			}
		}

		[DebuggerHidden]
		private void EndAsyncSend(IAsyncResult ar)
		{
			int num = 0;
			try
			{
				num = m_oComm.Client.EndSend(ar);
			}
			catch (SocketException ex)
			{
				ProjectData.SetProjectError(ex);
				SocketException e = ex;
				fireOnError("MyTCPClient.EndAsyncSend()", e);
				Close();
				ProjectData.ClearProjectError();
			}
			catch (Exception ex2)
			{
				ProjectData.SetProjectError(ex2);
				Exception e2 = ex2;
				fireOnError("MyTCPClient.EndAsyncSend()", e2);
				ProjectData.ClearProjectError();
			}
			if (num > 0)
			{
				fireOnSend(num);
			}
		}

		[DebuggerHidden]
		protected virtual void fireOnSend(int iBytesToSend)
		{
		}

		[DebuggerHidden]
		private void beginRead()
		{
			try
			{
				m_oComm.Client.BeginReceive(m_bSocketInBuffer, 0, m_bSocketInBuffer.Length, SocketFlags.None, m_oEndReceive, null);
			}
			catch (SocketException ex)
			{
				ProjectData.SetProjectError(ex);
				SocketException e = ex;
				fireOnError("MyTCPClient.beginRead()", e);
				Close();
				ProjectData.ClearProjectError();
			}
			catch (Exception ex2)
			{
				ProjectData.SetProjectError(ex2);
				Exception e2 = ex2;
				fireOnError("MyTCPClient.beginRead()", e2);
				ProjectData.ClearProjectError();
			}
		}

		[DebuggerHidden]
		private void EndAsyncReceive(IAsyncResult ar)
		{
			try
			{
				int num = m_oComm.Client.EndReceive(ar);
				if (num > 0)
				{
					byte[] bDatas = new byte[checked(num - 1 + 1)];
					Array.Copy(m_bSocketInBuffer, 0, bDatas, 0, num);
					OnMonitorHandler onMonitorEvent = OnMonitor;
					if (onMonitorEvent != null)
					{
						object sender = this;
						onMonitorEvent(ref sender, MonitorConstants.Input, ref bDatas);
					}
					fireOnReceive(num, ref bDatas);
				}
			}
			catch (SocketException ex)
			{
				ProjectData.SetProjectError(ex);
				SocketException e = ex;
				fireOnError("MyTCPClient.EndAsyncConnect:EndReceive()", e);
				Close();
				ProjectData.ClearProjectError();
			}
			catch (Exception ex2)
			{
				ProjectData.SetProjectError(ex2);
				Exception e2 = ex2;
				fireOnError("MyTCPClient.EndAsyncConnect:EndReceive()", e2);
				ProjectData.ClearProjectError();
			}
			beginRead();
		}

		[DebuggerHidden]
		public bool AsyncSend(ref byte[] bBuffer, int iOffset, int iLength)
		{
			bool result = false;
			checked
			{
				try
				{
					if (bBuffer != null)
					{
						m_oComm.Client.BeginSend(bBuffer, iOffset, iLength, SocketFlags.None, m_oEndSend, null);
						result = true;
						if (bBuffer != null)
						{
							if (iOffset != 0 || iLength != bBuffer.Length)
							{
								if (iOffset + iLength > bBuffer.Length)
								{
									iLength = bBuffer.Length - iOffset;
								}
								byte[] bDatas = new byte[iLength - 1 + 1];
								Array.Copy(bBuffer, iOffset, bDatas, 0, iLength);
								OnMonitorHandler onMonitorEvent = OnMonitor;
								if (onMonitorEvent != null)
								{
									OnMonitorHandler onMonitorHandler = onMonitorEvent;
									object sender = this;
									onMonitorHandler(ref sender, MonitorConstants.Output, ref bDatas);
								}
							}
							else
							{
								OnMonitorHandler onMonitorEvent = OnMonitor;
								if (onMonitorEvent != null)
								{
									OnMonitorHandler onMonitorHandler2 = onMonitorEvent;
									object sender = this;
									onMonitorHandler2(ref sender, MonitorConstants.Output, ref bBuffer);
								}
							}
						}
					}
				}
				catch (SocketException ex)
				{
					ProjectData.SetProjectError(ex);
					SocketException e = ex;
					fireOnError("MyTCPClient.AsyncSend()", e);
					Close();
					ProjectData.ClearProjectError();
				}
				catch (Exception ex2)
				{
					ProjectData.SetProjectError(ex2);
					Exception e2 = ex2;
					fireOnError("MyTCPClient.AsyncSend()", e2);
					ProjectData.ClearProjectError();
				}
				return result;
			}
		}

		[DebuggerHidden]
		private byte[] Read(int iLength)
		{
			byte[] array = null;
			lock (m_szRxBufferLock)
			{
				if (m_iRxBufferCount > 0)
				{
					if (iLength > m_iRxBufferCount)
					{
						iLength = m_iRxBufferCount;
					}
					array = new byte[checked(iLength - 1 + 1)];
					Array.Copy(m_bRxBuffer, 0, array, 0, iLength);
					m_iRxBufferCount = 0;
				}
			}
			return array;
		}

		[DebuggerHidden]
		public bool Read(ref byte[] bBuffer, int iOffset, int iLength)
		{
			bool result = false;
			checked
			{
				lock (m_szRxBufferLock)
				{
					if (m_iRxBufferCount > 0)
					{
						if (iOffset + iLength > bBuffer.Length)
						{
							iLength = bBuffer.Length - iOffset;
						}
						if (iLength > m_iRxBufferCount)
						{
							iLength = m_iRxBufferCount;
						}
						Array.Copy(m_bRxBuffer, 0, bBuffer, iOffset, iLength);
						m_iRxBufferCount -= iLength;
						Array.Copy(m_bRxBuffer, iLength, m_bRxBuffer, 0, m_iRxBufferCount);
						result = true;
					}
				}
				return result;
			}
		}

		[DebuggerHidden]
		protected virtual void fireOnReceive(int iBytesToReceive, ref byte[] bBuffer)
		{
			int num = iBytesToReceive;
			checked
			{
				if (m_iRxBufferCount + num >= m_bRxBuffer.Length)
				{
					num = m_bRxBuffer.Length - num;
				}
				if (num > 0)
				{
					Array.Copy(bBuffer, 0, m_bRxBuffer, m_iRxBufferCount, num);
					m_iRxBufferCount += num;
				}
				OnDataReceiveHandler onDataReceiveEvent = OnDataReceive;
				if (onDataReceiveEvent != null)
				{
					object sender = this;
					onDataReceiveEvent(ref sender, iBytesToReceive, ref bBuffer);
				}
			}
		}

		[DebuggerHidden]
		public string getClientMacAddress(string IPAddress)
		{
			byte[] array = new byte[7];
			int PhyAddrLen = array.Length;
			uint destIP = BitConverter.ToUInt32(System.Net.IPAddress.Parse(IPAddress).GetAddressBytes(), 0);
			SendARP(destIP, 0u, array, ref PhyAddrLen);
			return BitConverter.ToString(array, 0, PhyAddrLen);
		}
	}
}
