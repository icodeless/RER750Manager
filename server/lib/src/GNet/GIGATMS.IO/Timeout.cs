using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualBasic.CompilerServices;

namespace GIGATMS.IO
{
	[DebuggerNonUserCode]
	public class Timeout
	{
		private const int DELAY_DOEVENTS_TIME = 200;

		private int m_iBeginTime;

		private int m_iCalcTimeout;

		private int m_iTimeout;

		private int m_iDoEventTimer;

		public int Timeout
		{
			get
			{
				return m_iTimeout;
			}
			set
			{
				m_iBeginTime = Environment.TickCount;
				m_iTimeout = value;
				m_iCalcTimeout = m_iTimeout;
				m_iDoEventTimer = 0;
			}
		}

		public bool IsTimeout
		{
			get
			{
				checked
				{
					bool result = default(bool);
					try
					{
						result = true;
						int tickCount = Environment.TickCount;
						int value;
						if (tickCount < 0)
						{
							if (m_iBeginTime < 0)
							{
								value = (tickCount & 0x7FFFFFFF) - (m_iBeginTime & 0x7FFFFFFF);
							}
							else
							{
								value = (tickCount & 0x7FFFFFFF) - m_iBeginTime;
								value = ((value >= 0) ? int.MaxValue : (value + int.MaxValue));
							}
						}
						else if (m_iBeginTime < 0)
						{
							value = m_iBeginTime & 0x7FFFFFFF;
							value = ((value <= tickCount + 1) ? int.MaxValue : (int.MaxValue - value + 1 + tickCount));
						}
						else
						{
							value = tickCount - m_iBeginTime;
						}
						value = Math.Abs(value);
						if (m_iCalcTimeout > value)
						{
							m_iCalcTimeout -= value;
						}
						else
						{
							m_iCalcTimeout = 0;
						}
						if (m_iCalcTimeout > 0)
						{
							result = false;
						}
						if (value >= 200)
						{
							m_iDoEventTimer = 200;
						}
						else if (m_iDoEventTimer >= 200)
						{
							m_iDoEventTimer = 200;
						}
						else
						{
							m_iDoEventTimer += value;
						}
					}
					catch (Exception ex)
					{
						ProjectData.SetProjectError(ex);
						Exception ex2 = ex;
						ProjectData.ClearProjectError();
					}
					m_iBeginTime = Environment.TickCount;
					return result;
				}
			}
		}

		[DebuggerNonUserCode]
		public Timeout()
		{
			m_iBeginTime = Environment.TickCount;
			m_iTimeout = 1000;
		}

		[DebuggerNonUserCode]
		public Timeout(int iTimeout)
		{
			m_iBeginTime = Environment.TickCount;
			m_iTimeout = iTimeout;
		}

		public void Reset()
		{
			m_iBeginTime = Environment.TickCount;
			m_iCalcTimeout = m_iTimeout;
			m_iDoEventTimer = 0;
		}

		public bool WaitTimeout(bool bIsNonDoEvents = false)
		{
			Thread.Sleep(1);
			bool isTimeout = IsTimeout;
			if (!isTimeout && m_iDoEventTimer >= 200)
			{
				Application.DoEvents();
				if (bIsNonDoEvents)
				{
					m_iBeginTime = Environment.TickCount;
				}
				m_iDoEventTimer = 0;
			}
			return isTimeout;
		}
	}
}
