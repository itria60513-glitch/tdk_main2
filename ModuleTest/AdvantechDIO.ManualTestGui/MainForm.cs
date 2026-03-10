using AdvantechDIO.Config;
using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using LogUtility;
using TDKLogUtility.Module;

namespace AdvantechDIO.ManualTestGui
{
    public partial class MainForm : Form
    {
        private readonly ILogUtility _logger;

        private AdvantechDIO.Module.AdvantechDIO _dioDev0;
        private AdvantechDIO.Module.AdvantechDIO _dioDev1;
        private readonly AdvantechDIOConfig _configDev0;
        private readonly AdvantechDIOConfig _configDev1;
        private Label[] _diLedsDev0;
        private Label[] _diLedsDev1;
        private Label[] _doLedsDev0;
        private Label[] _doLedsDev1;
        private DeviceTestControls _testControlsDev0;
        private DeviceTestControls _testControlsDev1;
        private Timer _statusRefreshTimer;
        private bool _isRefreshingStatusPanel;
        private ToolTip _toolTip;

        public MainForm()
        {
            InitializeComponent();

            RemoveDesignPreviewControls(grpDevice0);
            RemoveDesignPreviewControls(grpDevice1);

            _logger = new LogUtilityClient();
            _toolTip = new ToolTip { AutoPopDelay = 5000, InitialDelay = 400, ShowAlways = true };

            _configDev0 = new AdvantechDIOConfig
            {
                DeviceID = 0,
                DIPortMax = 16,
                DOPortMax = 8,
                PinCountPerPort = 8
            };
            _configDev1 = new AdvantechDIOConfig
            {
                DeviceID = 1,
                DIPortMax = 16,
                DOPortMax = 8,
                PinCountPerPort = 8
            };

            InitializeDevicePanel(grpDevice0, 0, out _diLedsDev0, out _doLedsDev0);
            InitializeDevicePanel(grpDevice1, 1, out _diLedsDev1, out _doLedsDev1);

            SetDeviceOfflineStyle(_diLedsDev0, _doLedsDev0, lblDev0State);
            SetDeviceOfflineStyle(_diLedsDev1, _doLedsDev1, lblDev1State);

            _toolTip.SetToolTip(btnConnectDev0, "Connect to Device 0");
            _toolTip.SetToolTip(btnDisconnectDev0, "Disconnect from Device 0");
            _toolTip.SetToolTip(btnConnectDev1, "Connect to Device 1");
            _toolTip.SetToolTip(btnDisconnectDev1, "Disconnect from Device 1");

            _statusRefreshTimer = new Timer { Interval = 500 };
            _statusRefreshTimer.Tick += StatusRefreshTimer_Tick;
            _statusRefreshTimer.Start();

            WriteStatus("Application started.");
        }

        private static void RemoveDesignPreviewControls(Control parent)
        {
            for (int i = parent.Controls.Count - 1; i >= 0; i--)
            {
                if (Equals(parent.Controls[i].Tag, "DesignPreview"))
                {
                    parent.Controls.RemoveAt(i);
                }
            }
        }

