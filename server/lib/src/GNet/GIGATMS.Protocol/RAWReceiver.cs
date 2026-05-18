using System;
using System.Diagnostics;
using GIGATMS.IO;
using Microsoft.VisualBasic.CompilerServices;

namespace GIGATMS.Protocol
{
	[DebuggerNonUserCode]
	public class RAWReceiver : IReceiver
	{
		private ByteBuffer m_oDiscardBuffer;

		private Timeout m_oTimeout;

		private int m_iBufferSize;

		private ByteBuffer m_oRxBuffer;

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

		public RAWReceiver()
		{
			m_oDiscardBuffer = new ByteBuffer();
			m_oTimeout = new Timeout(25);
			m_iBufferSize = 1024;
			m_oRxBuffer = new ByteBuffer();
		}

		public bool AppendReceiveData(ref byte[] bBuffer, int iOffset, int iCount)
		{
			bool result = false;
			try
			{
				int size = m_oRxBuffer.GetSize();
				if (size < m_iBufferSize)
				{
					m_oRxBuffer.Append(ref bBuffer, iOffset, iCount);
					result = true;
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
					oCommand.ResetTimeout();
					if (oCommand.State == ICommand.CommandStateConstants.Receiving)
					{
						IReply reply = null;
						try
						{
							reply = new RAWReply(ref Value, Value.Length);
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
			if (m_oRxBuffer.GetSize() > 0 && m_oTimeout.IsTimeout)
			{
				FireReceiveEvent();
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
