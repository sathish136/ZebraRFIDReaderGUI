using System;
using System.Drawing;
using System.Windows.Forms;
using Symbol.RFID3;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace ZebraRFIDReaderGUI
{
    public partial class Form1 : Form
    {
        private RFIDReader reader;
        private bool isConnected = false;
        private string readerIP = "192.168.2.5";
        private int totalReads = 0;
        private int readsPerSec = 0;
        private DateTime startTime;
        private Timer updateTimer;
        private List<TagRecord> allTags = new List<TagRecord>();
        private bool isServiceMode;

        // Controls
        private TableLayoutPanel mainLayout;
        private Label lblTotalTags;
        private Label lblReads;
        private Button btnSettings;
        private Label lblStatus;

        public Form1(bool serviceMode)
        {
            isServiceMode = serviceMode;
            InitializeComponent();
            InitializeCustomComponents();
            SetupEventHandlers();

            // Auto-start on launch
            ConnectReader();
            if (isConnected)
            {
                StartReading();
            }
        }

        private void InitializeCustomComponents()
        {
            this.Text = "RFID Reader";
            this.Size = new Size(250, 150);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = true;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 9F);

            mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(10),
                BackColor = Color.White
            };

            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));

            // Tags Label
            var lblTagsText = new Label
            {
                Text = "Total Tags:",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            lblTotalTags = new Label
            {
                Text = "0",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold)
            };

            // Reads Label
            var lblReadsText = new Label
            {
                Text = "Total Reads:",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            lblReads = new Label
            {
                Text = "0",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold)
            };

            // Settings Button
            btnSettings = new Button
            {
                Text = "Settings",
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 5, 0, 0)
            };

            // Status Label
            lblStatus = new Label
            {
                Text = "Disconnected",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.Red
            };

            mainLayout.Controls.Add(lblTagsText, 0, 0);
            mainLayout.Controls.Add(lblTotalTags, 1, 0);
            mainLayout.Controls.Add(lblReadsText, 0, 1);
            mainLayout.Controls.Add(lblReads, 1, 1);
            mainLayout.Controls.Add(lblStatus, 0, 2);
            mainLayout.Controls.Add(btnSettings, 1, 2);

            this.Controls.Add(mainLayout);

            // Initialize timer
            updateTimer = new Timer();
            updateTimer.Interval = 1000;
            updateTimer.Tick += UpdateTimer_Tick;
        }

        private void SetupEventHandlers()
        {
            btnSettings.Click += BtnSettings_Click;
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                this.BeginInvoke((MethodInvoker)delegate
                {
                    // Update reads per second
                    readsPerSec = totalReads;
                    totalReads = 0;

                    // Update display
                    lblTotalTags.Text = allTags.Count.ToString();
                    lblReads.Text = readsPerSec.ToString();
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in timer tick: {ex.Message}");
            }
        }

        private void Events_ReadNotify(object sender, Events.ReadEventArgs e)
        {
            try
            {
                Debug.WriteLine($"Read event triggered. Getting tags...");
                TagData[] tags = reader.Actions.GetReadTags(1000);  // Increased buffer size
                if (tags != null && tags.Length > 0)
                {
                    Debug.WriteLine($"Received {tags.Length} tags");
                    foreach (TagData tag in tags)
                    {
                        Debug.WriteLine($"Tag ID: {tag.TagID}, Antenna: {tag.AntennaID}, RSSI: {tag.PeakRSSI}");
                    }
                    HandleTagData(tags);
                }
                else
                {
                    Debug.WriteLine("No tags received in read event");
                    // Try to restart reading if we're not getting tags
                    if (isConnected)
                    {
                        reader.Actions.Inventory.Stop();
                        System.Threading.Thread.Sleep(100);
                        reader.Actions.Inventory.Perform();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in ReadNotify: {ex.Message}");
            }
        }

        private void HandleTagData(TagData[] tags)
        {
            try
            {
                Debug.WriteLine("Processing tag data...");
                foreach (TagData tag in tags)
                {
                    var tagRecord = new TagRecord
                    {
                        TagID = tag.TagID,
                        LastSeenTime = DateTime.Now,
                        AntennaID = tag.AntennaID,
                        RSSI = tag.PeakRSSI,
                        SeenCount = 1
                    };

                    // Update or add the tag
                    var existingTag = allTags.FirstOrDefault(t => t.TagID == tag.TagID);
                    if (existingTag != null)
                    {
                        existingTag.LastSeenTime = tagRecord.LastSeenTime;
                        existingTag.RSSI = tagRecord.RSSI;
                        existingTag.AntennaID = tagRecord.AntennaID;
                        existingTag.SeenCount++;
                    }
                    else
                    {
                        allTags.Add(tagRecord);
                    }
                }
                Debug.WriteLine($"Total unique tags: {allTags.Count}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error handling tag data: {ex.Message}");
            }
        }

        private void Events_StatusNotify(object sender, Events.StatusEventArgs e)
        {
            Debug.WriteLine($"Status: {e.StatusEventData.StatusEventType}");
            
            if (e.StatusEventData.StatusEventType == Symbol.RFID3.Events.STATUS_EVENT_TYPE.INVENTORY_START_EVENT)
            {
                Debug.WriteLine("Inventory started");
            }
            else if (e.StatusEventData.StatusEventType == Symbol.RFID3.Events.STATUS_EVENT_TYPE.INVENTORY_STOP_EVENT)
            {
                Debug.WriteLine("Inventory stopped");
            }
            else if (e.StatusEventData.StatusEventType == Symbol.RFID3.Events.STATUS_EVENT_TYPE.DISCONNECTION_EVENT)
            {
                Debug.WriteLine("Reader disconnected");
                this.BeginInvoke((MethodInvoker)delegate
                {
                    lblStatus.Text = "Disconnected";
                    lblStatus.ForeColor = Color.Red;
                    isConnected = false;
                    
                    // Try to reconnect
                    ConnectReader();
                    if (isConnected)
                    {
                        StartReading();
                    }
                });
            }
        }

        private void StartReading()
        {
            try
            {
                if (reader != null && isConnected)
                {
                    Debug.WriteLine("Starting inventory");
                    
                    // Configure read parameters
                    reader.Actions.PreFilters.DeleteAll();
                    reader.Actions.Inventory.Stop();
                    System.Threading.Thread.Sleep(100);
                    
                    // Start continuous reading
                    reader.Actions.Inventory.Perform();
                    startTime = DateTime.Now;
                    updateTimer.Start();
                    
                    Debug.WriteLine("Inventory started successfully");
                }
                else
                {
                    Debug.WriteLine("Cannot start reading - reader not connected");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error starting reader: {ex.Message}");
                MessageBox.Show($"Error starting reader: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConnectReader()
        {
            try
            {
                if (reader != null)
                {
                    reader.Disconnect();
                    reader = null;
                }

                Debug.WriteLine($"Attempting to connect to reader at {readerIP}");
                reader = new RFIDReader(readerIP, 5084, 0);
                reader.Connect();
                
                // Set up events first
                reader.Events.ReadNotify += Events_ReadNotify;
                reader.Events.StatusNotify += Events_StatusNotify;
                reader.Events.AttachTagDataWithReadEvent = true;
                
                // Configure antenna settings
                try
                {
                    // Get current config
                    var rfConfig = reader.Config.Antennas[1].GetRfConfig();
                    reader.Config.Antennas[1].SetRfConfig(rfConfig);
                    Debug.WriteLine("Antenna config set successfully");
                }
                catch (Exception rfEx)
                {
                    Debug.WriteLine($"Antenna config error: {rfEx.Message}");
                    throw new Exception($"Antenna config error: {rfEx.Message}");
                }

                // Clear any existing tags
                reader.Actions.PurgeTags();
                
                Debug.WriteLine("Reader connected successfully");
                isConnected = true;
                lblStatus.Text = "Connected";
                lblStatus.ForeColor = Color.Green;

                // Start reading immediately
                StartReading();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to connect: {ex.Message}");
                MessageBox.Show($"Failed to connect: {ex.Message}", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Connection Failed";
                lblStatus.ForeColor = Color.Red;
                isConnected = false;
            }
        }

        private void ShowSettingsDialog()
        {
            using (var settingsForm = new Form())
            {
                settingsForm.Text = "Settings";
                settingsForm.Size = new Size(250, 130);
                settingsForm.StartPosition = FormStartPosition.CenterParent;
                settingsForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                settingsForm.MaximizeBox = false;
                settingsForm.MinimizeBox = false;

                var layout = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 2,
                    RowCount = 2,
                    Padding = new Padding(10)
                };

                var lblIP = new Label
                {
                    Text = "Reader IP:",
                    AutoSize = true,
                    Anchor = AnchorStyles.Left | AnchorStyles.Right
                };

                var txtIP = new TextBox
                {
                    Text = readerIP,
                    Dock = DockStyle.Fill
                };

                var btnPanel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.RightToLeft,
                    AutoSize = true
                };

                var btnSave = new Button
                {
                    Text = "Save",
                    DialogResult = DialogResult.OK,
                    Width = 70
                };

                var btnCancel = new Button
                {
                    Text = "Cancel",
                    DialogResult = DialogResult.Cancel,
                    Width = 70
                };

                btnPanel.Controls.AddRange(new Control[] { btnCancel, btnSave });

                layout.Controls.Add(lblIP, 0, 0);
                layout.Controls.Add(txtIP, 1, 0);
                layout.Controls.Add(btnPanel, 1, 1);

                settingsForm.Controls.Add(layout);
                settingsForm.AcceptButton = btnSave;
                settingsForm.CancelButton = btnCancel;

                if (settingsForm.ShowDialog() == DialogResult.OK)
                {
                    string newIP = txtIP.Text.Trim();
                    if (IsValidIP(newIP))
                    {
                        readerIP = newIP;
                        if (isConnected)
                        {
                            DisconnectReader();
                            ConnectReader();
                            StartReading();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please enter a valid IP address.", "Invalid IP", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        private bool IsValidIP(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip)) return false;
            string[] parts = ip.Split('.');
            if (parts.Length != 4) return false;

            foreach (string part in parts)
            {
                if (!int.TryParse(part, out int value) || value < 0 || value > 255)
                    return false;
            }

            return true;
        }

        private void BtnSettings_Click(object sender, EventArgs e)
        {
            ShowSettingsDialog();
        }

        private void DisconnectReader()
        {
            try
            {
                if (reader != null)
                {
                    reader.Disconnect();
                    reader = null;
                }

                isConnected = false;
                lblStatus.Text = "Disconnected";
                lblStatus.ForeColor = Color.Red;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error disconnecting: {ex.Message}", "Disconnect Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Public methods for API access
        public List<TagRecord> GetAllTags()
        {
            return allTags.ToList();
        }

        public void ShowSettings()
        {
            ShowSettingsDialog();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            DisconnectReader();
        }
    }
}
