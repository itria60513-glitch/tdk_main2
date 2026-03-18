using System;
using System.Drawing;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;
using Communication.Interface;
using Communication.Protocol;
using LogUtility;
using TDKController;
using TDKLogUtility.Module;

namespace CarrierIDReader.ManualTestGui
{
    public partial class MainForm : Form
    {
        private readonly ILogUtility _logger;

        private ICarrierIDReader _reader;
        private IConnector _connector;
        private ToolTip _toolTip;

        public MainForm()
        {
            InitializeComponent();

            _logger = new LogUtilityClient();
            _toolTip = new ToolTip { AutoPopDelay = 5000, InitialDelay = 400, ShowAlways = true };

            PopulateReaderTypeCombo();
            PopulateComPortCombo();
            PopulateSerialSettingsCombos();
            ApplyDefaultSerialSettings();

            cboReaderType.SelectedIndexChanged += CboReaderType_SelectedIndexChanged;

            _toolTip.SetToolTip(cboReaderType, "Select the carrier ID reader protocol type");
            _toolTip.SetToolTip(cboComPort, "Select the serial COM port");
            _toolTip.SetToolTip(btnConnect, "Create a reader instance with the current settings");
            _toolTip.SetToolTip(btnDisconnect, "Dispose the current reader instance");
            _toolTip.SetToolTip(btnRead, "Read carrier ID from the specified page");
            _toolTip.SetToolTip(btnWrite, "Write carrier ID to the specified page (RFID only)");
            _toolTip.SetToolTip(txtPage, "Page number: Barcode ignores this; Hermes 1-17; Omron 1-30");
            _toolTip.SetToolTip(txtCarrierId, "Carrier ID payload for write operations");

            WriteStatus("Application started.");
        }

        private void PopulateReaderTypeCombo()
        {
            cboReaderType.Items.Add("BarcodeReader (BL600)");
            cboReaderType.Items.Add("HermesRFID");
            cboReaderType.Items.Add("OmronASCII");
            cboReaderType.Items.Add("OmronHex");
            cboReaderType.SelectedIndex = 0;
        }

        private void PopulateComPortCombo()
        {
            string[] ports = System.IO.Ports.SerialPort.GetPortNames();
            cboComPort.Items.Clear();
            foreach (string port in ports)
            {
                cboComPort.Items.Add(port);
            }

            if (cboComPort.Items.Count > 0)
            {
                cboComPort.SelectedIndex = 0;
            }
        }

        private void PopulateSerialSettingsCombos()
        {
            cboBaudRate.Items.AddRange(new object[] { "9600", "19200", "38400", "57600", "115200" });
            cboParity.Items.AddRange(new object[] { "None", "Even", "Odd" });
            cboDataBits.Items.AddRange(new object[] { "7", "8" });
            cboStopBits.Items.AddRange(new object[] { "1", "2" });
        }

        private void ApplyDefaultSerialSettings()
        {
            // BarcodeReader (BL600) default: 9600/7E1
            cboBaudRate.SelectedItem = "9600";
            cboParity.SelectedItem = "Even";
            cboDataBits.SelectedItem = "7";
            cboStopBits.SelectedItem = "1";
        }

        private void CboReaderType_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = cboReaderType.SelectedIndex;
            switch (index)
            {
                case 0: // BarcodeReader: 9600/7E1
                    cboBaudRate.SelectedItem = "9600";
                    cboParity.SelectedItem = "Even";
                    cboDataBits.SelectedItem = "7";
                    cboStopBits.SelectedItem = "1";
                    break;
                case 1: // HermesRFID: 19200/8E1
                    cboBaudRate.SelectedItem = "19200";
                    cboParity.SelectedItem = "Even";
                    cboDataBits.SelectedItem = "8";
                    cboStopBits.SelectedItem = "1";
                    break;
                case 2: // OmronASCII: 9600/8E1
                case 3: // OmronHex: 9600/8E1
                    cboBaudRate.SelectedItem = "9600";
                    cboParity.SelectedItem = "Even";
                    cboDataBits.SelectedItem = "8";
                    cboStopBits.SelectedItem = "1";
                    break;
            }
        }

        private void BtnConnect_Click(object sender, EventArgs e)
        {
            if (_reader != null)
            {
                WriteStatus("Reader already created. Dispose first.");
                return;
            }

            if (cboComPort.SelectedItem == null)
            {
                WriteStatus("No COM port selected.");
                return;
            }

            if (!int.TryParse(txtTimeout.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int timeoutMs) || timeoutMs <= 0)
            {
                WriteStatus("Timeout must be a positive integer.");
                return;
            }

            if (!int.TryParse(txtRetryCount.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int retryCount) || retryCount < 0)
            {
                WriteStatus("Retry count must be a non-negative integer.");
                return;
            }

            string comPort = cboComPort.SelectedItem.ToString();
            int baudRate = int.Parse(cboBaudRate.SelectedItem.ToString(), CultureInfo.InvariantCulture);
            int parity = ParseParityIndex();
            int dataBits = int.Parse(cboDataBits.SelectedItem.ToString(), CultureInfo.InvariantCulture);
            int stopBits = int.Parse(cboStopBits.SelectedItem.ToString(), CultureInfo.InvariantCulture);

            try
            {
                IProtocol protocol = CreateProtocol(cboReaderType.SelectedIndex);
                _connector = new SimpleRs232Connector(protocol, comPort, baudRate, parity, dataBits, stopBits, WriteStatus);
                _connector.Connect();

                CarrierIDReaderConfig readerConfig = new CarrierIDReaderConfig
                {
                    TimeoutMs = timeoutMs,
                    BarcodeReaderMaxRetryCount = retryCount
                };

                _reader = CreateReader(cboReaderType.SelectedIndex, readerConfig, _connector);

                lblConnState.Text = "Reader Created";
                lblConnState.ForeColor = Color.Green;
                WriteStatus(string.Format("Reader created and port opened: {0}, Port={1}, Baud={2}, Parity={3}, DataBits={4}, StopBits={5}",
                    cboReaderType.SelectedItem, comPort, baudRate, cboParity.SelectedItem, dataBits, stopBits));
            }
            catch (Exception ex)
            {
                WriteStatus(string.Format("Failed to create reader: {0}", ex.Message));
                DisposeCurrentReader();
            }
        }

