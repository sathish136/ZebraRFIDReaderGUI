using System;
using System.Windows.Forms;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.ServiceProcess;
using System.Configuration.Install;
using System.Reflection;

namespace ZebraRFIDReaderGUI
{
    static class Program
    {
        private static Form1 mainForm;
        private static IWebHost webHost;
        private static NotifyIcon trayIcon;

        [STAThread]
        static void Main(string[] args)
        {
            if (Environment.UserInteractive)
            {
                if (args.Length > 0)
                {
                    switch (args[0].ToLower())
                    {
                        case "/install":
                            InstallService();
                            return;
                        case "/uninstall":
                            UninstallService();
                            return;
                    }
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // Initialize tray icon
                InitializeTrayIcon();

                // Start the web host in a background task
                Task.Run(() => StartWebHost());

                // Create and run the Windows Forms application
                mainForm = new Form1(false); // Pass false to indicate non-service mode
                TagController.Initialize(mainForm);
                
                // Hide the main form and show only tray icon
                mainForm.ShowInTaskbar = false;
                mainForm.WindowState = FormWindowState.Minimized;
                mainForm.Hide();

                Application.Run();

                // Stop the web host when the application exits
                webHost?.StopAsync().Wait();
                trayIcon?.Dispose();
            }
            else
            {
                ServiceBase[] servicesToRun = new ServiceBase[] 
                { 
                    new RFIDReaderService() 
                };
                ServiceBase.Run(servicesToRun);
            }
        }

        private static void InitializeTrayIcon()
        {
            trayIcon = new NotifyIcon
            {
                Icon = System.Drawing.SystemIcons.Application,
                Visible = true,
                Text = "RFID Reader"
            };

            var contextMenu = new ContextMenuStrip();
            var settingsItem = new ToolStripMenuItem("Settings");
            var exitItem = new ToolStripMenuItem("Exit");

            settingsItem.Click += (s, e) => mainForm?.ShowSettings();
            exitItem.Click += (s, e) =>
            {
                trayIcon.Visible = false;
                Application.Exit();
            };

            contextMenu.Items.Add(settingsItem);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add(exitItem);

            trayIcon.ContextMenuStrip = contextMenu;
            trayIcon.DoubleClick += (s, e) => mainForm?.ShowSettings();
        }

        private static void StartWebHost()
        {
            webHost = new WebHostBuilder()
                .UseKestrel()
                .UseUrls("http://localhost:5000")
                .ConfigureServices(services =>
                {
                    services.AddCors(options =>
                    {
                        options.AddDefaultPolicy(builder =>
                        {
                            builder.AllowAnyOrigin()
                                   .AllowAnyMethod()
                                   .AllowAnyHeader();
                        });
                    });
                    services.AddMvc();
                    services.AddSwaggerGen();
                })
                .Configure(app =>
                {
                    app.UseCors();
                    app.UseMvc();
                    app.UseSwagger();
                    app.UseSwaggerUI();
                })
                .Build();

            webHost.Run();
        }

        private static void InstallService()
        {
            try
            {
                ManagedInstallerClass.InstallHelper(new[] { Assembly.GetExecutingAssembly().Location });
                MessageBox.Show("Service installed successfully!", "Installation", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error installing service: {ex.Message}", "Installation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void UninstallService()
        {
            try
            {
                ManagedInstallerClass.InstallHelper(new[] { "/u", Assembly.GetExecutingAssembly().Location });
                MessageBox.Show("Service uninstalled successfully!", "Uninstallation", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error uninstalling service: {ex.Message}", "Uninstallation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}