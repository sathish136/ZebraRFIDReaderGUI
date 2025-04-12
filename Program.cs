using System;
using System.Windows.Forms;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ZebraRFIDReaderGUI
{
    static class Program
    {
        private static Form1 mainForm;
        private static IWebHost webHost;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Start the web host in a background task
            Task.Run(() => StartWebHost());

            // Create and run the Windows Forms application
            mainForm = new Form1();
            TagController.Initialize(mainForm);
            Application.Run(mainForm);

            // Stop the web host when the application exits
            webHost?.StopAsync().Wait();
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
    }
}