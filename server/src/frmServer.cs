// VBConversions Note: VB project level imports
using System.Diagnostics;
using System;
using System.Windows.Forms;
using System.Collections;
using System.Drawing;
using Microsoft.VisualBasic;
using System.Data;
using System.Collections.Generic;
// End of VB project level imports

using VB = Microsoft.VisualBasic;

namespace EthernetReader
{
	
	public partial class frmMain
	{
		public frmMain()
		{
			// VBConversions Note: Non-static class variable initialization is below.  Class variables cannot be initially assigned non-static values in C#.
			m_iTimeZoneOffset = GIGATMS.TimeZoneInfo.CurrentTimeZone.StandardUtcOffset;
			//m_oOnDataReceiveDelegate = new OnDataReceiveDelegate(OnDataReceive);
			
			InitializeComponent();
			
			//Added to support default instance behavour in C#
			if (defaultInstance == null)
				defaultInstance = this;
		}
		
#region Default Instance
		
		private static frmMain defaultInstance;
		
		/// <summary>
		/// Added by the VB.Net to C# Converter to support default instance behavour in C#
		/// </summary>
		public static frmMain Default
		{
			get
			{
				if (defaultInstance == null)
				{
					defaultInstance = new frmMain();
					defaultInstance.FormClosed += new FormClosedEventHandler(defaultInstance_FormClosed);
				}
				
				return defaultInstance;
			}
			set
			{
				defaultInstance = value;
			}
		}
		
		static void defaultInstance_FormClosed(object sender, FormClosedEventArgs e)
		{
			defaultInstance = null;
		}
		
#endregion
		private string m_sDateFormat = "yyyy/MM/dd";
		private bool m_bEnablePCtime;
		private TimeSpan m_iTimeZoneOffset; // VBConversions Note: Initial value cannot be assigned here since it is non-static.  Assignment has been moved to the class constructors.
		private const string APP_TITLE = "Ethernet Reader Event Server";
		public GIGATMS.ERReader m_oERReader;
		private delegate void ShowMsgToTextBoxDelegate(TextBox oTextBox, int iMaxTextLength, string szMessage);
		private ShowMsgToTextBoxDelegate m_oShowMsgToTextBox = null;
		private string m_sLogDirPath = "C:\\MonthlyRecords\\";
		private string m_sToday;
		private string m_szEnumPortList;
		
		private void ShowMsgToTextBox(TextBox oTextBox, int iMaxTextLength, string szMessage)
		{
			string szData = "";
			int l = 0;
			TextBox with_1 = oTextBox;
			szData = with_1.Text;
			if (with_1.TextLength > iMaxTextLength)
			{
				szData = oTextBox.Text;
				l = szData.IndexOf("\r\n", with_1.TextLength >> 1 - 1) + 1;
				if (l == 0)
				{
					l = Conversion.Int(with_1.TextLength >> 1);
				}
				with_1.Text = szData.Substring(l + 2 - 1);
			}
			with_1.SelectionStart = with_1.TextLength;
			with_1.SelectedText = System.DateTime.Now.ToString("hh:mm:ss ");
			with_1.SelectedText = szMessage;
			with_1.SelectedText = Constants.vbCr + Constants.vbLf;
		}
		
		private void ShowMsg(string szMeesage)
		{
			if (ReferenceEquals(m_oShowMsgToTextBox, null))
			{
				m_oShowMsgToTextBox = new ShowMsgToTextBoxDelegate(ShowMsgToTextBox);
			}
			this.BeginInvoke(m_oShowMsgToTextBox, txtView, 4096, szMeesage);
		}
		
		public void MyTCPListener1_OnServerStatusChanged(ref object sender, bool bIsServerStarted)
		{
			if (bIsServerStarted)
			{
				ShowMsg("Listen OK");
			}
			else
			{
				ShowMsg("Close OK");
			}
		}
		
