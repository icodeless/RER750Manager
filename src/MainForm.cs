using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Reflection;

namespace RER750Manager
{
    public partial class MainForm : Form
    {
        private GIGATMS.Devices.ER750.ER750Lib _er = new GIGATMS.Devices.ER750.ER750Lib();
        private readonly string _debugLogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "ER750_Debug.log");

        // Delegates
        private delegate void UpdateUxDeviceListViewCallBack(GIGATMS.Devices.ER750.Parameters.DeviceStatusFormat deviceStatus);
        private delegate void UpdateUxEventDataListViewCallBack(string deviceIP, GIGATMS.Devices.ER750.Parameters.EventDataFormat deviceStatus);
        private delegate void UpdateUxEventMessageListViewCallBack(string source, string message);

        public MainForm()
        {
            InitializeComponent();
            RegisterEventHandlers();
            LogDebug("Application initialized");
        }

        #region Debugging Utilities
        private void LogDebug(string message)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                string logMessage = $"[{timestamp}] {message}{Environment.NewLine}";
                File.AppendAllText(_debugLogPath, logMessage);
                Debug.WriteLine(logMessage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Logging failed: {ex.Message}");
            }
        }

        private void DumpObjectToLog(object obj, string prefix = "")
        {
            try
            {
                if (obj == null)
                {
                    LogDebug($"{prefix}Object is null");
                    return;
                }

                foreach (var prop in obj.GetType().GetProperties())
                {
                    try
                    {
                        LogDebug($"{prefix}{prop.Name} = {prop.GetValue(obj, null)}");
                    }
                    catch (Exception ex)
                    {
                        LogDebug($"{prefix}Failed to read {prop.Name}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                LogDebug($"{prefix}Dump failed: {ex.Message}");
            }
        }
        #endregion

        #region Event Handler Registration
        private void RegisterEventHandlers()
        {
            try
            {
                _er.ReceivedDeviceStatus += _er_ReceivedDeviceStatus;
                _er.ReceivedEventData += _er_ReceivedEventData;
                _er.EventMessage += _er_EventMessage;
                _er.ErrorOccured += _er_ErrorOccured;
                _er.ReceivedData += _er_ReceivedData;
                LogDebug("Event handlers registered successfully");
            }
            catch (Exception ex)
            {
                LogDebug($"Failed to register event handlers: {ex}");
                MessageBox.Show($"Failed to initialize communication: {ex.Message}");
            }
        }
        #endregion

        #region Core Device Communication
        private async void RefreshDevices()
        {
            try
            {
                LogDebug($"Starting broadcast using IP: {uxLocalIps.Text}");
                uxDeviceListView.BeginUpdate();
                uxDeviceListView.Items.Clear();

                if (string.IsNullOrEmpty(uxLocalIps.Text))
                {
                    MessageBox.Show("Please select a local IP first");
                    return;
                }

                // Remove the bool assignment since Broadcast() returns void
                _er.Broadcast(uxLocalIps.Text);
                LogDebug("Broadcast initiated");

                await Task.Delay(1000);
                LogDebug($"Broadcast completed. Found {uxDeviceListView.Items.Count} devices");
            }
            catch (Exception ex)
            {
                LogDebug($"Broadcast error: {ex}");
                MessageBox.Show($"Broadcast failed: {ex.Message}");
            }
            finally
            {
                uxDeviceListView.EndUpdate();
            }
        }
        private void RunBatchCommand(string deviceIP)
        {
            try
            {
                if (string.IsNullOrEmpty(deviceIP))
                {
                    LogDebug("Batch command failed - no device IP specified");
                    MessageBox.Show("Please select a device to run Batch Command");
                    return;
                }

                LogDebug($"Starting batch command for {deviceIP}");
                uxOpenDoorTimer.Tag = deviceIP;
                uxOpenDoorTimer.Enabled = true;
            }
            catch (Exception ex)
            {
                LogDebug($"Batch command failed: {ex}");
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        #region UI Update Methods
        private void UpdateUxDeviceListView(GIGATMS.Devices.ER750.Parameters.DeviceStatusFormat deviceStatus)
        {
            try
            {
                LogDebug($"Updating UI with device: {deviceStatus.IpAddress}");
                
                var li = new ListViewItem(deviceStatus.IpAddress);
                li.SubItems.Add(deviceStatus.MacAddress ?? "N/A");
                li.SubItems.Add(deviceStatus.FirmwareVersion ?? "N/A");
                li.SubItems.Add(deviceStatus.DeviceName ?? "N/A");

                if (uxDeviceListView.InvokeRequired)
                {
                    uxDeviceListView.Invoke(new Action(() => {
                        uxDeviceListView.Items.Add(li);
                        li.EnsureVisible();
                    }));
                }
                else
                {
                    uxDeviceListView.Items.Add(li);
                    li.EnsureVisible();
                }
            }
            catch (Exception ex)
            {
                LogDebug($"Device list update failed: {ex}");
            }
        }

        private void UpdateUxEventDataListView(string deviceIP, GIGATMS.Devices.ER750.Parameters.EventDataFormat eventData)
        {
            try
            {
                LogDebug($"New event data from {deviceIP}");
                
                var li = new ListViewItem(deviceIP);
                li.SubItems.Add(DateTime.Now.ToString());
                li.SubItems.Add(eventData.DataHexString ?? "N/A");
                li.SubItems.Add(eventData.DeviceName ?? "N/A");

                if (uxEventDataListView.InvokeRequired)
                {
                    uxEventDataListView.Invoke(new Action(() => {
                        uxEventDataListView.Items.Add(li);
                        li.EnsureVisible();
                    }));
                }
                else
                {
                    uxEventDataListView.Items.Add(li);
                    li.EnsureVisible();
                }

                RunBatchCommand(deviceIP);
            }
            catch (Exception ex)
            {
                LogDebug($"Event data update failed: {ex}");
            }
        }

        private void UpdateUxEventMessageListView(string source, string message)
        {
            try
            {
                var li = new ListViewItem(DateTime.Now.ToString());
                li.SubItems.Add(source ?? "N/A");
                li.SubItems.Add(message ?? "N/A");

                if (uxEventMessageListView.InvokeRequired)
                {
                    uxEventMessageListView.Invoke(new Action(() => {
                        uxEventMessageListView.Items.Add(li);
                        li.EnsureVisible();
                    }));
                }
                else
                {
                    uxEventMessageListView.Items.Add(li);
                    li.EnsureVisible();
                }
            }
            catch (Exception ex)
            {
                LogDebug($"Message update failed: {ex}");
            }
        }
        #endregion

        #region ER750 Library Event Handlers
        private void _er_ReceivedDeviceStatus(ref GIGATMS.Devices.ER750.Parameters.DeviceStatusFormat deviceStatus)
        {
            LogDebug($"Device status received from {deviceStatus.IpAddress}");
            DumpObjectToLog(deviceStatus, "DeviceStatus_");
            
            var statusCopy = deviceStatus;
            UpdateUxDeviceListView(statusCopy);
        }

        private void _er_ReceivedEventData(string deviceIP, ref GIGATMS.Devices.ER750.Parameters.EventDataFormat eventData)
        {
            LogDebug($"Event data received from {deviceIP}");
            DumpObjectToLog(eventData, "EventData_");
            
            var dataCopy = eventData;
            UpdateUxEventDataListView(deviceIP, dataCopy);
        }

        private void _er_EventMessage(string source, string eventMessage)
        {
            LogDebug($"Event message: {source} - {eventMessage}");
            UpdateUxEventMessageListView(source, eventMessage);
        }

        private void _er_ErrorOccured(string source, string errorMessage)
        {
            LogDebug($"ERROR [{source}]: {errorMessage}");
            UpdateUxEventMessageListView(source, $"***ERROR***: {errorMessage}");
        }

        private void _er_ReceivedData(string remoteIpAddress, int remotePort, byte[] dataByteArray)
        {
            string hexData = dataByteArray != null ? BitConverter.ToString(dataByteArray) : "null";
            LogDebug($"Raw data from {remoteIpAddress}:{remotePort} - {hexData}");
        }
        #endregion

        #region Form Events
        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                this.Text = $"{Application.ProductName} V{version.Major}.{version.Minor}R{version.Revision}";
                uxFormInitializeTimer.Enabled = true;
                LogDebug($"MainForm loaded. Version: {version}");
            }
            catch (Exception ex)
            {
                LogDebug($"Load error: {ex}");
                this.Text = Application.ProductName;
            }
        }

        private void uxFormInitializeTimer_Tick(object sender, EventArgs e)
        {
            uxFormInitializeTimer.Enabled = false;
            try
            {
                InitializeUx();
                LogDebug("UI initialization completed");
            }
            catch (Exception ex)
            {
                LogDebug($"UI init failed: {ex}");
            }
        }

        private void InitializeUx()
        {
            uxDeviceListView.View = View.Details;
            uxDeviceListView.FullRowSelect = true;
            uxDeviceListView.GridLines = true;
            uxDeviceListView.HideSelection = false;
            uxDeviceListView.Columns.Clear();
            uxDeviceListView.Columns.Add("IP", uxDeviceListView.Width / 5);
            uxDeviceListView.Columns.Add("MAC", uxDeviceListView.Width / 4);
            uxDeviceListView.Columns.Add("Version", uxDeviceListView.Width / 7);
            uxDeviceListView.Columns.Add("Name", uxDeviceListView.Width / 2);

            string[] localIPs = null;
            int localIpCount = 0;
            
            _er.GetLocalIP(ref localIpCount, ref localIPs);
            for (int i = 0; i < localIpCount; i++)
            {
                uxLocalIps.Items.Add(localIPs[i]);
            }

            if (localIpCount > 0)
            {
                uxLocalIps.SelectedIndex = 0;
            }

            uxEventDataListView.View = View.Details;
            uxEventDataListView.FullRowSelect = true;
            uxEventDataListView.GridLines = true;
            uxEventDataListView.HideSelection = false;
            uxEventDataListView.Columns.Clear();
            uxEventDataListView.Columns.Add("IP", uxEventDataListView.Width / 5);
            uxEventDataListView.Columns.Add("Date/Time", uxEventDataListView.Width / 3);
            uxEventDataListView.Columns.Add("Event Data", uxEventDataListView.Width / 5);
            uxEventDataListView.Columns.Add("Device Name", uxEventDataListView.Width / 2);

            uxEventMessageListView.View = View.Details;
            uxEventMessageListView.FullRowSelect = true;
            uxEventMessageListView.GridLines = true;
            uxEventMessageListView.HideSelection = false;
            uxEventMessageListView.Columns.Clear();
            uxEventMessageListView.Columns.Add("Date/Time", uxEventMessageListView.Width / 4);
            uxEventMessageListView.Columns.Add("Source", uxEventMessageListView.Width / 4);
            uxEventMessageListView.Columns.Add("Message", uxEventMessageListView.Width / 2);

            uxLedBuzzerControlSelectionList.DropDownStyle = ComboBoxStyle.DropDownList;
            uxLedBuzzerControlSelectionList.Items.Clear();
            uxLedBuzzerControlSelectionList.Items.Add("0- Turn LED off");
            uxLedBuzzerControlSelectionList.Items.Add("1- Green on");
            uxLedBuzzerControlSelectionList.Items.Add("2- Green off");
            uxLedBuzzerControlSelectionList.Items.Add("3- Red on");
            uxLedBuzzerControlSelectionList.Items.Add("4- Red off");
            uxLedBuzzerControlSelectionList.Items.Add("5- 1 beep");
            uxLedBuzzerControlSelectionList.Items.Add("6- 3 beeps");
            uxLedBuzzerControlSelectionList.Items.Add("7- Green on with 1 beep");
            uxLedBuzzerControlSelectionList.Items.Add("8- Red on with 3 beeps");
            uxLedBuzzerControlSelectionList.Items.Add("9- Red, Green LED on");
            uxLedBuzzerControlSelectionList.Items.Add("C- Buzzer always beep");
            uxLedBuzzerControlSelectionList.Items.Add("D- Buzzer off");
            uxLedBuzzerControlSelectionList.SelectedIndex = 0;
        }
        #endregion

        #region UI Event Handlers
        private void Button1_Click(object sender, EventArgs e) => RefreshDevices();
        private void Button3_Click(object sender, EventArgs e) => OpenDoor();
        private void Button5_Click(object sender, EventArgs e) => ControlLedBuzzer();
        private void Button7_Click(object sender, EventArgs e) => RunBatchCommand(uxSelectedDeviceIp.Text);

        private void OpenDoor()
        {
            try
            {
                LogDebug($"Opening door on {uxSelectedDeviceIp.Text}");
                _er.OpenDoor(uxSelectedDeviceIp.Text,
                            (ushort)uxRemotePort.Value,
                            (byte)uxOpenDoorDuration.Value);
            }
            catch (Exception ex)
            {
                LogDebug($"Open door failed: {ex}");
                MessageBox.Show(ex.Message);
            }
        }

        private void ControlLedBuzzer()
        {
            try
            {
                byte function = GetSelectedFunction();
                LogDebug($"Controlling LED/Buzzer on {uxSelectedDeviceIp.Text} with function {function}");
                
                _er.ControlLedBuzzer(uxSelectedDeviceIp.Text,
                                   (ushort)uxRemotePort.Value,
                                   function);
            }
            catch (Exception ex)
            {
                LogDebug($"LED/Buzzer control failed: {ex}");
                MessageBox.Show(ex.Message);
            }
        }

        private byte GetSelectedFunction()
        {
            string selection = uxLedBuzzerControlSelectionList.Text.Substring(0, 1);
            switch (selection)
            {
                case "C": return (byte)('C' - '0');
                case "D": return (byte)('D' - '0');
                default: return (byte)uxLedBuzzerControlSelectionList.SelectedIndex;
            }
        }
        #endregion

        #region Missing Event Handlers (Added to fix errors)
        private void GroupBox3_Enter(object sender, EventArgs e) { /* Required but empty handler */ }
        private void GroupBox4_Enter(object sender, EventArgs e) { /* Required but empty handler */ }
        private void TextBox1_TextChanged(object sender, EventArgs e) { /* Required but empty handler */ }
        private void uxBatchCommandTimer_Tick(object sender, EventArgs e) => uxOpenDoorTimer_Tick(sender, e);
        private void uxLocalIps_SelectedIndexChanged(object sender, EventArgs e) { /* Required but empty handler */ }
        private void uxOpenDoorDuration_ValueChanged(object sender, EventArgs e) { /* Required but empty handler */ }
        private void uxSelectedDeviceIp_TextChanged(object sender, EventArgs e) { /* Required but empty handler */ }
        private void uxStartListen_Click(object sender, EventArgs e)
        {
            try
            {
                switch (uxStartListen.Text)
                {
                    case "▶ Start Listening":
                        _er.StartListen(uxLocalIps.Text, (ushort)uxListenPort.Value);
                        uxStartListen.Text = "⏹ Stop Listening";
                        LogDebug("Started listening for device data");
                        break;
                    case "⏹ Stop Listening":
                        _er.StopListen();
                        uxStartListen.Text = "▶ Start Listening";
                        LogDebug("Stopped listening for device data");
                        break;
                    default:
                        Debug.Assert(false);
                        break;
                }
            }
            catch (Exception ex)
            {
                LogDebug($"Listen control failed: {ex}");
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        #region Timer Events
        private void uxOpenDoorTimer_Tick(object sender, EventArgs e)
        {
            uxOpenDoorTimer.Enabled = false;
            try
            {
                string deviceIP = uxOpenDoorTimer.Tag as string;
                LogDebug($"Batch door open for {deviceIP}");
                
                _er.OpenDoor(deviceIP,
                            (ushort)uxRemotePort.Value,
                            (byte)uxOpenDoorDuration.Value,
                            false);
                
                uxControlLedBuzzerTimer.Tag = deviceIP;
                uxControlLedBuzzerTimer.Enabled = true;
            }
            catch (Exception ex)
            {
                LogDebug($"Batch door open failed: {ex}");
            }
        }

        private void uxControlLedBuzzerTimer_Tick(object sender, EventArgs e)
        {
            uxControlLedBuzzerTimer.Enabled = false;
            try
            {
                string deviceIP = uxControlLedBuzzerTimer.Tag as string;
                LogDebug($"Batch LED control for {deviceIP}");
                
                _er.ControlLedBuzzer(deviceIP,
                                   (ushort)uxRemotePort.Value,
                                   (byte)uxLedBuzzerControlSelectionList.SelectedIndex,
                                   false);
            }
            catch (Exception ex)
            {
                LogDebug($"Batch LED control failed: {ex}");
            }
        }
        #endregion

        #region Other Event Handlers
        private void uxDeviceListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (uxDeviceListView.SelectedItems.Count > 0)
            {
                uxSelectedDeviceIp.Text = uxDeviceListView.SelectedItems[0].Text;
            }
        }

        private void uxEventDataListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (uxEventDataListView.SelectedItems.Count > 0)
            {
                uxSelectedDeviceIp.Text = uxEventDataListView.SelectedItems[0].Text;
            }
        }

        private void Button4_Click(object sender, EventArgs e) => Application.Exit();
        private void Button2_Click_1(object sender, EventArgs e) => uxEventMessageListView.Items.Clear();
        private void Button6_Click_1(object sender, EventArgs e) => uxEventDataListView.Items.Clear();
        #endregion
    }
}