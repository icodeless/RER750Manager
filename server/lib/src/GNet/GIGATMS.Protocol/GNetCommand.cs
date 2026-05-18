using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using GIGATMS.IO;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace GIGATMS.Protocol
{
	[DebuggerNonUserCode]
	public class GNetCommand : ICommand
	{
		private const byte STX = 2;

		private const byte CR = 13;

		private string m_szName;

		private object m_oTag;

		private bool m_bIsHaveToCheck;

		private byte[] m_bTxBuffer;

		private ICommand.CommandStateConstants m_iState;

		private bool m_bIsRxCheckSum;

		private GIGATMS.IO.Timeout m_oTimeout;

		private List<IReply> m_oReplyList;

		private bool m_bIsMultipleReply;

		private EventWaitHandle m_oWaitHandle;

		private IReceiver m_oReceiver;

		public string Name => m_szName;

		public ICommand.CommandStateConstants State
		{
			get
			{
				if (m_iState == ICommand.CommandStateConstants.Receiving && m_oTimeout.IsTimeout)
				{
					m_iState = ICommand.CommandStateConstants.Timeout;
				}
				return m_iState;
			}
		}

		public bool VerifyReplyValue => m_bIsRxCheckSum;

		public byte[] Buffer
		{
			get
			{
				byte[] array = null;
				if (m_bTxBuffer != null)
				{
					int num = m_bTxBuffer.Length;
					array = new byte[checked(num - 1 + 1)];
					Array.Copy(m_bTxBuffer, array, num);
				}
				return array;
			}
		}

		public int ReplyValueCount => m_oReplyList.Count;

		public IReply ReplyValue
		{
			get
			{
				IReply result = null;
				lock (m_oReplyList)
				{
					if (m_oReplyList.Count > 0)
					{
						result = m_oReplyList[0];
					}
				}
				return result;
			}
		}

		public IReply this[int Index]
		{
			get
			{
				IReply result = null;
				lock (m_oReplyList)
				{
					if (Index >= 0 && Index < m_oReplyList.Count)
					{
						result = m_oReplyList[Index];
					}
				}
				return result;
			}
		}

		public object Tag
		{
			get
			{
				return m_oTag;
			}
			set
			{
				m_oTag = RuntimeHelpers.GetObjectValue(value);
			}
		}

		public bool IsMultipleReply => m_bIsMultipleReply;

		public EventWaitHandle WaitHandle => m_oWaitHandle;

		public IReceiver Receiver => m_oReceiver;

		[method: DebuggerNonUserCode]
		public event ICommand.OnFinishHandler OnFinish;

		[DebuggerNonUserCode]
		public GNetCommand(ref IReceiver oReceiver, string szCommand, bool bIsTxCheckSum = false, bool bIsRxCheckSum = false, int iTimeout = 3000, bool bIsMultipleReply = false)
		{
			m_bTxBuffer = null;
			m_oTimeout = new GIGATMS.IO.Timeout(3000);
			m_oReplyList = new List<IReply>();
			m_oWaitHandle = new ManualResetEvent(initialState: false);
			SetTxBuffer(ref oReceiver, ref szCommand, ref szCommand, bIsTxCheckSum, bIsRxCheckSum, iTimeout, bIsMultipleReply);
		}

		[DebuggerNonUserCode]
		public GNetCommand(ref IReceiver oReceiver, string szName, string szCommand, bool bIsTxCheckSum = false, bool bIsRxCheckSum = false, int iTimeout = 3000, bool bIsMultipleReply = false)
		{
			m_bTxBuffer = null;
			m_oTimeout = new GIGATMS.IO.Timeout(3000);
			m_oReplyList = new List<IReply>();
			m_oWaitHandle = new ManualResetEvent(initialState: false);
			SetTxBuffer(ref oReceiver, ref szName, ref szCommand, bIsTxCheckSum, bIsRxCheckSum, iTimeout, bIsMultipleReply);
		}

		private void SetTxBuffer(ref IReceiver oReceiver, ref string szName, ref string szCommand, bool bIsTxCheckSum, bool bIsRxCheckSum, int iTimeout, bool bIsMultipleReply)
		{
			byte[] bBuffer = null;
			m_oReceiver = oReceiver;
			m_iState = ICommand.CommandStateConstants.UnknownError;
			m_szName = szName;
			if (szCommand != null)
			{
				bBuffer = Encoding.Default.GetBytes(szCommand);
			}
			if (bBuffer == null)
			{
				return;
			}
			long num = bBuffer.Length;
			m_bIsRxCheckSum = bIsRxCheckSum;
			m_oTimeout.Timeout = iTimeout;
			checked
			{
				int num2 = default(int);
				if (bIsTxCheckSum)
				{
					num2 = CalcBufferCheckSum(ref bBuffer, 0, (int)num);
					m_bTxBuffer = new byte[(int)(num + 4 - 1) + 1];
				}
				else
				{
					m_bTxBuffer = new byte[(int)(num + 2 - 1) + 1];
				}
				m_bTxBuffer[0] = 2;
				Array.Copy(bBuffer, 0L, m_bTxBuffer, 1L, num);
				if (bIsTxCheckSum)
				{
					byte b = (byte)((num2 >> 4) & 0xF);
					int num3 = (int)(1 + num);
					int num4 = (int)(1 + num + 2 - 1);
					for (int i = num3; i <= num4; i++)
					{
						b = ((b <= 9) ? ((byte)(b + 48)) : ((byte)(b + 55)));
						m_bTxBuffer[i] = b;
						b = (byte)(num2 & 0xF);
					}
				}
				m_bTxBuffer[Information.UBound(m_bTxBuffer)] = 13;
				m_iState = ICommand.CommandStateConstants.WaitToSend;
			}
		}

		private void OnReceiveToCommand(ref IReceiver oSender, ref ICommand oCommand)
		{
			oCommand = this;
		}

		public bool AsyncSendCommand(ref ISerialPort oSerialPort)
		{
			ICommand.CommandStateConstants iState = ICommand.CommandStateConstants.UnknownError;
			bool flag = default(bool);
			if (m_bTxBuffer != null && oSerialPort != null)
			{
				flag = oSerialPort.WriteBuffer(ref m_bTxBuffer, 0, m_bTxBuffer.Length);
				if (flag)
				{
					m_oTimeout.Reset();
					iState = ICommand.CommandStateConstants.Receiving;
				}
			}
			m_iState = iState;
			return flag;
		}

		bool ICommand.AsyncSendCommand(ref ISerialPort oSerialPort)
		{
			//ILSpy generated this explicit interface implementation from .override directive in AsyncSendCommand
			return this.AsyncSendCommand(ref oSerialPort);
		}

		public void ResetTimeout()
		{
			m_oTimeout.Reset();
		}

		void ICommand.ResetTimeout()
		{
			//ILSpy generated this explicit interface implementation from .override directive in ResetTimeout
			this.ResetTimeout();
		}

		private static int CalcBufferCheckSum(ref byte[] bBuffer, int iOffice, int iCount)
		{
			checked
			{
				int num2 = default(int);
				try
				{
					int num = iOffice + iCount - 1;
					for (int i = iOffice; i <= num; i++)
					{
						num2 = (num2 + bBuffer[i]) & 0xFF;
					}
				}
				catch (Exception ex)
				{
					ProjectData.SetProjectError(ex);
					Exception ex2 = ex;
					num2 = -1;
					ProjectData.ClearProjectError();
				}
				return num2;
			}
		}

		public void Finish()
		{
			switch (m_iState)
			{
			case ICommand.CommandStateConstants.WaitToSend:
			case ICommand.CommandStateConstants.Receiving:
				return;
			}
			ICommand.OnFinishHandler onFinishEvent = OnFinish;
			if (onFinishEvent != null)
			{
				ICommand oSender = this;
				onFinishEvent(ref oSender);
			}
		}

		void ICommand.Finish()
		{
			//ILSpy generated this explicit interface implementation from .override directive in Finish
			this.Finish();
		}

		public void AddReplyValue(ICommand.CommandStateConstants iCommandState)
		{
			lock (m_oReplyList)
			{
				m_iState = iCommandState;
			}
		}

		void ICommand.AddReplyValue(ICommand.CommandStateConstants iCommandState)
		{
			//ILSpy generated this explicit interface implementation from .override directive in AddReplyValue
			this.AddReplyValue(iCommandState);
		}

		public void AddReplyValue(ref IReply oReply)
		{
			lock (m_oReplyList)
			{
				if (oReply.ReplyType == IReply.ReplyTypeConstants.Timeout)
				{
					if (m_bIsMultipleReply)
					{
						GIGATMS.IO.Timeout oTimeout = m_oTimeout;
						if (oTimeout.IsTimeout)
						{
							switch (m_iState)
							{
							case ICommand.CommandStateConstants.WaitToSend:
							case ICommand.CommandStateConstants.Receiving:
								m_iState = ICommand.CommandStateConstants.Timeout;
								break;
							}
						}
						else
						{
							oTimeout.Reset();
						}
						oTimeout = null;
					}
					else
					{
						m_iState = ICommand.CommandStateConstants.Timeout;
					}
					return;
				}
				m_oReplyList.Add(oReply);
				switch (oReply.ReplyType)
				{
				case IReply.ReplyTypeConstants.ACK:
					m_iState = ICommand.CommandStateConstants.ACK;
					break;
				case IReply.ReplyTypeConstants.NAK:
					m_iState = ICommand.CommandStateConstants.NAK;
					break;
				case IReply.ReplyTypeConstants.CheckError:
					m_iState = ICommand.CommandStateConstants.CheckError;
					break;
				}
				m_oTimeout.Reset();
			}
		}

		void ICommand.AddReplyValue(ref IReply oReply)
		{
			//ILSpy generated this explicit interface implementation from .override directive in AddReplyValue
			this.AddReplyValue(ref oReply);
		}
	}
}
