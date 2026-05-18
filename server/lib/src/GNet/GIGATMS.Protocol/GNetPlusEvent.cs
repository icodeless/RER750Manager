using System;
using System.Diagnostics;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace GIGATMS.Protocol
{
	[DebuggerNonUserCode]
	public class GNetPlusEvent : IEvent
	{
		private GNetPlusReply m_oReply;

		public byte[] Buffer => m_oReply.Buffer;

		public int ID
		{
			get
			{
				string Value = null;
				int result = -1;
				if (m_oReply.GetReplyValue(ref Value))
				{
					if (Strings.Len(Value) > 8)
					{
						Value = Strings.Mid(Value, 1, 8);
					}
					try
					{
						result = Conversions.ToInteger("&H" + Value);
					}
					catch (Exception ex)
					{
						ProjectData.SetProjectError(ex);
						Exception ex2 = ex;
						result = -1;
						ProjectData.ClearProjectError();
					}
				}
				return result;
			}
		}

		[DebuggerNonUserCode]
		public GNetPlusEvent(ref GNetPlusPackage oBuffer)
		{
			m_oReply = new GNetPlusReply(ref oBuffer);
		}

		[DebuggerNonUserCode]
		public GNetPlusEvent(ref byte[] bBuffer)
		{
			if (bBuffer != null)
			{
				int num = bBuffer.Length;
			}
			m_oReply = new GNetPlusReply(ref bBuffer);
		}

		[DebuggerNonUserCode]
		public GNetPlusEvent(ref byte[] bBuffer, int iLength)
		{
			m_oReply = new GNetPlusReply(ref bBuffer, iLength, bReverse: false);
		}

		public override string ToString()
		{
			return m_oReply.ToString();
		}

		public bool GetEventValue(ref byte Value)
		{
			return m_oReply.GetReplyValue(ref Value);
		}

		bool IEvent.GetEventValue(ref byte Value)
		{
			//ILSpy generated this explicit interface implementation from .override directive in GetEventValue
			return this.GetEventValue(ref Value);
		}

		public bool GetEventValue(ref byte[] Values)
		{
			return m_oReply.GetReplyValue(ref Values);
		}

		bool IEvent.GetEventValue(ref byte[] Values)
		{
			//ILSpy generated this explicit interface implementation from .override directive in GetEventValue
			return this.GetEventValue(ref Values);
		}

		public bool GetEventValue(ref int Value)
		{
			return m_oReply.GetReplyValue(ref Value);
		}

		bool IEvent.GetEventValue(ref int Value)
		{
			//ILSpy generated this explicit interface implementation from .override directive in GetEventValue
			return this.GetEventValue(ref Value);
		}

		public bool GetEventValue(ref long Value)
		{
			return m_oReply.GetReplyValue(ref Value);
		}

		bool IEvent.GetEventValue(ref long Value)
		{
			//ILSpy generated this explicit interface implementation from .override directive in GetEventValue
			return this.GetEventValue(ref Value);
		}

		public bool GetEventValue(ref short Value)
		{
			return m_oReply.GetReplyValue(ref Value);
		}

		bool IEvent.GetEventValue(ref short Value)
		{
			//ILSpy generated this explicit interface implementation from .override directive in GetEventValue
			return this.GetEventValue(ref Value);
		}

		public bool GetEventValue(ref string Value)
		{
			return m_oReply.GetReplyValue(ref Value);
		}

		bool IEvent.GetEventValue(ref string Value)
		{
			//ILSpy generated this explicit interface implementation from .override directive in GetEventValue
			return this.GetEventValue(ref Value);
		}
	}
}
