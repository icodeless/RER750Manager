using System;
using System.Diagnostics;
using GIGATMS.IO;
using Microsoft.VisualBasic.CompilerServices;

namespace GIGATMS.Protocol
{
	[DebuggerNonUserCode]
	public class GNetReceiver : IReceiver
	{
		private const byte STX = 2;

		private const byte CR = 13;

		private ByteBuffer m_oDiscardBuffer;

		private Timeout m_oTimeout;

		private int m_iBufferSize;

		private ByteBuffer m_oRxBuffer;

		private bool m_bIsSTX;

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

		public GNetReceiver()
		{
			m_oDiscardBuffer = new ByteBuffer();
			m_oTimeout = new Timeout(250);
			m_iBufferSize = 1024;
			m_oRxBuffer = new ByteBuffer();
			m_bIsSTX = false;
		}

		public bool AppendReceiveData(ref byte[] bBuffer, int iOffset, int iCount)
		{
			bool result = false;
			checked
			{
				try
				{
					if ((m_bIsSTX && m_oTimeout.IsTimeout) || m_oRxBuffer.GetSize() > m_iBufferSize)
					{
						m_bIsSTX = false;
					}
					result = true;
					int num = iOffset + iCount - 1;
					for (int i = iOffset; i <= num; i++)
					{
						byte Value = bBuffer[i];
						if (Value == 2)
						{
							ByteBuffer oRxBuffer = m_oRxBuffer;
							oRxBuffer.Clear();
							oRxBuffer.Append(ref Value);
							oRxBuffer = null;
							m_bIsSTX = true;
						}
						else if (m_bIsSTX)
						{
							m_oRxBuffer.Append(ref Value);
							if (Value == 13)
							{
								FireReceiveEvent();
								m_bIsSTX = false;
							}
						}
						else if (m_oDiscardBuffer.GetSize() < m_iBufferSize)
						{
							m_oDiscardBuffer.Append(ref Value);
						}
						else
						{
							result = false;
						}
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
			byte[] Value = null;
			ICommand oCommand = null;
			if (!m_oRxBuffer.Take(ref Value))
			{
				return;
			}
			try
			{
				IReceiver.OnReceiveToCommandHandler onReceiveToCommandEvent = OnReceiveToCommand;
				if (onReceiveToCommandEvent != null)
				{
					IReceiver oSender = this;
					onReceiveToCommandEvent(ref oSender, ref oCommand);
				}
				if (oCommand != null)
				{
					if (oCommand.State == ICommand.CommandStateConstants.Receiving)
					{
						IReply reply = null;
						try
						{
							reply = new GNetReply(ref Value, Value.Length, oCommand.VerifyReplyValue);
							oCommand.AddReplyValue(ref reply);
						}
						catch (Exception ex)
						{
							ProjectData.SetProjectError(ex);
							Exception ex2 = ex;
							ProjectData.ClearProjectError();
						}
						reply = null;
					}
				}
				else
				{
					try
					{
						GNetEvent gNetEvent = new GNetEvent(ref Value);
						IReceiver.OnEventHandler onEventEvent = OnEvent;
						if (onEventEvent != null)
						{
							IReceiver oSender = this;
							IEvent oEvent = gNetEvent;
							onEventEvent(ref oSender, ref oEvent);
							gNetEvent = (GNetEvent)oEvent;
						}
						gNetEvent = null;
					}
					catch (Exception ex3)
					{
						ProjectData.SetProjectError(ex3);
						Exception ex4 = ex3;
						ProjectData.ClearProjectError();
					}
				}
			}
			catch (Exception ex5)
			{
				ProjectData.SetProjectError(ex5);
				Exception ex6 = ex5;
				ProjectData.ClearProjectError();
			}
			Value = null;
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

		public void CheckTimeout()
		{
			if (m_bIsSTX && m_oTimeout.IsTimeout)
			{
				m_bIsSTX = false;
			}
		}

		void IReceiver.CheckTimeout()
		{
			//ILSpy generated this explicit interface implementation from .override directive in CheckTimeout
			this.CheckTimeout();
		}

		public void ReadyForReceive()
		{
		}

		void IReceiver.ReadyForReceive()
		{
			//ILSpy generated this explicit interface implementation from .override directive in ReadyForReceive
			this.ReadyForReceive();
		}
	}
}
