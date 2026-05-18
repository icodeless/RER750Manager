using System;
using System.Diagnostics;
using Microsoft.VisualBasic.CompilerServices;

namespace GIGATMS.Protocol
{
	[DebuggerNonUserCode]
	public class GNetPlusReply : IReply
	{
		private IReply.ReplyTypeConstants m_iReplyType;

		private GNetPlusPackage m_oBuffer;

		private byte[] m_bBuffer;

		private bool m_bReverse;

		public IPackage BufferPackage
		{
			get
			{
				if (m_oBuffer.bCheckSum)
				{
					return m_oBuffer;
				}
				return null;
			}
		}

		public byte[] Buffer
		{
			get
			{
				if (m_oBuffer.bCheckSum)
				{
					return m_oBuffer.getBuffer();
				}
				return null;
			}
		}

		public IReply.ReplyTypeConstants ReplyType => m_iReplyType;

		private IReply.ReplyTypeConstants getReplyType(GNetPlusPackage oPackage)
		{
			switch (oPackage.QueryFunc)
			{
			case 6:
				return IReply.ReplyTypeConstants.ACK;
			case 21:
				return IReply.ReplyTypeConstants.NAK;
			default:
				return IReply.ReplyTypeConstants.CheckError;
			}
		}

		[DebuggerNonUserCode]
		public GNetPlusReply(IReply.ReplyTypeConstants iReplyType)
		{
			m_oBuffer = new GNetPlusPackage();
			m_bBuffer = null;
			m_iReplyType = iReplyType;
		}

		[DebuggerNonUserCode]
		public GNetPlusReply(ref GNetPlusPackage oBuffer, bool bReverse = true)
		{
			m_oBuffer = new GNetPlusPackage();
			m_bBuffer = null;
			if (oBuffer.bCheckSum)
			{
				GNetPlusPackage oBuffer2 = m_oBuffer;
				byte[] Value = oBuffer.getBuffer();
				oBuffer2.PackageFill(ref Value);
			}
			m_iReplyType = getReplyType(oBuffer);
			m_bReverse = bReverse;
		}

		[DebuggerNonUserCode]
		public GNetPlusReply(ref byte[] bBuffer, bool bReverse = true)
		{
			m_oBuffer = new GNetPlusPackage();
			m_bBuffer = null;
			int num = 0;
			if (bBuffer != null)
			{
				num = bBuffer.Length;
			}
			if (num > 0)
			{
				m_oBuffer.ClearPackage();
				m_oBuffer.PackageFill(ref bBuffer);
			}
			m_bReverse = bReverse;
			m_iReplyType = getReplyType(m_oBuffer);
		}

		[DebuggerNonUserCode]
		public GNetPlusReply(ref byte[] bBuffer, int iLength, bool bReverse = true)
		{
			m_oBuffer = new GNetPlusPackage();
			m_bBuffer = null;
			int num = 0;
			byte[] array = null;
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
					array = new byte[checked(num - 1 + 1)];
					Array.Copy(bBuffer, 0, array, 0, num);
					m_oBuffer.ClearPackage();
					m_oBuffer.PackageFill(ref array);
				}
				catch (Exception ex)
				{
					ProjectData.SetProjectError(ex);
					Exception ex2 = ex;
					ProjectData.ClearProjectError();
				}
			}
			m_bReverse = bReverse;
			m_iReplyType = getReplyType(m_oBuffer);
		}

		[DebuggerNonUserCode]
		public GNetPlusReply(ref byte[] bBuffer, int iOffset, int iLength, bool bReverse = true)
		{
			m_oBuffer = new GNetPlusPackage();
			m_bBuffer = null;
			int num = 0;
			byte[] array = null;
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
						array = new byte[num - 1 + 1];
						Array.Copy(bBuffer, iOffset, array, 0, num);
						m_oBuffer.ClearPackage();
						m_oBuffer.PackageFill(ref array);
					}
					catch (Exception ex)
					{
						ProjectData.SetProjectError(ex);
						Exception ex2 = ex;
						ProjectData.ClearProjectError();
					}
				}
				m_bReverse = bReverse;
				m_iReplyType = getReplyType(m_oBuffer);
			}
		}

		public int GetReplyValueSize()
		{
			if (m_oBuffer.bCheckSum)
			{
				return m_oBuffer.DataLength;
			}
			int result = default(int);
			return result;
		}

		int IReply.GetReplyValueSize()
		{
			//ILSpy generated this explicit interface implementation from .override directive in GetReplyValueSize
			return this.GetReplyValueSize();
		}

		public bool GetReplyValue(ref byte Value)
		{
			if (m_oBuffer.bCheckSum)
			{
				return m_oBuffer.getData(ref Value);
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
			if (m_oBuffer.bCheckSum)
			{
				return m_oBuffer.getData(ref Values);
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
			if (m_oBuffer.bCheckSum)
			{
				return m_oBuffer.getData(ref Value, m_bReverse);
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
			if (m_oBuffer.bCheckSum)
			{
				return m_oBuffer.getData(ref Value, m_bReverse);
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
			if (m_oBuffer.bCheckSum)
			{
				return m_oBuffer.getData(ref Value, m_bReverse);
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
			if (m_oBuffer.bCheckSum)
			{
				return m_oBuffer.getData(ref Value);
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
