<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class MainForm
    Inherits System.Windows.Forms.Form

    'Form 覆寫 Dispose 以清除元件清單。
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    '為 Windows Form 設計工具的必要項
    Private components As System.ComponentModel.IContainer

    '注意: 以下為 Windows Form 設計工具所需的程序
    '可以使用 Windows Form 設計工具進行修改。
    '請不要使用程式碼編輯器進行修改。
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(MainForm))
        Me.uxDeviceListView = New System.Windows.Forms.ListView()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.uxFormInitializeTimer = New System.Windows.Forms.Timer(Me.components)
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.uxEventDataListView = New System.Windows.Forms.ListView()
        Me.uxStartListen = New System.Windows.Forms.Button()
        Me.uxLocalIps = New System.Windows.Forms.ListBox()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.uxListenPort = New System.Windows.Forms.NumericUpDown()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.TextBox1 = New System.Windows.Forms.TextBox()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.GroupBox4 = New System.Windows.Forms.GroupBox()
        Me.uxLedBuzzerControlSelectionList = New System.Windows.Forms.ComboBox()
        Me.Button5 = New System.Windows.Forms.Button()
        Me.GroupBox3 = New System.Windows.Forms.GroupBox()
        Me.Button3 = New System.Windows.Forms.Button()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.uxOpenDoorDuration = New System.Windows.Forms.NumericUpDown()
        Me.uxRemotePort = New System.Windows.Forms.NumericUpDown()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.uxSelectedDeviceIp = New System.Windows.Forms.TextBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Button4 = New System.Windows.Forms.Button()
        Me.uxEventMessageListView = New System.Windows.Forms.ListView()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.Button2 = New System.Windows.Forms.Button()
        Me.Button6 = New System.Windows.Forms.Button()
        Me.Button7 = New System.Windows.Forms.Button()
        Me.uxOpenDoorTimer = New System.Windows.Forms.Timer(Me.components)
        Me.uxControlLedBuzzerTimer = New System.Windows.Forms.Timer(Me.components)
        Me.GroupBox1.SuspendLayout()
        CType(Me.uxListenPort, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.GroupBox2.SuspendLayout()
        Me.GroupBox4.SuspendLayout()
        Me.GroupBox3.SuspendLayout()
        CType(Me.uxOpenDoorDuration, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.uxRemotePort, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'uxDeviceListView
        '
        Me.uxDeviceListView.Cursor = System.Windows.Forms.Cursors.Default
        Me.uxDeviceListView.ForeColor = System.Drawing.SystemColors.WindowText
        Me.uxDeviceListView.HideSelection = False
        Me.uxDeviceListView.Location = New System.Drawing.Point(14, 40)
        Me.uxDeviceListView.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.uxDeviceListView.Name = "uxDeviceListView"
        Me.uxDeviceListView.Size = New System.Drawing.Size(716, 213)
        Me.uxDeviceListView.TabIndex = 0
        Me.uxDeviceListView.UseCompatibleStateImageBehavior = False
        Me.uxDeviceListView.View = System.Windows.Forms.View.Details
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(21, 225)
        Me.Button1.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(269, 47)
        Me.Button1.TabIndex = 1
        Me.Button1.Text = "Broadcast"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'uxFormInitializeTimer
        '
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(14, 16)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(84, 20)
        Me.Label1.TabIndex = 2
        Me.Label1.Text = "Device list:"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(14, 259)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(115, 20)
        Me.Label2.TabIndex = 3
        Me.Label2.Text = "Received data:"
        '
        'uxEventDataListView
        '
        Me.uxEventDataListView.Cursor = System.Windows.Forms.Cursors.Default
        Me.uxEventDataListView.ForeColor = System.Drawing.SystemColors.WindowText
        Me.uxEventDataListView.HideSelection = False
        Me.uxEventDataListView.Location = New System.Drawing.Point(14, 283)
        Me.uxEventDataListView.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.uxEventDataListView.Name = "uxEventDataListView"
        Me.uxEventDataListView.Size = New System.Drawing.Size(712, 225)
        Me.uxEventDataListView.TabIndex = 4
        Me.uxEventDataListView.UseCompatibleStateImageBehavior = False
        Me.uxEventDataListView.View = System.Windows.Forms.View.Details
        '
        'uxStartListen
        '
        Me.uxStartListen.Location = New System.Drawing.Point(19, 280)
        Me.uxStartListen.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.uxStartListen.Name = "uxStartListen"
        Me.uxStartListen.Size = New System.Drawing.Size(271, 47)
        Me.uxStartListen.TabIndex = 5
        Me.uxStartListen.Text = "Start Listening"
        Me.uxStartListen.UseVisualStyleBackColor = True
        '
        'uxLocalIps
        '
        Me.uxLocalIps.FormattingEnabled = True
        Me.uxLocalIps.ItemHeight = 20
        Me.uxLocalIps.Location = New System.Drawing.Point(21, 69)
        Me.uxLocalIps.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.uxLocalIps.Name = "uxLocalIps"
        Me.uxLocalIps.Size = New System.Drawing.Size(270, 104)
        Me.uxLocalIps.TabIndex = 6
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.uxListenPort)
        Me.GroupBox1.Controls.Add(Me.Label6)
        Me.GroupBox1.Controls.Add(Me.uxLocalIps)
        Me.GroupBox1.Controls.Add(Me.TextBox1)
        Me.GroupBox1.Controls.Add(Me.uxStartListen)
        Me.GroupBox1.Controls.Add(Me.Button1)
        Me.GroupBox1.Location = New System.Drawing.Point(741, 31)
        Me.GroupBox1.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Padding = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.GroupBox1.Size = New System.Drawing.Size(315, 344)
        Me.GroupBox1.TabIndex = 9
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Local IPs"
        '
        'uxListenPort
        '
        Me.uxListenPort.Location = New System.Drawing.Point(132, 181)
        Me.uxListenPort.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.uxListenPort.Maximum = New Decimal(New Integer() {65535, 0, 0, 0})
        Me.uxListenPort.Name = "uxListenPort"
        Me.uxListenPort.Size = New System.Drawing.Size(75, 26)
        Me.uxListenPort.TabIndex = 9
        Me.uxListenPort.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        Me.uxListenPort.Value = New Decimal(New Integer() {2168, 0, 0, 0})
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(22, 187)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(89, 20)
        Me.Label6.TabIndex = 8
        Me.Label6.Text = "Listen Port:"
        '
        'TextBox1
        '
        Me.TextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.TextBox1.Location = New System.Drawing.Point(21, 28)
        Me.TextBox1.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.TextBox1.Multiline = True
        Me.TextBox1.Name = "TextBox1"
        Me.TextBox1.ReadOnly = True
        Me.TextBox1.Size = New System.Drawing.Size(270, 48)
        Me.TextBox1.TabIndex = 7
        Me.TextBox1.Text = "Select a local IP that is used to broadcast and  listen for incoming data."
        '
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.GroupBox4)
        Me.GroupBox2.Controls.Add(Me.GroupBox3)
        Me.GroupBox2.Controls.Add(Me.uxRemotePort)
        Me.GroupBox2.Controls.Add(Me.Label7)
        Me.GroupBox2.Controls.Add(Me.uxSelectedDeviceIp)
        Me.GroupBox2.Controls.Add(Me.Label3)
        Me.GroupBox2.Location = New System.Drawing.Point(741, 384)
        Me.GroupBox2.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Padding = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.GroupBox2.Size = New System.Drawing.Size(315, 372)
        Me.GroupBox2.TabIndex = 10
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "Commands"
        '
        'GroupBox4
        '
        Me.GroupBox4.Controls.Add(Me.uxLedBuzzerControlSelectionList)
        Me.GroupBox4.Controls.Add(Me.Button5)
        Me.GroupBox4.Location = New System.Drawing.Point(20, 248)
        Me.GroupBox4.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.GroupBox4.Name = "GroupBox4"
        Me.GroupBox4.Padding = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.GroupBox4.Size = New System.Drawing.Size(288, 125)
        Me.GroupBox4.TabIndex = 19
        Me.GroupBox4.TabStop = False
        Me.GroupBox4.Text = "Control LED/Buzzer Selection"
        '
        'uxLedBuzzerControlSelectionList
        '
        Me.uxLedBuzzerControlSelectionList.FormattingEnabled = True
        Me.uxLedBuzzerControlSelectionList.Location = New System.Drawing.Point(26, 28)
        Me.uxLedBuzzerControlSelectionList.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.uxLedBuzzerControlSelectionList.Name = "uxLedBuzzerControlSelectionList"
        Me.uxLedBuzzerControlSelectionList.Size = New System.Drawing.Size(244, 28)
        Me.uxLedBuzzerControlSelectionList.TabIndex = 12
        '
        'Button5
        '
        Me.Button5.Location = New System.Drawing.Point(24, 67)
        Me.Button5.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.Button5.Name = "Button5"
        Me.Button5.Size = New System.Drawing.Size(246, 47)
        Me.Button5.TabIndex = 11
        Me.Button5.Text = "Control LED/Buzzer"
        Me.Button5.UseVisualStyleBackColor = True
        '
        'GroupBox3
        '
        Me.GroupBox3.Controls.Add(Me.Button3)
        Me.GroupBox3.Controls.Add(Me.Label5)
        Me.GroupBox3.Controls.Add(Me.uxOpenDoorDuration)
        Me.GroupBox3.Location = New System.Drawing.Point(20, 111)
        Me.GroupBox3.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.GroupBox3.Name = "GroupBox3"
        Me.GroupBox3.Padding = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.GroupBox3.Size = New System.Drawing.Size(288, 125)
        Me.GroupBox3.TabIndex = 18
        Me.GroupBox3.TabStop = False
        Me.GroupBox3.Text = "Control Relay"
        '
        'Button3
        '
        Me.Button3.Location = New System.Drawing.Point(24, 67)
        Me.Button3.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.Button3.Name = "Button3"
        Me.Button3.Size = New System.Drawing.Size(246, 47)
        Me.Button3.TabIndex = 11
        Me.Button3.Text = "Open Door"
        Me.Button3.UseVisualStyleBackColor = True
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(20, 28)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(74, 20)
        Me.Label5.TabIndex = 13
        Me.Label5.Text = "Duration:"
        '
        'uxOpenDoorDuration
        '
        Me.uxOpenDoorDuration.Location = New System.Drawing.Point(140, 25)
        Me.uxOpenDoorDuration.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.uxOpenDoorDuration.Maximum = New Decimal(New Integer() {255, 0, 0, 0})
        Me.uxOpenDoorDuration.Name = "uxOpenDoorDuration"
        Me.uxOpenDoorDuration.Size = New System.Drawing.Size(98, 26)
        Me.uxOpenDoorDuration.TabIndex = 14
        Me.uxOpenDoorDuration.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        Me.uxOpenDoorDuration.Value = New Decimal(New Integer() {3, 0, 0, 0})
        '
        'uxRemotePort
        '
        Me.uxRemotePort.Location = New System.Drawing.Point(160, 69)
        Me.uxRemotePort.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.uxRemotePort.Maximum = New Decimal(New Integer() {65535, 0, 0, 0})
        Me.uxRemotePort.Name = "uxRemotePort"
        Me.uxRemotePort.Size = New System.Drawing.Size(98, 26)
        Me.uxRemotePort.TabIndex = 17
        Me.uxRemotePort.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        Me.uxRemotePort.Value = New Decimal(New Integer() {2167, 0, 0, 0})
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Location = New System.Drawing.Point(24, 72)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(103, 20)
        Me.Label7.TabIndex = 15
        Me.Label7.Text = "Remote Port:"
        '
        'uxSelectedDeviceIp
        '
        Me.uxSelectedDeviceIp.Location = New System.Drawing.Point(160, 28)
        Me.uxSelectedDeviceIp.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.uxSelectedDeviceIp.Multiline = True
        Me.uxSelectedDeviceIp.Name = "uxSelectedDeviceIp"
        Me.uxSelectedDeviceIp.ReadOnly = True
        Me.uxSelectedDeviceIp.Size = New System.Drawing.Size(148, 32)
        Me.uxSelectedDeviceIp.TabIndex = 9
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(24, 32)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(144, 20)
        Me.Label3.TabIndex = 10
        Me.Label3.Text = "Selected device IP:"
        '
        'Button4
        '
        Me.Button4.Location = New System.Drawing.Point(778, 813)
        Me.Button4.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.Button4.Name = "Button4"
        Me.Button4.Size = New System.Drawing.Size(254, 47)
        Me.Button4.TabIndex = 12
        Me.Button4.Text = "&Exit"
        Me.Button4.UseVisualStyleBackColor = True
        '
        'uxEventMessageListView
        '
        Me.uxEventMessageListView.Cursor = System.Windows.Forms.Cursors.Default
        Me.uxEventMessageListView.ForeColor = System.Drawing.SystemColors.WindowText
        Me.uxEventMessageListView.HideSelection = False
        Me.uxEventMessageListView.Location = New System.Drawing.Point(14, 567)
        Me.uxEventMessageListView.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.uxEventMessageListView.Name = "uxEventMessageListView"
        Me.uxEventMessageListView.Size = New System.Drawing.Size(712, 188)
        Me.uxEventMessageListView.TabIndex = 14
        Me.uxEventMessageListView.UseCompatibleStateImageBehavior = False
        Me.uxEventMessageListView.View = System.Windows.Forms.View.Details
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(14, 543)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(123, 20)
        Me.Label4.TabIndex = 13
        Me.Label4.Text = "Event Message:"
        '
        'Button2
        '
        Me.Button2.Location = New System.Drawing.Point(502, 764)
        Me.Button2.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.Button2.Name = "Button2"
        Me.Button2.Size = New System.Drawing.Size(224, 47)
        Me.Button2.TabIndex = 15
        Me.Button2.Text = "Clear Event Message"
        Me.Button2.UseVisualStyleBackColor = True
        '
        'Button6
        '
        Me.Button6.Location = New System.Drawing.Point(502, 511)
        Me.Button6.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.Button6.Name = "Button6"
        Me.Button6.Size = New System.Drawing.Size(224, 47)
        Me.Button6.TabIndex = 16
        Me.Button6.Text = "Clear Received Data"
        Me.Button6.UseVisualStyleBackColor = True
        '
        'Button7
        '
        Me.Button7.Location = New System.Drawing.Point(778, 764)
        Me.Button7.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.Button7.Name = "Button7"
        Me.Button7.Size = New System.Drawing.Size(254, 47)
        Me.Button7.TabIndex = 17
        Me.Button7.Text = "Batch Colmmand"
        Me.Button7.UseVisualStyleBackColor = True
        '
        'uxOpenDoorTimer
        '
        Me.uxOpenDoorTimer.Interval = 20
        '
        'uxControlLedBuzzerTimer
        '
        Me.uxControlLedBuzzerTimer.Interval = 300
        '
        'MainForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(9.0!, 20.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1079, 876)
        Me.Controls.Add(Me.Button7)
        Me.Controls.Add(Me.Button6)
        Me.Controls.Add(Me.Button2)
        Me.Controls.Add(Me.uxEventMessageListView)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.Button4)
        Me.Controls.Add(Me.GroupBox2)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.uxEventDataListView)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.uxDeviceListView)
        Me.Controls.Add(Me.Label1)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.Name = "MainForm"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "RapidACCESS RER750 Manager"
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        CType(Me.uxListenPort, System.ComponentModel.ISupportInitialize).EndInit()
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox2.PerformLayout()
        Me.GroupBox4.ResumeLayout(False)
        Me.GroupBox3.ResumeLayout(False)
        Me.GroupBox3.PerformLayout()
        CType(Me.uxOpenDoorDuration, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.uxRemotePort, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents uxDeviceListView As System.Windows.Forms.ListView
    Friend WithEvents Button1 As System.Windows.Forms.Button
    Friend WithEvents uxFormInitializeTimer As System.Windows.Forms.Timer
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents uxEventDataListView As System.Windows.Forms.ListView
    Friend WithEvents uxStartListen As System.Windows.Forms.Button
    Friend WithEvents uxLocalIps As System.Windows.Forms.ListBox
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents TextBox1 As System.Windows.Forms.TextBox
    Friend WithEvents GroupBox2 As System.Windows.Forms.GroupBox
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents uxSelectedDeviceIp As System.Windows.Forms.TextBox
    Friend WithEvents Button3 As System.Windows.Forms.Button
    Friend WithEvents Button4 As System.Windows.Forms.Button
    Friend WithEvents uxEventMessageListView As System.Windows.Forms.ListView
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents uxOpenDoorDuration As System.Windows.Forms.NumericUpDown
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents Button2 As System.Windows.Forms.Button
    Friend WithEvents Button6 As System.Windows.Forms.Button
    Friend WithEvents uxListenPort As System.Windows.Forms.NumericUpDown
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents uxRemotePort As System.Windows.Forms.NumericUpDown
    Friend WithEvents GroupBox3 As GroupBox
    Friend WithEvents GroupBox4 As GroupBox
    Friend WithEvents Button5 As Button
    Friend WithEvents uxLedBuzzerControlSelectionList As ComboBox
    Friend WithEvents Button7 As Button
    Friend WithEvents uxOpenDoorTimer As Timer
    Friend WithEvents uxControlLedBuzzerTimer As Timer
End Class