		public void MyTCPListener1_OnMonitor(ref object sender, GIGATMS.MyTCPClient.MonitorConstants iMonitorDataType, ref byte[] bytDataBuffer)
		{
			string szMsg = "";
			int I = 0;
			string szLineHex = "";
			string szLineASC = "";
			int iLineLen = 0;
			GIGATMS.MyTCPClient oTcpClient = (GIGATMS.MyTCPClient) sender;
			const int MAX_LINE_CHARS = 16;
			iLineLen = 0;
			szLineHex = "";
			szLineASC = "";
			szMsg = "";
			for (I = 0; I <= bytDataBuffer.Length - 1; I++)
			{
				if (iLineLen > 0)
				{
					szLineHex += " ";
				}
				szLineHex += System.Convert.ToString(bytDataBuffer[I].ToString("X").PadLeft(2, '0'));
				if (bytDataBuffer[I] >= 32 && bytDataBuffer[I] <= 127)
				{
					szLineASC += System.Convert.ToString(Strings.ChrW(bytDataBuffer[I]));
				}
				else
				{
					szLineASC += ".";
				}
				iLineLen++;
				if (iLineLen >= MAX_LINE_CHARS)
				{
					iLineLen = 0;
					szMsg += szLineHex + "   " + szLineASC;
					szMsg += Constants.vbCr + Constants.vbLf;
					szLineHex = "";
					szLineASC = "";
				}
			}
			if (iLineLen > 0)
			{
				for (I = iLineLen; I <= MAX_LINE_CHARS - 1; I++)
				{
					szLineHex += "   ";
					szLineASC += " ";
				}
				szMsg += szLineHex + "   " + szLineASC;
				szMsg += Constants.vbCr + Constants.vbLf;
				szLineHex = "";
				szLineASC = "";
			}
			szMsg = oTcpClient.ConnectTo + ": " + iMonitorDataType.ToString() + Constants.vbCr + Constants.vbLf + szMsg;
		}
		
		public void MyTCPListener1_OnConnectStatusChanged(ref System.Object sender, GIGATMS.MyTCPClient.ConnectStatusConstants iStatus, System.String szConnectTo)
		{
			ShowMsg(iStatus.ToString() + " with " + Convert.ToString(szConnectTo));
		}
		
		private delegate void OnDataReceiveDelegate(ref object sender, int iBytesToReceive, ref byte[] bytDataBuffer);
		private OnDataReceiveDelegate m_oOnDataReceiveDelegate; // VBConversions Note: Initial value cannot be assigned here since it is non-static.  Assignment has been moved to the class constructors.
		
