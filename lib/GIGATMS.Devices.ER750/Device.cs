namespace GIGATMS.Devices.ER750
{
	public abstract class Device
	{
		protected string _connectionString;

		protected int _timeout;

		protected int _retries;

		public string ConnectionString
		{
			get
			{
				return _connectionString;
			}
			set
			{
				_connectionString = value;
			}
		}

		public abstract int Timeout { get; set; }

		public abstract int Retries { get; set; }

		public abstract void Open();

		public abstract void Open(string connectionString);

		public abstract void Close();
	}
}