        private void InitializeDevicePanel(GroupBox grp, int deviceId, out Label[] diLeds, out Label[] doLeds)
        {
            const int bitCount = 8;
            const int ledSize = 28;
            const int cellWidth = 42;
            const int startX = 48;
            const int diBitLabelY = 202;
            const int diLedY = 218;
            const int doBitLabelY = 252;
            const int doLedY = 268;

            DeviceTestControls testControls = CreateDeviceTestControls(grp, deviceId);
            if (deviceId == 0)
            {
                _testControlsDev0 = testControls;
            }
            else
            {
                _testControlsDev1 = testControls;
            }

            grp.Controls.Add(new Label
            {
                Text = "DI",
                AutoSize = true,
                Location = new Point(10, diLedY + 8),
                Font = new Font(Font, FontStyle.Bold)
            });

            grp.Controls.Add(new Label
            {
                Text = "DO",
                AutoSize = true,
                Location = new Point(10, doLedY + 8),
                Font = new Font(Font, FontStyle.Bold)
            });

            diLeds = new Label[bitCount];
            doLeds = new Label[bitCount];

            for (int bit = 0; bit < bitCount; bit++)
            {
                int x = startX + bit * cellWidth;

                // DI bit number label
                grp.Controls.Add(new Label
                {
                    Text = bit.ToString(CultureInfo.InvariantCulture),
                    AutoSize = true,
                    Location = new Point(x + 9, diBitLabelY)
                });

                // DI LED (read-only indicator)
                Label diLed = new Label
                {
                    BorderStyle = BorderStyle.FixedSingle,
                    Location = new Point(x, diLedY),
                    Size = new Size(ledSize, ledSize),
                    BackColor = Color.DarkGray,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                grp.Controls.Add(diLed);
                diLeds[bit] = diLed;

                // DO bit number label
                grp.Controls.Add(new Label
                {
                    Text = bit.ToString(CultureInfo.InvariantCulture),
                    AutoSize = true,
                    Location = new Point(x + 9, doBitLabelY)
                });

                // DO LED (clickable to toggle)
                Label doLed = new Label
                {
                    BorderStyle = BorderStyle.Fixed3D,
                    Location = new Point(x, doLedY),
                    Size = new Size(ledSize, ledSize),
                    BackColor = Color.DarkGray,
                    Cursor = Cursors.Hand,
                    Tag = (deviceId << 8) | bit,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                doLed.Click += DoLed_Click;
                _toolTip.SetToolTip(doLed, $"Click to toggle DO bit {bit}");
                grp.Controls.Add(doLed);
                doLeds[bit] = doLed;
            }
        }

        private DeviceTestControls CreateDeviceTestControls(GroupBox grp, int deviceId)
        {
            DeviceTestControls controls = new DeviceTestControls();

            // SnapStart / SnapStop — row below Connect/Disconnect to avoid overlap
            controls.SnapStartButton = new Button { Name = $"btnDev{deviceId}SnapStart", Location = new Point(10, 52), Size = new Size(88, 24), Text = "SnapStart" };
            controls.SnapStartButton.Click += (s, e) => ExecuteSnapStart(deviceId);
            _toolTip.SetToolTip(controls.SnapStartButton, "Start interrupt-driven DI monitoring (enables ChangeOfState events)");
            grp.Controls.Add(controls.SnapStartButton);

            controls.SnapStopButton = new Button { Name = $"btnDev{deviceId}SnapStop", Location = new Point(104, 52), Size = new Size(88, 24), Text = "SnapStop" };
            controls.SnapStopButton.Click += (s, e) => ExecuteSnapStop(deviceId);
            _toolTip.SetToolTip(controls.SnapStopButton, "Stop interrupt-driven DI monitoring");
            grp.Controls.Add(controls.SnapStopButton);

            // DI test row
            const int diY = 86;
            grp.Controls.Add(new Label { AutoSize = true, Location = new Point(10, diY + 2), Text = "DI Port" });
            controls.DiPortTextBox = new TextBox { Location = new Point(58, diY), Size = new Size(36, 20), Text = "0" };
            _toolTip.SetToolTip(controls.DiPortTextBox, "Port index (0-based): 0 or 1 for DI");
            grp.Controls.Add(controls.DiPortTextBox);

            grp.Controls.Add(new Label { AutoSize = true, Location = new Point(103, diY + 2), Text = "Bit" });
            controls.DiBitTextBox = new TextBox { Location = new Point(126, diY), Size = new Size(36, 20), Text = "0" };
            _toolTip.SetToolTip(controls.DiBitTextBox, "Bit index within the port (0-7)");
            grp.Controls.Add(controls.DiBitTextBox);

            controls.GetDiPortButton = new Button { Name = $"btnDev{deviceId}GetDiPort", Location = new Point(172, diY - 1), Size = new Size(88, 23), Text = "Get DI Port" };
            controls.GetDiPortButton.Click += (s, e) => ExecuteGetDiPort(deviceId);
            _toolTip.SetToolTip(controls.GetDiPortButton, "Read all 8 DI bits as a byte (uses Port value)");
            grp.Controls.Add(controls.GetDiPortButton);

            controls.GetDiBitButton = new Button { Name = $"btnDev{deviceId}GetDiBit", Location = new Point(266, diY - 1), Size = new Size(88, 23), Text = "Get DI Bit" };
            controls.GetDiBitButton.Click += (s, e) => ExecuteGetDiBit(deviceId);
            _toolTip.SetToolTip(controls.GetDiBitButton, "Read a single DI bit (uses Port and Bit)");
            grp.Controls.Add(controls.GetDiBitButton);

            controls.DiResultLabel = new Label { AutoSize = true, Location = new Point(10, diY + 27), Text = "DI Value: --" };
            grp.Controls.Add(controls.DiResultLabel);

            // DO test row
            const int doY = 130;
            grp.Controls.Add(new Label { AutoSize = true, Location = new Point(10, doY + 2), Text = "DO Port" });
            controls.DoPortTextBox = new TextBox { Location = new Point(58, doY), Size = new Size(36, 20), Text = "0" };
            _toolTip.SetToolTip(controls.DoPortTextBox, "Port index (0-based): DO supports only 0");
            grp.Controls.Add(controls.DoPortTextBox);

            grp.Controls.Add(new Label { AutoSize = true, Location = new Point(103, doY + 2), Text = "Bit" });
            controls.DoBitTextBox = new TextBox { Location = new Point(126, doY), Size = new Size(36, 20), Text = "0" };
            _toolTip.SetToolTip(controls.DoBitTextBox, "Bit index within the port (0-7)");
            grp.Controls.Add(controls.DoBitTextBox);

            grp.Controls.Add(new Label { AutoSize = true, Location = new Point(171, doY + 2), Text = "Value" });
            controls.DoValueTextBox = new TextBox { Location = new Point(209, doY), Size = new Size(36, 20), Text = "0" };
            _toolTip.SetToolTip(controls.DoValueTextBox, "0-255 for Set DO Port; 0 or 1 for Set DO Bit");
            grp.Controls.Add(controls.DoValueTextBox);

            controls.GetDoPortButton = new Button { Name = $"btnDev{deviceId}GetDoPort", Location = new Point(10, doY + 24), Size = new Size(88, 23), Text = "Get DO Port" };
            controls.GetDoPortButton.Click += (s, e) => ExecuteGetDoPort(deviceId);
            _toolTip.SetToolTip(controls.GetDoPortButton, "Read all 8 DO bits as a byte (uses Port value)");
            grp.Controls.Add(controls.GetDoPortButton);

            controls.GetDoBitButton = new Button { Name = $"btnDev{deviceId}GetDoBit", Location = new Point(104, doY + 24), Size = new Size(88, 23), Text = "Get DO Bit" };
            controls.GetDoBitButton.Click += (s, e) => ExecuteGetDoBit(deviceId);
            _toolTip.SetToolTip(controls.GetDoBitButton, "Read a single DO bit (uses Port and Bit)");
            grp.Controls.Add(controls.GetDoBitButton);

            controls.SetDoPortButton = new Button { Name = $"btnDev{deviceId}SetDoPort", Location = new Point(10, doY + 48), Size = new Size(88, 23), Text = "Set DO Port" };
            controls.SetDoPortButton.Click += (s, e) => ExecuteSetDoPort(deviceId);
            _toolTip.SetToolTip(controls.SetDoPortButton, "Write Value (0-255) to all DO bits (uses Port and Value)");
            grp.Controls.Add(controls.SetDoPortButton);

            controls.SetDoBitButton = new Button { Name = $"btnDev{deviceId}SetDoBit", Location = new Point(104, doY + 48), Size = new Size(88, 23), Text = "Set DO Bit" };
            controls.SetDoBitButton.Click += (s, e) => ExecuteSetDoBit(deviceId);
            _toolTip.SetToolTip(controls.SetDoBitButton, "Set a single DO bit to 0 or 1 (uses Port, Bit, and Value)");
            grp.Controls.Add(controls.SetDoBitButton);

            controls.DoResultLabel = new Label { AutoSize = true, Location = new Point(10, doY + 52), Text = "DO Value: --" };
            grp.Controls.Add(controls.DoResultLabel);

            return controls;
        }

        private void ExecuteSnapStart(int deviceId)
        {
            AdvantechDIO.Module.AdvantechDIO device = GetConnectedDevice(deviceId);
            if (device == null)
            {
                return;
            }

            int result = device.SnapStart();
            WriteStatus($"Dev{deviceId} SnapStart: Code={result}");
        }

        private void ExecuteSnapStop(int deviceId)
        {
            AdvantechDIO.Module.AdvantechDIO device = GetConnectedDevice(deviceId);
            if (device == null)
            {
                return;
            }

            int result = device.SnapStop();
            WriteStatus($"Dev{deviceId} SnapStop: Code={result}");
        }

        private void ExecuteGetDiPort(int deviceId)
        {
            AdvantechDIO.Module.AdvantechDIO device = GetConnectedDevice(deviceId);
            DeviceTestControls controls = GetDeviceControls(deviceId);
            if (device == null || controls == null)
            {
                return;
            }

            if (!TryParseNonNegative(controls.DiPortTextBox.Text, $"Dev{deviceId} DI Port", out int portIndex))
            {
                return;
            }

            byte value;
            int result = device.GetInput(portIndex, out value);
            controls.DiResultLabel.Text = result == 0 ? $"DI Value: {value}" : "DI Value: --";
            WriteStatus($"Dev{deviceId} GetInput(port={portIndex}): Code={result}" + (result == 0 ? $", Value={value}" : string.Empty));
        }

        private void ExecuteGetDiBit(int deviceId)
        {
            AdvantechDIO.Module.AdvantechDIO device = GetConnectedDevice(deviceId);
            DeviceTestControls controls = GetDeviceControls(deviceId);
            if (device == null || controls == null)
            {
                return;
            }

            if (!TryParseNonNegative(controls.DiPortTextBox.Text, $"Dev{deviceId} DI Port", out int portIndex) ||
                !TryParseNonNegative(controls.DiBitTextBox.Text, $"Dev{deviceId} DI Bit", out int bitIndex))
            {
                return;
            }

            byte value;
            int result = device.GetInputBit(portIndex, bitIndex, out value);
            controls.DiResultLabel.Text = result == 0 ? $"DI Value: {value}" : "DI Value: --";
            WriteStatus($"Dev{deviceId} GetInputBit(port={portIndex}, bit={bitIndex}): Code={result}" + (result == 0 ? $", Value={value}" : string.Empty));
        }

        private void ExecuteGetDoPort(int deviceId)
        {
            AdvantechDIO.Module.AdvantechDIO device = GetConnectedDevice(deviceId);
            DeviceTestControls controls = GetDeviceControls(deviceId);
            if (device == null || controls == null)
            {
                return;
            }

            if (!TryParseNonNegative(controls.DoPortTextBox.Text, $"Dev{deviceId} DO Port", out int portIndex))
            {
                return;
            }

            byte value;
            int result = device.GetOutput(portIndex, out value);
            controls.DoResultLabel.Text = result == 0 ? $"DO Value: {value}" : "DO Value: --";
            WriteStatus($"Dev{deviceId} GetOutput(port={portIndex}): Code={result}" + (result == 0 ? $", Value={value}" : string.Empty));
        }

        private void ExecuteGetDoBit(int deviceId)
        {
            AdvantechDIO.Module.AdvantechDIO device = GetConnectedDevice(deviceId);
            DeviceTestControls controls = GetDeviceControls(deviceId);
            if (device == null || controls == null)
            {
                return;
            }

            if (!TryParseNonNegative(controls.DoPortTextBox.Text, $"Dev{deviceId} DO Port", out int portIndex) ||
                !TryParseNonNegative(controls.DoBitTextBox.Text, $"Dev{deviceId} DO Bit", out int bitIndex))
            {
                return;
            }

            byte value;
            int result = device.GetOutputBit(portIndex, bitIndex, out value);
            controls.DoResultLabel.Text = result == 0 ? $"DO Value: {value}" : "DO Value: --";
            WriteStatus($"Dev{deviceId} GetOutputBit(port={portIndex}, bit={bitIndex}): Code={result}" + (result == 0 ? $", Value={value}" : string.Empty));
        }

        private void ExecuteSetDoPort(int deviceId)
        {
            AdvantechDIO.Module.AdvantechDIO device = GetConnectedDevice(deviceId);
            DeviceTestControls controls = GetDeviceControls(deviceId);
            if (device == null || controls == null)
            {
                return;
            }

            if (!TryParseNonNegative(controls.DoPortTextBox.Text, $"Dev{deviceId} DO Port", out int portIndex))
            {
                return;
            }

            if (!byte.TryParse(controls.DoValueTextBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out byte value))
            {
                WriteStatus($"Dev{deviceId} validation failed: DO Value must be 0..255.");
                return;
            }

            int result = device.SetOutput(portIndex, value);
            WriteStatus($"Dev{deviceId} SetOutput(port={portIndex}, value={value}): Code={result}");
            if (result == 0)
            {
                controls.DoResultLabel.Text = $"DO Value: {value}";
                RefreshDioPanel();
            }
        }

        private void ExecuteSetDoBit(int deviceId)
        {
            AdvantechDIO.Module.AdvantechDIO device = GetConnectedDevice(deviceId);
            DeviceTestControls controls = GetDeviceControls(deviceId);
            if (device == null || controls == null)
            {
                return;
            }

            if (!TryParseNonNegative(controls.DoPortTextBox.Text, $"Dev{deviceId} DO Port", out int portIndex) ||
                !TryParseNonNegative(controls.DoBitTextBox.Text, $"Dev{deviceId} DO Bit", out int bitIndex))
            {
                return;
            }

            if (!byte.TryParse(controls.DoValueTextBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out byte value) || (value != 0 && value != 1))
            {
                WriteStatus($"Dev{deviceId} validation failed: DO Bit Value must be 0 or 1.");
                return;
            }

            int result = device.SetOutputBit(portIndex, bitIndex, value);
            WriteStatus($"Dev{deviceId} SetOutputBit(port={portIndex}, bit={bitIndex}, value={value}): Code={result}");
            if (result == 0)
            {
                controls.DoResultLabel.Text = $"DO Value: {value}";
                RefreshDioPanel();
            }
        }

        private DeviceTestControls GetDeviceControls(int deviceId)
        {
            return deviceId == 0 ? _testControlsDev0 : _testControlsDev1;
        }

        private AdvantechDIO.Module.AdvantechDIO GetConnectedDevice(int deviceId)
        {
            AdvantechDIO.Module.AdvantechDIO device = deviceId == 0 ? _dioDev0 : _dioDev1;
            if (device == null || !device.IsConnected)
            {
                WriteStatus($"Dev{deviceId}: Not connected.");
                return null;
            }

            return device;
        }

        private bool TryParseNonNegative(string raw, string fieldName, out int value)
        {
            if (!int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out value) || value < 0)
            {
                WriteStatus($"Validation failed: {fieldName} must be non-negative integer.");
                return false;
            }

            return true;
        }

        private void DoLed_Click(object sender, EventArgs e)
        {
            Label led = (Label)sender;
            int tag = (int)led.Tag;
            int deviceId = tag >> 8;
            int bit = tag & 0xFF;

            AdvantechDIO.Module.AdvantechDIO device = deviceId == 0 ? _dioDev0 : _dioDev1;
            if (device == null || !device.IsConnected)
            {
                WriteStatus($"Dev{deviceId}: Not connected.");
                return;
            }

            byte currentBitValue;
            int result = device.GetOutputBit(0, bit, out currentBitValue);
            if (result != 0)
            {
                WriteStatus($"Dev{deviceId} DO[{bit}] read failed: Code={result}");
                return;
            }

            byte newValue = currentBitValue == 0 ? (byte)1 : (byte)0;
            result = device.SetOutputBit(0, bit, newValue);
            if (result == 0)
            {
                WriteStatus($"Dev{deviceId} DO[{bit}] -> {newValue}");
                RefreshDioPanel();
            }
            else
            {
                WriteStatus($"Dev{deviceId} DO[{bit}] set failed: Code={result}");
            }
        }

        private void BtnConnectDev0_Click(object sender, EventArgs e)
        {
            EnsureDeviceConnection(0);
            RefreshDioPanel();
        }

        private void BtnDisconnectDev0_Click(object sender, EventArgs e)
        {
            DisconnectDevice(0);
            RefreshDioPanel();
        }

        private void BtnConnectDev1_Click(object sender, EventArgs e)
        {
            EnsureDeviceConnection(1);
            RefreshDioPanel();
        }

        private void BtnDisconnectDev1_Click(object sender, EventArgs e)
        {
            DisconnectDevice(1);
            RefreshDioPanel();
        }

        private void StatusRefreshTimer_Tick(object sender, EventArgs e)
        {
            RefreshDioPanel();
        }

        private void RefreshDioPanel()
        {
            if (_isRefreshingStatusPanel)
            {
                return;
            }

            _isRefreshingStatusPanel = true;
            try
            {
                UpdateDeviceLeds(_dioDev0, _diLedsDev0, _doLedsDev0, lblDev0State);
                UpdateDeviceLeds(_dioDev1, _diLedsDev1, _doLedsDev1, lblDev1State);
            }
            finally
            {
                _isRefreshingStatusPanel = false;
            }
        }

        private void UpdateDeviceLeds(AdvantechDIO.Module.AdvantechDIO device, Label[] diLeds, Label[] doLeds, Label stateLabel)
        {
            if (device == null || !device.IsConnected)
            {
                SetDeviceOfflineStyle(diLeds, doLeds, stateLabel);
                return;
            }

            byte diValue;
            int diResult = device.GetInput(0, out diValue);
            byte doValue;
            int doResult = device.GetOutput(0, out doValue);

            if (diResult != 0 || doResult != 0)
            {
                stateLabel.Text = $"Error ({diResult}/{doResult})";
                SetDeviceOfflineStyle(diLeds, doLeds, null);
                return;
            }

            stateLabel.Text = "Connected";
            stateLabel.ForeColor = Color.Green;

            for (int bit = 0; bit < 8; bit++)
            {
                bool diOn = (diValue & (1 << bit)) != 0;
                bool doOn = (doValue & (1 << bit)) != 0;
                diLeds[bit].BackColor = diOn ? Color.Lime : Color.Silver;
                doLeds[bit].BackColor = doOn ? Color.Tomato : Color.Silver;
            }
        }

        private void SetDeviceOfflineStyle(Label[] diLeds, Label[] doLeds, Label stateLabel)
        {
            if (stateLabel != null)
            {
                stateLabel.Text = "Disconnected";
                stateLabel.ForeColor = Color.Gray;
            }

            for (int bit = 0; bit < 8; bit++)
            {
                diLeds[bit].BackColor = Color.DarkGray;
                doLeds[bit].BackColor = Color.DarkGray;
            }
        }

        private void EnsureDeviceConnection(int deviceId)
        {
            AdvantechDIO.Module.AdvantechDIO device = deviceId == 0 ? _dioDev0 : _dioDev1;
            AdvantechDIOConfig config = deviceId == 0 ? _configDev0 : _configDev1;

            if (device != null && device.IsConnected)
            {
                return;
            }

            if (device == null)
            {
                device = new AdvantechDIO.Module.AdvantechDIO(_logger, config);
                if (deviceId == 0)
                {
                    _dioDev0 = device;
                }
                else
                {
                    _dioDev1 = device;
                }
            }

            int result = device.Connect();
            WriteStatus($"Connect Dev{deviceId}: Code={result}");
            if (result == 0)
            {
                SubscribeDeviceEvents(device);
            }
        }

        private void DisconnectDevice(int deviceId)
        {
            AdvantechDIO.Module.AdvantechDIO device = deviceId == 0 ? _dioDev0 : _dioDev1;
            if (device == null)
            {
                return;
            }

            UnsubscribeDeviceEvents(device);

            int result = device.Disconnect();
            WriteStatus($"Disconnect Dev{deviceId}: Code={result}");
        }

        private void SubscribeDeviceEvents(AdvantechDIO.Module.AdvantechDIO device)
        {
            device.DI_ValueChanged -= Device_DiValueChanged;
            device.DI_ValueChanged += Device_DiValueChanged;
            device.DO_ValueChanged -= Device_DoValueChanged;
            device.DO_ValueChanged += Device_DoValueChanged;
        }

        private void UnsubscribeDeviceEvents(AdvantechDIO.Module.AdvantechDIO device)
        {
            device.DI_ValueChanged -= Device_DiValueChanged;
            device.DO_ValueChanged -= Device_DoValueChanged;
        }

        private void Device_DiValueChanged(object sender, EventArgs e)
        {
            UiSafe(() =>
            {
                int deviceId = -1;
                if (ReferenceEquals(sender, _dioDev0))
                {
                    deviceId = 0;
                }
                else if (ReferenceEquals(sender, _dioDev1))
                {
                    deviceId = 1;
                }

                WriteStatus(deviceId >= 0 ? $"Dev{deviceId} DI ChangeOfState event." : "DI ChangeOfState event.");
                RefreshDioPanel();
            });
        }

        private void Device_DoValueChanged(object sender, EventArgs e)
        {
            UiSafe(() =>
            {
                int deviceId = -1;
                if (ReferenceEquals(sender, _dioDev0))
                {
                    deviceId = 0;
                }
                else if (ReferenceEquals(sender, _dioDev1))
                {
                    deviceId = 1;
                }

                WriteStatus(deviceId >= 0 ? $"Dev{deviceId} DO callback event." : "DO callback event.");
                RefreshDioPanel();
            });
        }

        private void DisposeDioInstance(ref AdvantechDIO.Module.AdvantechDIO device)
        {
            if (device == null)
            {
                return;
            }

            try
            {
                UnsubscribeDeviceEvents(device);
                device.Disconnect();
            }
            catch
            {
                // Swallow cleanup exceptions to keep form shutdown safe.
            }

            device.Dispose();
            device = null;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_statusRefreshTimer != null)
            {
                _statusRefreshTimer.Stop();
                _statusRefreshTimer.Tick -= StatusRefreshTimer_Tick;
                _statusRefreshTimer.Dispose();
                _statusRefreshTimer = null;
            }

            _toolTip?.Dispose();
            DisposeDioInstance(ref _dioDev0);
            DisposeDioInstance(ref _dioDev1);
        }

