using System;
using System.Diagnostics;
using System.Text;
using Microsoft.VisualBasic.CompilerServices;

namespace GIGATMS.Protocol
{
	[DebuggerNonUserCode]
	public class RAWReply : IReply
	{
		private IReply.ReplyTypeConstants m_iReplyType;

		private byte[] m_bBuffer;

		public byte[] Buffer
		{
			get
			{
				byte[] array = null;
				int num = default(int);
				if (m_bBuffer != null)
				{
					num = m_bBuffer.Length;
				}
				if (num > 0)
				{
					array = new byte[checked(num - 1 + 1)];
					Array.Copy(m_bBuffer, array, num);
				}
				return array;
			}
		}

		public IReply.ReplyTypeConstants ReplyType => m_iReplyType;

		public RAWReply(IReply.ReplyTypeConstants iReplyType)
		{
			m_bBuffer = null;
			m_iReplyType = iReplyType;
		}

		public RAWReply(ref byte[] bBuffer)
		{
			m_bBuffer = null;
			int num = 0;
			if (bBuffer != null)
			{
				num = bBuffer.Length;
			}
			if (num > 0)
			{
				m_bBuffer = new byte[checked(num - 1 + 1)];
				Array.Copy(bBuffer, m_bBuffer, num);
			}
			m_iReplyType = IReply.ReplyTypeConstants.ACK;
		}

		public RAWReply(ref byte[] bBuffer, int iLength)
		{
			m_bBuffer = null;
			int num = 0;
			if (bBuffer != null)
			{
				num = bBuffer.Length;
			}
			if (num > iLength)
			{
				num = iLength;
			}
			if (num > 0)
			{
				try
				{
					m_bBuffer = new byte[checked(num - 1 + 1)];
					Array.Copy(bBuffer, 0, m_bBuffer, 0, num);
				}
				catch (Exception ex)
				{
					ProjectData.SetProjectError(ex);
					Exception ex2 = ex;
					ProjectData.ClearProjectError();
				}
			}
			m_iReplyType = IReply.ReplyTypeConstants.ACK;
		}

		public RAWReply(ref byte[] bBuffer, int iOffset, int iLength)
		{
			m_bBuffer = null;
			int num = 0;
			if (bBuffer != null)
			{
				num = bBuffer.Length;
			}
			checked
			{
				if (iOffset + num > iLength)
				{
					num = iLength;
				}
				if (num > 0)
				{
					try
					{
						m_bBuffer = new byte[num - 1 + 1];
						Array.Copy(bBuffer, iOffset, m_bBuffer, 0, num);
					}
					catch (Exception ex)
					{
						ProjectData.SetProjectError(ex);
						Exception ex2 = ex;
						ProjectData.ClearProjectError();
					}
				}
				m_iReplyType = IReply.ReplyTypeConstants.ACK;
			}
		}

		public int GetReplyValueSize()
		{
			int result = 0;
			if (m_bBuffer != null)
			{
				result = m_bBuffer.Length;
			}
			return result;
		}

		int IReply.GetReplyValueSize()
		{
			//ILSpy generated this explicit interface implementation from .override directive in GetReplyValueSize
			return this.GetReplyValueSize();
		}

		public bool GetReplyValue(ref byte Value)
		{
			if (m_bBuffer != null)
			{
				Value = m_bBuffer[0];
				return true;
			}
			bool result = default(bool);
			return result;
		}

		bool IReply.GetReplyValue(ref byte Value)
		{
			//ILSpy generated this explicit interface implementation from .override directive in GetReplyValue
			return this.GetReplyValue(ref Value);
		}

		public bool GetReplyValue(ref byte[] Values)
		{
			if (m_bBuffer != null)
			{
				Values = new byte[checked(m_bBuffer.Length - 1 + 1)];
				Array.Copy(m_bBuffer, Values, m_bBuffer.Length);
				return true;
			}
			bool result = default(bool);
			return result;
		}

		bool IReply.GetReplyValue(ref byte[] Values)
		{
			//ILSpy generated this explicit interface implementation from .override directive in GetReplyValue
			return this.GetReplyValue(ref Values);
		}

		public bool GetReplyValue(ref int Value)
		{
			if (m_bBuffer != null)
			{
				int num = 4;
				byte[] array = new byte[checked(num - 1 + 1)];
				if (num > m_bBuffer.Length)
				{
					num = m_bBuffer.Length;
				}
				Array.Copy(m_bBuffer, array, num);
				Value = BitConverter.ToInt32(array, 0);
				return true;
			}
			bool result = default(bool);
			return result;
		}

		bool IReply.GetReplyValue(ref int Value)
		{
			//ILSpy generated this explicit interface implementation from .override directive in GetReplyValue
			return this.GetReplyValue(ref Value);
		}

		public bool GetReplyValue(ref long Value)
		{
			if (m_bBuffer != null)
			{
				int num = 8;
				byte[] array = new byte[checked(num - 1 + 1)];
				if (num > m_bBuffer.Length)
				{
					num = m_bBuffer.Length;
				}
				Array.Copy(m_bBuffer, array, num);
				Value = BitConverter.ToInt64(array, 0);
				return true;
			}
			bool result = default(bool);
			return result;
		}

		bool IReply.GetReplyValue(ref long Value)
		{
			//ILSpy generated this explicit interface implementation from .override directive in GetReplyValue
			return this.GetReplyValue(ref Value);
		}

		public bool GetReplyValue(ref short Value)
		{
			if (m_bBuffer != null)
			{
				int num = 2;
				byte[] array = new byte[checked(num - 1 + 1)];
				if (num > m_bBuffer.Length)
				{
					num = m_bBuffer.Length;
				}
				Array.Copy(m_bBuffer, array, num);
				Value = BitConverter.ToInt16(array, 0);
				return true;
			}
			bool result = default(bool);
			return result;
		}

		bool IReply.GetReplyValue(ref short Value)
		{
			//ILSpy generated this explicit interface implementation from .override directive in GetReplyValue
			return this.GetReplyValue(ref Value);
		}

		public bool GetReplyValue(ref string Value)
		{
			if (m_bBuffer != null)
			{
				Value = Encoding.Default.GetString(m_bBuffer);
				return true;
			}
			bool result = default(bool);
			return result;
		}

		bool IReply.GetReplyValue(ref string Value)
		{
			//ILSpy generated this explicit interface implementation from .override directive in GetReplyValue
			return this.GetReplyValue(ref Value);
		}

		public override string ToString()
		{
			string Value = null;
			try
			{
				GetReplyValue(ref Value);
			}
			catch (Exception ex)
			{
				ProjectData.SetProjectError(ex);
				Exception ex2 = ex;
				ProjectData.ClearProjectError();
			}
			return Value;
		}
	}
}
