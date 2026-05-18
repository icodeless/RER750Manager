namespace GIGATMS.Protocol
{
	public interface IReply
	{
		public enum ReplyTypeConstants
		{
			ACK,
			NAK,
			CheckError,
			Timeout
		}

		byte[] Buffer { get; }

		ReplyTypeConstants ReplyType { get; }

		int GetReplyValueSize();

		bool GetReplyValue(ref byte Value);

		bool GetReplyValue(ref short Value);

		bool GetReplyValue(ref int Value);

		bool GetReplyValue(ref long Value);

		bool GetReplyValue(ref byte[] Values);

		bool GetReplyValue(ref string Value);
	}
}
