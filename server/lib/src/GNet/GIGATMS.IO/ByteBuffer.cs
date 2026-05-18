using System;
using System.Diagnostics;
using System.Text;
using Microsoft.VisualBasic.CompilerServices;

namespace GIGATMS.IO
{
	[DebuggerNonUserCode]
	public class ByteBuffer
	{
		public byte[] Buffer;

		private int m_iLBound;

		private int m_iUBound;

		internal bool m_bLostByte;

		internal ASCIIEncoding m_oEncode;

		internal bool LostByte => m_bLostByte;

		internal int UBound => m_iUBound;

		internal int LBound => m_iLBound;

		internal byte[] Seek
		{
			get
			{
				byte[] Value = null;
				CopyTo(ref Value);
				return Value;
			}
			set
			{
				Clear();
				Append(ref value);
			}
		}

		internal byte Seek
		{
			get
			{
				if (Index >= 0)
				{
					Index = checked(Index + m_iLBound);
					if (Index > m_iUBound)
					{
						Index = -1;
					}
				}
				return Buffer[Index];
			}
			set
			{
				if (Index >= 0)
				{
					Index = checked(Index + m_iLBound);
					if (Index > m_iUBound)
					{
						Index = -1;
					}
				}
				Buffer[Index] = value;
			}
		}

		[DebuggerNonUserCode]
		public ByteBuffer()
		{
			m_iLBound = 0;
			m_iUBound = -1;
			m_bLostByte = false;
			m_oEncode = new ASCIIEncoding();
			Buffer = new byte[65];
		}

		public void Clear()
		{
			lock (m_oEncode)
			{
				m_iLBound = 0;
				m_iUBound = -1;
			}
		}

		public int GetSize()
		{
			if (Buffer == null)
			{
				return 0;
			}
			return checked(m_iUBound - m_iLBound + 1);
		}

		public void AllocMem(int iSize)
		{
			checked
			{
				lock (m_oEncode)
				{
					try
					{
						if (iSize < Buffer.Length)
						{
							Array.Clear(Buffer, 0, Buffer.Length);
							m_iLBound = 0;
							m_iUBound = iSize - 1;
						}
						else
						{
							Buffer = new byte[((iSize + 383) & -256) + 1];
							m_iLBound = 0;
							m_iUBound = iSize - 1;
						}
					}
					catch (Exception ex)
					{
						ProjectData.SetProjectError(ex);
						Exception ex2 = ex;
						m_iLBound = 0;
						m_iUBound = -1;
						ProjectData.ClearProjectError();
					}
				}
			}
		}

		public void FreeMem()
		{
			lock (m_oEncode)
			{
				Buffer = null;
				m_iLBound = 0;
				m_iUBound = -1;
			}
		}

		public void DumpBuffer(int iSize)
		{
			checked
			{
				lock (m_oEncode)
				{
					m_iLBound += iSize;
					if (m_iLBound > m_iUBound)
					{
						m_iLBound = 0;
						m_iUBound = -1;
					}
				}
			}
		}

		public void DumpRevBuffer(int iSize)
		{
			checked
			{
				lock (m_oEncode)
				{
					m_iUBound -= iSize;
					if (m_iLBound > m_iUBound)
					{
						m_iLBound = 0;
						m_iUBound = -1;
					}
				}
			}
		}

		public void ResizeBuffer(int iNewSize)
		{
			lock (m_oEncode)
			{
				m_ResizeBuffer(iNewSize);
			}
		}

		private void m_ResizeBuffer(int iNewSize)
		{
			int num = ((Buffer != null) ? Buffer.Length : 0);
			checked
			{
				if (m_iLBound + iNewSize < num)
				{
					return;
				}
				num = GetSize();
				if (iNewSize < Buffer.Length)
				{
					Array.Copy(Buffer, m_iLBound, Buffer, 0, num);
					m_iLBound = 0;
					m_iUBound = num - 1;
					return;
				}
				byte[] array;
				try
				{
					array = new byte[num + 1];
					Array.Copy(Buffer, m_iLBound, array, 0, num);
					Buffer = new byte[((iNewSize + 383) & -256) + 1];
					Array.Copy(array, 0, Buffer, 0, num);
					m_iLBound = 0;
					m_iUBound = num - 1;
				}
				catch (Exception ex)
				{
					ProjectData.SetProjectError(ex);
					Exception ex2 = ex;
					m_iLBound = 0;
					m_iUBound = -1;
					ProjectData.ClearProjectError();
				}
				array = null;
			}
		}

		public void Append(ref string Value)
		{
			byte[] Value2 = m_oEncode.GetBytes(Value);
			Append(ref Value2, 0, Value2.Length);
		}

		public void Append(ref byte Value)
		{
			byte[] Value2 = new byte[1] { Value };
			Append(ref Value2, 0, 1);
		}

		public void Append(ref byte[] Value)
		{
			if (Value != null)
			{
				Append(ref Value, 0, Value.Length);
			}
		}

		public void Append(ref byte[] Value, int iStart)
		{
			if (Value != null)
			{
				Append(ref Value, iStart, Value.Length);
			}
		}

		public void Append(ref byte[] Value, int iStart, int iLength)
		{
			checked
			{
				lock (m_oEncode)
				{
					if (Value != null)
					{
						int num = ((iStart + iLength <= Value.Length) ? iLength : (Value.Length - iStart));
						ResizeBuffer(GetSize() + num + 1);
						Array.Copy(Value, iStart, Buffer, m_iUBound + 1, num);
						m_iUBound += num;
					}
				}
			}
		}

