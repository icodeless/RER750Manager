namespace RER750Manager
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.uxDeviceListView = new System.Windows.Forms.ListView();
            this.Button1 = new System.Windows.Forms.Button();
            this.uxFormInitializeTimer = new System.Windows.Forms.Timer(this.components);
            this.Label1 = new System.Windows.Forms.Label();
            this.Label2 = new System.Windows.Forms.Label();
            this.uxEventDataListView = new System.Windows.Forms.ListView();
            this.uxStartListen = new System.Windows.Forms.Button();
            this.uxLocalIps = new System.Windows.Forms.ListBox();
            this.GroupBox1 = new System.Windows.Forms.GroupBox();
            this.uxListenPort = new System.Windows.Forms.NumericUpDown();
            this.Label6 = new System.Windows.Forms.Label();
            this.TextBox1 = new System.Windows.Forms.TextBox();
            this.GroupBox2 = new System.Windows.Forms.GroupBox();
            this.GroupBox4 = new System.Windows.Forms.GroupBox();
            this.uxLedBuzzerControlSelectionList = new System.Windows.Forms.ComboBox();
            this.Button5 = new System.Windows.Forms.Button();
            this.GroupBox3 = new System.Windows.Forms.GroupBox();
            this.Button3 = new System.Windows.Forms.Button();
            this.Label5 = new System.Windows.Forms.Label();
            this.uxOpenDoorDuration = new System.Windows.Forms.NumericUpDown();
            this.uxRemotePort = new System.Windows.Forms.NumericUpDown();
            this.Label7 = new System.Windows.Forms.Label();
            this.uxSelectedDeviceIp = new System.Windows.Forms.TextBox();
            this.Label3 = new System.Windows.Forms.Label();
            this.Button4 = new System.Windows.Forms.Button();
            this.uxEventMessageListView = new System.Windows.Forms.ListView();
            this.Label4 = new System.Windows.Forms.Label();
            this.Button2 = new System.Windows.Forms.Button();
            this.Button6 = new System.Windows.Forms.Button();
            this.Button7 = new System.Windows.Forms.Button();
            this.uxOpenDoorTimer = new System.Windows.Forms.Timer(this.components);
            this.uxControlLedBuzzerTimer = new System.Windows.Forms.Timer(this.components);
            this.GroupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.uxListenPort)).BeginInit();
            this.GroupBox2.SuspendLayout();
            this.GroupBox4.SuspendLayout();
            this.GroupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.uxOpenDoorDuration)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.uxRemotePort)).BeginInit();
            this.SuspendLayout();
            // 
            // uxDeviceListView
            // 
            this.uxDeviceListView.Cursor = System.Windows.Forms.Cursors.Default;
            this.uxDeviceListView.ForeColor = System.Drawing.SystemColors.WindowText;
            this.uxDeviceListView.HideSelection = false;
            this.uxDeviceListView.Location = new System.Drawing.Point(12, 32);
            this.uxDeviceListView.Name = "uxDeviceListView";
            this.uxDeviceListView.Size = new System.Drawing.Size(637, 171);
            this.uxDeviceListView.TabIndex = 0;
            this.uxDeviceListView.UseCompatibleStateImageBehavior = false;
            this.uxDeviceListView.View = System.Windows.Forms.View.Details;
            this.uxDeviceListView.SelectedIndexChanged += new System.EventHandler(this.uxDeviceListView_SelectedIndexChanged);
            // 
            // Button1
            // 
            this.Button1.Location = new System.Drawing.Point(19, 180);
            this.Button1.Name = "Button1";
            this.Button1.Size = new System.Drawing.Size(239, 38);
            this.Button1.TabIndex = 1;
            this.Button1.Text = "Broadcast";
            this.Button1.UseVisualStyleBackColor = true;
            this.Button1.Click += new System.EventHandler(this.Button1_Click);
            // 
            // uxFormInitializeTimer
            // 
            this.uxFormInitializeTimer.Tick += new System.EventHandler(this.uxFormInitializeTimer_Tick);
            // 
            // Label1
            // 
            this.Label1.AutoSize = true;
            this.Label1.Location = new System.Drawing.Point(12, 13);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(72, 16);
            this.Label1.TabIndex = 2;
            this.Label1.Text = "Device list:";
            // 
            // Label2
            // 
            this.Label2.AutoSize = true;
            this.Label2.Location = new System.Drawing.Point(12, 207);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(99, 16);
            this.Label2.TabIndex = 3;
            this.Label2.Text = "Received data:";
            // 
            // uxEventDataListView
            // 
            this.uxEventDataListView.Cursor = System.Windows.Forms.Cursors.Default;
            this.uxEventDataListView.ForeColor = System.Drawing.SystemColors.WindowText;
            this.uxEventDataListView.HideSelection = false;
            this.uxEventDataListView.Location = new System.Drawing.Point(12, 226);
            this.uxEventDataListView.Name = "uxEventDataListView";
            this.uxEventDataListView.Size = new System.Drawing.Size(633, 181);
            this.uxEventDataListView.TabIndex = 4;
            this.uxEventDataListView.UseCompatibleStateImageBehavior = false;
            this.uxEventDataListView.View = System.Windows.Forms.View.Details;
            this.uxEventDataListView.SelectedIndexChanged += new System.EventHandler(this.uxEventDataListView_SelectedIndexChanged);
            // 
            // uxStartListen
            // 
            this.uxStartListen.Location = new System.Drawing.Point(17, 224);
            this.uxStartListen.Name = "uxStartListen";
            this.uxStartListen.Size = new System.Drawing.Size(241, 38);
            this.uxStartListen.TabIndex = 5;
            this.uxStartListen.Text = "Start Listening";
            this.uxStartListen.UseVisualStyleBackColor = true;
            this.uxStartListen.Click += new System.EventHandler(this.uxStartListen_Click);
            // 
            // uxLocalIps
            // 
            this.uxLocalIps.FormattingEnabled = true;
            this.uxLocalIps.ItemHeight = 16;
            this.uxLocalIps.Location = new System.Drawing.Point(19, 55);
            this.uxLocalIps.Name = "uxLocalIps";
            this.uxLocalIps.Size = new System.Drawing.Size(240, 84);
            this.uxLocalIps.TabIndex = 6;
            this.uxLocalIps.SelectedIndexChanged += new System.EventHandler(this.uxLocalIps_SelectedIndexChanged);
            // 
            // GroupBox1
            // 
            this.GroupBox1.Controls.Add(this.uxListenPort);
            this.GroupBox1.Controls.Add(this.Label6);
            this.GroupBox1.Controls.Add(this.uxLocalIps);
            this.GroupBox1.Controls.Add(this.TextBox1);
            this.GroupBox1.Controls.Add(this.uxStartListen);
            this.GroupBox1.Controls.Add(this.Button1);
            this.GroupBox1.Location = new System.Drawing.Point(659, 25);
            this.GroupBox1.Name = "GroupBox1";
            this.GroupBox1.Size = new System.Drawing.Size(280, 275);
            this.GroupBox1.TabIndex = 9;
            this.GroupBox1.TabStop = false;
            this.GroupBox1.Text = "Local IPs";
            // 
            // uxListenPort
            // 
            this.uxListenPort.Location = new System.Drawing.Point(117, 145);
            this.uxListenPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.uxListenPort.Name = "uxListenPort";
            this.uxListenPort.Size = new System.Drawing.Size(67, 22);
            this.uxListenPort.TabIndex = 9;
            this.uxListenPort.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.uxListenPort.Value = new decimal(new int[] {
            2168,
            0,
            0,
            0});
            // 
            // Label6
            // 
            this.Label6.AutoSize = true;
            this.Label6.Location = new System.Drawing.Point(20, 150);
            this.Label6.Name = "Label6";
            this.Label6.Size = new System.Drawing.Size(72, 16);
            this.Label6.TabIndex = 8;
            this.Label6.Text = "Listen Port:";
            // 
            // TextBox1
            // 
            this.TextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.TextBox1.Location = new System.Drawing.Point(19, 22);
            this.TextBox1.Multiline = true;
            this.TextBox1.Name = "TextBox1";
            this.TextBox1.ReadOnly = true;
            this.TextBox1.Size = new System.Drawing.Size(240, 38);
            this.TextBox1.TabIndex = 7;
            this.TextBox1.Text = "Select a local IP that is used to broadcast and  listen for incoming data.";
            this.TextBox1.TextChanged += new System.EventHandler(this.TextBox1_TextChanged);
            // 
            // GroupBox2
            // 
            this.GroupBox2.Controls.Add(this.GroupBox4);
            this.GroupBox2.Controls.Add(this.GroupBox3);
            this.GroupBox2.Controls.Add(this.uxRemotePort);
            this.GroupBox2.Controls.Add(this.Label7);
            this.GroupBox2.Controls.Add(this.uxSelectedDeviceIp);
            this.GroupBox2.Controls.Add(this.Label3);
            this.GroupBox2.Location = new System.Drawing.Point(659, 307);
            this.GroupBox2.Name = "GroupBox2";
            this.GroupBox2.Size = new System.Drawing.Size(280, 298);
            this.GroupBox2.TabIndex = 10;
            this.GroupBox2.TabStop = false;
            this.GroupBox2.Text = "Commands";
            // 
            // GroupBox4
            // 
            this.GroupBox4.Controls.Add(this.uxLedBuzzerControlSelectionList);
            this.GroupBox4.Controls.Add(this.Button5);
            this.GroupBox4.Location = new System.Drawing.Point(18, 197);
            this.GroupBox4.Name = "GroupBox4";
            this.GroupBox4.Size = new System.Drawing.Size(256, 100);
            this.GroupBox4.TabIndex = 19;
            this.GroupBox4.TabStop = false;
            this.GroupBox4.Text = "Control LED/Buzzer Selection";
            this.GroupBox4.Enter += new System.EventHandler(this.GroupBox4_Enter);
            // 
            // uxLedBuzzerControlSelectionList
            // 
            this.uxLedBuzzerControlSelectionList.FormattingEnabled = true;
            this.uxLedBuzzerControlSelectionList.Location = new System.Drawing.Point(23, 22);
            this.uxLedBuzzerControlSelectionList.Name = "uxLedBuzzerControlSelectionList";
            this.uxLedBuzzerControlSelectionList.Size = new System.Drawing.Size(217, 24);
            this.uxLedBuzzerControlSelectionList.TabIndex = 12;
            // 
            // Button5
            // 
            this.Button5.Location = new System.Drawing.Point(21, 52);
            this.Button5.Name = "Button5";
            this.Button5.Size = new System.Drawing.Size(219, 38);
            this.Button5.TabIndex = 11;
            this.Button5.Text = "Control LED/Buzzer";
            this.Button5.UseVisualStyleBackColor = true;
            this.Button5.Click += new System.EventHandler(this.Button5_Click);
            // 
            // GroupBox3
            // 
            this.GroupBox3.Controls.Add(this.Button3);
            this.GroupBox3.Controls.Add(this.Label5);
            this.GroupBox3.Controls.Add(this.uxOpenDoorDuration);
            this.GroupBox3.Location = new System.Drawing.Point(18, 89);
            this.GroupBox3.Name = "GroupBox3";
            this.GroupBox3.Size = new System.Drawing.Size(256, 100);
            this.GroupBox3.TabIndex = 18;
            this.GroupBox3.TabStop = false;
            this.GroupBox3.Text = "Control Relay";
            this.GroupBox3.Enter += new System.EventHandler(this.GroupBox3_Enter);
            // 
            // Button3
            // 
            this.Button3.Location = new System.Drawing.Point(21, 54);
            this.Button3.Name = "Button3";
            this.Button3.Size = new System.Drawing.Size(219, 38);
            this.Button3.TabIndex = 11;
            this.Button3.Text = "Open Door";
            this.Button3.UseVisualStyleBackColor = true;
            this.Button3.Click += new System.EventHandler(this.Button3_Click);
            // 
            // Label5
            // 
            this.Label5.AutoSize = true;
            this.Label5.Location = new System.Drawing.Point(18, 22);
            this.Label5.Name = "Label5";
            this.Label5.Size = new System.Drawing.Size(60, 16);
            this.Label5.TabIndex = 13;
            this.Label5.Text = "Duration:";
            // 
            // uxOpenDoorDuration
            // 
            this.uxOpenDoorDuration.Location = new System.Drawing.Point(124, 20);
            this.uxOpenDoorDuration.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.uxOpenDoorDuration.Name = "uxOpenDoorDuration";
            this.uxOpenDoorDuration.Size = new System.Drawing.Size(87, 22);
            this.uxOpenDoorDuration.TabIndex = 14;
            this.uxOpenDoorDuration.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.uxOpenDoorDuration.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.uxOpenDoorDuration.ValueChanged += new System.EventHandler(this.uxOpenDoorDuration_ValueChanged);
            // 
            // uxRemotePort
            // 
            this.uxRemotePort.Location = new System.Drawing.Point(142, 55);
            this.uxRemotePort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.uxRemotePort.Name = "uxRemotePort";
            this.uxRemotePort.Size = new System.Drawing.Size(87, 22);
            this.uxRemotePort.TabIndex = 17;
            this.uxRemotePort.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.uxRemotePort.Value = new decimal(new int[] {
            2167,
            0,
            0,
            0});
            // 
            // Label7
            // 
            this.Label7.AutoSize = true;
            this.Label7.Location = new System.Drawing.Point(21, 58);
            this.Label7.Name = "Label7";
            this.Label7.Size = new System.Drawing.Size(85, 16);
            this.Label7.TabIndex = 15;
            this.Label7.Text = "Remote Port:";
            // 
            // uxSelectedDeviceIp
            // 
            this.uxSelectedDeviceIp.Location = new System.Drawing.Point(141, 22);
            this.uxSelectedDeviceIp.Multiline = true;
            this.uxSelectedDeviceIp.Name = "uxSelectedDeviceIp";
            this.uxSelectedDeviceIp.ReadOnly = true;
            this.uxSelectedDeviceIp.Size = new System.Drawing.Size(128, 26);
            this.uxSelectedDeviceIp.TabIndex = 9;
            this.uxSelectedDeviceIp.TextChanged += new System.EventHandler(this.uxSelectedDeviceIp_TextChanged);
            // 
            // Label3
            // 
            this.Label3.AutoSize = true;
            this.Label3.Location = new System.Drawing.Point(21, 26);
            this.Label3.Name = "Label3";
            this.Label3.Size = new System.Drawing.Size(68, 16);
            this.Label3.TabIndex = 10;
            this.Label3.Text = "Device IP:";
            // 
            // Button4
            // 
            this.Button4.Location = new System.Drawing.Point(692, 650);
            this.Button4.Name = "Button4";
            this.Button4.Size = new System.Drawing.Size(226, 38);
            this.Button4.TabIndex = 12;
            this.Button4.Text = "&Exit";
            this.Button4.UseVisualStyleBackColor = true;
            this.Button4.Click += new System.EventHandler(this.Button4_Click);
            // 
            // uxEventMessageListView
            // 
            this.uxEventMessageListView.Cursor = System.Windows.Forms.Cursors.Default;
            this.uxEventMessageListView.ForeColor = System.Drawing.SystemColors.WindowText;
            this.uxEventMessageListView.HideSelection = false;
            this.uxEventMessageListView.Location = new System.Drawing.Point(12, 454);
            this.uxEventMessageListView.Name = "uxEventMessageListView";
            this.uxEventMessageListView.Size = new System.Drawing.Size(633, 151);
            this.uxEventMessageListView.TabIndex = 14;
            this.uxEventMessageListView.UseCompatibleStateImageBehavior = false;
            this.uxEventMessageListView.View = System.Windows.Forms.View.Details;
            // 
            // Label4
            // 
            this.Label4.AutoSize = true;
            this.Label4.Location = new System.Drawing.Point(12, 434);
            this.Label4.Name = "Label4";
            this.Label4.Size = new System.Drawing.Size(104, 16);
            this.Label4.TabIndex = 13;
            this.Label4.Text = "Event Message:";
            // 
            // Button2
            // 
            this.Button2.Location = new System.Drawing.Point(446, 611);
            this.Button2.Name = "Button2";
            this.Button2.Size = new System.Drawing.Size(199, 38);
            this.Button2.TabIndex = 15;
            this.Button2.Text = "Clear Event Message";
            this.Button2.UseVisualStyleBackColor = true;
            this.Button2.Click += new System.EventHandler(this.Button2_Click_1);
            // 
            // Button6
            // 
            this.Button6.Location = new System.Drawing.Point(446, 409);
            this.Button6.Name = "Button6";
            this.Button6.Size = new System.Drawing.Size(199, 38);
            this.Button6.TabIndex = 16;
            this.Button6.Text = "Clear Received Data";
            this.Button6.UseVisualStyleBackColor = true;
            this.Button6.Click += new System.EventHandler(this.Button6_Click_1);
            // 
            // Button7
            // 
            this.Button7.Location = new System.Drawing.Point(692, 611);
            this.Button7.Name = "Button7";
            this.Button7.Size = new System.Drawing.Size(226, 38);
            this.Button7.TabIndex = 17;
            this.Button7.Text = "Batch Command";
            this.Button7.UseVisualStyleBackColor = true;
            this.Button7.Click += new System.EventHandler(this.Button7_Click);
            // 
            // uxOpenDoorTimer
            // 
            this.uxOpenDoorTimer.Interval = 20;
            this.uxOpenDoorTimer.Tick += new System.EventHandler(this.uxBatchCommandTimer_Tick);
            // 
            // uxControlLedBuzzerTimer
            // 
            this.uxControlLedBuzzerTimer.Interval = 300;
            this.uxControlLedBuzzerTimer.Tick += new System.EventHandler(this.uxControlLedBuzzerTimer_Tick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(959, 708);
            this.Controls.Add(this.Button7);
            this.Controls.Add(this.Button6);
            this.Controls.Add(this.Button2);
            this.Controls.Add(this.uxEventMessageListView);
            this.Controls.Add(this.Label4);
            this.Controls.Add(this.Button4);
            this.Controls.Add(this.GroupBox2);
            this.Controls.Add(this.GroupBox1);
            this.Controls.Add(this.uxEventDataListView);
            this.Controls.Add(this.Label2);
            this.Controls.Add(this.uxDeviceListView);
            this.Controls.Add(this.Label1);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "RAPIDACCESS RER75X Manager";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.GroupBox1.ResumeLayout(false);
            this.GroupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.uxListenPort)).EndInit();
            this.GroupBox2.ResumeLayout(false);
            this.GroupBox2.PerformLayout();
            this.GroupBox4.ResumeLayout(false);
            this.GroupBox3.ResumeLayout(false);
            this.GroupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.uxOpenDoorDuration)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.uxRemotePort)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.ListView uxDeviceListView;
        private System.Windows.Forms.Button Button1;
        private System.Windows.Forms.Timer uxFormInitializeTimer;
        private System.Windows.Forms.Label Label1;
        private System.Windows.Forms.Label Label2;
        private System.Windows.Forms.ListView uxEventDataListView;
        private System.Windows.Forms.Button uxStartListen;
        private System.Windows.Forms.ListBox uxLocalIps;
        private System.Windows.Forms.GroupBox GroupBox1;
        private System.Windows.Forms.NumericUpDown uxListenPort;
        private System.Windows.Forms.Label Label6;
        private System.Windows.Forms.TextBox TextBox1;
        private System.Windows.Forms.GroupBox GroupBox2;
        private System.Windows.Forms.GroupBox GroupBox4;
        private System.Windows.Forms.ComboBox uxLedBuzzerControlSelectionList;
        private System.Windows.Forms.Button Button5;
        private System.Windows.Forms.GroupBox GroupBox3;
        private System.Windows.Forms.Button Button3;
        private System.Windows.Forms.Label Label5;
        private System.Windows.Forms.NumericUpDown uxOpenDoorDuration;
        private System.Windows.Forms.NumericUpDown uxRemotePort;
        private System.Windows.Forms.Label Label7;
        private System.Windows.Forms.TextBox uxSelectedDeviceIp;
        private System.Windows.Forms.Label Label3;
        private System.Windows.Forms.Button Button4;
        private System.Windows.Forms.ListView uxEventMessageListView;
        private System.Windows.Forms.Label Label4;
        private System.Windows.Forms.Button Button2;
        private System.Windows.Forms.Button Button6;
        private System.Windows.Forms.Button Button7;
        private System.Windows.Forms.Timer uxOpenDoorTimer;
        private System.Windows.Forms.Timer uxControlLedBuzzerTimer;
    }
}