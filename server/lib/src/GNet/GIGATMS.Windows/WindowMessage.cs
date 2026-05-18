using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualBasic.CompilerServices;

namespace GIGATMS.Windows
{
	[DebuggerNonUserCode]
	public class WindowMessage
	{
		public delegate void WindowProcEventHandler(ref int iResult, int hWnd, int uMsg, int wParam, ref IntPtr lParam);

		public delegate void DeviceArrivalChangeEventHandler(string szDevice);

		public delegate void DeviceRemoveCompleteEventHandler(string szDevice);

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		private struct DEV_BROADCAST_PORT
		{
			public int DBCP_Size;

			public int DBCP_DeviceType;

			public int DBCP_Reserved;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
			public byte[] DBCP_Names;
		}

		private delegate int Win32API_WindowProcDelegate(int hWnd, int uMsg, int wParam, IntPtr lParam);

		public const int WM_USER = 1024;

		public const int WM_DESTROY = 2;

		private const int WM_DEVICECHANGE = 537;

		private const int DBT_DEVTYP_OEM = 0;

		private const int DBT_DEVTYP_DEVNODE = 1;

		private const int DBT_DEVTYP_VOLUME = 2;

		private const int DBT_DEVTYP_PORT = 3;

		private const int DBT_DEVTYP_NET = 4;

		private const int DBT_DEVICEARRIVAL = 32768;

		private const int DBT_DEVICEQUERYREMOVE = 32769;

		private const int DBT_DEVICEQUERYREMOVEFAILED = 32770;

		private const int DBT_DEVICEREMOVEPENDING = 32771;

		private const int DBT_DEVICEREMOVECOMPLETE = 32772;

		private const int DBT_DEVICETYPESPECIFIC = 32773;

		private const int WM_ACTIVATE = 6;

		private const int WM_ACTIVATEAPP = 28;

		private const int GWL_WNDPROC = -4;

		private const long GWLP_WNDPROC = -4L;

		private int m_hWnd;

		private IntPtr m_hPrevProc;

		private IntPtr m_hWndProc;

		private Win32API_WindowProcDelegate m_oWndProc;

		private IntPtr m_hInstance;

		private List<string> m_oRegisterMessageList;

		public int hWnd => m_hWnd;

		[method: DebuggerNonUserCode]
		public event WindowProcEventHandler WindowProc;

		[method: DebuggerNonUserCode]
		public event DeviceArrivalChangeEventHandler DeviceArrivalChange;

		[method: DebuggerNonUserCode]
		public event DeviceRemoveCompleteEventHandler DeviceRemoveComplete;

		[DllImport("user32", CharSet = CharSet.Ansi, EntryPoint = "PostMessageA", ExactSpelling = true, SetLastError = true)]
		private static extern bool Win32API_PostMessage(int hWnd, int uMsg, int wParam, int lParam);

		[DllImport("user32", CharSet = CharSet.Ansi, EntryPoint = "PostMessageA", ExactSpelling = true, SetLastError = true)]
		private static extern bool Win32API_PostMessage(int hWnd, int uMsg, int wParam, IntPtr lParam);

		[DllImport("user32", CharSet = CharSet.Ansi, EntryPoint = "SendMessageA", ExactSpelling = true, SetLastError = true)]
		private static extern int Win32API_SendMessage(int hWnd, int uMsg, int wParam, int lParam);

		[DllImport("user32", CharSet = CharSet.Ansi, EntryPoint = "SendMessageA", ExactSpelling = true, SetLastError = true)]
		private static extern int Win32API_SendMessage(int hWnd, int uMsg, int wParam, IntPtr lParam);

		[DllImport("user32", CharSet = CharSet.Ansi, EntryPoint = "CreateWindowExA", ExactSpelling = true, SetLastError = true)]
		private static extern int Win32API_CreateWindowEx(int dwExStyle, [MarshalAs(UnmanagedType.VBByRefStr)] ref string lpClassName, [MarshalAs(UnmanagedType.VBByRefStr)] ref string lpWindowName, int dwStyle, int x, int y, int nWidth, int nHeight, int hWndParent, int hMenu, int hInstance, int lpParam);

