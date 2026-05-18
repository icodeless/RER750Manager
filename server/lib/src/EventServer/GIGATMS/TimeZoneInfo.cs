using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Microsoft.Win32;

namespace GIGATMS
{
	[DebuggerDisplay("{_displayName}")]
	public class TimeZoneInfo : IComparer<TimeZoneInfo>
	{
		private struct SYSTEMTIME
		{
			public ushort wYear;

			public ushort wMonth;

			public ushort wDayOfWeek;

			public ushort wDay;

			public ushort wHour;

			public ushort wMinute;

			public ushort wSecond;

			public ushort wMilliseconds;

			[DebuggerHidden]
			public void SetInfo(byte[] info)
			{
				if (info.Length != Marshal.SizeOf(this))
				{
					throw new ArgumentException("Information size is incorrect", "info");
				}
				wYear = BitConverter.ToUInt16(info, 0);
				wMonth = BitConverter.ToUInt16(info, 2);
				wDayOfWeek = BitConverter.ToUInt16(info, 4);
				wDay = BitConverter.ToUInt16(info, 6);
				wHour = BitConverter.ToUInt16(info, 8);
				wMinute = BitConverter.ToUInt16(info, 10);
				wSecond = BitConverter.ToUInt16(info, 12);
				wMilliseconds = BitConverter.ToUInt16(info, 14);
			}
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		private struct TimeZoneInformation
		{
			public int bias;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public string standardName;

			public SYSTEMTIME standardDate;

			public int standardBias;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public string daylightName;

			public SYSTEMTIME daylightDate;

			public int daylightBias;

			[DebuggerHidden]
			public void SetBytes(byte[] info)
			{
				if (info.Length != 44)
				{
					throw new ArgumentException("Information size is incorrect", "info");
				}
				bias = BitConverter.ToInt32(info, 0);
				standardBias = BitConverter.ToInt32(info, 4);
				daylightBias = BitConverter.ToInt32(info, 8);
				byte[] array = new byte[16];
				Array.Copy(info, 12, array, 0, 16);
				standardDate.SetInfo(array);
				Array.Copy(info, 28, array, 0, 16);
				daylightDate.SetInfo(array);
			}
		}

		private string _id;

		private TimeZoneInformation _tzi;

		private string _displayName;

		public string DisplayName
		{
			[DebuggerHidden]
			get
			{
				Refresh();
				return _displayName;
			}
		}

		public string StandardName
		{
			[DebuggerHidden]
			get
			{
				Refresh();
				return _tzi.standardName;
			}
		}

		public static TimeZoneInfo CurrentTimeZone
		{
			[DebuggerHidden]
			get
			{
				return new TimeZoneInfo(TimeZone.CurrentTimeZone.StandardName);
			}
			[DebuggerHidden]
			set
			{
				value.Refresh();
				if (!SetTimeZoneInformation(ref value._tzi))
				{
					throw new Win32Exception();
				}
			}
		}

		public TimeSpan StandardUtcOffset
		{
			[DebuggerHidden]
			get
			{
				Refresh();
				return new TimeSpan(0, checked(-_tzi.bias), 0);
			}
		}

		public byte[] StandardUtcOffsetNumeral
		{
			[DebuggerHidden]
			get
			{
				byte[] array = new byte[2];
				Refresh();
				string text = StandardUtcOffset.ToString();
				int num = Strings.Len(text);
				checked
				{
					if (num == 8)
					{
						int num2 = Conversions.ToInteger(Strings.Mid(text, 1, 2));
						int num3 = Conversions.ToInteger(Strings.Mid(text, 4, 2));
						array[0] = (byte)num2;
						array[1] = (byte)num3;
					}
					else
					{
						int num2 = Conversions.ToInteger(Strings.Mid(text, 1, 3));
						int num3 = Conversions.ToInteger(Strings.Mid(text, 5, 2));
						array[0] = (byte)num2;
						array[1] = (byte)num3;
					}
					return array;
				}
			}
		}

		public string Id
		{
			[DebuggerHidden]
			get
			{
				Refresh();
				return _id;
			}
		}

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern bool SetTimeZoneInformation(ref TimeZoneInformation lpTimeZoneInformation);

		[DebuggerHidden]
		public TimeZoneInfo(string standardName)
		{
			_tzi = default(TimeZoneInformation);
			SetValues(standardName);
		}

