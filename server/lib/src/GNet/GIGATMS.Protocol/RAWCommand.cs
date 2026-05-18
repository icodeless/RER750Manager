using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using GIGATMS.IO;

namespace GIGATMS.Protocol
{
	[DebuggerNonUserCode]
	public class RAWCommand : ICommand
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

		public RAWCommand(ref IReceiver oReceiver, ref string szName, ref byte[] bCommandBytes, int iTimeout)
		{
			m_bTxBuffer = null;
			m_oTimeout = new GIGATMS.IO.Timeout(500);
			m_oReplyList = new List<IReply>();
			m_oWaitHandle = new ManualResetEvent(initialState: false);
			m_szName = szName;
			if (bCommandBytes != null)
			{
				int num = bCommandBytes.Length;
				if (num > 0)
				{
					m_bTxBuffer = new byte[checked(num - 1 + 1)];
					Array.Copy(bCommandBytes, m_bTxBuffer, num);
				}
			}
			m_oTimeout.Timeout = iTimeout;
			m_oReceiver = oReceiver;
		}

		public RAWCommand(ref IReceiver oReceiver, ref byte[] bCommandBytes, int iTimeout)
		{
			m_bTxBuffer = null;
			m_oTimeout = new GIGATMS.IO.Timeout(500);
			m_oReplyList = new List<IReply>();
			m_oWaitHandle = new ManualResetEvent(initialState: false);
			if (bCommandBytes != null)
			{
				int num = bCommandBytes.Length;
				if (num > 0)
				{
					m_bTxBuffer = new byte[checked(num - 1 + 1)];
					Array.Copy(bCommandBytes, m_bTxBuffer, num);
				}
			}
			m_oTimeout.Timeout = iTimeout;
			m_oReceiver = oReceiver;
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