		private void OnDataReceive(object sender, int iBytesToReceive, ref byte[] bytDataBuffer)
		{
			GIGATMS.MyTCPClient oTcpClient = (GIGATMS.MyTCPClient) sender;
			List<GIGATMS.ClientEvents> oClientEventsList = default(List<GIGATMS.ClientEvents>);
			ListViewItem oItem = default(ListViewItem);
			bool bIsEnablePCTime = false;
			string sDeviceIP = ""; //same to client IP.
			string sDateTime = "";
			string sCardUID = "";
			string sName = "";
			string szPort = "";
			int L = 0;
			int R = 0;
			int I = 0;
			
			//On Error Goto ERR_PROC VBConversions Warning: could not be converted to try/catch - logic too complex
			//true:Local PC time; false:Standard UTC time
			bIsEnablePCTime = m_bEnablePCtime;
			
			//get data here.
			oClientEventsList = GIGATMS.ClientEvents.ToClientEventsList(bIsEnablePCTime, true, ref bytDataBuffer, iBytesToReceive, m_iTimeZoneOffset);
			
			sDeviceIP = oTcpClient.ConnectTo;
			L = sDeviceIP.IndexOf(":") + 1;
			if (L > 0)
			{
				sDeviceIP = VB.Strings.Left(sDeviceIP, L - 1);
			}
			szPort = sDeviceIP;
			szPort = "TCP" + szPort;
			L = m_szEnumPortList.IndexOf(Constants.vbNullChar + szPort + ":") + 1;
			szPort = szPort + ":2167";
			if (L > 0)
			{
				L++;
				R = m_szEnumPortList.IndexOf(Constants.vbNullChar, L - 1) + 1;
				if (R > L)
				{
					szPort = m_szEnumPortList.Substring(L - 1, R - L);
				}
			}
			L = szPort.IndexOf(" ") + 1;
			if (L > 0)
			{
				szPort = VB.Strings.Left(szPort, L - 1);
			}
			
			int iEventListCount = oClientEventsList.Count;
			
			// Remove 250 items when itemcount over 1000.
			if (lvEvent.Items.Count >= 1000)
			{
				while (lvEvent.Items.Count > 750)
				{
					lvEvent.Items.RemoveAt(0);
				}
			}
			
			for (I = 0; I <= (iEventListCount - 1); I++)
			{
				sDateTime = Strings.Format(oClientEventsList[I].DateTime, m_sDateFormat + " HH:mm:ss");
				sCardUID = System.Convert.ToString(oClientEventsList[I].Data);
				sName = System.Convert.ToString(oClientEventsList[I].Name);
				//'Add data to listview
				oItem = lvEvent.Items.Add(sDeviceIP);
				oItem.SubItems.Add(sDateTime);
				oItem.SubItems.Add(sCardUID);
				oItem.SubItems.Add(sName);
				oItem.Selected = true;
				
				if (lvEvent.SelectedItems.Count > 0) //When changing selection, the ListView will first deselect current row and then select new one, so we
				{
					lvEvent.SelectedItems[0].EnsureVisible(); //scroll to the lastest one. in win10 error occur when index is not 0 here.
				}
				
				SaveLog(oItem);
				ShowMsg("Event From: " + oTcpClient.ConnectTo + ", Time Zone: " + m_iTimeZoneOffset.ToString() + "\r\n" + oClientEventsList[I].ToString() + "\r\n");
				
			}
			
			//' ~~~~~>>> cancel below comment if you need to presents the led/buzzer changes when every event comes. <<<~~~~~~~~
			//' ~~~~~>>> remember to enable the RS232 command set control in the utility first. <<<~~~~~~~~
			//With m_oERReader
			//    If .PortOpen Then
			//        .PortOpen = False
			//        Call mDoEventSleep(150)
			//    End If
			//    .PortName = szPort
			//    .PortOpen = True
			//    If .PortOpen Then
			//        ShowMsg("LED connection successfully")
			//        If m_oERReader.setLedBuzzer(1) = False Then ' let the green led on.
			//            ShowMsg("Port closed. LED operation failed.")
			//        End If
			//        .PortOpen = False
			//    End If
			//End With
ERR_EXIT:
			return;
			
ERR_PROC:
			MessageBox.Show("Err.Description=" + Information.Err().Description + "__I=" + System.Convert.ToString(I) + "_iEventListCount=" + System.Convert.ToString(iEventListCount) + "_lvEvent.Items.Count=" + System.Convert.ToString(lvEvent.Items.Count) + "_lvEvent.SelectedItems.Count" + System.Convert.ToString(lvEvent.SelectedItems.Count));
		}
		
		//This function to save .log file.
		private void SaveLog(ListViewItem oEventItem)
		{
			string sEventLog = "";
			string sMyDay = "";
			string sLogFileName = "";
			bool bIsaNewDay = false;
			string sMyFilePath = "";
			
			sLogFileName = getLogFileName();
			
			// check the dir has been existed or not. in win10 must or get the error path not exist.
			if (Strings.Trim(FileSystem.Dir(m_sLogDirPath, Constants.vbDirectory)) == "")
			{
				FileSystem.MkDir(m_sLogDirPath);
			}
			
			sMyFilePath = m_sLogDirPath + sLogFileName;
			
			sEventLog = oEventItem.Text + "," + oEventItem.SubItems[1].Text + "," + oEventItem.SubItems[2].Text;
			sMyDay = Strings.Format(DateTime.Now, "dd");
			if (!string.IsNullOrEmpty(m_sToday))
			{
				if (m_sToday != sMyDay)
				{
					m_sToday = sMyDay;
					bIsaNewDay = true;
				}
			}
			else
			{
				m_sToday = sMyDay;
				bIsaNewDay = true;
			}
			
			if (bIsaNewDay)
			{
				appendFileTitle(sMyFilePath);
			}
			
			// try..catch is the must when use the WriteAllText function, or get the error in win10.
			try
			{
				(new Microsoft.VisualBasic.Devices.ServerComputer()).FileSystem.WriteAllText(sMyFilePath, sEventLog + "\r\n", true);
			}
			catch (Exception ex)
			{
				MessageBox.Show("SaveLog - " + ex.Message);
			}
			
		}
		
