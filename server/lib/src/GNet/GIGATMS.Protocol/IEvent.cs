namespace GIGATMS.Protocol
{
	public interface IEvent
	{
		int ID { get; }

		byte[] Buffer { get; }

		bool GetEventValue(ref byte Value);

		bool GetEventValue(ref short Value);

		bool GetEventValue(ref int Value);

		bool GetEventValue(ref long Value);

		bool GetEventValue(ref byte[] Values);

		bool GetEventValue(ref string Value);
	}
}
