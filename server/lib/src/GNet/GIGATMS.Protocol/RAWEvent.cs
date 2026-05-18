using System.Diagnostics;

namespace GIGATMS.Protocol
{
	[DebuggerNonUserCode]
	public class RAWEvent : IEvent
	{
		protected RAWReply m_oReply;

		public byte[] Buffer => m_oReply.Buffer;

		public int ID
		{
			get
			{
				int Value = -1;
				m_oReply.GetReplyValue(ref Value);
				return Value;
			}
		}

		public RAWEvent(ref byte[] bBuffer)
		{
			m_oReply = new RAWReply(ref bBuffer);
		}

		public RAWEvent(ref byte[] bBuffer, int iLength)
		{
			m_oReply = new RAWReply(ref bBuffer, 0, iLength);
		}

		public RAWEvent(ref byte[] bBuffer, int iOffset, int iLength)
		{
			m_oReply = new RAWReply(ref bBuffer, iOffset, iLength);
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
