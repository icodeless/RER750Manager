using System;
using System.Diagnostics;
using System.Text;
using Microsoft.VisualBasic.CompilerServices;

namespace GIGATMS.Protocol
{
	[DebuggerNonUserCode]
	public class GNetReply : IReply
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

		[DebuggerNonUserCode]
		public GNetReply(IReply.ReplyTypeConstants iReplyType)
		{
			m_bBuffer = null;
			m_iReplyType = iReplyType;
		}

		[DebuggerNonUserCode]
		public GNetReply(ref byte[] bBuffer)
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

		[DebuggerNonUserCode]
		public GNetReply(ref byte[] bBuffer, int iLength)
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

		[DebuggerNonUserCode]
		public GNetReply(ref byte[] bBuffer, int iOffset, int iLength)
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

		[DebuggerNonUserCode]
		public GNetReply(ref byte[] bBuffer, int iLength, bool bIsRxCheck)
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
			if ((bIsRxCheck && num < 6) || num < 2)
			{
				m_iReplyType = IReply.ReplyTypeConstants.CheckError;
				return;
			}
			checked
			{
				switch (bBuffer[1])
				{
				case 65:
					m_iReplyType = IReply.ReplyTypeConstants.ACK;
					if (bIsRxCheck)
					{
						int num2 = 0;
						int num3 = num - 3;
						int num4 = num - 2;
						for (int i = num3; i <= num4; i++)
						{
							byte b = bBuffer[i];
							byte b2 = b;
							if (unchecked((uint)b2 >= 48u && (uint)b2 <= 57u))
							{
								b -= 48;
							}
							else if (unchecked((uint)b2 >= 65u && (uint)b2 <= 70u))
							{
								b -= 55;
							}
							else
							{
								if (unchecked((uint)b2 < 97u || (uint)b2 > 102u))
								{
									num2 = -1;
									break;
								}
								b -= 87;
							}
							num2 = (num2 << 4) | b;
						}
						if (num2 != -1 && num2 == CalcBufferCheckSum(ref bBuffer, 1, num - 4))
						{
							num -= 5;
							break;
						}
						num -= 3;
						m_iReplyType = IReply.ReplyTypeConstants.CheckError;
					}
					else
					{
						num -= 3;
					}
					break;
				case 78:
					m_iReplyType = IReply.ReplyTypeConstants.NAK;
					num -= 3;
					break;
				case 66:
				case 67:
				case 68:
				case 69:
				case 70:
				case 71:
				case 72:
				case 73:
				case 74:
				case 75:
				case 76:
				case 77:
				case 79:
				case 80:
				case 81:
				case 82:
				case 83:
				case 84:
				case 85:
				case 86:
				case 87:
				case 88:
				case 89:
				case 90:
					m_iReplyType = IReply.ReplyTypeConstants.NAK;
					num -= 2;
					break;
				default:
					m_iReplyType = IReply.ReplyTypeConstants.CheckError;
					num = 0;
					break;
				}
				if (num > 0)
				{
					m_bBuffer = new byte[num - 1 + 1];
					Array.Copy(bBuffer, 2, m_bBuffer, 0, num);
				}
			}
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
