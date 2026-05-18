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


namespace EthernetReader
{
	[global::Microsoft.VisualBasic.CompilerServices.DesignerGenerated()]public 
	partial class frmMain : System.Windows.Forms.Form
	{
		
		//Form 覆寫 Dispose 以清除元件清單。
		[System.Diagnostics.DebuggerNonUserCode()]protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing && components != null)
				{
					components.Dispose();
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}
		
		//為 Windows Form 設計工具的必要項
		private System.ComponentModel.Container components = null;
		
		//注意: 以下為 Windows Form 設計工具所需的程序
		//可以使用 Windows Form 設計工具進行修改。
		//請不要使用程式碼編輯器進行修改。
		[System.Diagnostics.DebuggerStepThrough()]private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(frmMain_FormClosed);
			base.Load += new System.EventHandler(frmMain_Load);
			this.MyTCPListener1 = new GIGATMS.MyTCPListener(this.components);
			this.MyTCPListener1.OnServerStatusChanged += new GIGATMS.MyTCPListener.OnServerStatusChangedHandler(this.MyTCPListener1_OnServerStatusChanged);
			this.MyTCPListener1.OnMonitor += new GIGATMS.MyTCPClient.OnMonitorHandler(this.MyTCPListener1_OnMonitor);
			this.MyTCPListener1.OnConnectStatusChanged += new GIGATMS.MyTCPClient.OnConnectStatusChangedHandler(this.MyTCPListener1_OnConnectStatusChanged);
			this.MyTCPListener1.OnDataReceive += new GIGATMS.MyTCPClient.OnDataReceiveHandler(this.MyTCPListener1_OnDataReceive);
			this.OpenFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			this.GroupBox2 = new System.Windows.Forms.GroupBox();
			this.lblPortOpenState = new System.Windows.Forms.Label();
			this.GroupBox4 = new System.Windows.Forms.GroupBox();
			this.lblLedBuzzerActionDescription = new System.Windows.Forms.Label();
			this.Button10 = new System.Windows.Forms.Button();
			this.Button10.Click += new System.EventHandler(this.Button1_Click);
			this.Button9 = new System.Windows.Forms.Button();
			this.Button9.Click += new System.EventHandler(this.Button1_Click);
			this.Button8 = new System.Windows.Forms.Button();
			this.Button8.Click += new System.EventHandler(this.Button1_Click);
			this.Button7 = new System.Windows.Forms.Button();
			this.Button7.Click += new System.EventHandler(this.Button1_Click);
			this.Button6 = new System.Windows.Forms.Button();
			this.Button6.Click += new System.EventHandler(this.Button1_Click);
			this.Button5 = new System.Windows.Forms.Button();
			this.Button5.Click += new System.EventHandler(this.Button1_Click);
			this.Button4 = new System.Windows.Forms.Button();
			this.Button4.Click += new System.EventHandler(this.Button1_Click);
			this.Button3 = new System.Windows.Forms.Button();
			this.Button3.Click += new System.EventHandler(this.Button1_Click);
			this.Button2 = new System.Windows.Forms.Button();
			this.Button2.Click += new System.EventHandler(this.Button1_Click);
			this.Button1 = new System.Windows.Forms.Button();
			this.Button1.Click += new System.EventHandler(this.Button1_Click);
			this.GroupBox3 = new System.Windows.Forms.GroupBox();
			this.txtDoorOpenTime = new System.Windows.Forms.TextBox();
			this.Label6 = new System.Windows.Forms.Label();
			this.btnOpenDoor = new System.Windows.Forms.Button();
			this.btnOpenDoor.Click += new System.EventHandler(this.btnOpenDoor_Click);
			this.cmdOpenPort = new System.Windows.Forms.Button();
			this.cmdOpenPort.Click += new System.EventHandler(this.cmdOpenPort_Click);
			this.cboCommport = new System.Windows.Forms.ComboBox();
			this.btnClearListView = new System.Windows.Forms.Button();
			this.btnClearListView.Click += new System.EventHandler(this.btnClearListView_Click);
			this.lvEvent = new System.Windows.Forms.ListView();
			this.lvEvent.DoubleClick += new System.EventHandler(this.lvEvent_DoubleClick);
			this.chdDeviceIP = new System.Windows.Forms.ColumnHeader();
			this.chdDateTime = new System.Windows.Forms.ColumnHeader();
			this.chdCardUID = new System.Windows.Forms.ColumnHeader();
			this.chdName = new System.Windows.Forms.ColumnHeader();
			this.lblTimeZone = new System.Windows.Forms.Label();
			this.Label3 = new System.Windows.Forms.Label();
			this.btnReLink = new System.Windows.Forms.Button();
			this.btnReLink.Click += new System.EventHandler(this.btnReLink_Click);
			this.txtListenerIP = new System.Windows.Forms.Label();
			this.Label2 = new System.Windows.Forms.Label();
			this.Label1 = new System.Windows.Forms.Label();
			this.txtListenerPort = new System.Windows.Forms.TextBox();
			this.txtView = new System.Windows.Forms.TextBox();
			this.txtView.DoubleClick += new System.EventHandler(this.txtView_DoubleClick);
			this.GroupBox6 = new System.Windows.Forms.GroupBox();
			this.cboDateFormat = new System.Windows.Forms.ComboBox();
			this.cboDateFormat.SelectedIndexChanged += new System.EventHandler(this.cboDateFormat_SelectedIndexChanged);
			this.Label5 = new System.Windows.Forms.Label();
			this.GroupBox1 = new System.Windows.Forms.GroupBox();
			this.txtFilePath = new System.Windows.Forms.TextBox();
			this.btnFileDir = new System.Windows.Forms.Button();
			this.btnFileDir.Click += new System.EventHandler(this.btnFileDir_Click);
			this.Label4 = new System.Windows.Forms.Label();
			this.chkEnablePCtime = new System.Windows.Forms.CheckBox();
			this.chkEnablePCtime.CheckedChanged += new System.EventHandler(this.chkEnablePCtime_CheckedChanged);
			this.GroupBox2.SuspendLayout();
			this.GroupBox4.SuspendLayout();
			this.GroupBox3.SuspendLayout();
			this.GroupBox6.SuspendLayout();
			this.GroupBox1.SuspendLayout();
			this.SuspendLayout();
			//
			//MyTCPListener1
			//
			this.MyTCPListener1.ListenAddress = "0.0.0.0:1001";
			this.MyTCPListener1.MaxConnectionCount = 0;
			//
			//OpenFileDialog1
			//
			this.OpenFileDialog1.FileName = "OpenFileDialog1";
			//
			//GroupBox2
			//
			this.GroupBox2.Controls.Add(this.lblPortOpenState);
			this.GroupBox2.Controls.Add(this.GroupBox4);
			this.GroupBox2.Controls.Add(this.GroupBox3);
			this.GroupBox2.Controls.Add(this.cmdOpenPort);
			this.GroupBox2.Controls.Add(this.cboCommport);
			this.GroupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (9.0F), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, System.Convert.ToByte(0));
			this.GroupBox2.Location = new System.Drawing.Point(3, 371);
			this.GroupBox2.Name = "GroupBox2";
			this.GroupBox2.Size = new System.Drawing.Size(620, 165);
			this.GroupBox2.TabIndex = 43;
			this.GroupBox2.TabStop = false;
			this.GroupBox2.Text = "Hardware Control:";
			//
			//lblPortOpenState
			//
			this.lblPortOpenState.BackColor = System.Drawing.Color.Red;
			this.lblPortOpenState.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.lblPortOpenState.Location = new System.Drawing.Point(7, 21);
			this.lblPortOpenState.Name = "lblPortOpenState";
			this.lblPortOpenState.Size = new System.Drawing.Size(24, 22);
			this.lblPortOpenState.TabIndex = 35;
			//
			//GroupBox4
			//
			this.GroupBox4.Controls.Add(this.lblLedBuzzerActionDescription);
			this.GroupBox4.Controls.Add(this.Button10);
			this.GroupBox4.Controls.Add(this.Button9);
			this.GroupBox4.Controls.Add(this.Button8);
			this.GroupBox4.Controls.Add(this.Button7);
			this.GroupBox4.Controls.Add(this.Button6);
			this.GroupBox4.Controls.Add(this.Button5);
			this.GroupBox4.Controls.Add(this.Button4);
			this.GroupBox4.Controls.Add(this.Button3);
			this.GroupBox4.Controls.Add(this.Button2);
			this.GroupBox4.Controls.Add(this.Button1);
			this.GroupBox4.Location = new System.Drawing.Point(304, 50);
			this.GroupBox4.Name = "GroupBox4";
			this.GroupBox4.Size = new System.Drawing.Size(297, 109);
			this.GroupBox4.TabIndex = 34;
			this.GroupBox4.TabStop = false;
			this.GroupBox4.Text = "LED/Buzzer Control:";
			//
			//lblLedBuzzerActionDescription
			//
			this.lblLedBuzzerActionDescription.AutoSize = true;
			this.lblLedBuzzerActionDescription.Location = new System.Drawing.Point(13, 41);
			this.lblLedBuzzerActionDescription.Name = "lblLedBuzzerActionDescription";
			this.lblLedBuzzerActionDescription.Size = new System.Drawing.Size(267, 60);
			this.lblLedBuzzerActionDescription.TabIndex = 34;
			this.lblLedBuzzerActionDescription.Text = "Command Description Here." + System.Convert.ToString(global::Microsoft.VisualBasic.Strings.ChrW(13)) + System.Convert.ToString(global::Microsoft.VisualBasic.Strings.ChrW(10)) + "! Please enable \"Enable LED/Buzzer Command" + System.Convert.ToString(global::Microsoft.VisualBasic.Strings.ChrW(13)) + System.Convert.ToString(global::Microsoft.VisualBasic.Strings.ChrW(10)) + "Set Contro" + 
				"l\" function in the Mifare Reader " + System.Convert.ToString(global::Microsoft.VisualBasic.Strings.ChrW(13)) + System.Convert.ToString(global::Microsoft.VisualBasic.Strings.ChrW(10)) + "Utility first.";
			this.lblLedBuzzerActionDescription.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			//
			//Button10
			//
			this.Button10.Location = new System.Drawing.Point(246, 16);
			this.Button10.Name = "Button10";
			this.Button10.Size = new System.Drawing.Size(24, 24);
			this.Button10.TabIndex = 33;
			this.Button10.Text = "9";
			this.Button10.UseVisualStyleBackColor = true;
			//
			//Button9
			//
			this.Button9.Location = new System.Drawing.Point(220, 16);
			this.Button9.Name = "Button9";
			this.Button9.Size = new System.Drawing.Size(24, 24);
			this.Button9.TabIndex = 33;
			this.Button9.Text = "8";
			this.Button9.UseVisualStyleBackColor = true;
			//
			//Button8
			//
			this.Button8.Location = new System.Drawing.Point(194, 16);
			this.Button8.Name = "Button8";
			this.Button8.Size = new System.Drawing.Size(24, 24);
			this.Button8.TabIndex = 33;
			this.Button8.Text = "7";
			this.Button8.UseVisualStyleBackColor = true;
			//
			//Button7
			//
			this.Button7.Location = new System.Drawing.Point(168, 16);
			this.Button7.Name = "Button7";
			this.Button7.Size = new System.Drawing.Size(24, 24);
			this.Button7.TabIndex = 33;
			this.Button7.Text = "6";
			this.Button7.UseVisualStyleBackColor = true;
			//
			//Button6
			//
			this.Button6.Location = new System.Drawing.Point(142, 16);
			this.Button6.Name = "Button6";
			this.Button6.Size = new System.Drawing.Size(24, 24);
			this.Button6.TabIndex = 33;
			this.Button6.Text = "5";
			this.Button6.UseVisualStyleBackColor = true;
			//
			//Button5
			//
			this.Button5.Location = new System.Drawing.Point(116, 16);
			this.Button5.Name = "Button5";
			this.Button5.Size = new System.Drawing.Size(24, 24);
			this.Button5.TabIndex = 33;
			this.Button5.Text = "4";
			this.Button5.UseVisualStyleBackColor = true;
			//
			//Button4
			//
			this.Button4.Location = new System.Drawing.Point(90, 16);
			this.Button4.Name = "Button4";
			this.Button4.Size = new System.Drawing.Size(24, 24);
			this.Button4.TabIndex = 33;
			this.Button4.Tag = "";
			this.Button4.Text = "3";
			this.Button4.UseVisualStyleBackColor = true;
			//
			//Button3
			//
			this.Button3.Location = new System.Drawing.Point(64, 16);
			this.Button3.Name = "Button3";
			this.Button3.Size = new System.Drawing.Size(24, 24);
			this.Button3.TabIndex = 33;
			this.Button3.Tag = "";
			this.Button3.Text = "2";
			this.Button3.UseVisualStyleBackColor = true;
			//
			//Button2
			//
			this.Button2.Location = new System.Drawing.Point(38, 16);
			this.Button2.Name = "Button2";
			this.Button2.Size = new System.Drawing.Size(24, 24);
			this.Button2.TabIndex = 33;
			this.Button2.Tag = "";
			this.Button2.Text = "1";
			this.Button2.UseVisualStyleBackColor = true;
			//
			//Button1
			//
			this.Button1.Location = new System.Drawing.Point(12, 16);
			this.Button1.Name = "Button1";
			this.Button1.Size = new System.Drawing.Size(24, 24);
			this.Button1.TabIndex = 33;
			this.Button1.Tag = "";
			this.Button1.Text = "0";
			this.Button1.UseVisualStyleBackColor = true;
			//
			//GroupBox3
			//
			this.GroupBox3.Controls.Add(this.txtDoorOpenTime);
			this.GroupBox3.Controls.Add(this.Label6);
			this.GroupBox3.Controls.Add(this.btnOpenDoor);
			this.GroupBox3.Location = new System.Drawing.Point(6, 50);
			this.GroupBox3.Name = "GroupBox3";
			this.GroupBox3.Size = new System.Drawing.Size(292, 109);
			this.GroupBox3.TabIndex = 32;
			this.GroupBox3.TabStop = false;
			this.GroupBox3.Text = "Relay Control:";
			//
			//txtDoorOpenTime
			//
			this.txtDoorOpenTime.Location = new System.Drawing.Point(15, 42);
			this.txtDoorOpenTime.Name = "txtDoorOpenTime";
			this.txtDoorOpenTime.Size = new System.Drawing.Size(132, 21);
			this.txtDoorOpenTime.TabIndex = 28;
			this.txtDoorOpenTime.Text = "0";
			//
			//Label6
			//
			this.Label6.AutoSize = true;
			this.Label6.Location = new System.Drawing.Point(15, 21);
			this.Label6.Name = "Label6";
			this.Label6.Size = new System.Drawing.Size(132, 15);
			this.Label6.TabIndex = 29;
			this.Label6.Text = "Period Time (0~255s) :";
			//
			//btnOpenDoor
			//
			this.btnOpenDoor.Location = new System.Drawing.Point(167, 37);
			this.btnOpenDoor.Name = "btnOpenDoor";
			this.btnOpenDoor.Size = new System.Drawing.Size(104, 30);
			this.btnOpenDoor.TabIndex = 27;
			this.btnOpenDoor.Text = "Open Door";
			this.btnOpenDoor.UseVisualStyleBackColor = true;
			//
			//cmdOpenPort
			//
			this.cmdOpenPort.Location = new System.Drawing.Point(516, 20);
			this.cmdOpenPort.Name = "cmdOpenPort";
			this.cmdOpenPort.Size = new System.Drawing.Size(91, 27);
			this.cmdOpenPort.TabIndex = 31;
			this.cmdOpenPort.Text = "Open Port";
			this.cmdOpenPort.UseVisualStyleBackColor = true;
			//
			//cboCommport
			//
			this.cboCommport.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboCommport.FormattingEnabled = true;
			this.cboCommport.Location = new System.Drawing.Point(32, 21);
			this.cboCommport.Name = "cboCommport";
			this.cboCommport.Size = new System.Drawing.Size(478, 23);
			this.cboCommport.TabIndex = 30;
			//
			//btnClearListView
			//
			this.btnClearListView.Image = global::My.Resources.Resources.delete2;
			this.btnClearListView.Location = new System.Drawing.Point(596, 1);
			this.btnClearListView.Name = "btnClearListView";
			this.btnClearListView.Size = new System.Drawing.Size(27, 25);
			this.btnClearListView.TabIndex = 42;
			this.btnClearListView.UseVisualStyleBackColor = true;
			//
			//lvEvent
			//
			this.lvEvent.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {this.chdDeviceIP, this.chdDateTime, this.chdCardUID, this.chdName});
			this.lvEvent.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (8.25F), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, System.Convert.ToByte(0));
			this.lvEvent.FullRowSelect = true;
			this.lvEvent.GridLines = true;
			this.lvEvent.Location = new System.Drawing.Point(3, 27);
			this.lvEvent.MultiSelect = false;
			this.lvEvent.Name = "lvEvent";
			this.lvEvent.Size = new System.Drawing.Size(620, 305);
			this.lvEvent.TabIndex = 40;
			this.lvEvent.UseCompatibleStateImageBehavior = false;
			this.lvEvent.View = System.Windows.Forms.View.Details;
			//
			//chdDeviceIP
			//
			this.chdDeviceIP.Text = "Device IP";
			this.chdDeviceIP.Width = 110;
			//
			//chdDateTime
			//
			this.chdDateTime.Text = "Date Time";
			this.chdDateTime.Width = 220;
			//
			//chdCardUID
			//
			this.chdCardUID.Text = "Card UID";
			this.chdCardUID.Width = 260;
			//
			//chdName
			//
			this.chdName.Text = "Name";
			this.chdName.Width = 0;
			//
			//lblTimeZone
			//
			this.lblTimeZone.AutoSize = true;
			this.lblTimeZone.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (8.25F), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, System.Convert.ToByte(0));
			this.lblTimeZone.Location = new System.Drawing.Point(541, 8);
			this.lblTimeZone.Name = "lblTimeZone";
			this.lblTimeZone.Size = new System.Drawing.Size(49, 13);
			this.lblTimeZone.TabIndex = 39;
			this.lblTimeZone.Text = "+xx:xx:xx";
			//
			//Label3
			//
			this.Label3.AutoSize = true;
			this.Label3.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (8.25F), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, System.Convert.ToByte(0));
			this.Label3.Location = new System.Drawing.Point(429, 9);
			this.Label3.Name = "Label3";
			this.Label3.Size = new System.Drawing.Size(107, 13);
			this.Label3.TabIndex = 38;
			this.Label3.Text = "Local PC Time Zone:";
			//
			//btnReLink
			//
			this.btnReLink.Image = global::My.Resources.Resources.link2;
			this.btnReLink.Location = new System.Drawing.Point(253, 1);
			this.btnReLink.Name = "btnReLink";
			this.btnReLink.Size = new System.Drawing.Size(25, 25);
			this.btnReLink.TabIndex = 37;
			this.btnReLink.UseVisualStyleBackColor = true;
			//
			//txtListenerIP
			//
			this.txtListenerIP.AutoSize = true;
			this.txtListenerIP.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (8.25F), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, System.Convert.ToByte(0));
			this.txtListenerIP.Location = new System.Drawing.Point(24, 8);
			this.txtListenerIP.Name = "txtListenerIP";
			this.txtListenerIP.Size = new System.Drawing.Size(76, 13);
			this.txtListenerIP.TabIndex = 36;
			this.txtListenerIP.Text = "xxx.xxx.xxx.xxx";
			//
			//Label2
			//
			this.Label2.AutoSize = true;
			this.Label2.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (8.25F), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, System.Convert.ToByte(0));
			this.Label2.Location = new System.Drawing.Point(7, 9);
			this.Label2.Name = "Label2";
			this.Label2.Size = new System.Drawing.Size(20, 13);
			this.Label2.TabIndex = 35;
			this.Label2.Text = "IP:";
			//
			//Label1
			//
			this.Label1.AutoSize = true;
			this.Label1.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (8.25F), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, System.Convert.ToByte(0));
			this.Label1.Location = new System.Drawing.Point(173, 9);
			this.Label1.Name = "Label1";
			this.Label1.Size = new System.Drawing.Size(29, 13);
			this.Label1.TabIndex = 34;
			this.Label1.Text = "Port:";
			//
			//txtListenerPort
			//
			this.txtListenerPort.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (8.25F), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, System.Convert.ToByte(0));
			this.txtListenerPort.Location = new System.Drawing.Point(203, 3);
			this.txtListenerPort.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
			this.txtListenerPort.Name = "txtListenerPort";
			this.txtListenerPort.Size = new System.Drawing.Size(48, 20);
			this.txtListenerPort.TabIndex = 33;
			this.txtListenerPort.Text = "2168";
			this.txtListenerPort.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			//
			//txtView
			//
			this.txtView.Location = new System.Drawing.Point(631, 27);
			this.txtView.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
			this.txtView.Multiline = true;
			this.txtView.Name = "txtView";
			this.txtView.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtView.Size = new System.Drawing.Size(178, 509);
			this.txtView.TabIndex = 32;
			//
			//GroupBox6
			//
			this.GroupBox6.BackColor = System.Drawing.SystemColors.ButtonFace;
			this.GroupBox6.Controls.Add(this.cboDateFormat);
			this.GroupBox6.Controls.Add(this.Label5);
			this.GroupBox6.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (8.25F), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, System.Convert.ToByte(0));
			this.GroupBox6.Location = new System.Drawing.Point(455, 334);
			this.GroupBox6.Name = "GroupBox6";
			this.GroupBox6.Size = new System.Drawing.Size(168, 35);
			this.GroupBox6.TabIndex = 46;
			this.GroupBox6.TabStop = false;
			//
			//cboDateFormat
			//
			this.cboDateFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboDateFormat.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (8.25F), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, System.Convert.ToByte(0));
			this.cboDateFormat.FormattingEnabled = true;
			this.cboDateFormat.Items.AddRange(new object[] {"yyyy/MM/dd", "MM/dd/yyyy", "dd/MM/yyyy"});
			this.cboDateFormat.Location = new System.Drawing.Point(72, 9);
			this.cboDateFormat.Name = "cboDateFormat";
			this.cboDateFormat.Size = new System.Drawing.Size(90, 21);
			this.cboDateFormat.TabIndex = 29;
			//
			//Label5
			//
			this.Label5.AutoSize = true;
			this.Label5.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (8.25F), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, System.Convert.ToByte(0));
			this.Label5.Location = new System.Drawing.Point(2, 13);
			this.Label5.Name = "Label5";
			this.Label5.Size = new System.Drawing.Size(68, 13);
			this.Label5.TabIndex = 28;
			this.Label5.Text = "Date Format:";
			//
			//GroupBox1
			//
			this.GroupBox1.BackColor = System.Drawing.SystemColors.Control;
			this.GroupBox1.Controls.Add(this.txtFilePath);
			this.GroupBox1.Controls.Add(this.btnFileDir);
			this.GroupBox1.Controls.Add(this.Label4);
			this.GroupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (8.25F), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, System.Convert.ToByte(0));
			this.GroupBox1.Location = new System.Drawing.Point(110, 335);
			this.GroupBox1.Name = "GroupBox1";
			this.GroupBox1.Size = new System.Drawing.Size(337, 34);
			this.GroupBox1.TabIndex = 45;
			this.GroupBox1.TabStop = false;
			//
			//txtFilePath
			//
			this.txtFilePath.BackColor = System.Drawing.SystemColors.ButtonFace;
			this.txtFilePath.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.txtFilePath.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (8.25F), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, System.Convert.ToByte(0));
			this.txtFilePath.Location = new System.Drawing.Point(121, 14);
			this.txtFilePath.Name = "txtFilePath";
			this.txtFilePath.Size = new System.Drawing.Size(180, 13);
			this.txtFilePath.TabIndex = 28;
			//
			//btnFileDir
			//
			this.btnFileDir.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (8.25F), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, System.Convert.ToByte(0));
			this.btnFileDir.Location = new System.Drawing.Point(307, 10);
			this.btnFileDir.Name = "btnFileDir";
			this.btnFileDir.Size = new System.Drawing.Size(25, 20);
			this.btnFileDir.TabIndex = 27;
			this.btnFileDir.Text = "...";
			this.btnFileDir.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.btnFileDir.UseVisualStyleBackColor = true;
			//
			//Label4
			//
			this.Label4.AutoSize = true;
			this.Label4.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (8.25F), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, System.Convert.ToByte(0));
			this.Label4.Location = new System.Drawing.Point(3, 13);
			this.Label4.Name = "Label4";
			this.Label4.Size = new System.Drawing.Size(115, 13);
			this.Label4.TabIndex = 25;
			this.Label4.Text = "Auto Save Records to:";
			this.Label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			//
			//chkEnablePCtime
			//
			this.chkEnablePCtime.AutoSize = true;
			this.chkEnablePCtime.Checked = true;
			this.chkEnablePCtime.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkEnablePCtime.Enabled = false;
			this.chkEnablePCtime.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (8.25F), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, System.Convert.ToByte(0));
			this.chkEnablePCtime.Location = new System.Drawing.Point(5, 347);
			this.chkEnablePCtime.Name = "chkEnablePCtime";
			this.chkEnablePCtime.Size = new System.Drawing.Size(102, 17);
			this.chkEnablePCtime.TabIndex = 44;
			this.chkEnablePCtime.Text = "Enable PC Time";
			this.chkEnablePCtime.UseVisualStyleBackColor = true;
			//
			//frmMain
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF((float) (7.0F), (float) (13.0F));
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(814, 539);
			this.Controls.Add(this.GroupBox6);
			this.Controls.Add(this.GroupBox1);
			this.Controls.Add(this.chkEnablePCtime);
			this.Controls.Add(this.GroupBox2);
			this.Controls.Add(this.btnClearListView);
			this.Controls.Add(this.lvEvent);
			this.Controls.Add(this.lblTimeZone);
			this.Controls.Add(this.Label3);
			this.Controls.Add(this.btnReLink);
			this.Controls.Add(this.txtListenerIP);
			this.Controls.Add(this.Label2);
			this.Controls.Add(this.Label1);
			this.Controls.Add(this.txtListenerPort);
			this.Controls.Add(this.txtView);
			this.Font = new System.Drawing.Font("細明體", (float) (9.75F), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, System.Convert.ToByte(136));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "frmMain";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Ethernet Reader Event Server";
			this.GroupBox2.ResumeLayout(false);
			this.GroupBox4.ResumeLayout(false);
			this.GroupBox4.PerformLayout();
			this.GroupBox3.ResumeLayout(false);
			this.GroupBox3.PerformLayout();
			this.GroupBox6.ResumeLayout(false);
			this.GroupBox6.PerformLayout();
			this.GroupBox1.ResumeLayout(false);
			this.GroupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
			
		}
		internal GIGATMS.MyTCPListener MyTCPListener1;
		internal System.Windows.Forms.OpenFileDialog OpenFileDialog1;
		internal System.Windows.Forms.GroupBox GroupBox2;
		internal System.Windows.Forms.Label lblPortOpenState;
		internal System.Windows.Forms.GroupBox GroupBox4;
		internal System.Windows.Forms.Label lblLedBuzzerActionDescription;
		internal System.Windows.Forms.Button Button10;
		internal System.Windows.Forms.Button Button9;
		internal System.Windows.Forms.Button Button8;
		internal System.Windows.Forms.Button Button7;
		internal System.Windows.Forms.Button Button6;
		internal System.Windows.Forms.Button Button5;
		internal System.Windows.Forms.Button Button4;
		internal System.Windows.Forms.Button Button3;
		internal System.Windows.Forms.Button Button2;
		internal System.Windows.Forms.Button Button1;
		internal System.Windows.Forms.GroupBox GroupBox3;
		internal System.Windows.Forms.TextBox txtDoorOpenTime;
		internal System.Windows.Forms.Label Label6;
		internal System.Windows.Forms.Button btnOpenDoor;
		internal System.Windows.Forms.Button cmdOpenPort;
		internal System.Windows.Forms.ComboBox cboCommport;
		internal System.Windows.Forms.Button btnClearListView;
		internal System.Windows.Forms.ListView lvEvent;
		internal System.Windows.Forms.ColumnHeader chdDeviceIP;
		internal System.Windows.Forms.ColumnHeader chdDateTime;
		internal System.Windows.Forms.ColumnHeader chdCardUID;
		internal System.Windows.Forms.ColumnHeader chdName;
		internal System.Windows.Forms.Label lblTimeZone;
		internal System.Windows.Forms.Label Label3;
		internal System.Windows.Forms.Button btnReLink;
		internal System.Windows.Forms.Label txtListenerIP;
		internal System.Windows.Forms.Label Label2;
		internal System.Windows.Forms.Label Label1;
		private System.Windows.Forms.TextBox txtListenerPort;
		private System.Windows.Forms.TextBox txtView;
		internal System.Windows.Forms.GroupBox GroupBox6;
		internal System.Windows.Forms.ComboBox cboDateFormat;
		internal System.Windows.Forms.Label Label5;
		internal System.Windows.Forms.GroupBox GroupBox1;
		internal System.Windows.Forms.TextBox txtFilePath;
		internal System.Windows.Forms.Button btnFileDir;
		internal System.Windows.Forms.Label Label4;
		internal System.Windows.Forms.CheckBox chkEnablePCtime;
	}
	
}
