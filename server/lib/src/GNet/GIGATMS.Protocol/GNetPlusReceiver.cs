using System;
using System.Diagnostics;
using GIGATMS.IO;
using Microsoft.VisualBasic.CompilerServices;

namespace GIGATMS.Protocol
{
	[DebuggerNonUserCode]
	public class GNetPlusReceiver : IReceiver
	{
		private ByteBuffer m_oDiscardBuffer;

		private Timeout m_oTimeout;

		private int m_iBufferSize;

		private GNetPlusPackage m_oRxBuffer;

		public int BufferSize
		{
			get
			{
				return m_iBufferSize;
			}
			set
			{
				if (value < 256)
				{
					value = 256;
				}
				else if (value > 10240)
				{
					value = 10240;
				}
				m_iBufferSize = value;
			}
		}

		[method: DebuggerNonUserCode]
		public event IReceiver.OnReceiveToCommandHandler OnReceiveToCommand;

		[method: DebuggerNonUserCode]
		public event IReceiver.OnEventHandler OnEvent;

		public GNetPlusReceiver()
		{
			m_oDiscardBuffer = new ByteBuffer();
			m_oTimeout = new Timeout(500);
			m_iBufferSize = 1024;
			m_oRxBuffer = new GNetPlusPackage();
		}

		public bool AppendReceiveData(ref byte[] bBuffer, int iOffset, int iCount)
		{
			bool result = false;
			checked
			{
				try
				{
					if (m_oRxBuffer.GetSize() > m_iBufferSize)
					{
						m_oRxBuffer.ClearPackage();
					}
					result = true;
					int num = iOffset + iCount - 1;
					byte Value2 = default(byte);
					for (int i = iOffset; i <= num; i++)
					{
						GNetPlusPackage oRxBuffer = m_oRxBuffer;
						if (oRxBuffer.PackageAppend(ref bBuffer[i]))
						{
							FireReceiveEvent();
						}
						else if (oRxBuffer.bReceived)
						{
							if (oRxBuffer.IsFullSize && m_oDiscardBuffer.GetSize() < m_iBufferSize)
							{
								ByteBuffer oDiscardBuffer = m_oDiscardBuffer;
								byte[] Value = oRxBuffer.PackageDiscardHeader();
								oDiscardBuffer.Append(ref Value);
								if (oRxBuffer.bCheckSum)
								{
									FireReceiveEvent();
								}
							}
						}
						else if (m_oDiscardBuffer.GetSize() < m_iBufferSize)
						{
							m_oDiscardBuffer.Append(ref Value2);
						}
						else
						{
							result = false;
						}
						oRxBuffer = null;
					}
				}
				catch (Exception ex)
				{
					ProjectData.SetProjectError(ex);
					Exception ex2 = ex;
					ProjectData.ClearProjectError();
				}
				m_oTimeout.Reset();
				return result;
			}
		}

		bool IReceiver.AppendReceiveData(ref byte[] bBuffer, int iOffset, int iCount)
		{
			//ILSpy generated this explicit interface implementation from .override directive in AppendReceiveData
			return this.AppendReceiveData(ref bBuffer, iOffset, iCount);
		}

		private void FireReceiveEvent()
		{
			byte[] array = null;
			ICommand oCommand = null;
			if (!m_oRxBuffer.bCheckSum)
			{
				return;
			}
			array = m_oRxBuffer.getBuffer();
			try
			{
				IReceiver.OnReceiveToCommandHandler onReceiveToCommandEvent = OnReceiveToCommand;
				if (onReceiveToCommandEvent != null)
				{
					IReceiver oSender = this;
					onReceiveToCommandEvent(ref oSender, ref oCommand);
				}
				if (m_oRxBuffer.QueryFunc == 18 || oCommand == null)
				{
					try
					{
						GNetPlusEvent gNetPlusEvent = new GNetPlusEvent(ref array);
						IReceiver.OnEventHandler onEventEvent = OnEvent;
						if (onEventEvent != null)
						{
							IReceiver oSender = this;
							IEvent oEvent = gNetPlusEvent;
							onEventEvent(ref oSender, ref oEvent);
							gNetPlusEvent = (GNetPlusEvent)oEvent;
						}
						gNetPlusEvent = null;
					}
					catch (Exception ex)
					{
						ProjectData.SetProjectError(ex);
						Exception ex2 = ex;
						ProjectData.ClearProjectError();
					}
				}
				else if (oCommand.State == ICommand.CommandStateConstants.Receiving)
				{
					IReply reply = null;
					try
					{
						reply = new GNetPlusReply(ref array, array.Length);
						oCommand.AddReplyValue(ref reply);
					}
					catch (Exception ex3)
					{
						ProjectData.SetProjectError(ex3);
						Exception ex4 = ex3;
						ProjectData.ClearProjectError();
					}
					reply = null;
				}
			}
			catch (Exception ex5)
			{
				ProjectData.SetProjectError(ex5);
				Exception ex6 = ex5;
				ProjectData.ClearProjectError();
			}
			array = null;
			array = m_oRxBuffer.PackageTaken();
			if (array.Length > 0 && m_oDiscardBuffer.GetSize() < m_iBufferSize)
			{
				m_oDiscardBuffer.Append(ref array);
			}
			if (m_oRxBuffer.bCheckSum)
			{
				FireReceiveEvent();
			}
		}

		public bool DiscardData(ref byte[] bBuffer)
		{
			return m_oDiscardBuffer.Take(ref bBuffer);
		}

		bool IReceiver.DiscardData(ref byte[] bBuffer)
		{
			//ILSpy generated this explicit interface implementation from .override directive in DiscardData
			return this.DiscardData(ref bBuffer);
		}

		public void ReadyForReceive()
		{
			ByteBuffer oDiscardBuffer = m_oDiscardBuffer;
			byte[] Value = m_oRxBuffer.getBuffer();
			oDiscardBuffer.Append(ref Value);
			m_oRxBuffer.ClearPackage();
		}

		void IReceiver.ReadyForReceive()
		{
			//ILSpy generated this explicit interface implementation from .override directive in ReadyForReceive
			this.ReadyForReceive();
		}

		public void CheckTimeout()
		{
			if (m_oRxBuffer.bReceived && m_oTimeout.IsTimeout && m_oDiscardBuffer.GetSize() < m_iBufferSize)
			{
				ByteBuffer oDiscardBuffer = m_oDiscardBuffer;
				byte[] Value = m_oRxBuffer.PackageDiscardHeader();
				oDiscardBuffer.Append(ref Value);
				if (m_oRxBuffer.bCheckSum)
				{
					FireReceiveEvent();
				}
			}
		}

		void IReceiver.CheckTimeout()
		{
			//ILSpy generated this explicit interface implementation from .override directive in CheckTimeout
			this.CheckTimeout();
		}
	}
}
