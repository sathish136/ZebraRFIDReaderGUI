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

        // Controls
        private TableLayoutPanel mainLayout;
        private Label lblTotalTags;
        private Label lblReads;
        private Button btnSettings;
        private Label lblStatus;

        public Form1()
        {
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
            TagData[] tags = reader.Actions.GetReadTags(100);
            if (tags != null)
            {
                HandleTagData(tags);
            }
        }

        private void HandleTagData(TagData[] tagData)
        {
            try
            {
                this.BeginInvoke((MethodInvoker)delegate
                {
                    foreach (TagData tag in tagData)
                    {
                        totalReads++;
                        string tagId = tag.TagID;
                        var existingTag = allTags.FirstOrDefault(t => t.TagID == tagId);

                        if (existingTag != null)
                        {
                            existingTag.SeenCount++;
                            existingTag.LastSeen = DateTime.Now;
                            if (tag.PeakRSSI > existingTag.PeakRSSI)
                            {
                                existingTag.PeakRSSI = tag.PeakRSSI;
                                existingTag.AntennaID = tag.AntennaID;
                            }
                        }
                        else
                        {
                            allTags.Add(new TagRecord
                            {
                                TagID = tagId,
                                SeenCount = 1,
                                PeakRSSI = tag.PeakRSSI,
                                AntennaID = tag.AntennaID,
                                LastSeen = DateTime.Now
                            });
                        }
                    }

                    // Update display
                    lblTotalTags.Text = allTags.Count.ToString();
                    lblReads.Text = totalReads.ToString();
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error handling tag data: {ex.Message}");
            }
        }

        private void Events_StatusNotify(object sender, Events.StatusEventArgs e)
        {
            Debug.WriteLine($"Status: {e.StatusEventData.StatusEventType}");
        }

        private void StartReading()
        {
            try
            {
                if (reader != null && isConnected)
                {
                    reader.Actions.Inventory.Perform();
                    startTime = DateTime.Now;
                    updateTimer.Start();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting reader: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void ConnectReader()
        {
            try
            {
                if (reader != null)
                {
                    reader.Disconnect();
                    reader = null;
                }

                reader = new RFIDReader(readerIP, 5084, 0);
                reader.Connect();
                reader.Events.ReadNotify += Events_ReadNotify;
                reader.Events.StatusNotify += Events_StatusNotify;
                reader.Events.AttachTagDataWithReadEvent = false;
                reader.Actions.PurgeTags();

                isConnected = true;
                lblStatus.Text = "Connected";
                lblStatus.ForeColor = Color.Green;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to connect: {ex.Message}", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Connection Failed";
                lblStatus.ForeColor = Color.Red;
            }
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

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            DisconnectReader();
        }
    }
}