		private void appendFileTitle(string sFilePath)
		{
			string sString = "";
			
			try
			{
				sString = "Attendance Records" + "," + "List" + "     " + DateTime.Now.ToString();
				(new Microsoft.VisualBasic.Devices.ServerComputer()).FileSystem.WriteAllText(sFilePath, sString + "\r\n", true);
				sString = " ";
				(new Microsoft.VisualBasic.Devices.ServerComputer()).FileSystem.WriteAllText(sFilePath, sString + "\r\n", true);
				sString = "Device IP " + "," + "  Date Time" + "," + "Card UID";
				(new Microsoft.VisualBasic.Devices.ServerComputer()).FileSystem.WriteAllText(sFilePath, sString + "\r\n", true);
				sString = "----------------------------" + "," + "-------------------------------------------" + "," + "--------------------------------";
				(new Microsoft.VisualBasic.Devices.ServerComputer()).FileSystem.WriteAllText(sFilePath, sString + "\r\n", true);
			}
			catch (Exception ex)
			{
				MessageBox.Show("appendFileTitle - " + ex.Message);
			}
			
			
		}
		
		private string getLogFileName()
		{
			string returnValue = "";
			string sMyYear = "";
			string sMyMonth = "";
			string sLogFileName = "";
			
			sMyYear = Strings.Format(DateTime.Now, "yyyy");
			sMyMonth = Strings.Format(DateTime.Now, "MM");
			
			sLogFileName = sMyYear + "_" + sMyMonth +".log";
			returnValue = sLogFileName;
			return returnValue;
		}
		
		public void MyTCPListener1_OnDataReceive(ref object sender, int iBytesToReceive, ref byte[] bytDataBuffer)
		{
			this.BeginInvoke(m_oOnDataReceiveDelegate, sender, iBytesToReceive, bytDataBuffer);
		}
		
		public void frmMain_FormClosed(object sender, System.Windows.Forms.FormClosedEventArgs e)
		{
			Interaction.SaveSetting(APP_TITLE, "Converter", "DateTimeFormat", (cboDateFormat.SelectedIndex).ToString());
		}
		
		public void frmMain_Load(System.Object sender, System.EventArgs e)
		{
			string MeVersion = "";
			TimeSpan UtcOffset = GIGATMS.TimeZoneInfo.CurrentTimeZone.StandardUtcOffset;
			
			// show S/W version
			MeVersion = " V" + System.Convert.ToString((new Microsoft.VisualBasic.ApplicationServices.WindowsFormsApplicationBase()).Info.Version.Major) +"." + System.Convert.ToString((new Microsoft.VisualBasic.ApplicationServices.WindowsFormsApplicationBase()).Info.Version.Minor) + "R" + System.Convert.ToString((new Microsoft.VisualBasic.ApplicationServices.WindowsFormsApplicationBase()).Info.Version.Revision);
			this.Text = this.Text + Strings.Space(3) + MeVersion;
			
			txtListenerIP.Text = MyTCPListener1.getListenerIpAddress_IPv4();
			
			
			lblTimeZone.Text = UtcOffset.ToString();
			
			cboDateFormat.SelectedIndex = System.Convert.ToInt32(Conversion.Val(Interaction.GetSetting(APP_TITLE, "Converter", "DateTimeFormat", "0")));
			
			//initial Event Server
			LinkToListener();
			
			//initial form's settings
			txtFilePath.Text = m_sLogDirPath;
			ListAvailableComPorts();
		}
		
		public void btnReLink_Click(System.Object sender, System.EventArgs e)
		{
			LinkToListener();
		}
		
		private void LinkToListener()
		{
			if (MyTCPListener1.IsServerStarted)
			{
				MyTCPListener1.Close();
			}
			else
			{
				if (MyTCPListener1.Listen(int.Parse(txtListenerPort.Text)))
				{
					ShowMsg("Listen Port " + txtListenerPort.Text + " OK.");
				}
				else
				{
					ShowMsg("Can't listen this port " + txtListenerPort.Text);
				}
			}
		}
		
		public void lvEvent_DoubleClick(object sender, System.EventArgs e)
		{
			lvEvent.Columns.Clear();
		}
		public void cboDateFormat_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			m_sDateFormat = cboDateFormat.Text;
		}
		