        private void WriteStatus(string message)
        {
            string line = $"[{DateTime.Now:HH:mm:ss}] {message}";

            try
            {
                LogHeadType logType = ResolveLogHeadType(message);
                _logger?.WriteLog("AdvantechDIO.ManualTestGui", logType, message);
            }
            catch
            {
            }

            UiSafe(() =>
            {
                lstStatus.Items.Insert(0, line);
                if (lstStatus.Items.Count > 500)
                {
                    lstStatus.Items.RemoveAt(lstStatus.Items.Count - 1);
                }
            });
        }

        private static LogHeadType ResolveLogHeadType(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return LogHeadType.Info;
            }

            string lower = message.ToLowerInvariant();
            if (TryExtractResultCode(message, out int code))
            {
                if (code < 0)
                {
                    return LogHeadType.Error;
                }

                if (code > 0)
                {
                    return LogHeadType.Warning;
                }

                return LogHeadType.Info;
            }

            if (lower.Contains("error") ||
                lower.Contains("failed") ||
                lower.Contains("exception") ||
                lower.Contains("abort"))
            {
                return LogHeadType.Error;
            }

            if (lower.Contains("validation") ||
                lower.Contains("not connected") ||
                lower.Contains("warn") ||
                lower.Contains("timeout"))
            {
                return LogHeadType.Warning;
            }