        private void BtnDisconnect_Click(object sender, EventArgs e)
        {
            if (_reader == null)
            {
                WriteStatus("No reader to dispose.");
                return;
            }

            DisposeCurrentReader();
            WriteStatus("Reader disposed.");
        }

        private void BtnRead_Click(object sender, EventArgs e)
        {
            if (_reader == null)
            {
                WriteStatus("No reader created.");
                return;
            }

            if (!int.TryParse(txtPage.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int page))
            {
                WriteStatus("Page must be a valid integer.");
                return;
            }

            SetOperationButtonsEnabled(false);
            WriteStatus(string.Format("Reading page {0}...", page));

            Task.Run(() =>
            {
                string carrierID;
                ErrorCode result = _reader.GetCarrierID(page, out carrierID);
                UiSafe(() =>
                {
                    if (result == ErrorCode.Success)
                    {
                        txtResult.Text = string.Format("OK: \"{0}\"", carrierID);
                        WriteStatus(string.Format("Read OK: Page={0}, CarrierID=\"{1}\"", page, carrierID));
                    }
                    else
                    {
                        txtResult.Text = string.Format("Error: {0} ({1})", result, (int)result);
                        WriteStatus(string.Format("Read failed: Page={0}, ErrorCode={1} ({2})", page, result, (int)result));
                    }

                    SetOperationButtonsEnabled(true);
                });
            });
        }

        private void BtnWrite_Click(object sender, EventArgs e)
        {
            if (_reader == null)
            {
                WriteStatus("No reader created.");
                return;
            }

            if (!int.TryParse(txtPage.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int page))
            {
                WriteStatus("Page must be a valid integer.");
                return;
            }

            string carrierID = txtCarrierId.Text;
            if (string.IsNullOrEmpty(carrierID))
            {
                WriteStatus("Carrier ID must not be empty.");
                return;
            }

            SetOperationButtonsEnabled(false);
            WriteStatus(string.Format("Writing page {0}, CarrierID=\"{1}\"...", page, carrierID));

            Task.Run(() =>
            {
                ErrorCode result = _reader.SetCarrierID(page, carrierID);
                UiSafe(() =>
                {
                    if (result == ErrorCode.Success)
                    {
                        txtResult.Text = "Write OK";
                        WriteStatus(string.Format("Write OK: Page={0}", page));
                    }
                    else
                    {
                        txtResult.Text = string.Format("Error: {0} ({1})", result, (int)result);
                        WriteStatus(string.Format("Write failed: Page={0}, ErrorCode={1} ({2})", page, result, (int)result));
                    }

                    SetOperationButtonsEnabled(true);
                });
            });
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _toolTip?.Dispose();
            DisposeCurrentReader();
        }

        private IProtocol CreateProtocol(int readerTypeIndex)
        {
            switch (readerTypeIndex)
            {
                case 0: return new BarcodeProtocol();
                case 1: return new HermosProtocol();
                case 2: return new OmronProtocol();
                case 3: return new OmronProtocol();
                default: return new BarcodeProtocol();
            }
        }

        private ICarrierIDReader CreateReader(int readerTypeIndex, CarrierIDReaderConfig config, IConnector connector)
        {
            switch (readerTypeIndex)
            {
                case 0: return new IDReaderBarcodeReader(config, connector, _logger);
                case 1: return new IDReaderHermesRFID(config, connector, _logger);
                case 2: return new IDReaderOmronASCII(config, connector, _logger);
                case 3: return new IDReaderOmronHex(config, connector, _logger);
                default: throw new ArgumentOutOfRangeException(nameof(readerTypeIndex));
            }
        }

        private int ParseParityIndex()
        {
            string selected = cboParity.SelectedItem?.ToString() ?? "None";
            switch (selected)
            {
                case "Even": return 2;
                case "Odd": return 1;
                default: return 0; // None
            }
        }

        private void DisposeCurrentReader()
        {
            if (_reader is IDisposable disposable)
            {
                try
                {
                    disposable.Dispose();
                }
                catch
                {
                    // Swallow cleanup exceptions.
                }
            }

            _reader = null;

            if (_connector != null)
            {
                try
                {
                    _connector.Disconnect();
                }
                catch
                {
                    // Swallow cleanup exceptions.
                }

                _connector = null;
            }

            lblConnState.Text = "No Reader";
            lblConnState.ForeColor = Color.Gray;
        }

        private void SetOperationButtonsEnabled(bool enabled)
        {
            btnRead.Enabled = enabled;
            btnWrite.Enabled = enabled;
        }

        private void WriteStatus(string message)
        {
            string line = string.Format("[{0:HH:mm:ss}] {1}", DateTime.Now, message);

            UiSafe(() =>
            {
                lstStatus.Items.Insert(0, line);
                if (lstStatus.Items.Count > 500)
                {
                    lstStatus.Items.RemoveAt(lstStatus.Items.Count - 1);
                }
            });
        }

        private void UiSafe(Action action)
        {
            if (InvokeRequired)
            {
                BeginInvoke(action);
            }
            else
            {
                action();
            }
        }
    }
}