		[DllImport("user32", CharSet = CharSet.Ansi, EntryPoint = "DestroyWindow", ExactSpelling = true, SetLastError = true)]
		private static extern int Win32API_DestroyWindow(int hWnd);

		[DllImport("user32", CharSet = CharSet.Ansi, EntryPoint = "SetWindowLongA", ExactSpelling = true, SetLastError = true)]
		private static extern IntPtr Win32API_SetWindowLong(int hWnd, int nIndex, IntPtr dwNewInteger);

		[DllImport("user32", CharSet = CharSet.Ansi, EntryPoint = "SetWindowLongPtrA", ExactSpelling = true, SetLastError = true)]
		private static extern IntPtr Win32API_SetWindowLongPtr(int hWnd, long nIndex, IntPtr dwNewInteger);

		[DllImport("user32", CharSet = CharSet.Ansi, EntryPoint = "IsWindow", ExactSpelling = true, SetLastError = true)]
		private static extern bool Win32API_IsWindow(int hWnd);

		[DllImport("user32", CharSet = CharSet.Ansi, EntryPoint = "DefWindowProcA", ExactSpelling = true, SetLastError = true)]
		private static extern int Win32API_DefWindowProc(int hWnd, int uMsg, int wParam, IntPtr lParam);

		[DebuggerNonUserCode]
		public WindowMessage()
		{
			m_oWndProc = Win32API_WindowProc;
			m_oRegisterMessageList = new List<string>();
			Module[] modules = Assembly.GetExecutingAssembly().GetModules();
			m_hInstance = Marshal.GetHINSTANCE(modules[0]);
			string lpClassName = "STATIC";
			string lpWindowName = "Windows_Message";
			m_hWnd = Win32API_CreateWindowEx(0, ref lpClassName, ref lpWindowName, 0, 0, 0, 0, 0, 0, 0, m_hInstance.ToInt32(), 0);
			if (Win32API_IsWindow(m_hWnd))
			{
				m_hWndProc = Marshal.GetFunctionPointerForDelegate(m_oWndProc);
				if (IntPtr.Size == 8)
				{
					m_hPrevProc = Win32API_SetWindowLongPtr(m_hWnd, -4L, m_hWndProc);
				}
				else
				{
					m_hPrevProc = Win32API_SetWindowLong(m_hWnd, -4, m_hWndProc);
				}
			}
		}

		protected virtual void Finalize()
		{
			base.Finalize();
			DestroyWindow();
		}

		private void DestroyWindow()
		{
			if (Win32API_IsWindow(m_hWnd))
			{
				if (IntPtr.Size == 8)
				{
					Win32API_SetWindowLongPtr(m_hWnd, -4L, m_hPrevProc);
				}
				else
				{
					Win32API_SetWindowLong(m_hWnd, -4, m_hPrevProc);
				}
				Win32API_DestroyWindow(m_hWnd);
			}
		}