		public bool Take(ref string Value)
		{
			int size = GetSize();
			Value = "";
			if (size > 0)
			{
				byte[] Value2 = new byte[checked(size - 1 + 1)];
				if (Take(ref Value2, 0, size))
				{
					Value = m_oEncode.GetString(Value2, 0, size);
					return true;
				}
			}
			bool result = default(bool);
			return result;
		}

		public bool Take(ref string Value, int iTakeSize)
		{
			bool result = false;
			int num = GetSize();
			Value = "";
			if (iTakeSize < num)
			{
				num = iTakeSize;
			}
			if (num > 0)
			{
				byte[] Value2 = new byte[checked(num - 1 + 1)];
				if (Take(ref Value2, 0, num))
				{
					Value = m_oEncode.GetString(Value2, 0, num);
					result = true;
				}
			}
			return result;
		}

		public bool Take(ref byte Value)
		{
			byte[] Value2 = new byte[1];
			bool result = Take(ref Value2, 0, 1);
			Value = Value2[0];
			return result;
		}

		public bool Take(ref byte[] Value)
		{
			return Take(ref Value, 0, -1);
		}

		public bool Take(ref byte[] Value, int iStart)
		{
			return Take(ref Value, iStart, -1);
		}

		public bool Take(ref byte[] Value, int iStart, int iLength)
		{
			int num = 0;
			bool result = false;
			checked
			{
				lock (m_oEncode)
				{
					if (Value == null)
					{
						num = GetSize();
						if (num > 0)
						{
							Value = new byte[num - 1 + 1];
						}
					}
					else
					{
						num = Value.Length;
					}
					if (iLength == -1)
					{
						iLength = num;
					}
					if (iStart + iLength > num)
					{
						iLength = num - iStart;
					}
					if (iLength > 0)
					{
						Array.Clear(Value, iStart, iLength);
						if (m_iLBound + iLength - 1 <= m_iUBound)
						{
							Array.Copy(Buffer, m_iLBound, Value, iStart, iLength);
							m_iLBound += iLength;
							if (m_iLBound > m_iUBound)
							{
								m_iLBound = 0;
								m_iUBound = -1;
							}
							result = true;
						}
					}
				}
				return result;
			}
		}

		public bool CopyTo(ref string Value)
		{
			int size = GetSize();
			Value = "";
			if (size > 0)
			{
				byte[] Value2 = new byte[checked(size - 1 + 1)];
				if (CopyTo(ref Value2, 0, size))
				{
					Value = m_oEncode.GetString(Value2, 0, size);
					return true;
				}
			}
			bool result = default(bool);
			return result;
		}

		public bool CopyTo(ref string Value, int iCopySize)
		{
			bool result = false;
			int num = GetSize();
			Value = "";
			if (iCopySize < num)
			{
				num = iCopySize;
			}
			if (num > 0)
			{
				byte[] Value2 = new byte[checked(num - 1 + 1)];
				if (CopyTo(ref Value2, 0, num))
				{
					Value = m_oEncode.GetString(Value2, 0, num);
					result = true;
				}
			}
			return result;
		}

		public bool CopyTo(ref byte Value)
		{
			byte[] Value2 = new byte[1];
			bool result = CopyTo(ref Value2, 0, 1);
			Value = Value2[0];
			return result;
		}

		public bool CopyTo(ref short Value)
		{
			byte[] Value2 = new byte[2];
			bool result = CopyTo(ref Value2, 0, 2);
			Value = BitConverter.ToInt16(Value2, 0);
			return result;
		}

		public bool CopyTo(ref int Value)
		{
			byte[] Value2 = new byte[4];
			bool result = CopyTo(ref Value2, 0, 4);
			Value = BitConverter.ToInt32(Value2, 0);
			return result;
		}

		public bool CopyTo(ref long Value)
		{
			byte[] Value2 = new byte[8];
			bool result = CopyTo(ref Value2, 0, 8);
			Value = BitConverter.ToInt64(Value2, 0);
			return result;
		}

		public bool CopyTo(ref byte[] Value)
		{
			return CopyTo(ref Value, 0, -1);
		}

		public bool CopyTo(ref byte[] Value, int iStart)
		{
			return CopyTo(ref Value, iStart, -1);
		}

		public bool CopyTo(ref byte[] Value, int iStart, int iLength)
		{
			int num = 0;
			bool result = false;
			checked
			{
				lock (m_oEncode)
				{
					if (Value == null)
					{
						num = GetSize();
						if (num > 0)
						{
							Value = new byte[num - 1 + 1];
						}
					}
					else
					{
						num = Value.Length;
					}
					if (iLength == -1)
					{
						iLength = num;
					}
					if (iStart + iLength > num)
					{
						iLength = num - iStart;
					}
					m_bLostByte = true;
					if (iLength > 0)
					{
						Array.Clear(Value, iStart, iLength);
						if (m_iLBound + iLength - 1 <= m_iUBound)
						{
							Array.Copy(Buffer, m_iLBound, Value, iStart, iLength);
							m_bLostByte = false;
							result = true;
						}
						else if (m_iUBound >= m_iLBound)
						{
							Array.Copy(Buffer, m_iLBound, Value, iStart, m_iUBound - m_iLBound + 1);
						}
					}
					else if (iLength == 0)
					{
						m_bLostByte = false;
						result = true;
					}
				}
				return result;
			}
		}
	}
}
