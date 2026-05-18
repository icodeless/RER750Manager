using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using GIGATMS.IO;
using Microsoft.VisualBasic;

namespace GIGATMS.Protocol
{
	[DebuggerNonUserCode]
	public class GNetPlusCommand : ICommand
	{
		private List<IReply> m_oReplyList;

		private IReceiver m_oReceiver;

		private GIGATMS.IO.Timeout m_oTimeout;

		private EventWaitHandle m_oWaitHandle;

		private string m_szName;

		private object m_oTag;

		private ICommand.CommandStateConstants m_iState;

		private GNetPlusPackage m_oTxBuffer;

		private bool m_bIsMultipleReply;

		public int Timeout
		{
			get
			{
				return m_oTimeout.Timeout;
			}
			set
			{
				m_oTimeout.Timeout = value;
			}
		}

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

		public bool VerifyReplyValue => true;

		public byte[] Buffer
		{
			get
			{
				byte[] array = null;
				if (m_oTxBuffer != null && m_oTxBuffer.bCheckSum)
				{
					int num = m_oTxBuffer.getBuffer().Length;
					array = new byte[checked(num - 1 + 1)];
					Array.Copy(m_oTxBuffer.getBuffer(), array, num);
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
		public GNetPlusCommand(ref IReceiver oReceiver, byte bQueryFunc, byte[] bParam, byte bAddress = 0, int iTimeout = 3000, bool bIsMultipleReply = false)
		{
			m_oReplyList = new List<IReply>();
			m_oTimeout = new GIGATMS.IO.Timeout(30000);
			m_oWaitHandle = new ManualResetEvent(initialState: false);
			string szName = Conversion.Hex(bQueryFunc);
			SetTxBuffer(ref oReceiver, ref szName, ref bQueryFunc, ref bParam, bAddress, iTimeout, bIsMultipleReply);
		}

		[DebuggerNonUserCode]
		public GNetPlusCommand(ref IReceiver oReceiver, string szName, byte bQueryFunc, byte[] bParam, byte bAddress = 0, int iTimeout = 3000, bool bIsMultipleReply = false)
		{
			m_oReplyList = new List<IReply>();
			m_oTimeout = new GIGATMS.IO.Timeout(30000);
			m_oWaitHandle = new ManualResetEvent(initialState: false);
			SetTxBuffer(ref oReceiver, ref szName, ref bQueryFunc, ref bParam, bAddress, iTimeout, bIsMultipleReply);
		}

		private void SetTxBuffer(ref IReceiver oReceiver, ref string szName, ref byte bQueryFunc, ref byte[] bParam, byte bAddress, int iTimeout, bool bIsMultipleReply)
		{
			m_oReceiver = oReceiver;
			m_iState = ICommand.CommandStateConstants.UnknownError;
			m_szName = szName;
			m_oTxBuffer = new GNetPlusPackage();
			GNetPlusPackage oTxBuffer = m_oTxBuffer;
			oTxBuffer.Address = bAddress;
			oTxBuffer.QueryFunc = bQueryFunc;
			oTxBuffer.Datas = bParam;
			oTxBuffer = null;
			m_oTimeout.Timeout = iTimeout;
			m_iState = ICommand.CommandStateConstants.WaitToSend;
		}

		private void OnReceiveToCommand(ref IReceiver oSender, ref ICommand oCommand)
		{
			oCommand = this;
		}

		public bool AsyncSendCommand(ref ISerialPort oSerialPort)
		{
			ICommand.CommandStateConstants iState = ICommand.CommandStateConstants.UnknownError;
			bool flag = default(bool);
			if (m_oTxBuffer.bCheckSum && oSerialPort != null)
			{
				m_oReceiver.ReadyForReceive();
				ISerialPort obj = oSerialPort;
				byte[] bBuffer = m_oTxBuffer.getBuffer();
				flag = obj.WriteBuffer(ref bBuffer, 0, m_oTxBuffer.getBuffer().Length);
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
