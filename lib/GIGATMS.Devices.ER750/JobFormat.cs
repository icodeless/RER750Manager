using System.Runtime.CompilerServices;

namespace GIGATMS.Devices.ER750
{
	public class JobFormat
	{
		private E_Jobs _job;

		private object _parameter;

		public E_Jobs Job
		{
			get
			{
				return _job;
			}
			set
			{
				_job = value;
			}
		}

		public object Parameter
		{
			get
			{
				return _parameter;
			}
			set
			{
				_parameter = RuntimeHelpers.GetObjectValue(value);
			}
		}

		public JobFormat(E_Jobs job, object parameter)
		{
			_job = job;
			_parameter = RuntimeHelpers.GetObjectValue(parameter);
		}

		public JobFormat(E_Jobs job)
		{
			_job = job;
		}
	}
}