		public void chkEnablePCtime_CheckedChanged(object sender, System.EventArgs e)
		{
			if (chkEnablePCtime.Checked)
			{
				m_bEnablePCtime = true;
			}
			else
			{
				m_bEnablePCtime = false;
			}
		}
		public void txtView_DoubleClick(object sender, System.EventArgs e)
		{
			txtView.Clear();
		}
		
		public void btnClearListView_Click(System.Object sender, System.EventArgs e)
		{
			lvEvent.Items.Clear();
		}
		
		public void btnOpenDoor_Click(System.Object sender, System.EventArgs e)
		{
			bool bResult = false;
			
			// using ERReader.dll API control the reader's relay, led and buzzer after read card.
			bResult = OpenDoor();
			//If OpenDoor() = False Then
			if (bResult == false)
			{
				MessageBox.Show("Open door fail.");
			}
			LedBuzzerEffect(bResult);
			
			
		}
		
		private void LedBuzzerEffect(bool bIsOpenDoor)
		{
			int iSeconds = 0;
			iSeconds = (int.Parse(txtDoorOpenTime.Text)) * 1000;
			
			if (bIsOpenDoor)
			{
				m_oERReader.setLedBuzzer("7"); // #7 = Green LED ON with Beep once.
				mDoEventSleep(iSeconds);
				m_oERReader.setLedBuzzer("0"); // #0 = Red and Green LED Off, Buzzer Off.
			}
			else
			{
				m_oERReader.setLedBuzzer("8"); // #8 = Red LED ON with Beep 3 Times(process about 3s).
				
				iSeconds = 3;
				mDoEventSleep(iSeconds);
				
				m_oERReader.setLedBuzzer("0"); // #0 = Red and Green LED Off, Buzzer Off.
			}
			
			
			//' for lower level(by using GNet.dll) to control the reader's relay, led and buzzer after read card.
			//Dim theDataBuf_action7() As Byte = {2, 74, 55, 13} ' =02H, 4AH, 37H, 0DH
			//Dim theDataBuf_action8() As Byte = {2, 74, 56, 13} ' =02H, 4AH, 38H, 0DH
			//Dim theDataBuf_action0() As Byte = {2, 74, 48, 13} ' =02H, 4AH, 30H, 0DH
			
			//If bIsOpenDoor Then
			//    If (m_oERReader.PortOpen) Then
			//        m_oERReader.PortOpen = False
			//       Call mDoEventSleep(150)
			//    End If
			//    m_oERReader.PortName = cboCommport.Text
			//    m_oERReader.PortOpen = True
			
			//    m_oERReader.WriteBuffer(theDataBuf_action7, 1, 4)
			
			//    Call mDoEventSleep(iSeconds)
			
			//    m_oERReader.WriteBuffer(theDataBuf_action0, 1, 4)
			//Else
			//    m_oERReader.WriteBuffer(theDataBuf_action8, 1, 4)
			
			//    iSeconds = 3
			//    Call mDoEventSleep(iSeconds)
			
			//    m_oERReader.WriteBuffer(theDataBuf_action0, 1, 4)
			//End If
			
		}
		
		private bool OpenDoor()
		{
			bool returnValue = false;
			bool bResult = false;
			string sDoorOpenTime = "";
			int iCount = 0;
			
			sDoorOpenTime = txtDoorOpenTime.Text;
			iCount = System.Convert.ToInt32(Conversion.Val(sDoorOpenTime));
			if (iCount == 0)
			{
				returnValue = true;
				return returnValue;
			}
			else if (iCount > 255)
			{
				iCount = 255;
				txtDoorOpenTime.Text = (iCount).ToString();
			}
			
			if (m_oERReader.setRelay((byte) iCount))
			{
				bResult = true;
			}
			else
			{
				bResult = false;
			}
			returnValue = bResult;
			return returnValue;
		}
		
		private void ListAvailableComPorts()
		{
			string[] szPorts = null;
			m_oERReader = new GIGATMS.ERReader();
			szPorts = m_oERReader.EnumCommPortEx();
			cboCommport.Items.AddRange(szPorts);
			m_szEnumPortList = Constants.vbNullChar + string.Join(Constants.vbNullChar, szPorts) + Constants.vbNullChar;
			
			if (cboCommport.Items.Count > 0)
			{
				cboCommport.SelectedIndex = 0;
			}
		}
		