		[DebuggerHidden]
		private TimeZoneInfo()
		{
			_tzi = default(TimeZoneInformation);
		}

		[DebuggerHidden]
		public static TimeZoneInfo[] GetTimeZones()
		{
			List<TimeZoneInfo> list = new List<TimeZoneInfo>();
			RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Time Zones", writable: false);
			if (registryKey != null)
			{
				string[] subKeyNames = registryKey.GetSubKeyNames();
				foreach (string id in subKeyNames)
				{
					TimeZoneInfo timeZoneInfo = new TimeZoneInfo();
					timeZoneInfo._id = id;
					timeZoneInfo.SetValues();
					list.Add(timeZoneInfo);
				}
				Sort(list);
				return list.ToArray();
			}
			throw new KeyNotFoundException("Cannot find the windows registry key (Time Zone).");
		}

		[DebuggerHidden]
		public static void Sort(List<TimeZoneInfo> tzInfos)
		{
			tzInfos.Sort(new TimeZoneInfo());
		}

		[DebuggerHidden]
		public static void Sort(TimeZoneInfo[] tzInfos)
		{
			Array.Sort(tzInfos, new TimeZoneInfo());
		}

		[DebuggerHidden]
		public static TimeZoneInfo FromId(string id)
		{
			if (id != null)
			{
				if (Operators.CompareString(id, string.Empty, TextCompare: false) != 0)
				{
					RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Time Zones", writable: false);
					if (registryKey == null)
					{
						throw new KeyNotFoundException("Cannot find the windows registry key (Time Zone).");
					}
					RegistryKey registryKey2 = registryKey.OpenSubKey(id, writable: false);
					if (registryKey2 != null)
					{
						TimeZoneInfo timeZoneInfo = new TimeZoneInfo();
						timeZoneInfo._id = registryKey2.Name;
						timeZoneInfo._displayName = Conversions.ToString(registryKey2.GetValue("Display"));
						timeZoneInfo._tzi.daylightName = Conversions.ToString(registryKey2.GetValue("Dlt"));
						timeZoneInfo._tzi.standardName = Conversions.ToString(registryKey2.GetValue("Std"));
						timeZoneInfo._tzi.SetBytes((byte[])registryKey2.GetValue("Tzi"));
						return timeZoneInfo;
					}
				}
				throw new ArgumentException("Unknown time zone.", "id");
			}
			throw new ArgumentNullException("id", "Value cannot be null.");
		}

		[DebuggerHidden]
		private DateTime CreateDate(int wYear, int wMonth, int wDay, int wDayOfWeek, int wHour, int wMinute, int wSecond, int wMilliseconds)
		{
			if (wDay < 1 || wDay > 5)
			{
				throw new ArgumentOutOfRangeException("wDat", wDay, "The value is out of acceptable range (1 to 5).");
			}
			if (wDayOfWeek < 0 || wDayOfWeek > 6)
			{
				throw new ArgumentOutOfRangeException("wDayOfWeek", wDayOfWeek, "The value is out of acceptable range (0 to 6).");
			}
			int num = DateTime.DaysInMonth(wYear, wMonth);
			DateTime dateTime = new DateTime(wYear, wMonth, 1);
			DateTime dateTime2 = dateTime;
			int dayOfWeek = (int)dateTime2.DayOfWeek;
			int i = 1;
			int num2 = 1;
			checked
			{
				if (dayOfWeek != wDayOfWeek)
				{
					if (wDayOfWeek == 0)
					{
						num2 += 7 - dayOfWeek;
					}
					else if (wDayOfWeek > dayOfWeek)
					{
						num2 += wDayOfWeek - dayOfWeek;
					}
					else if (wDayOfWeek < dayOfWeek)
					{
						num2 = wDayOfWeek + dayOfWeek;
					}
				}
				for (; i < wDay; i++)
				{
					if (num2 > num - 7)
					{
						break;
					}
					num2 += 7;
				}
				return new DateTime(wYear, wMonth, num2, wHour, wMinute, wSecond, wMilliseconds, DateTimeKind.Local);
			}
		}

