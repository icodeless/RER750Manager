using System.Threading;
using GIGATMS.IO;

namespace GIGATMS.Protocol
{
	public interface ICommand
	{
		public enum CommandStateConstants
		{
			WaitToSend,
			Receiving,
			ACK,
			NAK,
			CheckError,
			Timeout,
			PortIsClosed,
			Cancel,
			UnknownError
		}

		public delegate void OnFinishHandler(ref ICommand oSender);

		bool VerifyReplyValue { get; }

		byte[] Buffer { get; }

		int ReplyValueCount { get; }

		IReply ReplyValue { get; }

		IReply this[int index] { get; }

		bool IsMultipleReply { get; }

		CommandStateConstants State { get; }

		string Name { get; }

		EventWaitHandle WaitHandle { get; }

		IReceiver Receiver { get; }

		object Tag { get; set; }

		event OnFinishHandler OnFinish;

		bool AsyncSendCommand(ref ISerialPort oSerialPort);

		void AddReplyValue(ref IReply oReply);

		void AddReplyValue(CommandStateConstants iCommandState);

		void Finish();

		void ResetTimeout();
	}
}
