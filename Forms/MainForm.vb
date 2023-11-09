Public Class MainForm
    Private WithEvents _er As New GIGATMS.Devices.ER750.ER750Lib

    Private Delegate Sub UpdateUxDeviceListViewCallBack(ByVal deviceStatus As GIGATMS.Devices.ER750.Parameters.DeviceStatusFormat)
    Private Delegate Sub UpdateUxEventDataListViewCallBack(ByVal deviceIP As String, _
                                                           ByVal deviceStatus As GIGATMS.Devices.ER750.Parameters.EventDataFormat)
    Private Delegate Sub UpdateUxEventMessageListViewCallBack(ByVal source As String, _
                                                              ByVal message As String)

    Private Sub RefreshDevices()
        uxDeviceListView.Items.Clear()
        _er.Broadcast(uxLocalIps.Text)
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        RefreshDevices()
    End Sub

    Private Sub UpdateUxEventMessageListView(ByVal source As String, _
                                             ByVal message As String)

        Dim li As Windows.Forms.ListViewItem

        Try
            li = uxEventMessageListView.Items.Add(Now)
            li.SubItems.Add(source)
            li.SubItems.Add(message)
            li.EnsureVisible()
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Private Sub _er_ErrorOccured(ByVal source As String, ByVal errorMessage As String) Handles _er.ErrorOccured
        If Me.InvokeRequired() Then
            Dim cb As New UpdateUxEventMessageListViewCallBack(AddressOf UpdateUxEventMessageListView)
            Me.Invoke(cb, New Object() {source, "***Error***: " & errorMessage})
        Else
            UpdateUxEventMessageListView(source, "***Error***: " & errorMessage)
        End If
    End Sub

    Private Sub _er_EventMessage(ByVal source As String, ByVal eventMessage As String) Handles _er.EventMessage
        If Me.InvokeRequired() Then
            Dim cb As New UpdateUxEventMessageListViewCallBack(AddressOf UpdateUxEventMessageListView)
            Me.Invoke(cb, New Object() {source, eventMessage})
        Else
            UpdateUxEventmessageListView(source, eventMessage)
        End If
    End Sub


    Private Sub _er_ReceivedData(ByVal remoteIpAddress As String, ByVal remotePort As Integer, ByVal dataByteArray() As Byte) Handles _er.ReceivedData
        Debug.Print(remoteIpAddress)
    End Sub

    Private Sub UpdateUxDeviceListView(ByVal deviceStatus As GIGATMS.Devices.ER750.Parameters.DeviceStatusFormat)
        Dim li As Windows.Forms.ListViewItem

        Try
            li = uxDeviceListView.Items.Add(deviceStatus.IpAddress)
            li.SubItems.Add(deviceStatus.MacAddress)
            li.SubItems.Add(deviceStatus.FirmwareVersion)
            li.SubItems.Add(deviceStatus.DeviceName)
            li.EnsureVisible()
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

    End Sub


    Private Sub UpdateUxEventDataListView(ByVal deviceIP As String, _
                                          ByVal eventData As GIGATMS.Devices.ER750.Parameters.EventDataFormat)
        Dim li As Windows.Forms.ListViewItem

        Try
            li = uxEventDataListView.Items.Add(deviceIP)
            li.SubItems.Add(Now)
            li.SubItems.Add(eventData.DataHexString)
            li.SubItems.Add(eventData.DeviceName)
            li.EnsureVisible()

            RunBatchCommand(deviceIP)
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Private Sub _er_ReceivedDeviceStatus(ByRef deviceStatus As GIGATMS.Devices.ER750.Parameters.DeviceStatusFormat) Handles _er.ReceivedDeviceStatus
        If Me.InvokeRequired() Then
            Dim cb As New UpdateUxDeviceListViewCallBack(AddressOf UpdateUxDeviceListView)
            Me.Invoke(cb, deviceStatus)
        Else
            UpdateUxDeviceListView(deviceStatus)
        End If
    End Sub

    Private Sub MainForm_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Me.Text = My.Application.Info.Title & _
                  " V" & My.Application.Info.Version.Major & "." & _
                  My.Application.Info.Version.Minor & "R" & _
                  My.Application.Info.Version.Revision

        uxFormInitializeTimer.Enabled = True

    End Sub


    Private Sub InitializeUx()

        With uxDeviceListView
            .View = View.Details
            .FullRowSelect = True
            .GridLines = True
            .HideSelection = False
            .Columns.Clear()
            .Columns.Add("IP", CInt(.Width / 5))
            .Columns.Add("MAC", CInt(.Width / 4))
            .Columns.Add("Version", CInt(.Width / 7))
            .Columns.Add("Name", CInt(.Width / 2))
        End With

        Dim localIPs() As String = Nothing
        Dim localIpCount As Integer
        Dim i As Integer

        _er.GetLocalIP(localIpCount, localIPs)
        For i = 0 To localIpCount - 1
            uxLocalIps.Items.Add(localIPs(i))
        Next

        If localIpCount > 0 Then
            uxLocalIps.SelectedIndex = 0
        End If

        With uxEventDataListView
            .View = View.Details
            .FullRowSelect = True
            .GridLines = True
            .HideSelection = False
            .Columns.Clear()
            .Columns.Add("IP", CInt(.Width / 5))
            .Columns.Add("Date/Time", CInt(.Width / 3))
            .Columns.Add("Event Data", CInt(.Width / 5))
            .Columns.Add("Device Name", CInt(.Width / 2))
        End With

        With uxEventMessageListView
            .View = View.Details
            .FullRowSelect = True
            .GridLines = True
            .HideSelection = False
            .Columns.Clear()
            .Columns.Add("Date/Time", CInt(.Width / 4))
            .Columns.Add("Sourece", CInt(.Width / 4))
            .Columns.Add("Message", CInt(.Width / 2))
        End With


        With uxLedBuzzerControlSelectionList
            .DropDownStyle = ComboBoxStyle.DropDownList
            .Items.Clear()
            .Items.Add("0- Turn LED off")
            .Items.Add("1- Green on")
            .Items.Add("2- Green off")
            .Items.Add("3- Red on")
            .Items.Add("4- Red off")
            .Items.Add("5- 1 beep")
            .Items.Add("6- 3 beeps")
            .Items.Add("7- Green on with 1 beep")
            .Items.Add("8- Red on with 3 beeps")
            .Items.Add("9- Red, Green LED on")
            .Items.Add("C- Buzzer always beep")
            .Items.Add("D- Buzzer off")
            .SelectedIndex = 0
        End With
    End Sub

    Private Sub uxFormInitializeTimer_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles uxFormInitializeTimer.Tick
        uxFormInitializeTimer.Enabled = False

        InitializeUx()
    End Sub

    Private Sub uxDeviceListView_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles uxDeviceListView.SelectedIndexChanged
        If uxDeviceListView.SelectedItems.Count > 0 Then
            uxSelectedDeviceIp.Text = uxDeviceListView.SelectedItems.Item(0).Text
        End If
    End Sub

    Private Sub Button4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button4.Click
        End
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles uxStartListen.Click
        Select Case uxStartListen.Text
            Case "Start Listening"
                _er.StartListen(uxLocalIps.Text, uxListenPort.Value)
                uxStartListen.Text = "Stop Listening"
            Case "Stop Listening"
                _er.StopListen()
                uxStartListen.Text = "Start Listening"
            Case Else
                Stop
        End Select


    End Sub

    Private Sub _er_ReceivedEventData(ByVal deviceIP As String, _
                                      ByRef eventData As GIGATMS.Devices.ER750.Parameters.EventDataFormat) Handles _er.ReceivedEventData
        If Me.InvokeRequired() Then
            Dim cb As New UpdateUxEventDataListViewCallBack(AddressOf UpdateUxEventDataListView)
            Me.Invoke(cb, New Object() {deviceIP, eventData})
        Else
            UpdateUxEventDataListView(deviceIP, eventData)
        End If
    End Sub

    Private Sub uxEventDataListView_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles uxEventDataListView.SelectedIndexChanged
        If uxEventDataListView.SelectedItems.Count > 0 Then
            uxSelectedDeviceIp.Text = uxEventDataListView.SelectedItems.Item(0).Text
        End If
    End Sub

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        Try
            _er.OpenDoor(uxSelectedDeviceIp.Text, _
                         uxRemotePort.Value, _
                         uxOpenDoorDuration.Value)
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Private Sub Button6_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button6.Click
        uxEventDataListView.Items.Clear()
    End Sub

    Private Sub Button2_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        uxEventMessageListView.Items.Clear()
    End Sub

    Private Sub uxSelectedDeviceIp_TextChanged(sender As Object, e As EventArgs) Handles uxSelectedDeviceIp.TextChanged

    End Sub

    Private Sub GroupBox4_Enter(sender As Object, e As EventArgs) Handles GroupBox4.Enter

    End Sub

    Private Sub uxOpenDoorDuration_ValueChanged(sender As Object, e As EventArgs) Handles uxOpenDoorDuration.ValueChanged

    End Sub

    Private Sub GroupBox3_Enter(sender As Object, e As EventArgs) Handles GroupBox3.Enter

    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        Try
            Dim selectedFunction As Integer

            Select Case uxLedBuzzerControlSelectionList.Text.Substring(0, 1)
                Case "C"
                    selectedFunction = Asc("C") - Asc("0")
                Case "D"
                    selectedFunction = Asc("D") - Asc("0")
                Case Else
                    selectedFunction = uxLedBuzzerControlSelectionList.SelectedIndex
            End Select

            _er.ControlLedBuzzer(uxSelectedDeviceIp.Text,
                                 uxRemotePort.Value,
                                 selectedFunction)
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Private Sub uxLocalIps_SelectedIndexChanged(sender As Object, e As EventArgs) Handles uxLocalIps.SelectedIndexChanged

    End Sub

    Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles TextBox1.TextChanged

    End Sub

    Private Sub RunBatchCommand(ByVal deviceIP As String)
        If deviceIP <> "" Then
            uxOpenDoorTimer.Tag = deviceIP
            uxOpenDoorTimer.Enabled = True
        Else
            MsgBox("Please select a device to run Batch Command")
        End If

    End Sub

    Private Sub Button7_Click(sender As Object, e As EventArgs) Handles Button7.Click

        RunBatchCommand(uxSelectedDeviceIp.Text)
    End Sub

    Private Sub uxBatchCommandTimer_Tick(sender As Object, e As EventArgs) Handles uxOpenDoorTimer.Tick
        uxOpenDoorTimer.Enabled = False

        Dim deviceIP As String

        deviceIP = uxOpenDoorTimer.Tag

        _er.OpenDoor(deviceIP,
                     uxRemotePort.Value,
                     uxOpenDoorDuration.Value,
                     False)
        uxControlLedBuzzerTimer.Tag = deviceIP

        uxControlLedBuzzerTimer.Enabled = True
    End Sub

    Private Sub uxControlLedBuzzerTimer_Tick(sender As Object, e As EventArgs) Handles uxControlLedBuzzerTimer.Tick
        uxControlLedBuzzerTimer.Enabled = False

        Dim deviceIP As String

        deviceIP = uxControlLedBuzzerTimer.Tag

        _er.ControlLedBuzzer(deviceIP,
                             uxRemotePort.Value,
                             uxLedBuzzerControlSelectionList.SelectedIndex,
                             False)
    End Sub
End Class