            return LogHeadType.Info;
        }

        private static bool TryExtractResultCode(string message, out int code)
        {
            code = 0;
            const string marker = "Code=";
            int start = message.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (start < 0)
            {
                return false;
            }

            start += marker.Length;
            while (start < message.Length && message[start] == ' ')
            {
                start++;
            }

            int end = start;
            while (end < message.Length)
            {
                char c = message[end];
                if ((c >= '0' && c <= '9') || c == '-' || c == '+')
                {
                    end++;
                    continue;
                }
                break;
            }

            if (end <= start)
            {
                return false;
            }

            string raw = message.Substring(start, end - start);
            return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out code);
        }

        private void UiSafe(Action action)
        {
            if (IsDisposed || !IsHandleCreated)
            {
                return;
            }

            if (InvokeRequired)
            {
                BeginInvoke(action);
            }
            else
            {
                action();
            }
        }

        private sealed class DeviceTestControls
        {
            public Button SnapStartButton { get; set; }
            public Button SnapStopButton { get; set; }
            public TextBox DiPortTextBox { get; set; }
            public TextBox DiBitTextBox { get; set; }
            public Button GetDiPortButton { get; set; }
            public Button GetDiBitButton { get; set; }
            public Label DiResultLabel { get; set; }
            public TextBox DoPortTextBox { get; set; }
            public TextBox DoBitTextBox { get; set; }
            public TextBox DoValueTextBox { get; set; }
            public Button GetDoPortButton { get; set; }
            public Button GetDoBitButton { get; set; }
            public Button SetDoPortButton { get; set; }
            public Button SetDoBitButton { get; set; }
            public Label DoResultLabel { get; set; }
        }
    }
}