		[DebuggerHidden]
		private DateTime GetStartDate(TimeZoneInformation tzi, int year)
		{
			if (tzi.daylightDate.wMonth != 0)
			{
				if (tzi.daylightDate.wYear == 0)
				{
					return CreateDate(year, tzi.daylightDate.wMonth, tzi.daylightDate.wDay, tzi.daylightDate.wDayOfWeek, tzi.daylightDate.wHour, tzi.daylightDate.wMinute, tzi.daylightDate.wSecond, tzi.daylightDate.wMilliseconds);
				}
				return new DateTime(tzi.daylightDate.wYear, tzi.daylightDate.wMonth, tzi.daylightDate.wDay, tzi.daylightDate.wHour, tzi.daylightDate.wMinute, tzi.daylightDate.wSecond, tzi.daylightDate.wMilliseconds, DateTimeKind.Local);
			}
			DateTime result = default(DateTime);
			return result;
		}

		[DebuggerHidden]
		private DateTime GetEndDate(TimeZoneInformation tzi, int year)
		{
			if (tzi.standardDate.wMonth != 0)
			{
				if (tzi.standardDate.wYear == 0)
				{
					return CreateDate(year, tzi.standardDate.wMonth, tzi.standardDate.wDay, tzi.standardDate.wDayOfWeek, tzi.standardDate.wHour, tzi.standardDate.wMinute, tzi.standardDate.wSecond, tzi.standardDate.wMilliseconds);
				}
				return new DateTime(tzi.standardDate.wYear, tzi.standardDate.wMonth, tzi.standardDate.wDay, tzi.standardDate.wHour, tzi.standardDate.wMinute, tzi.standardDate.wSecond, tzi.standardDate.wMilliseconds, DateTimeKind.Local);
			}
			DateTime result = default(DateTime);
			return result;
		}

		[DebuggerHidden]
		public void Refresh()
		{
			SetValues();
		}

		[DebuggerHidden]
		private void SetValues()
		{
			RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Time Zones", writable: false);
			if (registryKey != null)
			{
				RegistryKey registryKey2 = registryKey.OpenSubKey(_id, writable: false);
				if (registryKey2 != null)
				{
					_displayName = Conversions.ToString(registryKey2.GetValue("Display"));
					_tzi.daylightName = Conversions.ToString(registryKey2.GetValue("Dlt"));
					_tzi.standardName = Conversions.ToString(registryKey2.GetValue("Std"));
					_tzi.SetBytes((byte[])registryKey2.GetValue("Tzi"));
					return;
				}
				throw new Exception("Unknown time zone.");
			}
			throw new KeyNotFoundException("Cannot find the windows registry key (Time Zone).");
		}

		[DebuggerHidden]
		private void SetValues(string standardName)
		{
			if (standardName != null)
			{
				bool flag = false;
				if (Operators.CompareString(standardName, string.Empty, TextCompare: false) != 0)
				{
					RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Time Zones", writable: false);
					if (registryKey == null)
					{
						throw new KeyNotFoundException("Cannot find the windows registry key (Time Zone).");
					}
					string[] subKeyNames = registryKey.GetSubKeyNames();
					foreach (string text in subKeyNames)
					{
						RegistryKey registryKey2 = registryKey.OpenSubKey(text, writable: false);
						if (Operators.CompareString(Conversions.ToString(registryKey2.GetValue("Std")), standardName, TextCompare: false) == 0)
						{
							_id = text;
							_displayName = Conversions.ToString(registryKey2.GetValue("Display"));
							_tzi.daylightName = Conversions.ToString(registryKey2.GetValue("Dlt"));
							_tzi.standardName = Conversions.ToString(registryKey2.GetValue("Std"));
							_tzi.SetBytes((byte[])registryKey2.GetValue("Tzi"));
							flag = true;
							break;
						}
					}
				}
				if (!flag)
				{
					throw new ArgumentException("Unknown time zone.", "standardName");
				}
				return;
			}
			throw new ArgumentNullException("id", "Value cannot be null.");
		}

		[DebuggerHidden]
		public override string ToString()
		{
			return DisplayName;
		}

		[DebuggerHidden]
		protected virtual int Compare(TimeZoneInfo x, TimeZoneInfo y)
		{
			if (x._tzi.bias == y._tzi.bias)
			{
				return x._displayName.CompareTo(y._displayName);
			}
			if (x._tzi.bias > y._tzi.bias)
			{
				return -1;
			}
			if (x._tzi.bias < y._tzi.bias)
			{
				return 1;
			}
			int result = default(int);
			return result;
		}

		int IComparer<TimeZoneInfo>.Compare(TimeZoneInfo x, TimeZoneInfo y)
		{
			//ILSpy generated this explicit interface implementation from .override directive in Compare
			return this.Compare(x, y);
		}
	}
}