		[DebuggerNonUserCode]
		private int Win32API_WindowProc(int hWnd, int uMsg, int wParam, IntPtr lParam)
		{
			string szDevice = null;
			checked
			{
				switch (uMsg)
				{
				case 537:
					switch (wParam)
					{
					case 32768:
					case 32772:
					{
						object obj = Marshal.PtrToStructure(lParam, typeof(DEV_BROADCAST_PORT));
						DEV_BROADCAST_PORT dEV_BROADCAST_PORT2 = default(DEV_BROADCAST_PORT);
						DEV_BROADCAST_PORT dEV_BROADCAST_PORT = ((obj != null) ? ((DEV_BROADCAST_PORT)obj) : dEV_BROADCAST_PORT2);
						if (dEV_BROADCAST_PORT.DBCP_DeviceType != 3)
						{
							break;
						}
						if (dEV_BROADCAST_PORT.DBCP_Size > 14)
						{
							int num = dEV_BROADCAST_PORT.DBCP_Size - 12;
							int num2 = num - 1;
							int i;
							for (i = 0; i <= num2 && dEV_BROADCAST_PORT.DBCP_Names[i] != 0; i++)
							{
							}
							if (i == 0)
							{
								i = num;
							}
							szDevice = Encoding.Default.GetString(dEV_BROADCAST_PORT.DBCP_Names, 0, i);
						}
						try
						{
							if (wParam == 32768)
							{
								DeviceArrivalChange?.Invoke(szDevice);
							}
							else
							{
								DeviceRemoveComplete?.Invoke(szDevice);
							}
						}
						catch (Exception ex)
						{
							ProjectData.SetProjectError(ex);
							Exception ex2 = ex;
							ProjectData.ClearProjectError();
						}
						break;
					}
					}
					break;
				case 2:
					DestroyWindow();
					break;
				}
				int iResult = 0;
				try
				{
					WindowProc?.Invoke(ref iResult, hWnd, uMsg, wParam, ref lParam);
				}
				catch (Exception ex3)
				{
					ProjectData.SetProjectError(ex3);
					Exception ex4 = ex3;
					ProjectData.ClearProjectError();
				}
				if (iResult == 0 && hWnd == m_hWnd && uMsg < 1024)
				{
					return Win32API_DefWindowProc(hWnd, uMsg, wParam, lParam);
				}
				return 1;
			}
		}

		[DebuggerNonUserCode]
		public int RegisterWindowMessage(string szMessageName)
		{
			bool flag = false;
			int result = -1;
			checked
			{
				int num = m_oRegisterMessageList.Count - 1;
				for (int i = 0; i <= num; i++)
				{
					if (Operators.CompareString(m_oRegisterMessageList[i], szMessageName, TextCompare: false) == 0)
					{
						result = 1025 + i;
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					result = 1025 + m_oRegisterMessageList.Count;
					if (m_oRegisterMessageList.Count < 524287)
					{
						m_oRegisterMessageList.Add(szMessageName);
					}
					else
					{
						result = -1;
					}
				}
				return result;
			}
		}

		public void PostMessage(int uMsg, int wParam, int lParam)
		{
			Win32API_PostMessage(m_hWnd, uMsg, wParam, lParam);
		}

		public void PostMessage(int uMsg, int wParam, IntPtr lParam)
		{
			Win32API_PostMessage(m_hWnd, uMsg, wParam, lParam);
		}

		public void PostMessage(int hWnd, int uMsg, int wParam, int lParam)
		{
			Win32API_PostMessage(hWnd, uMsg, wParam, lParam);
		}

		public void PostMessage(int hWnd, int uMsg, int wParam, IntPtr lParam)
		{
			Win32API_PostMessage(hWnd, uMsg, wParam, lParam);
		}

		public void PostMessage(ref Message oMsg)
		{
			Win32API_PostMessage(m_hWnd, oMsg.Msg, oMsg.WParam.ToInt32(), oMsg.LParam);
		}

		public void SendMessage(int uMsg, int wParam, int lParam)
		{
			Win32API_SendMessage(m_hWnd, uMsg, wParam, lParam);
		}

		public void SendMessage(int uMsg, int wParam, IntPtr lParam)
		{
			Win32API_SendMessage(m_hWnd, uMsg, wParam, lParam);
		}

		public void SendMessage(int hWnd, int uMsg, int wParam, int lParam)
		{
			Win32API_SendMessage(hWnd, uMsg, wParam, lParam);
		}

		public void SendMessage(int hWnd, int uMsg, int wParam, IntPtr lParam)
		{
			Win32API_SendMessage(hWnd, uMsg, wParam, lParam);
		}

		public void SendMessage(ref Message oMsg)
		{
			Win32API_SendMessage(m_hWnd, oMsg.Msg, oMsg.WParam.ToInt32(), oMsg.LParam);
		}
	}
}
