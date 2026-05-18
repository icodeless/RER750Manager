namespace GIGATMS.Protocol
{
	public interface IReceiver
	{
		public delegate void OnReceiveToCommandHandler(ref IReceiver oSender, ref ICommand oCommand);

		public delegate void OnEventHandler(ref IReceiver oSender, ref IEvent oEvent);

		int BufferSize { get; set; }

		event OnReceiveToCommandHandler OnReceiveToCommand;

		event OnEventHandler OnEvent;

		bool AppendReceiveData(ref byte[] bBuffer, int iOffset, int iCount);

		void CheckTimeout();

		bool DiscardData(ref byte[] bBuffer);

		void ReadyForReceive();
	}
}