		public void cmdOpenPort_Click(System.Object sender, System.EventArgs e)
		{
			string szPort = "";
			//Dim iType As GIGATMS.ERReader.MachineTypeConstants, iReadModal As GIGATMS.ERReader.ReaderModal
			szPort = cboCommport.Text;
			
			if (Microsoft.VisualBasic.Strings.Left(szPort, 3) == "COM" || Microsoft.VisualBasic.Strings.Left(szPort, 3) == "TCP")
			{
				//iType = GIGATMS.ERReader.MachineTypeConstants.Any
				//iReadModal = GIGATMS.ERReader.ReaderModal.Any
				//If m_oERReader.AutoScan(iType, iReadModal, szPort, True) Then
				//    ShowMsg("Connect to " & Replace(szPort, vbCrLf, " "))
				//    lblPortOpenState.BackColor = System.Drawing.Color.Chartreuse
				//Else
				//    MsgBox("Ethernet Reader Connect NG.")
				//    lblPortOpenState.BackColor = System.Drawing.Color.Red
				//End If
				if (m_oERReader.ConnectToReader(szPort))
				{
					ShowMsg("Connect to " + szPort.Replace("\r\n", " "));
					lblPortOpenState.BackColor = System.Drawing.Color.Chartreuse;
				}
				else
				{
					MessageBox.Show("Ethernet Reader Connect NG.");
					lblPortOpenState.BackColor = System.Drawing.Color.Red;
				}
			}
			else
			{
				MessageBox.Show("Please Select Commport.");
				lblPortOpenState.BackColor = System.Drawing.Color.Red;
			}
		}
		
		public void Button1_Click(System.Object sender, System.EventArgs e)
		{
			bool bResult;
			string sButtonCaption = "";
			
			sButtonCaption = ((Button) (sender)).Text;
			
			lblLedBuzzerActionDescription.Text = showLedBuzzerActionDescription(sButtonCaption);
			
			bResult = m_oERReader.setLedBuzzer(sButtonCaption);
			if (bResult == false)
			{
				MessageBox.Show("Set LED/Buzzer NG.");
			}
			
		}
		
		private string showLedBuzzerActionDescription(string sNUMBER)
		{
			string returnValue = "";
			string sActionDescription = "";
			sActionDescription = "No Action.";
			switch (sNUMBER)
			{
				case "0":
					sActionDescription = "Red and Green LED Off, Buzzer Off.";
					break;
				case "1":
					sActionDescription = "Green LED ON.";
					break;
				case "2":
					sActionDescription = "Green LED OFF.";
					break;
				case "3":
					sActionDescription = "Red LED ON.";
					break;
				case "4":
					sActionDescription = "Red LED OFF.";
					break;
				case "5":
					sActionDescription = "Buzzer Beep once.";
					break;
				case "6":
					sActionDescription = "Buzzer Beep 3 Times.";
					break;
				case "7":
					sActionDescription = "Green LED ON with Beep once.";
					break;
				case "8":
					sActionDescription = "Red LED ON with Beep 3 Times.";
					break;
				case "9":
					sActionDescription = "Red and Green LED ON.";
					break;
			}
			returnValue = sActionDescription;
			return returnValue;
		}
		
		public void btnFileDir_Click(System.Object sender, System.EventArgs e)
		{
			FolderBrowserDialog FolderBrowserDialog1 = new FolderBrowserDialog();
			
			// Desktop is the root folder in the dialog.
			FolderBrowserDialog1.RootFolder = Environment.SpecialFolder.Desktop;
			// Select the directory on entry.
			FolderBrowserDialog1.SelectedPath = m_sLogDirPath;
			// Prompt the user with a custom message.
			FolderBrowserDialog1.Description = "Select the log file directory";
			if (FolderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				// Display the selected folder if the user clicked on the OK button.
				txtFilePath.Text = FolderBrowserDialog1.SelectedPath;
				m_sLogDirPath = txtFilePath.Text.ToString() + "\\";
			}
		}
		
		private void mDoEventSleep(int seconds)
		{
			(new Microsoft.VisualBasic.ApplicationServices.WindowsFormsApplicationBase()).DoEvents(); // release CPU usage.
			System.Threading.Thread.Sleep(seconds);
		}
		
	}
}
